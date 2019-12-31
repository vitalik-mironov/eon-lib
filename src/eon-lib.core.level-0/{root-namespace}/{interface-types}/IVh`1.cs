using System;
using System.Runtime.ExceptionServices;

using Eon.Threading.Tasks;

namespace Eon {

	/// <summary>
	/// Defines a disposable value holder.
	/// </summary>
	/// <typeparam name="T">Value type constraint.</typeparam>
	public interface IVh<out T>
		:IOxyDisposable {

		/// <summary>
		/// Indicates whether holder contains error (<see cref="Exception"/>.
		/// <para>Property is dispose tolerant.</para>
		/// </summary>
		bool HasException { get; }

		/// <summary>
		/// Gets <see cref="ExceptionDispatchInfo"/>, that represents an error contained by this holder.
		/// </summary>
		ExceptionDispatchInfo Exception { get; }

		/// <summary>
		/// Gets a value contained by this holder.
		/// <para>Accessing this property will cause <see cref="ValueGetterException"/>, if holder contains error (<see cref="HasException"/>).</para>
		/// </summary>
		T Value { get; }

		/// <summary>
		/// Gets a value contained by this holder.
		/// <para>Accessing this property will cause <see cref="ValueGetterException"/>, if holder contains error (<see cref="HasException"/>).</para>
		/// <para>Dispose tolerant.</para>
		/// </summary>
		T ValueDisposeTolerant { get; }

		/// <summary>
		/// Возвращает признак, указывающий "владеет" ли хранилище значением <see cref="Value"/>.
		/// <para>Значение <see langword="true"/> указывает, что при выгрузке хранилища (см. <see cref="IDisposable.Dispose"/>) значение <see cref="Value"/> также будет выгружено (если его тип реализует <see cref="IDisposable"/>).</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		bool OwnsValue { get; }

		/// <summary>
		/// Removes value from this holder.
		/// <para>Dispose tolerant.</para>
		/// </summary>
		void RemoveValue();

		/// <summary>
		/// Возвращает значение, содержащееся в данном хранилище, как объект задачи с одним из финальных состояний (в зависимости от того, ошибка или значение содержится в хранилище).
		/// </summary>
		ITaskWrap<T> Awaitable();

	}

}