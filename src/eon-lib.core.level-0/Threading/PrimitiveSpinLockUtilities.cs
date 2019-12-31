using System;

namespace Eon.Threading {

	public static class PrimitiveSpinLockUtilities {

		/// <summary>
		/// Вызывает указанный метод в try...finally секции спин-блокировки.
		/// <para>Указанный метод будет выполнен только в случае успешного захвата блокировки <paramref name="lck"/>. После выполнения метода блокировка будет освобождена вне зависимости от результата выполнения метода.</para>
		/// </summary>
		/// <param name="lck">Спин-блокировка.</param>
		/// <param name="action">Метод.</param>
		public static void Invoke(this PrimitiveSpinLock lck, Action action) {
			if (lck is null)
				throw new ArgumentNullException(paramName: nameof(lck));
			else if (action is null)
				throw new ArgumentNullException(paramName: nameof(action));
			//
			var lckTaken = false;
			try {
				lck.EnterLock(ref lckTaken);
				action();
			}
			finally {
				if (lckTaken)
					lck.ExitLock();
			}
		}

		/// <summary>
		/// Вызывает указанный метод в try...finally секции спин-блокировки.
		/// <para>Указанный метод будет выполнен только в случае успешного захвата блокировки <paramref name="lck"/>. После выполнения метода блокировка будет освобождена вне зависимости от результата выполнения метода.</para>
		/// </summary>
		/// <typeparam name="TResult">Тип возвращаемого методом <paramref name="func"/> значения.</typeparam>
		/// <param name="lck">Спин-блокировка.</param>
		/// <param name="func">Метод.</param>
		/// <returns>Возвращаемое методом <paramref name="func"/> значение типа <typeparamref name="TResult"/>.</returns>
		public static TResult Invoke<TResult>(this PrimitiveSpinLock lck, Func<TResult> func) {
			if (lck is null)
				throw new ArgumentNullException(paramName: nameof(lck));
			else if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			var lckTaken = false;
			try {
				lck.EnterLock(ref lckTaken);
				return func();
			}
			finally {
				if (lckTaken)
					lck.ExitLock();
			}
		}

	}

}