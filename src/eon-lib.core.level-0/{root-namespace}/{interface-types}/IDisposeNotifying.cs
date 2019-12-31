using System;

namespace Eon {

	/// <summary>
	/// Определяет расширеную версию программного интерфейса <seealso cref="IOxyDisposable"/>, поддерживающую события выгрузки объекта.
	/// </summary>
	public interface IDisposeNotifying
		:IOxyDisposable {

		/// <summary>
		/// Событие, вызываемое перед выполнением явной выгрузки.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Когда объект находится в состоянии выгрузки или уже выгружен.</exception>
		event EventHandler<DisposeEventArgs> BeforeDispose;

		/// <summary>
		/// Событие, вызываемое после выполнения явной выгрузки.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Когда объект находится в состоянии выгрузки или уже выгружен.</exception>
		event EventHandler<DisposeEventArgs> AfterDisposed;

	}

}