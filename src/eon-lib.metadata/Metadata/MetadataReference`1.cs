using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Metadata {

	/// <summary>
	/// Представляет базовый тип ссылки на метаданные типа <typeparamref name="TMetadata"/>.
	/// </summary>
	/// <typeparam name="TMetadata">Тип, метаданных, которые представляет ссылка.</typeparam>
	[DataContract(Name = "MetadataRef")]
	public abstract class MetadataReference<TMetadata>
		:MetadataReference, IMetadataReference<TMetadata>
		where TMetadata : class, IMetadata {

		static readonly Type __MetadataType = typeof(TMetadata);

		[JsonConstructor]
		protected MetadataReference(SerializationContext ctx)
			: base(ctx: ctx) { }

		protected MetadataReference() { }

		/// <summary>
		/// Возвращает ограничение типа метаданных, представляемых данной ссылкой.
		/// <para>Фактически, представляет тип <typeparamref name="TMetadata"/>.</para>
		/// </summary>
		public sealed override Type TargetMetadataType
			=> __MetadataType;

	}

}