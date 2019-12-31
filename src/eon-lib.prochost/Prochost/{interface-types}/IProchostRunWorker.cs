using System;

namespace Eon.Prochost {

	/// <summary>
	/// In-process hosting worker.
	/// </summary>
	public interface IProchostRunWorker
	 :IDisposable {

		/// <summary>
		/// Runs worker loop.
		/// <para>Returns:</para>
		/// <para>• when an exception occurred during the loop start or when an exception occurred in the loop;</para>
		/// <para>• when the loop completed.</para>
		/// </summary>
		void Run();

	}

}