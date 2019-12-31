using System.Threading;

using Eon.Runtime.Options;

namespace Eon.Threading {

	public static class MonitorUtilities {

		/// <summary>
		/// Gets the current lock wait small timeout set by <see cref="LockSmallTimeoutOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static TimeoutDuration SmallTimeout
			=> LockSmallTimeoutOption.Require();

		/// <summary>
		/// Gets the current lock wait timeout set by <see cref="LockTimeoutOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static int DefaultLockTimeoutMilliseconds
			=> DefaultLockTimeout.Milliseconds;

		/// <summary>
		/// Gets the current lock wait timeout set by <see cref="LockTimeoutOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static TimeoutDuration DefaultLockTimeout
			=> LockTimeoutOption.Require();

		/// <summary>
		/// Выполняет попытку захвата исключительной блокировки ресурса <paramref name="resource"/> в течение таймаута <seealso cref="SmallTimeout"/> и в случае успеха освобождает блокировку.
		/// <para>Если в течение указанного таймаута захватить блокировку не удалось, генерируется исключения <seealso cref="LockAcquisitionFailException"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип ресурсра.</typeparam>
		/// <param name="resource">
		/// Ресурс (объект).
		/// <para>Не может быть null.</para>
		/// </param>
		public static void EnterAndExitLockSmallTimeout<T>(this T resource)
			where T : class {
			var lckTaken = false;
			try {
				resource.EnterLockSmallTimeout(ref lckTaken);
			}
			finally {
				if (lckTaken)
					resource.ExitLock();
			}
		}

		/// <summary>
		/// Выполняет попытку захвата исключительной блокировки ресурса <paramref name="resource"/> в течение таймаута <seealso cref="SmallTimeout"/>.
		/// <para>Если в течение указанного таймаута захватить блокировку не удалось, генерируется исключения <seealso cref="LockAcquisitionFailException"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип ресурсра.</typeparam>
		/// <param name="resource">
		/// Ресурс (объект).
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="lockTaken">
		/// Адрес переменной, куда будет помещен результат захвата блокировки.
		/// </param>
		public static void EnterLockSmallTimeout<T>(this T resource, ref bool lockTaken)
			where T : class
			=> EnterLock(resource, SmallTimeout.Milliseconds, ref lockTaken);

		/// <summary>
		/// Выполняет попытку захвата исключительной блокировки ресурса <paramref name="resource"/> в течение таймаута <seealso cref="DefaultLockTimeoutMilliseconds"/> и в случае успеха освобождает блокировку.
		/// <para>Если в течение указанного таймаута захватить блокировку не удалось, генерируется исключения <seealso cref="LockAcquisitionFailException"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип ресурсра.</typeparam>
		/// <param name="resource">
		/// Ресурс (объект).
		/// <para>Не может быть null.</para>
		/// </param>
		public static void EnterAndExitLock<T>(this T resource)
			where T : class {
			var lckTaken = false;
			try {
				resource.EnterLock(ref lckTaken);
			}
			finally {
				if (lckTaken)
					resource.ExitLock();
			}
		}

		/// <summary>
		/// Выполняет попытку захвата исключительной блокировки ресурса <paramref name="resource"/> в течение таймаута <seealso cref="DefaultLockTimeoutMilliseconds"/>.
		/// <para>Если в течение указанного таймаута захватить блокировку не удалось, генерируется исключения <seealso cref="LockAcquisitionFailException"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип ресурсра.</typeparam>
		/// <param name="resource">
		/// Ресурс (объект).
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="lockTaken">
		/// Адрес переменной, куда будет помещен результат захвата блокировки.
		/// </param>
		public static void EnterLock<T>(this T resource, ref bool lockTaken)
			where T : class
			=> EnterLock(resource: resource, millisecondsTimeout: DefaultLockTimeoutMilliseconds, lockTaken: ref lockTaken);

		/// <summary>
		/// Выполняет попытку захвата исключительной блокировки ресурса <paramref name="resource"/> в течение таймаута <paramref name="millisecondsTimeout"/>.
		/// <para>Если в течение указанного таймаута захватить блокировку не удалось, генерируется исключения <seealso cref="LockAcquisitionFailException"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип ресурсра.</typeparam>
		/// <param name="resource">
		/// Ресурс (объект).
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="millisecondsTimeout">
		/// Таймаут ожидания захвата блокировки в миллисекундах.
		/// <para>Значение <seealso cref="Timeout.Infinite"/> соответствует бесконечному таймауту ожидания захвата блокировки.</para>
		/// </param>
		/// <param name="lockTaken">
		/// Адрес переменной, куда будет помещен результат захвата блокировки.
		/// </param>
		public static void EnterLock<T>(this T resource, int millisecondsTimeout, ref bool lockTaken)
			where T : class {
			resource.EnsureNotNull(nameof(resource));
			//
			Monitor.TryEnter(resource, millisecondsTimeout, ref lockTaken);
			if (!lockTaken)
				throw new LockAcquisitionFailException(resource, LockAcquisitionFailReason.TimeoutElapsed);
		}

		public static void TryEnterLock<T>(this T resource, ref bool lockTaken)
			where T : class {
			resource.EnsureNotNull(nameof(resource));
			//
			Monitor.TryEnter(obj: resource, millisecondsTimeout: DefaultLockTimeoutMilliseconds, lockTaken: ref lockTaken);
		}

		public static void ExitLock<T>(this T resource)
			where T : class {
			resource.EnsureNotNull(nameof(resource));
			//
			Monitor.Exit(resource);
		}

	}

}