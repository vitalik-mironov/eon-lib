using System;

namespace Eon.ComponentModel.Dependencies {

	public class DefaultOuterDependencyScopeGetter
		:Disposable, IOuterDependencyScopeGetter {

		IDependencyScope _outerScope;

		Func<IDependencyScope> _outerScopeGetter;

		protected DefaultOuterDependencyScopeGetter() {
			_outerScope = null;
			_outerScopeGetter = DoGetOuterScope;
		}

		public DefaultOuterDependencyScopeGetter(IDependencyScope outerScope) {
			_outerScope = outerScope;
			_outerScopeGetter = () => ReadDA(ref _outerScope);
		}

		public DefaultOuterDependencyScopeGetter(Func<IDependencyScope> outerScopeGetter) {
			outerScopeGetter.EnsureNotNull(nameof(outerScopeGetter));
			//
			_outerScope = null;
			_outerScopeGetter = outerScopeGetter;
		}

		public IDependencyScope GetOuterScope()
			=> ReadDA(ref _outerScopeGetter)();

		/// <summary>
		/// Gets outer scope.
		/// <para>Warning! This method throws <see cref="NotImplementedException"/> and should be overridden by inheritor.</para>
		/// </summary>
		protected virtual IDependencyScope DoGetOuterScope() {
			// Этот метод должен реализовываться наследниками.
			//
			throw new NotImplementedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotImplemented);
		}

		protected override void Dispose(bool explicitDispose) {
			_outerScope = null;
			_outerScopeGetter = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}