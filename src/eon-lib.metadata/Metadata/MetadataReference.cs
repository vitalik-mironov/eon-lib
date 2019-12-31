using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

namespace Eon.Metadata {

	[DataContract(Name = DataContractName)]
	public abstract class MetadataReference
		:IMetadataReference {

		#region Static & constant members

		public const string DataContractName = "MetadataRefBase";

		#endregion

		protected MetadataReference() { }

		protected MetadataReference(SerializationContext ctx) { }

		public abstract MetadataName TargetMetadataName { get; }

		IMetadataReferenceResolver IMetadataReference.GetResolver() {
			return GetResolver();
		}

		protected abstract IMetadataReferenceResolver GetResolver();

		[OnDeserializing]
		void P_OnDeserializing(StreamingContext context)
			=> OnDeserializing(context);

		protected virtual void OnDeserializing(StreamingContext context) { }

		[OnDeserialized]
		void P_OnDeserialized(StreamingContext context)
			=> OnDeserialized(context);

		protected virtual void OnDeserialized(StreamingContext context) { }

		public abstract Type TargetMetadataType { get; }

	}

}