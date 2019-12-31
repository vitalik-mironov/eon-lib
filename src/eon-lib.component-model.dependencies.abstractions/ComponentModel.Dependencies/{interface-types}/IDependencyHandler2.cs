
namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// The second version of the dependency resolution handler <see cref="IDependencyHandler"/>.
	/// </summary>
	public interface IDependencyHandler2
#pragma warning disable CS0618 // Type or member is obsolete
		:IDependencyHandler {
#pragma warning restore CS0618 // Type or member is obsolete

		/// <summary>
		/// Возвращает признак, указывающий, что данный обработчик, следуя спецификации разрешения зависимости (см. <seealso cref="IDependencyResolutionSpecs"/>), может возвращать совместно используемый экземпляр зависимости.
		/// <para>Необходимым условием возврата совместно используемого экземпляра зависимости (сервиса) является отсутствие требования в спецификации разрешения получить именно новый экземпляр (см. <seealso cref="IDependencyResolutionSpecs.IsNewInstanceRequired"/>).</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		bool CanShareDependency { get; }

		/// <summary>
		/// Возвращает признак, указывающий, что данный обработчик в качестве результата может указать другой обработчик (см. <seealso cref="DependencyResult.RedirectHandler"/>), которому должна быть передана обработка зависимости.
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		bool CanRedirect { get; }

	}

}