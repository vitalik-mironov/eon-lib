using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Eon.Collections;
using Eon.ComponentModel;
using Eon.Context;
using Eon.Linq;
using Eon.Triggers.Description;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Triggers {
	using ITriggerXInstance = ITrigger<ITriggerDescription>;

	public class AggregateTrigger<TDescription>
		:TriggerBase<TDescription>
		where TDescription : class, IAggregateTriggerDescription {

		#region Nested types

		protected class ActivationAttemptTag {

			ITriggerSignalProperties _recentDuringActivationSignal;

			public ActivationAttemptTag() { }

			public ITriggerSignalProperties RecentDuringActivationSignal {
				get => itrlck.Get(ref _recentDuringActivationSignal);
				set => itrlck.Set(ref _recentDuringActivationSignal, value.EnsureNotNull(nameof(value)).Value);
			}

		}

		// TODO: Перенести в ActivateAttemptTag.
		//
		sealed class P_ActivationStateObject {

			public readonly IImmutableSet<ITriggerXInstance> TriggerSet;

			public readonly IReadOnlyList<IDisposable> TriggerSignalSubscriptions;

			internal P_ActivationStateObject(IImmutableSet<ITriggerXInstance> triggerSet, IReadOnlyList<IDisposable> triggerSignalSubscriptions) {
				TriggerSet = triggerSet;
				TriggerSignalSubscriptions = triggerSignalSubscriptions;
			}

		}

		#endregion

		IReadOnlyList<ITriggerXInstance> _triggers;

		P_ActivationStateObject _activationState;

		public AggregateTrigger(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) { }

		// TODO: Put strings into the resources.
		//
		P_ActivationStateObject P_ActivationState {
			get {
				var state = ReadIA(ref _activationState, isNotRequired: true, locationName: nameof(_activationState));
				if (state is null)
					throw new EonException(message: $"Компонент не был еще активирован или уже деактивирован.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
				else
					return state;
			}
		}

		public IReadOnlyList<ITriggerXInstance> Triggers
			=> ReadIA(ref _triggers, locationName: nameof(_triggers));

		protected sealed override bool IsAfterActivationEnabled
			=> true;

		protected override void CreateActivateAttemptState(in RunControlAttemptStateFactoryArgs args, out IRunControlAttemptState state)
			=> state = new RunControlAttemptState(args: in args, overrideTag: args.Tag.ArgProp($"{nameof(args)}.{nameof(args.Tag)}").EnsureIsNull().Value ?? new ActivationAttemptTag());

		protected override async Task OnInitializeAsync(IContext ctx = default) {
			await base.OnInitializeAsync(ctx: ctx).ConfigureAwait(false);
			//
			IList<ITriggerXInstance> triggers = default;
			try {
				triggers = await TriggerFactoryUtilities.CreateInitializeAsync(scope: this, descriptions: Description.Triggers.Where(locDescription => !locDescription.IsDisabled).Arg($"{nameof(Description)}.{nameof(Description.Triggers)}"), ctx: ctx).ConfigureAwait(false);
				WriteDA(ref _triggers, new ListReadOnlyWrap<ITriggerXInstance>(list: triggers));
			}
			catch (Exception exception) {
				triggers.DisposeMany(exception: exception);
				throw;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected override async Task DoActivateAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			var tag = state.Tag.ArgProp($"{nameof(state)}.{nameof(state.Tag)}").EnsureNotNull().EnsureOfType<ActivationAttemptTag>().Value;
			//
			ImmutableHashSet<ITriggerXInstance>.Builder triggerSetBuilder = default;
			List<IDisposable> triggerSubscriptions = default;
			List<ITriggerXInstance> activatedTriggers = default;
			P_ActivationStateObject activationState = default;
			try {
				var triggers = Triggers;
				for (var y = 0; y < triggers.Count; y++) {
					var trigger = triggers[ y ];
					if (HasDeactivationRequested)
						throw new OperationCanceledException();
					else {
						triggerSetBuilder = triggerSetBuilder ?? ImmutableHashSet.CreateBuilder<ITriggerXInstance>();
						if (triggerSetBuilder.Add(trigger)) {
							triggerSubscriptions = triggerSubscriptions ?? new List<IDisposable>();
							triggerSubscriptions.Add(TriggerUtilities.SubscribeToSignal(trigger: trigger, onSignal: P_Trigger_OnSignal, state1: state, state2: tag));
						}
					}
				}
				activationState = new P_ActivationStateObject(triggerSet: triggerSetBuilder?.ToImmutable(), triggerSignalSubscriptions: triggerSubscriptions?.ReadOnlyWrap());
				if (!UpdDAIfNullBool(location: ref _activationState, value: activationState))
					throw new EonException(message: $"Недопустимая операция. Активация компонента ранее уже была выполнена.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
				if (triggerSetBuilder?.Count > 0) {
					activatedTriggers = new List<ITriggerXInstance>();
					foreach (var trigger in triggerSetBuilder) {
						if (HasDeactivationRequested)
							throw new OperationCanceledException();
						else {
							await trigger.ActivateControl.StartAsync().ConfigureAwait(false);
							activatedTriggers.Add(trigger);
						}
					}
				}
			}
			catch (Exception exception) {
				itrlck.SetNullBool(ref _activationState, activationState);
				var caughtExceptions = new List<Exception>() { exception };
				foreach (var triggerSubscription in triggerSubscriptions.EmptyIfNull())
					try { triggerSubscription.Dispose(); }
					catch (Exception secondException) { caughtExceptions.Add(secondException); }
				foreach (var activatedTrigger in activatedTriggers.EmptyIfNull())
					try { await activatedTrigger.ActivateControl.StopAsync().ConfigureAwait(false); }
					catch (Exception secondException) { caughtExceptions.Add(secondException); }
				if (caughtExceptions.Count > 1)
					throw new AggregateException(innerExceptions: caughtExceptions);
				else
					throw;
			}
		}

		protected override async Task DoAfterActivateAsync(IRunControlAttemptSuccess result) {
			var tag = result.Tag.ArgProp($"{nameof(result)}.{nameof(result.Tag)}").EnsureNotNull().EnsureOfType<ActivationAttemptTag>().Value;
			//
			await base.DoAfterActivateAsync(result: result).ConfigureAwait(false);
			//
			if (!(tag.RecentDuringActivationSignal is null))
				SignalAtMostOne(signalProps: new TriggerSignalProperties(trigger: this, source: tag.RecentDuringActivationSignal));
		}


		protected override async Task DoDeactivateAsync() {
			await base.DoDeactivateAsync().ConfigureAwait(false);
			//
			var activationState = ReadDA(ref _activationState);
			if (!(activationState is null)) {
				var caughtExceptions = new List<Exception>();
				if (!(activationState.TriggerSignalSubscriptions is null))
					foreach (var subscription in activationState.TriggerSignalSubscriptions)
						try { subscription.Dispose(); }
						catch (Exception exception) { caughtExceptions.Add(exception); }
				if (!(activationState.TriggerSet is null))
					foreach (var trigger in activationState.TriggerSet)
						try { await trigger.ActivateControl.StopAsync().ConfigureAwait(false); }
						catch (Exception exception) { caughtExceptions.Add(exception); }
				itrlck.SetNullBool(location: ref _activationState, comparand: activationState);
				if (caughtExceptions.Count > 0)
					throw new AggregateException(innerExceptions: caughtExceptions);
			}
		}

		void P_Trigger_OnSignal(ITriggerSignalProperties signalProps, IDisposable subscription, IRunControlAttemptProperties activationAttempt, ActivationAttemptTag activationAttemptTag) {
			signalProps.EnsureNotNull(nameof(signalProps));
			subscription.EnsureNotNull(nameof(subscription));
			activationAttempt.EnsureNotNull(nameof(activationAttempt));
			activationAttemptTag.EnsureNotNull(nameof(activationAttemptTag));
			//
			var activationState = TryReadDA(ref _activationState, considerDisposeRequest: true);
			if (!(activationState is null)
				&& (activationState.TriggerSet?.Contains(signalProps.Trigger) ?? false)
				&& (activationState.TriggerSignalSubscriptions?.Contains(subscription) ?? false)) {
				//
				try { if (signalProps.Trigger.IsDisabled) return; }
				catch (ObjectDisposedException) { return; }
				if (activationAttempt.RunControl.IsStarting)
					activationAttemptTag.RecentDuringActivationSignal = signalProps;
				else
					Signal(signalProps: new TriggerSignalProperties(trigger: this, source: signalProps));
			}
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_triggers?.DisposeMany();
			}
			_triggers = null;
			_activationState = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}