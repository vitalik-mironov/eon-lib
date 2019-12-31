using System.Collections.Generic;
using System.Linq;

namespace Eon.ComponentModel.Dependencies {

	public class DependencyResolutionModel<TScope>
		:DependencyResolutionModel, IDependencyResolutionModel<TScope>
		where TScope : class, IDependencyScope {

		TScope _scope;

		public DependencyResolutionModel(TScope scope, IEnumerable<IVh<IDependencyHandler2>> handlerChainSource = default)
			: base(handlerChainSource: handlerChainSource) {
			//
			scope.EnsureNotNull(nameof(scope));
			//
			_scope = scope;
		}

		public DependencyResolutionModel(TScope scope, IEnumerable<IDependencyHandler2> handlerChainSource = default, bool ownsHandlers = default)
			: this(scope: scope, handlerChainSource: handlerChainSource?.Select(locItem => locItem.ToValueHolder(ownsValue: ownsHandlers))) { }

		public TScope Scope
			=> ReadDA(ref _scope);

		protected override void Dispose(bool explicitDispose) {
			_scope = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}