using System;
using System.Runtime.Serialization;

using Eon.ComponentModel.Dependencies;
using Eon.Description.ComponentModel.Dependencies;
using Eon.Description.Tree;
using Eon.Metadata;
using Eon.Reflection;
using Eon.Runtime.Serialization;
using Eon.Threading;

namespace Eon.Description {

	[DataContract(Name = DataContractName)]
	public abstract class DescriptionBase
		:MetadataBase, IDescription {

		#region Static & constant members.

		public new const string DataContractName = "Description";

		internal static readonly Type Internal_DescriptionType = typeof(DescriptionBase);

		#endregion

		[DataMember(Order = 0, Name = nameof(ContractType), IsRequired = false)]
		TypeNameReference _contractType;

		[DataMember(Order = 1, Name = nameof(IsDisabled), IsRequired = false)]
		bool _isDisabled;

		[DataMember(Order = 2, Name = nameof(InitializationTimeout), IsRequired = false)]
		TimeoutDuration _initializationTimeout;

		protected DescriptionBase(MetadataName name)
			: base(name: name) {
			_isDisabled = false;
		}

		protected DescriptionBase(SerializationContext ctx)
			: base(ctx: ctx) {
			_isDisabled = false;
		}

		public TypeNameReference ContractType {
			get => ReadDA(ref _contractType);
			set {
				EnsureNotReadOnly();
				WriteDA(ref _contractType, value);
			}
		}

		public bool IsDisabled {
			get => ReadDA(location: ref _isDisabled);
			set {
				EnsureNotReadOnly();
				_isDisabled = value;
			}
		}

		public TimeoutDuration InitializationTimeout {
			get => ReadDA(ref _initializationTimeout);
			set {
				EnsureNotReadOnly();
				WriteDA(location: ref _initializationTimeout, value: value);
			}
		}

		public bool HasPackage {
			get {
				GetTree(tree: out var tree);
				return !(tree?.Package is null);
			}
		}

		// TODO: Put strings into the resources.
		//
		public IDescriptionPackage Package {
			get {
				GetTree(tree: out var tree);
				var package = tree?.Package;
				if (package is null)
					throw new EonException(message: $"This description is not a member of description package.{Environment.NewLine}\tDescription:{this.FmtStr().GNLI2()}");
				else
					return package;
			}
		}

		protected virtual void GetTree(out IDescriptionTree tree)
			=> tree = TreeElement?.Node?.Root as IDescriptionTree;

		public virtual IDescriptionSummary GetSummary() {
			Guid guid;
			string fullName;
			string displayName;
			try {
				guid = Guid;
				fullName = FullName;
				displayName = FullName.FmtStr().Short();
			}
			catch (ObjectDisposedException) {
				return null;
			}
			//
			IDescriptionPackage package;
			DescriptionPackageIdentity packageIdentity;
			Uri packageSiteOrigin;
			try {
				package = Package;
				packageIdentity = package?.Identity;
				packageSiteOrigin = package?.HasSiteOrigin == true ? package.SiteOrigin : null;
			}
			catch (ObjectDisposedException) {
				packageIdentity = null;
				packageSiteOrigin = null;
			}
			//
			return new DescriptionSummary(guid: guid, fullName: fullName, displayName: displayName, packageIdentity: packageIdentity, packageSiteOrigin: packageSiteOrigin);
		}

		protected override void BuildDependencyExporter(IOuterDependencyScopeGetter outerScope, out IVh<IDependencyExporter> exporter)
			=> exporter = outerScope.EnsureNotNull(nameof(outerScope)).Value.GetOuterScope().ResolveDependency<IDescriptionDependencyExporterBuilder>(ensureResolution: true).BuildFor(description: this);

		protected override void OnValidate() {
			base.OnValidate();
			//
			this
				.ArgProp(value: ContractType?.Resolve(), name: nameof(ContractType))
				.EnsureValid(validator: XInstanceContractUtilities.EnsureXInstanceContractTypeValid);
		}

		protected sealed override void PopulateCopy(CopyArgs args, MetadataBase copy)
			=> PopulateCopy(args: args, copy: copy.Arg(nameof(copy)).EnsureOfType<MetadataBase, DescriptionBase>().Value);

		protected virtual void PopulateCopy(CopyArgs args, DescriptionBase copy)
			=> base.PopulateCopy(args: args, copy: copy);

		protected override void Dispose(bool explicitDispose) {
			_initializationTimeout = null;
			_contractType = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}