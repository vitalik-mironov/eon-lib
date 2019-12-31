using System.Threading.Tasks;

namespace Eon.Threading.Tasks {

	/// <summary>
	/// Обертка задачи <see cref="Task{TResult}"/>.
	/// </summary>
	/// <typeparam name="TResult">Тип результата выполнения задачи.</typeparam>
	public interface ITaskWrap<out TResult> {

		/// <summary>
		/// Обернутая задача.
		/// <para>Фактический тип значения — <see cref="Task{TResult}"/>.</para>
		/// </summary>
		/// <returns>Объект задачи <see cref="Task{TResult}"/>.</returns>
		Task Task { get; }

		/// <summary>
		/// Получает результат выполнения задачи.
		/// <para>Вызов данного метода блокирует выполнение текущего потока до тех пор, пока задача не перейдет в одно из финальных состояний.</para>
		/// </summary>
		/// <returns>Результат <typeparamref name="TResult"/> выполнения задачи.</returns>
		TResult GetResult();

		/// <summary>
		/// Возвращает объект задачи <see cref="Task{TResult}"/>, где TResult — <typeparamref name="TToResult"/>.
		/// <para>Метод не конвертирует сам объект обернутой задачи <see cref="Task"/>, конвертирует результат обернутой задачи и возвращает его в виде <see cref="Task{TResult}"/>, где TResult — <typeparamref name="TToResult"/>.</para>
		/// <para>Метод не предназначен для прямого вызова из кода.</para>
		/// </summary>
		/// <typeparam name="TToResult">Тип результата.</typeparam>
		/// <returns>Объект задачи <seealso cref="Task{TResult}"/>.</returns>
		Task<TToResult> ConvertTask<TToResult>();

	}

}