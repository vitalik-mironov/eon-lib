using System;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Diagnostics;

namespace Eon {

	/// <summary>
	/// XApp-container control.
	/// </summary>
	public interface IXAppContainerControl
		:IDisposeNotifying {

		/// <summary>
		/// Gets control configuration.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		IXAppContainerControlConfiguration Configuration { get; }

		/// <summary>
		/// Gets container run state (see <see cref="XAppContainerControlRunState"/>).
		/// <para>Throws <see cref="EonException"/> if no start has been initiated or start was failed.</para>
		/// </summary>
		XAppContainerControlRunState RunState { get; }

		/// <summary>
		/// Starts container.
		/// </summary>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		Task StartAsync(IContext ctx = default);

		/// <summary>
		/// Indicates whether shutdown has been requested (see <seealso cref="ShutdownAsync"/>).
		/// <para>Shutdown implies stop and dispose container.</para>
		/// </summary>
		bool HasShutdownRequested { get; }

		IUnhandledExceptionObserver UnhandledExceptionObserver { get; }

		/// <summary>
		/// Fires when shutdown finished successfully (see <see cref="ShutdownAsync"/>).
		/// </summary>
		event EventHandler ShutdownFinished;

		/// <summary>
		/// Starts container.
		/// </summary>
		/// <param name="progress">
		/// Provider for progress updates.
		/// </param>
		Task StartAsync(IAsyncProgress<IFormattable> progress = default);

		/// <summary>
		/// Shutdowns container.
		/// <para>Shutdown implies stop and dispose container.</para>
		/// </summary>
		Task ShutdownAsync();

	}

}