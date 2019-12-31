namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Represents the dependency resolution result (see <see cref="IDependencyHandler2"/>).
	/// </summary>
	public readonly struct DependencyResult<TDependencyInstance>
		where TDependencyInstance : class {

		#region Static members

		/// <summary>
		/// Представляет пустой результат разрешения функциональной зависимости.
		/// </summary>
		public static readonly DependencyResult<TDependencyInstance> None = new DependencyResult<TDependencyInstance>();

		public static implicit operator DependencyResult(DependencyResult<TDependencyInstance> value)
			=> DependencyResult.CreateFrom(result: value);

		#endregion

		readonly TDependencyInstance _instance;

		readonly bool _isNewInstance;

		readonly IDependencyHandler2 _redirectHandler;

		public DependencyResult(TDependencyInstance instance, bool isNewInstance) {
			instance.EnsureNotNull(nameof(instance));
			//
			_instance = instance;
			_isNewInstance = isNewInstance;
			_redirectHandler = null;
		}

		public DependencyResult(IDependencyHandler2 redirectHandler) {
			redirectHandler.EnsureNotNull(nameof(redirectHandler));
			//
			_redirectHandler = redirectHandler;
			_instance = null;
			_isNewInstance = false;
		}

		/// <summary>
		/// Возвращает экземпляр, представляющий результат разрешения функциональной зависимости.
		/// </summary>
		public TDependencyInstance Instance
			=> _instance;

		/// <summary>
		/// Возвращает признак, указывающий, что результат разрешения функциональной зависимости есть новый экземпляр (объект).
		/// </summary>
		public bool IsNewInstance
			=> _isNewInstance;

		/// <summary>
		/// Возвращает обработчик функциональной зависимости (<see cref="IDependencyHandler2"/>), которому должно быть передано разрешение функциональной зависисимости.
		/// </summary>
		public IDependencyHandler2 RedirectHandler
			=> _redirectHandler;

	}

}