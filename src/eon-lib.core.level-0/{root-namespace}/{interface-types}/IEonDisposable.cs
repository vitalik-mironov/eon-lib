using System;

namespace Eon {

	/// <summary>
	/// Определяет расширенную версию <seealso cref="IDisposable"/>.
	/// </summary>
	public interface IEonDisposable
		:IDisposable {

		/// <summary>
		/// Выполняет проверку на предмет состояния выгрузки данного объекта.
		/// <para>Если для объекта была начата выгрузка или он уже выгружен (см. <seealso cref="Disposing"/> и <seealso cref="IsDisposed"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// </summary>
		/// <exception cref="ObjectDisposedException">Когда данный объект либо выгружается, либо уже выгружен.</exception>
		void EnsureNotDisposeState();

		/// <summary>
		/// Выполняет проверку на предмет состояния выгрузки данного объекта или наличия запроса на его выгрузку (см. <seealso cref="IsDisposeRequested"/>).
		/// <para>Если для объекта была начата выгрузка или он уже выгружен (см. <seealso cref="Disposing"/> и <seealso cref="IsDisposed"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// <para>Если <paramref name="considerDisposeRequest"/> == true и для объекта была запрошена выгрузка (см. <seealso cref="IsDisposeRequested"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// </summary>
		/// <param name="considerDisposeRequest">
		/// Признак, указывающий проверять ли наличие запроса на выгрузку.
		/// </param>
		void EnsureNotDisposeState(bool considerDisposeRequest);

		/// <summary>
		/// Возвращает признак, указывающий, находится ли данный объект в состоянии выгрузки, но при этом еще не выгружен.
		/// </summary>
		bool Disposing { get; }

		/// <summary>
		/// Возвращает признак, указывающий, что данный объект выгружен.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// Возвращает признак, указывающий был ли хотя бы один запрос выгрузки данного объекта (вызов метода <seealso cref="IDisposable.Dispose"/>).
		/// <para>Следует учитывать, что после того, как поступил запрос выгрузки, модификация состояния объекта приведет к вызову исключения <seealso cref="ObjectDisposedException"/>.</para>
		/// </summary>
		bool IsDisposeRequested { get; }

	}

}