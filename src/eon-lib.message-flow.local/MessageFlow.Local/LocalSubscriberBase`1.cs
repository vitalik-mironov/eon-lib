using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Threading;
using Eon.Threading.Tasks;

namespace Eon.MessageFlow.Local {

	[DebuggerDisplay("{ToString(),nq}")]
	public abstract class LocalSubscriberBase<TPayload>
		:DisposeNotifying, ILocalSubscriber, ILocalPublicationFilterListener {

		readonly string _aboutInfo;

		ILocalSubscription _activatedSubscription;

		SemaphoreSlim _processMessagePostLock;

		protected LocalSubscriberBase(string aboutInfo = default) {
			// TODO_HIGH: Implement lock naming — $"{nameof(LocalSubscriberBase<TData>)}.{nameof(_processMessagePostLock)}".
			//
			_processMessagePostLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
			_aboutInfo = aboutInfo;
		}

		// TODO: Put strings into the resources.
		//
		async Task ILocalSubscriber.OnSubscriptionActivationAsync(ILocalSubscription subscription, IContext ctx) {
			subscription.EnsureNotNull(nameof(subscription));
			//
			if (!ReferenceEquals(objA: subscription.Subscriber, objB: this))
				throw
					new ArgumentOutOfRangeException(
						paramName: nameof(subscription),
						message: $"Указанная подписка не относится к данному подписчику.{Environment.NewLine}\tПодписка:{subscription.FmtStr().GNLI2()}{Environment.NewLine}\tПодписчик:{this.FmtStr().GNLI2()}");
			else if (!ReferenceEquals(ReadDA(ref _activatedSubscription), subscription)) {
				ILocalSubscription original;
				var lck = ReadDA(ref _processMessagePostLock);
				var lckAcquired = false;
				try {
					lckAcquired = await lck.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds, cancellationToken: ctx.Ct()).ConfigureAwait(false);
					if (!lckAcquired)
						throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
					//
					if (!((original = WriteDA(ref _activatedSubscription, subscription, null)) == null || ReferenceEquals(original, subscription)))
						throw
							new InvalidOperationException(
								$"Невозможно активировать еще одну подписку для данного подписчика. Подписчик не поддерживает работу с несколькими подписками.{Environment.NewLine}\tПодписка:{Environment.NewLine}{subscription.FmtStr().GI2()}{Environment.NewLine}\tПодписчик:{Environment.NewLine}{this.FmtStr().GI2()}");
					else if (original == null)
						try {
							await OnSubscriptionActivationAsync(subscription: subscription, ctx: ctx).ConfigureAwait(false);
						}
						catch {
							Interlocked.CompareExchange(ref _activatedSubscription, original, subscription);
							throw;
						}
				}
				finally {
					if (lckAcquired)
						try { lck.Release(); }
						catch (ObjectDisposedException) { }
				}
			}
		}

		protected abstract Task OnSubscriptionActivationAsync(ILocalSubscription subscription, IContext ctx = default);

		// TODO: Put strings into the resources.
		//
		async Task ILocalSubscriber.OnSubscriptionDeactivationAsync(ILocalSubscription subscription, IContext ctx) {
			subscription.EnsureNotNull(nameof(subscription));
			//
			if (ReferenceEquals(subscription, ReadDA(ref _activatedSubscription))) {
				var lck = ReadDA(ref _processMessagePostLock);
				var lckAcquired = false;
				try {
					lckAcquired = await lck.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds, cancellationToken: ctx.Ct()).ConfigureAwait(false);
					if (!lckAcquired)
						throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
					//
					await OnSubscriptionDeactivationAsync(subscription: subscription, ctx: ctx).ConfigureAwait(false);
					Interlocked.CompareExchange(ref _activatedSubscription, null, comparand: subscription);
				}
				finally {
					if (lckAcquired)
						try { lck.Release(); }
						catch (ObjectDisposedException) { }
				}
			}
			else
				throw
					new InvalidOperationException(
						$"Недопустимый вызов операции деактивации подписки для данного подписчика. Указанная подписка не была активирована для подписчика.{Environment.NewLine}\tПодписка:{subscription.FmtStr().GNLI2()}{Environment.NewLine}\tПодписчик:{this.FmtStr().GNLI2()}");
		}

		protected abstract Task OnSubscriptionDeactivationAsync(ILocalSubscription subscription, IContext ctx = default);

		// TODO: Put strings into the resources.
		//
		public void EnsureHasActiveSubscription() {
			var subscription = ReadDA(ref _activatedSubscription);
			if (subscription == null)
				throw
					new InvalidOperationException($"Для подписчика не установлена подписка.{Environment.NewLine}\tПодписчик:{this.FmtStr().GNLI2()}");
			else if (!subscription.IsActive)
				throw
					new InvalidOperationException($"Ранее установленная для подписчика подписка не активна.{Environment.NewLine}\tПодписчик:{this.FmtStr().GNLI2()}{Environment.NewLine}\tПодписка:{subscription.FmtStr().GNLI2()}");
		}

		#region PreProcessMessageIssuing

		void ILocalPublicationFilterListener.PublicationFilter(object sender, LocalPublicationFilterEventArgs eventArgs) {
			eventArgs.EnsureNotNull(nameof(eventArgs));
			//
			var msg = eventArgs.Message as ILocalMessage<TPayload>;
			if (msg is null)
				eventArgs.Cancel = true;
			else {
				var locEventArgs = new LocalPublicationFilterEventArgs<TPayload>(subscription: eventArgs.Subscription, message: msg);
				PublicationFilter(locEventArgs);
				eventArgs.Cancel = locEventArgs.OnceCanceled;
			}
		}

		protected virtual void PublicationFilter(LocalPublicationFilterEventArgs<TPayload> e) {
			e.EnsureNotNull(nameof(e));
			//
			if (TryReadDA(location: ref _activatedSubscription, considerDisposeRequest: true, result: out var activatedSubscription))
				e.Cancel = !ReferenceEquals(e.Subscription, activatedSubscription);
			else
				e.Cancel = true;
		}

		#endregion

		#region ProcessMessagePostAsync

		// TODO: Put strings into the resources.
		//
		async Task ILocalSubscriber.ProcessMessagePostAsync(ILocalSubscription subscription, ILocalMessage msg, IContext ctx) {
			subscription.EnsureNotNull(nameof(subscription));
			msg.EnsureNotNull(nameof(msg));
			//
			if (TryReadDA(ref _activatedSubscription, considerDisposeRequest: true, out var activatedSubscription)
				&& !(activatedSubscription is null)
				&& TryReadDA(ref _processMessagePostLock, considerDisposeRequest: true, out var lck)) {
				//
				if (!ReferenceEquals(subscription, activatedSubscription))
					throw
						new EonException(
							message: $"Подписчик не может выполнить обработку поступившего сообщения, так как указанное сообщение не соответствует подписке, установленной для подписчика.{Environment.NewLine}\tПодписчик:{Environment.NewLine}{this.FmtStr().GI2()}{Environment.NewLine}\tПодписка:{Environment.NewLine}{activatedSubscription.FmtStr().GI2()}{Environment.NewLine}\tСообщение:{Environment.NewLine}{msg.FmtStr().GI2()}");
				else if (!(msg is ILocalMessage<TPayload> msgStricted))
					throw
						new EonException(
							message: $"Подписчик не может выполнить обработку поступившего сообщения. Тип сообщения не совместим с требуемым.{Environment.NewLine}\tПодписчик:{Environment.NewLine}{this.FmtStr().GI2()}{Environment.NewLine}\tПодписка:{Environment.NewLine}{activatedSubscription.FmtStr().GI2()}{Environment.NewLine}\tСообщение:{Environment.NewLine}{msg.FmtStr().GI2()}{Environment.NewLine}\tТип сообщения:{Environment.NewLine}{msg.GetType().FmtStr().G().IndentLines2()}{Environment.NewLine}\tТребуемый тип сообщения:{Environment.NewLine}{typeof(ILocalMessage<TPayload>).FmtStr().G().IndentLines2()}");
				else {
					var lckAcquired = false;
					try {
						lckAcquired = await lck.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds, cancellationToken: ctx.Ct()).ConfigureAwait(false);
						if (!lckAcquired)
							throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
						//
						if (TryReadDA(ref _activatedSubscription, considerDisposeRequest: true, out activatedSubscription) && !(activatedSubscription is null))
							await ProcessMessageAsync(subscription: subscription, message: msgStricted, ctx: ctx).ConfigureAwait(false);
					}
					finally {
						if (lckAcquired)
							try { lck.Release(); }
							catch (ObjectDisposedException) { }
					}
				}
			}
		}

		protected abstract Task ProcessMessageAsync(ILocalSubscription subscription, ILocalMessage<TPayload> message, IContext ctx = default);

		#endregion

		public string AboutInfo
			=> _aboutInfo;

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=> $"Type:{GetType().FmtStr().GNLI()}" + $"{Environment.NewLine}Info:{_aboutInfo.FmtStr().GNLI()}";

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_processMessagePostLock?.Dispose();
			}
			_activatedSubscription = null;
			_processMessagePostLock = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}