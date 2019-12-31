using System.Runtime.Serialization;
using System.Threading.Tasks;

using Eon.Threading.Tasks;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Представляет 2-ю версию обработчика (исполнителя разрешения) функциональной зависимости <seealso cref="DependencyHandlerBase"/>.
	/// </summary>
	[DataContract]
	public abstract class DependencyHandlerBase2<TDependencyInstance>
		:DependencyHandlerBase, IDependencyHandler2
		where TDependencyInstance : class {

		protected DependencyHandlerBase2() { }

		/// <summary>
		/// Возвращает признак, указывающий, что данный обработчик в качестве результата может указать другой обработчик (см. <seealso cref="DependencyResult.RedirectHandler"/>), которому должна быть передана обработка зависимости.
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		public virtual bool CanRedirect {
			get {
				EnsureNotDisposeState();
				return false;
			}
		}

		/// <summary>
		/// Возвращает признак, указывающий, что данный обработчик, следуя спецификации разрешения зависимости (см. <seealso cref="IDependencyResolutionSpecs"/>), может возвращать совместно используемый экземпляр зависимости.
		/// <para>Необходимым условием возврата совместно используемого экземпляра зависимости (сервиса) является отсутствие требования в спецификации разрешения получить именно новый экземпляр (см. <seealso cref="IDependencyResolutionSpecs.IsNewInstanceRequired"/>).</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		public abstract bool CanShareDependency { get; }

		protected sealed override DependencyResult ResolveDependency(IDependencyResolutionContext context)
			=> ResolveDependencyAsync(context: context).WaitResultWithTimeout();

		protected abstract Task<DependencyResult<TDependencyInstance>> ResolveDependencyAsync(IDependencyResolutionContext context);

	}

}