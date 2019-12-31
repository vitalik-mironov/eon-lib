using Eon.Description;

namespace Eon.Context {

	/// <summary>
	/// Description package load operation context.
	/// </summary>
	public interface IDescriptionPackageLoadContext
		:IMetadataLoadContext {

		/// <summary>
		/// Gets description package locator (reference) to be loaded.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		DescriptionPackageLocator Locator { get; }

	}

}