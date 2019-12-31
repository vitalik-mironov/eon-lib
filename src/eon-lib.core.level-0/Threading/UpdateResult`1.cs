using System;

namespace Eon.Threading {

	/// <summary>
	/// Result of the interlocked-update (see <seealso cref="InterlockedUtilities.Update{T}(ref T, Transform2{T})"/>).
	/// </summary>
	/// <typeparam name="T">Type of updatable location (var).</typeparam>
	public readonly struct UpdateResult<T>
		:IInterlockedUpdateResult<T>
		where T : class {

		#region Static members

		public static readonly UpdateResult<T> Default = new UpdateResult<T>(current: default, original: default);

		public static implicit operator T(UpdateResult<T> value) => value.Current;

		#endregion

		readonly T _current;

		readonly T _original;

		readonly bool _isUpdated;

		readonly bool _isProposedSameAsOriginal;

		readonly bool _isValid;

		public UpdateResult(T current, T original) {
			_current = current;
			_original = original;
			_isUpdated = !ReferenceEquals(current, original);
			_isProposedSameAsOriginal = !_isUpdated;
			_isValid = true;
		}

		public UpdateResult(T current, T original, bool isProposedSameAsOriginal) {
			_current = current;
			_original = original;
			_isUpdated = !ReferenceEquals(current, original);
			_isProposedSameAsOriginal = isProposedSameAsOriginal;
			_isValid = true;
		}

		/// <summary>
		/// Возвращает текущую (после выполнения операции) ссылку.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если структура не является валидной (см. <seealso cref="IsValid"/>).</para>
		/// </summary>
		public T Current {
			get {
				EnsureValid();
				return _current;
			}
		}

		/// <summary>
		/// Возвращает оригинальную ссылку.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если структура не является валидной (см. <seealso cref="IsValid"/>).</para>
		/// </summary>
		public T Original {
			get {
				EnsureValid();
				return _original;
			}
		}

		/// <summary>
		/// Возвращает признак, указывающий, что ссылка была заменена.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если структура не является валидной (см. <seealso cref="IsValid"/>).</para>
		/// </summary>
		public bool IsUpdated {
			get {
				EnsureValid();
				return _isUpdated;
			}
		}

		/// <summary>
		/// Возвращает признак, обозначающий, что указанная для замены новая ссылка представляет объект, эквивалентный объекту, на который указывает оригинальная ссылка.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если структура не является валидной (см. <seealso cref="IsValid"/>).</para>
		/// </summary>
		public bool IsProposedSameAsOriginal {
			get {
				EnsureValid();
				return _isProposedSameAsOriginal;
			}
		}

		/// <summary>
		/// Возвращает признак, указывающий на валидность структуры.
		/// <para>Структура считается валидной, если она была создана путем вызова одного из параметризованных конструкторов.</para>
		/// </summary>
		public bool IsValid
			=> _isValid;

		// TODO: Put strings into the resources.
		//
		/// <summary>
		/// Выполняется проверку данной структуры на предмет её валидности (см. <seealso cref="IsValid"/>.
		/// <para>Метод вызывает исключение <seealso cref="InvalidOperationException"/>, если структура не является валидной.</para>
		/// </summary>
		public void EnsureValid() {
			if (!_isValid)
				throw
					new InvalidOperationException(
						message: $"Невозможно выполнить операцию, так как объект не был корректно инициализирован и, как следствие, не имеет валидного состояния.{Environment.NewLine}\tОбъект:{Environment.NewLine}\t\t{ToString()}");
		}

	}

}