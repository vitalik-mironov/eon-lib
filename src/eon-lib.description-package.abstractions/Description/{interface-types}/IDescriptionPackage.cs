using System;
using System.Collections.Generic;

using Eon.ComponentModel;
using Eon.Description.Tree;
using Eon.Metadata;

namespace Eon.Description {

	public interface IDescriptionPackage
		:IDisposeNotifying, IReadOnlyScope, ISiteOriginSupport {

		DescriptionPackageIdentity Identity { get; }

		/// <summary>
		/// Возвращает дерево описаний данного пакета.
		/// <para>Ображение к данному свойству вызовет исключение <see cref="InvalidOperationException"/>, если дерево описаний отсутствует (см. <seealso cref="HasTree"/>).</para>
		/// </summary>
		IDescriptionTree Tree { get; }

		/// <summary>
		/// Возвращает признак, указывающий, имеет ли данный пакет дерево описаний (см. <seealso cref="Tree"/>).
		/// </summary>
		bool HasTree { get; }

		IMetadataNamespace RootNamespace { get; }

		IEnumerable<DotNetStrongAssemblyNameReference> ReferencedDotNetAssemblies { get; }

		TMetadata RequireMetadata<TMetadata>(MetadataPathName fullName)
			where TMetadata : class, IMetadata;

		TMetadata RequireMetadata<TMetadata>(MetadataPathNameRef<TMetadata> locator)
			where TMetadata : class, IMetadata;

		IEnumerable<IMetadata> AllMetadata { get; }

		IDescription DefaultDescription { get; }

	}

}