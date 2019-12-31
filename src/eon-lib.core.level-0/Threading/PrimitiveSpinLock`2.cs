﻿using System;
using System.Threading;

namespace Eon.Threading {

	public sealed class PrimitiveSpinLock<TInvokeState, TInvokeResult> {

		#region Static & constant members

		const int __LockHeldTrueFlag = 1;

		const int __LockHeldFalseFlag = 0;

		/// <summary>
		/// Значение: '536870910'.
		/// </summary>
		public static readonly int SpinCountLimit = 536870910;

		#endregion

		int _lockHeldFlag;

		public PrimitiveSpinLock() {
			_lockHeldFlag = __LockHeldFalseFlag;
		}

		public PrimitiveSpinLock(string name) {
			if (name is null)
				throw new ArgumentNullException(paramName: nameof(name));
			else if (name == string.Empty)
				throw new ArgumentException(paramName: nameof(name), message: "Can not be an empty string.");
			//
			Name = name;
		}

		public string Name { get; }

		public void ExitLock() {
			if (Interlocked.CompareExchange(ref _lockHeldFlag, __LockHeldFalseFlag, __LockHeldTrueFlag) != __LockHeldTrueFlag)
				throw new LockReleaseFailException(reason: LockReleaseFailReason.InconsequentRelease);
		}

		public void EnterLock(ref bool lockTaken) {
			if (lockTaken)
				throw new ArgumentOutOfRangeException(nameof(lockTaken));
			//
			var spinCountLimit = SpinCountLimit;
			var spinCount = 0;
			int originalLockHeldFlag;
			for (; ; ) {
				if (++spinCount > spinCountLimit)
					throw new LockAcquisitionFailException(resource: this, reason: LockAcquisitionFailReason.SpinningLimitReached);
				originalLockHeldFlag = __LockHeldTrueFlag;
				Thread.BeginCriticalRegion();
				try {
					lockTaken = (originalLockHeldFlag = Interlocked.CompareExchange(ref _lockHeldFlag, __LockHeldTrueFlag, __LockHeldFalseFlag)) == __LockHeldFalseFlag;
					if (originalLockHeldFlag == __LockHeldFalseFlag)
						break;
					Thread.Sleep(millisecondsTimeout: 1); // Выполнение любого готового к выполнению потока в системе (ОС).
				}
				catch {
					if (originalLockHeldFlag == __LockHeldFalseFlag) {
						Interlocked.Exchange(ref _lockHeldFlag, __LockHeldFalseFlag);
						lockTaken = false;
					}
					throw;
				}
				Thread.EndCriticalRegion();
			}
		}

		public void EnterAndExitLock() {
			var lckTaken = false;
			try {
				EnterLock(ref lckTaken);
			}
			finally {
				if (lckTaken)
					ExitLock();
			}
		}

		public override string ToString()
			=> $"{nameof(PrimitiveSpinLock)}{(string.IsNullOrEmpty(Name) ? string.Empty : $" '{Name}'")}";

		public TInvokeResult Invoke(TInvokeState state, Func<TInvokeState, TInvokeResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			var lckTaken = false;
			try {
				EnterLock(ref lckTaken);
				return func(arg: state);
			}
			finally {
				if (lckTaken)
					ExitLock();
			}
		}

	}

}