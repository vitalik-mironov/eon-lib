using System;
using System.Collections.Generic;

namespace Eon.ComponentModel.Dependencies {

	public class DefaultDependencyExporter
		:Disposable, IDependencyExporter {

		IDependencySupport _dependencies;

		IVh<IDependencyHandler2>[ ] _handlerChain;

		Func<IEnumerable<IVh<IDependencyHandler2>>> _exportDependencies;

		public DefaultDependencyExporter(IDependencySupport dependencies) {
			dependencies.EnsureNotNull(nameof(dependencies));
			//
			_dependencies = dependencies;
			_handlerChain = null;
			_exportDependencies = P_ExportDependencies_FromDependencySupport;
		}

		public DefaultDependencyExporter(IDependencyHandler2 handler, bool ownsHandler = default) {
			handler.EnsureNotNull(nameof(handler));
			//
			_dependencies = null;
			_handlerChain = new IVh<IDependencyHandler2>[ ] { handler.ToValueHolder(ownsValue: ownsHandler) };
			_exportDependencies = P_ExportDependencies_FromHandlerChain;
		}

		IEnumerable<IVh<IDependencyHandler2>> P_ExportDependencies_FromDependencySupport()
			=> ReadDA(location: ref _dependencies).LocalDependencies();

		IEnumerable<IVh<IDependencyHandler2>> P_ExportDependencies_FromHandlerChain()
			=> EnumerateDA(location: ref _handlerChain);

		public virtual IEnumerable<IVh<IDependencyHandler2>> ExportDependencies()
			=> ReadDA(location: ref _exportDependencies)();

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_handlerChain?.DisposeAndClearArray();
			_dependencies = null;
			_handlerChain = null;
			_exportDependencies = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}