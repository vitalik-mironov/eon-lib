using System;

namespace Eon.Metadata {

	public interface IMetadataReferenceResolver
		:IDisposable {

		bool Resolve(IMetadata @base, IMetadataReference reference, out IMetadata metadata);

	}

}