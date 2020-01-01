using System;

namespace Eon.Runtime {

	/// <summary>
	/// Info about the version of EON runtime.
	/// </summary>
	public interface IEonRuntimeVersionInfo {

		string CompanyProducerViewName { get; }

		Version Version { get; }

		string VersionName { get; }

		string DisplayName { get; }

		string ProductName { get; }

	}

}