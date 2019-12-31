using System;
using System.Collections.Generic;

namespace Eon {

	/// <summary>
	/// Disposable instances registry.
	/// </summary>
	public interface IDisposeRegistry
		:IDisposable {

		/// <summary>
		/// Registers disposable instance.
		/// </summary>
		/// <param name="disposable">
		/// Instance.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		void Register(IDisposable disposable);

		IReadOnlyList<IDisposable> GetAlive(bool disposeTolerant = default);

	}

}