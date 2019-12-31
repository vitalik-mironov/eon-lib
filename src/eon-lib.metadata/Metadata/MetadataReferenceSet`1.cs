using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Eon.Linq;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Metadata {

	/// <summary>
	/// Представляет набор ссылок на метаданные типа <typeparamref name="TMetadata"/>.
	/// <para>Каждый элемент набора имеет тип, совместимый с <see cref="MetadataReference{TMetadata}"/>.</para>
	/// <para>Набор не может содержать <see langword="null"/>-элементы. При десериализации null-элементы игнорируются и в набор не добавляются.</para>
	/// </summary>
	/// <typeparam name="TMetadata">Тип метаданных, которые представляет каждая ссылка в наборе.</typeparam>
	[DataContract(Name = "MetadataRefSet")]
	public class MetadataReferenceSet<TMetadata>
		:MetadataReferenceSetBase
		where TMetadata : class, IMetadata {

		MetadataReference<TMetadata>[ ] _refs;
		[DataMember(Order = 0, Name = "Refs", IsRequired = false, EmitDefaultValue = false)]
		IEnumerable<MetadataReference<TMetadata>> P_Refs_DataMember {
			get {
				var refs = ReadDA(location: ref _refs);
				return refs.IsNullOrEmpty() ? null : refs;
			}
			set => WriteDA(ref _refs, value?.SkipNull().ToArray());
		}

		public MetadataReferenceSet()
			: base() { }

		[JsonConstructor]
		protected MetadataReferenceSet(SerializationContext ctx)
			: base(ctx: ctx) { }

		public int Count
			=> ReadDA(ref _refs)?.Length ?? 0;

		public sealed override IEnumerable<IMetadataReference> Refs
			=> EnumerateDA(ref _refs);

		internal IEnumerable<TMetadata> Resolve() {
			var ownerMetadata = OwnerMetadata;
			return EnumerateDA(location: ref _refs).Select(selector: locItem => locItem.Resolve(@base: ownerMetadata));
		}

		internal sealed override void EnsureReferencesReachable() {
			var ownerMetadata = OwnerMetadata;
			foreach (var @ref in EnumerateDA(location: ref _refs))
				@ref.Resolve(ownerMetadata);
		}

		#region Copy impls.

		protected sealed override void PopulateCopy(object args, CopyableDisposableBase copy) {
			var locCopy =
				copy
				.EnsureNotNull(nameof(copy))
				.EnsureOfType<CopyableDisposableBase, MetadataReferenceSet<TMetadata>>()
				.Value;
			//
			locCopy._refs = null;
			PopulateCopy(args, locCopy);
			base.PopulateCopy(args, copy);
		}

		protected virtual void PopulateCopy(object args, MetadataReferenceSet<TMetadata> copy) {
			copy.EnsureNotNull(nameof(copy));
			//
			copy.WriteDA(ref copy._refs, EnumerateDA(ref _refs).ToArray().NullIfEmpty());
		}

		public new MetadataReferenceSet<TMetadata> CreateCopy(object args)
			=> (MetadataReferenceSet<TMetadata>)base.CreateCopy(args);

		#endregion

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_refs.ClearArray();
			_refs = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}