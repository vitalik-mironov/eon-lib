using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Eon.Linq;

namespace Eon.Threading {

	public static class WaitHandleUtilities {

		/// <summary>
		/// Waits for any of the elements in the specified array to receive a signal, infinitely.
		/// </summary>
		/// <param name="first">A first element to receive signal.</param>
		/// <param name="others">Other elements to received signal. Can be null, empty.</param>
		/// <returns>Returns a signaled element.</returns>
		/// <exception cref="System.ArgumentNullException">When <paramref name="first"/> is null.</exception>
		/// <exception cref="System.ArgumentException">When <paramref name="others"/> contains a null.</exception>
		public static WaitHandle WaitAnyWith(this WaitHandle first, params WaitHandle[ ] others) {
			if (first is null)
				throw new ArgumentNullException(paramName: nameof(first));
			WaitHandle[ ] allWaitHandles;
			if (others is null || others.Length < 1)
				allWaitHandles = new WaitHandle[ ] { first };
			else
				allWaitHandles = first.Sequence().Concat(second: ((IEnumerable<WaitHandle>)others).Arg(name: nameof(others)).EnsureNoNullElements().Value).ToArray();
			var signaledWaitHandleIndex = WaitHandle.WaitAny(waitHandles: allWaitHandles, millisecondsTimeout: Timeout.Infinite);
			return allWaitHandles[ signaledWaitHandleIndex ];
		}

		public static WaitHandle WaitAny(this WaitHandle[ ] handles, int millisecondsTimeout)
			=> WaitAny(handles: handles, timeout: new TimeoutDuration(millisecondsTimeout));

		public static WaitHandle WaitAny(this WaitHandle[ ] handles, TimeoutDuration timeout) {
			handles.EnsureNotNull(nameof(handles));
			timeout.EnsureNotNull(nameof(timeout));
			//
			var signaledWaitHandleIndex = WaitHandle.WaitAny(waitHandles: handles, millisecondsTimeout: timeout.IsInfinite ? Timeout.Infinite : timeout.Milliseconds);
			if (signaledWaitHandleIndex == WaitHandle.WaitTimeout)
				return null;
			else
				return handles[ signaledWaitHandleIndex ];
		}

	}

}