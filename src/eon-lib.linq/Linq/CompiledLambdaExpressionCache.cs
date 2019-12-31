using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

using Eon.Threading;

namespace Eon.Linq.Expressions {

	public class CompiledLambdaExpressionCache
		:Disposable {

		#region Nested types

		sealed class P_DefaultHolder {

			public readonly CompiledLambdaExpressionCache Value;

			public readonly IVh<CompiledLambdaExpressionCache> Holder;

			internal P_DefaultHolder(IVh<CompiledLambdaExpressionCache> holder) {
				holder.EnsureNotNull(nameof(holder));
				//
				Holder = holder;
				Value = holder.Value;
			}

		}

		abstract class P_CacheEntryBase
			:Disposable {

			protected P_CacheEntryBase() { }

		}

		sealed class P_CacheEntry<TDelegate>
			:P_CacheEntryBase {

			TDelegate _delegate;

			internal P_CacheEntry(TDelegate @delegate) {
				@delegate.EnsureNotNull(nameof(@delegate));
				//
				_delegate = @delegate;
			}

			public TDelegate Delegate
				=> ReadDA(ref _delegate);

			protected override void Dispose(bool explicitDispose) {
				_delegate = default;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		#region Static members

		static P_DefaultHolder __Default;

		public static CompiledLambdaExpressionCache Default
			=> VolatileUtilities.Read(ref __Default).Value;

		public static void SetDefault(CompiledLambdaExpressionCache defaultCache, bool ownsDefaultCache) {
			defaultCache.EnsureNotNull(nameof(defaultCache));
			//
			Interlocked.Exchange(ref __Default, new P_DefaultHolder(defaultCache.ToValueHolder(ownsDefaultCache)))?.Holder?.Dispose();
		}

		static CompiledLambdaExpressionCache() {
			__Default = new P_DefaultHolder(new CompiledLambdaExpressionCache().ToValueHolder(true));
		}

		#endregion

		PrimitiveSpinLock _spinLock;

		Dictionary<MethodInfo, P_CacheEntryBase> _methodCallCacheEntries;

		public CompiledLambdaExpressionCache() {
			_spinLock = new PrimitiveSpinLock();
			_methodCallCacheEntries = new Dictionary<MethodInfo, P_CacheEntryBase>();
		}

		public TDelegate GetOrAddMethodCall<TDelegate>(Expression<TDelegate> expression) {
			MethodCallExpression methodCallExpression;
			expression.EnsureNotNull(nameof(expression)).EnsureMethodCallBody(out methodCallExpression);
			//
			var key = methodCallExpression.Method;
			var result =
				ReadDA(ref _spinLock)
				.Invoke(
					func:
					() => {
						EnsureNotDisposeState();
						//
						P_CacheEntryBase cacheEntry;
						if (ReadDA(ref _methodCallCacheEntries).TryGetValue(key, out cacheEntry))
							return ((P_CacheEntry<TDelegate>)cacheEntry).Delegate;
						else
							return default;
					});
			//
			if (result == null) {
				result = expression.Compile();
				return
					ReadDA(ref _spinLock)
					.Invoke(
						() => {
							EnsureNotDisposeState();
							//
							P_CacheEntryBase cacheEntry;
							var locMethodCallCacheEntries = ReadDA(ref _methodCallCacheEntries);
							if (locMethodCallCacheEntries.TryGetValue(key, out cacheEntry))
								return ((P_CacheEntry<TDelegate>)cacheEntry).Delegate;
							else {
								locMethodCallCacheEntries.Add(key, new P_CacheEntry<TDelegate>(result));
								return result;
							}
						});
			}
			else
				return result;
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose && !Environment.HasShutdownStarted) {
				var defaultHolder = VolatileUtilities.Read(ref __Default);
				if (!(defaultHolder is null) && ReferenceEquals(this, defaultHolder.Value))
					Interlocked.CompareExchange(location1: ref __Default, value: new P_DefaultHolder(holder: new CompiledLambdaExpressionCache().ToValueHolder(ownsValue: true)), comparand: defaultHolder);
			}
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				// Данный (ниже) вызов производится для того, чтобы убедиться в отсутствии вызовов, использующих spinLock и возникших до выгрузки объекта.
				// После завершения вызова ниже попытка выполнения кода внутри захвата spinLock вызовет исключение ObjectDisposedException, при условии, что внутри захвата первой инструкцией является вызов метода EnsureNotDisposeState.
				//
				_spinLock?.EnterAndExitLock();
				//
				_methodCallCacheEntries?.Observe(i => i.Value.Dispose());
				_methodCallCacheEntries?.Clear();
			}
			_methodCallCacheEntries = null;
			_spinLock = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}