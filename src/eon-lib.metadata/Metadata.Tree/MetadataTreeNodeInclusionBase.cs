using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Eon.Collections.Trees;
using Eon.Context;
using Eon.Runtime.Serialization;

namespace Eon.Metadata.Tree {

	[DataContract(Name = DataContractName)]
	public abstract class MetadataTreeNodeInclusionBase
		:MetadataTreeStructureComponentBase, IMetadataTreeNodeInclusion {

		public new const string DataContractName = "MetadataTreeNodeInclusion";

		Uri _locationUri;
		[DataMember(Order = 0, Name = nameof(LocationUri), IsRequired = true)]
		string P_LocationUri_DataMember {
			get { return ReadDA(ref _locationUri)?.ToString(); }
			set {
				try {
					value.Arg(nameof(value)).EnsureNotNullOrWhiteSpace();
					//
					var valueAsUri = new Uri(uriString: value, uriKind: UriKind.RelativeOrAbsolute);
					OnValidateLocationUri(locationUri: valueAsUri);
					WriteDA(ref _locationUri, valueAsUri);
				} catch (Exception exception) {
					throw new MemberDeserializationException(GetType(), nameof(LocationUri), exception);
				}
			}
		}

		protected MetadataTreeNodeInclusionBase() { }

		protected MetadataTreeNodeInclusionBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public Uri LocationUri {
			get { return ReadDA(ref _locationUri); }
			set {
				value.EnsureNotNull(nameof(value));
				//
				Uri currentValue;
				if ((currentValue = ReadDA(ref _locationUri)) != value) {
					EnsureNotReadOnly();
					//
					OnValidateLocationUri(value);
					WriteDA(ref _locationUri, value);
				}
			}
		}

		protected virtual void OnValidateLocationUri(Uri locationUri) {
			locationUri.EnsureNotNull(nameof(locationUri));
			//
		}

		public abstract Task<IEnumerable<IMetadataTreeNode>> LoadNodesAsync(IMetadataLoadContext loadCtx);

		public new MetadataTreeNodeInclusionBase CreateCopy(ReadOnlyStateTag readOnlyState = null)
			=> (MetadataTreeNodeInclusionBase)base.CreateCopy(readOnlyState: readOnlyState);

		IMetadataTreeNodeInclusion IMetadataTreeNodeInclusion.CreateCopy(ReadOnlyStateTag readOnlyState)
			=> CreateCopy(readOnlyState: readOnlyState);

		protected sealed override void OnSettingParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> link) {
			var locLink =
				link
				.Arg(nameof(link))
				.EnsureOfType<ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent>, ILink<IMetadataTreeStructureComponent, IMetadataTreeNodeInclusion>>()
				.Value;
			//
			OnSettingParent(locLink);
		}

		protected virtual void OnSettingParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeNodeInclusion> link) {
			link.EnsureNotNull(nameof(link));
			//
		}

		protected sealed override void OnRemovingParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> link) {
			var locLink =
				link
				.Arg(nameof(link))
				.EnsureOfType<ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent>, ILink<IMetadataTreeStructureComponent, IMetadataTreeNodeInclusion>>()
				.Value;
			//
			OnRemovingParent(locLink);
		}

		protected virtual void OnRemovingParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeNodeInclusion> link) {
			link.EnsureNotNull(nameof(link));
			//
		}

		protected sealed override void PopulateCopy(CopyArgs args, MetadataTreeStructureComponentBase copy) {
			var locCopy =
				copy
				.EnsureNotNull(nameof(copy))
				.EnsureOfType<MetadataTreeStructureComponentBase, MetadataTreeNodeInclusionBase>()
				.Value;
			//
			PopulateCopy(args, locCopy);
		}

		protected abstract void PopulateCopy(CopyArgs args, MetadataTreeNodeInclusionBase copy);

		protected override void Dispose(bool explicitDispose) {
			_locationUri = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}