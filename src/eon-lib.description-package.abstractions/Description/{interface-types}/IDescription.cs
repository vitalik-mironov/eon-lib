using Eon.Description.Tree;
using Eon.Metadata;
using Eon.Reflection;
using Eon.Threading;

namespace Eon.Description {

	/// <summary>
	/// Defines base description of configuration for XInstance-component.
	/// </summary>
	public interface IDescription
		:IMetadata, ISettings {

		/// <summary>
		/// Gets/sets XInstance-contract of this description.
		/// <para>This XInstance-contract have precedence over other defined XInstance-contracts.</para>
		/// <para>Specified reference must point to the concrete type implementing XInstance-component.</para>
		/// <para>Can be <see langword="null"/>.</para>
		/// </summary>
		TypeNameReference ContractType { get; set; }

		/// <summary>
		/// Gets/sets the initialization timeout of XInstance-component created from this description.
		/// <para>Can be <see langword="null"/>. If <see langword="null"/>, then XInstance-component defines own initialization timeout according to its implementation.</para>
		/// </summary>
		TimeoutDuration InitializationTimeout { get; set; }

		/// <summary>
		/// Indicates whether this description belongs to the description package (see <seealso cref="Package"/>, <seealso cref="IDescriptionTree.SetPackage(IDescriptionPackage)"/>).
		/// </summary>
		bool HasPackage { get; }

		/// <summary>
		/// Gets the description package to which this description belongs.
		/// <para>Throws <see cref="EonException"/> if this description not belongs to package (see <seealso cref="IDescriptionTree.SetPackage(IDescriptionPackage)"/>).</para>
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		IDescriptionPackage Package { get; }

		/// <summary>
		/// Gets the summary of basic properties of this description.
		/// Возвращает сводку основных сведений о данном описании.
		/// <para>Can return <see langword="null"/>.</para>
		/// <para>Dispose tolerant.</para>
		/// </summary>
		IDescriptionSummary GetSummary();

	}

}