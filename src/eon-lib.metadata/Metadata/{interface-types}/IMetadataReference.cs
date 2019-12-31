using System;

namespace Eon.Metadata {

	public interface IMetadataReference {

		MetadataName TargetMetadataName { get; }

		Type TargetMetadataType { get; }

		IMetadataReferenceResolver GetResolver();

	}

}