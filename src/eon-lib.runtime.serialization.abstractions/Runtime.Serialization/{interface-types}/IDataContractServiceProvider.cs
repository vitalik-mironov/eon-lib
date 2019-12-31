using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;

using Eon.ComponentModel.Dependencies;

namespace Eon.Runtime.Serialization {

	public interface IDataContractServiceProvider
		:IDisposableDependencySupport {

		XmlObjectSerializer CreateDefaultXmlDataContractSerializer(Type type);

		XmlWriterSettings CreateXmlWriterDefaultSettings(bool closeOutput = true, bool disableFormatting = true);

		IEnumerable<IDataContractNamespaceAssemblyMap> GetDataContractNamespaceAssemblyMaps();

	}

}