namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Represents the dependency resolution result (see <see cref="IDependencyHandler2"/>).
	/// </summary>
	public readonly struct DependencyResult {

		#region Static members

		/// <summary>
		/// Представляет пустой результат разрешения функциональной зависимости.
		/// </summary>
		public static readonly DependencyResult None = default;

		public static DependencyResult CreateFrom<TDependenceInstance>(DependencyResult<TDependenceInstance> result)
			where TDependenceInstance : class
			=> new DependencyResult(instance: result.Instance, isNewInstance: result.IsNewInstance, redirectHandler: result.RedirectHandler);

		#endregion

		readonly object _instance;

		readonly bool _isNewInstance;

		readonly IDependencyHandler2 _redirectHandler;

		DependencyResult(object instance, bool isNewInstance, IDependencyHandler2 redirectHandler) {
			_instance = instance;
			_isNewInstance = isNewInstance;
			_redirectHandler = redirectHandler;
		}

		public DependencyResult(object instance, bool isNewInstance) {
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
		public object Instance
			=> _instance;

		/// <summary>
		/// Возвращает признак, указывающий, что результат разрешения функциональной зависимости есть новый экземпляр (объект).
		/// </summary>
		public bool IsNewInstance
			=> _isNewInstance;

		/// <summary>
		/// Возвращает исполнителя функциональной зависимости (<see cref="IDependencyHandler2"/>), которому должно быть передано разрешение функциональной зависисимости.
		/// </summary>
		public IDependencyHandler2 RedirectHandler
			=> _redirectHandler;

	}

}