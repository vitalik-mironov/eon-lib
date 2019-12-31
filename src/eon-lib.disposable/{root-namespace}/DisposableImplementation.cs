#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Collections;
using Eon.Threading;

#endif

using System.Security;
using System.Threading;

using vlt = Eon.Threading.VolatileUtilities;

namespace Eon {

	public abstract class DisposableImplementation {

		#region Nested types

		sealed class P_Counters {

			public long CreateCount;

			public long ExplicitDisposeCount;

			public long ImplicitDisposeCount;

			internal P_Counters() { }

			public long CreateWithoutDisposeCount
				=> vlt.Read(ref CreateCount) - vlt.Read(ref ExplicitDisposeCount) - vlt.Read(ref ImplicitDisposeCount);

		}

		#endregion

		#region Static members

		const int __TrueFlag = 1;

		const int __FalseFlag = 0;

#if DEBUG
		static readonly Dictionary<Type, P_Counters> __Counters;

		static readonly PrimitiveSpinLock __CountersSpinLock;
#else
		static readonly P_Counters __Counters;
#endif

		static DisposableImplementation() {
#if DEBUG
			__Counters = new Dictionary<Type, P_Counters>();
			__CountersSpinLock = new PrimitiveSpinLock();
#else
			__Counters = new P_Counters();
#endif
		}

#if !DEBUG
		public static long CreateCount
			=> vlt.Read(ref __Counters.CreateCount);

		public static long ExplicitDisposeCount
			=> vlt.Read(ref __Counters.ExplicitDisposeCount);

		public static long ImplicitDisposeCount
			=> vlt.Read(ref __Counters.ImplicitDisposeCount);
#endif

		public static long GetCreateWithoutDisposeCount() {
#if DEBUG
			//var arr = __CountersSpinLock.Invoke(() => __Counters.OrderByDescending(locItem => locItem.Value.CreateWithoutDisposeCount).Select(locItem => (T: locItem.Key, V: locItem.Value.CreateWithoutDisposeCount)).Take(20).ToArray());
			return __CountersSpinLock.Invoke(() => __Counters.Values.Sum(locItem => locItem.CreateWithoutDisposeCount));
#else
			return __Counters.CreateWithoutDisposeCount;
#endif
		}

		#endregion

#if DEBUG
		readonly Type _type;
#endif

		int _isDisposeCounted;

#if DEBUG
		private protected DisposableImplementation(object disposable)
#else
		private protected DisposableImplementation() 
#endif
			{
			_isDisposeCounted = __FalseFlag;
#if DEBUG
			disposable.EnsureNotNull(nameof(disposable));
			_type = disposable.GetType();
			var counters = __Counters.GetOrAdd(spinLock: __CountersSpinLock, key: _type, factory: locKey => new P_Counters());
			Interlocked.Increment(ref counters.CreateCount);
#else
			Interlocked.Increment(ref __Counters.CreateCount);
#endif
		}

		public void Dispose(bool explicitDispose) {
			if (Interlocked.CompareExchange(ref _isDisposeCounted, __TrueFlag, __FalseFlag) == __FalseFlag) {
#if DEBUG
				var counters = __CountersSpinLock.Invoke(() => __Counters[ _type ]);
#else
				var counters = __Counters;
#endif
				if (explicitDispose)
					Interlocked.Increment(ref counters.ExplicitDisposeCount);
				else
					Interlocked.Increment(ref counters.ImplicitDisposeCount);
			}
			DoDispose(explicitDispose: explicitDispose);
		}

		[SecuritySafeCritical]
		private protected abstract void DoDispose(bool explicitDispose);

	}

}