using System;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;

using Eon.Collections.Trees;
using Eon.Context;
using Eon.Runtime.Serialization;
using Eon.Threading.Tasks;

using static Eon.DisposableUtilities;

namespace Eon.Metadata.Tree {

	[DataContract]
	public class EmbeddedMetadataTreeElement
		:MetadataTreeElementBase, IEmbeddedMetadataTreeElement {

		IVh<IMetadata> _embeddedMetadata;
		[DataMember(Order = 0, Name = nameof(EmbeddedMetadata), IsRequired = true)]
		IMetadata P_EmbeddedMetadata_DataMember {
			get { return ReadDA(ref _embeddedMetadata).Value; }
			set {
				try {
					value.EnsureNotNull(nameof(value));
					//
					WriteDA(ref _embeddedMetadata, value.ToValueHolder(true));
				}
				catch (Exception firstException) {
					throw new MemberDeserializationException(GetType(), nameof(EmbeddedMetadata), firstException);
				}
			}
		}

		public EmbeddedMetadataTreeElement(IMetadataTreeNode node, IMetadata embeddedMetadata, bool ownsEmbeddedMetadata, bool linkEmbeddedMetadata = false, ReadOnlyStateTag readOnlyState = null)
			: base(node: node, readOnlyState: readOnlyState) {
			//
			embeddedMetadata.EnsureNotNull(nameof(embeddedMetadata));
			//
			_embeddedMetadata = embeddedMetadata.ToValueHolder(ownsEmbeddedMetadata);
			//
			if (linkEmbeddedMetadata)
				SetMetadata(new Link<IMetadataTreeElement, IVh<IMetadata>>(this, embeddedMetadata.ToValueHolder()));
		}

		public EmbeddedMetadataTreeElement(IMetadata embeddedMetadata, bool ownsEmbeddedMetadata, bool linkEmbeddedMetadata = default, ReadOnlyStateTag readOnlyState = default)
			: this(node: null, embeddedMetadata: embeddedMetadata, ownsEmbeddedMetadata: ownsEmbeddedMetadata, linkEmbeddedMetadata: linkEmbeddedMetadata, readOnlyState: readOnlyState) { }

		[Newtonsoft.Json.JsonConstructor]
		protected EmbeddedMetadataTreeElement(SerializationContext ctx)
			: base(ctx) { }

		public IMetadata EmbeddedMetadata
			=> ReadDA(ref _embeddedMetadata)?.Value;

		/// <summary>
		/// Вызывается при копировании объекта.
		/// <para>Выполняет заполнение копии объекта <paramref name="copy"/>.</para>
		/// <para>Связь данного элемента с объектом метаданных (<seealso cref="MetadataTreeElementBase.MetadataLink"/>) в объект <paramref name="copy"/> не копируется.</para>
		/// </summary>
		/// <param name="args">Параметры копирования.</param>
		/// <param name="copy">Копия объекта.</param>
		protected sealed override void PopulateCopy(CopyArgs args, MetadataTreeElementBase copy) {
			var locCopy = copy.EnsureNotNull(nameof(copy)).EnsureOfType<MetadataTreeElementBase, EmbeddedMetadataTreeElement>().Value;
			//
			locCopy._embeddedMetadata = null;
			PopulateCopy(args, locCopy);
			base.PopulateCopy(args, copy);
		}

		/// <summary>
		/// Вызывается при копировании объекта.
		/// <para>Выполняет заполнение копии объекта <paramref name="copy"/>.</para>
		/// </summary>
		/// <param name="args">Параметры копирования.</param>
		/// <param name="copy">Копия объекта.</param>
		protected virtual void PopulateCopy(CopyArgs args, EmbeddedMetadataTreeElement copy) {
			copy.EnsureNotNull(nameof(copy));
			//
			var originalEmbeddedMetadata = EmbeddedMetadata;
			var copyEmbeddedMetadata = default(IMetadata);
			try {
				copy.WriteDA(
					ref _embeddedMetadata,
					originalEmbeddedMetadata == null ? null : (copyEmbeddedMetadata = originalEmbeddedMetadata.CreateCopy(readOnlyState: null)).ToValueHolder(true));
			}
			catch (Exception firstException) {
				DisposeMany(firstException, copyEmbeddedMetadata);
				throw;
			}
		}

		protected sealed override async Task<IVh<IMetadata>> DoLoadMetadataAsync(IMetadataLoadContext loadCtx) {
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			await Task.CompletedTask;
			return EmbeddedMetadata.ToValueHolder(ownsValue: false);
		}

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState: newState);
			//
			base.OnSetReadOnly(previousState, newState);
			//
			IReadOnlyScope metadataAsReadOnlyScope = ReadDA(ref _embeddedMetadata, considerDisposeRequest: true)?.Value;
			metadataAsReadOnlyScope?.SetReadOnly(isReadOnly: newState.IsReadOnly, isPermanent: newState.IsPermanent);
		}

		[SecuritySafeCritical]
		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			ReadDA(ref _embeddedMetadata)?.Value.Fluent().NullCond(embeddedMetadata => SetMetadata(new Link<IMetadataTreeElement, IVh<IMetadata>>(this, embeddedMetadata.ToValueHolder())));
		}

		[SecuritySafeCritical]
		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_embeddedMetadata?.Dispose();
			_embeddedMetadata = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}