
namespace Eon.Metadata {

	public interface IMetadataReference<out TMetadata>
		:IMetadataReference
		where TMetadata : class, IMetadata { }

}