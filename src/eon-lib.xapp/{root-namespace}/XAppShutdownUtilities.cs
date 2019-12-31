using System;
using System.Threading.Tasks;

using Eon.Description;
using Eon.Threading.Tasks;

namespace Eon {
	using IXApp = IXApp<IXAppDescription>;

	public static class XAppShutdownUtilities {

		public static void ShutdownApp(this IXApp app)
			=> ShutdownApp(app: app, exception: null);

		public static void ShutdownApp(this IXApp app, Exception exception) {
			if (!(app is null)) {
				try {
					app.ShutdownAppAsync().WaitWithTimeout();
				}
				catch (Exception firstException) {
					if (exception is null)
						throw;
					else
						throw new AggregateException(exception, firstException);
				}
			}
		}

		/// <summary>
		/// Выполняет <seealso cref="IXApp{TDescription}.ShutdownAppAsync"/>.
		/// <para>Если <seealso cref="IXApp{TDescription}.ShutdownAppAsync"/> вызывает исключение, то данный метод оборачивает его и передает вызывающему коду. Кроме этого, если указано <paramref name="exception"/>, то это исключение будет включено во внутренний стек передаваемого исключения.</para>
		/// </summary>
		/// <param name="app">
		/// Приложение.
		/// <para>Может быть null. В этом случае возврат из метода производится немедленно.</para> 
		/// </param>
		/// <param name="exception">
		/// Исключение, которое будет включено во внутренний стек исключения, вызванного данным методом.
		/// <para>Может быть null.</para>
		/// </param>
		/// <returns>Объект <seealso cref="Task"/>.</returns>
		public static async Task ShutdownAppAsync(this IXApp app, Exception exception) {
			if (!(app is null)) {
				try {
					await app.ShutdownAppAsync().ConfigureAwait(false);
				}
				catch (Exception locException) {
					if (locException is null)
						throw;
					else
						throw new AggregateException(locException, exception);
				}
			}
		}

	}

}