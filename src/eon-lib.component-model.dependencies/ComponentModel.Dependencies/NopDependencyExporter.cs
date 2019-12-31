using System.Collections.Generic;
using System.Linq;

namespace Eon.ComponentModel.Dependencies {

	public sealed class NopDependencyExporter
		:IDependencyExporter {

		#region Static members

		public static readonly NopDependencyExporter Instance = new NopDependencyExporter();

		#endregion

		readonly IEnumerable<IVh<IDependencyHandler2>> _dependencies;

		NopDependencyExporter() {
			_dependencies = Enumerable.Empty<IVh<IDependencyHandler2>>();
		}

		public IEnumerable<IVh<IDependencyHandler2>> ExportDependencies()
			=> _dependencies;

		public void Dispose() { }

	}

}