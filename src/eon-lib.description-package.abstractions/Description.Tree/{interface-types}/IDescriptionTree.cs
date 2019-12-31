using Eon.Metadata.Tree;

namespace Eon.Description.Tree {

	/// <summary>
	/// Defines description tree.
	/// </summary>
	public interface IDescriptionTree
		:IMetadataTree {

		/// <summary>
		/// Gets the root of this tree.
		/// </summary>
		new IDescriptionTree Root { get; }

		/// <summary>
		/// Links this tree with the description package.
		/// </summary>
		/// <param name="package">
		/// Description package.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		void SetPackage(IDescriptionPackage package);

		/// <summary>
		/// Gets the description package containing this tree.
		/// </summary>
		IDescriptionPackage Package { get; }

	}

}