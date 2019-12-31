using System.Runtime.Serialization;

using Eon.ComponentModel.Dependencies.Description;
using Eon.Metadata;
using Eon.Runtime.Serialization;

namespace Eon.Data.Description {

	[DataContract]
	public abstract class DataContextDescriptionBase
		:DependencySupportDescriptionBase, IDataContextDescription {

		[DataMember(Order = 0, Name = nameof(PreferNoTracking), IsRequired = true)]
		bool _preferNoTracking;

		protected DataContextDescriptionBase(MetadataName name)
			: base(name: name) {
			_preferNoTracking = default;
		}

		protected DataContextDescriptionBase(SerializationContext context)
			: base(ctx: context) { }

		public bool PreferNoTracking {
			get => ReadDA(ref _preferNoTracking);
			set {
				EnsureNotReadOnly();
				_preferNoTracking = value;
			}
		}

	}

}