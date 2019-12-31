using System;

namespace Eon.Runtime {

	/// <summary>
	/// Info about the version of Oxy runtime.
	/// </summary>
	public interface IOxyRuntimeVersionInfo {

		string CompanyProducerViewName { get; }

		Version Version { get; }

		string VersionName { get; }

		string DisplayName { get; }

		string ProductName { get; }

	}

}