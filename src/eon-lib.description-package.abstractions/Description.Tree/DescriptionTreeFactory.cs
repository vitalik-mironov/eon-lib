using Eon.Metadata;

namespace Eon.Description.Tree {

	public delegate IDescriptionTree DescriptionTreeFactory(IMetadataNamespace rootNamespace, bool ownsRootNamespace, ReadOnlyStateTag readOnlyState = default);

}