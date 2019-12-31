using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

using Eon.Collections.Trees;
using Eon.ComponentModel.Dependencies;
using Eon.Linq;
using Eon.Metadata.ComponentModel.Dependencies;
using Eon.Metadata.Internal;
using Eon.Metadata.Tree;
using Eon.Runtime.Serialization;
using Eon.Threading;

namespace Eon.Metadata {

#pragma warning disable IDE0034 // Simplify 'default' expression

	[DataContract(Name = DataContractName)]
	public abstract partial class MetadataBase
		:CopyableReadOnlyScopeBase, IMetadata {

		public const string DataContractName = "Metadata";

		#region Nested types

		protected new class CopyArgs
			:CopyableReadOnlyScopeBase.CopyArgs {

			public readonly IMetadataTreeElement Element;

			public CopyArgs(IMetadataTreeElement element, ReadOnlyStateTag readOnlyState)
				: base(readOnlyState: readOnlyState) {
				Element = element;
			}

		}

		sealed class P_MetadataDependencySupport
			:DependencySupport {

			MetadataBase _metadata;

			DefaultOuterDependencyScopeGetter _outerScopeProvider;

			internal P_MetadataDependencySupport(MetadataBase metadata)
				: base(outerDependencies: null) {
				metadata.EnsureNotNull(nameof(metadata));
				//
				_metadata = metadata;
				_outerScopeProvider = new DefaultOuterDependencyScopeGetter(outerScopeGetter: metadata.P_GetOuterDependencyScope);
			}

			protected override void GetOuterDependencyScopeGetter(out IOuterDependencyScopeGetter getter)
				=> getter = ReadDA(ref _outerScopeProvider);

			protected override void BuildDependencyExporter(IOuterDependencyScopeGetter outerScope, out IVh<IDependencyExporter> exporter)
				=> ReadDA(ref _metadata).BuildDependencyExporter(outerScope: outerScope, out exporter);

			protected override void BuildDependencyScope(IOuterDependencyScopeGetter outerScopeGetter, IDependencyExporter exporter, bool ownsExporter, out IDependencyScope scope)
				=> scope = new DependencyScope(outerScopeGetter: outerScopeGetter, exporter: exporter, ownsExporter: ownsExporter, owner: ReadDA(ref _metadata));

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose)
					_outerScopeProvider?.Dispose();
				_outerScopeProvider = null;
				_metadata = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		sealed class P_Name {

			public readonly MetadataName Name;

			public readonly bool IsAutoName;

			internal P_Name(MetadataName name, bool isAutoName) {
				name.EnsureNotNull(nameof(name));
				//
				Name = name;
				IsAutoName = isAutoName;
			}

		}

		#endregion

		#region Static & constant members

		static P_MetadataDependencySupport P_CreateDependencySupport(MetadataBase metadata)
			=> new P_MetadataDependencySupport(metadata);

		#endregion

		List<MetadataReferenceSetOwnershipRegistration> _referenceSetRegistry;

		PrimitiveSpinLock _referenceSetRegistrySpinLock;

		/*
		 * 0x1 - metadata is validated.
		 */
		int _helpFlags = 0;

		P_MetadataDependencySupport _dependencySupport;

		ILink<IMetadataTreeElement, IVh<IMetadata>> _treeElementLink;

		P_Name _name;
		[DataMember(Order = 0, Name = nameof(Name), IsRequired = false, EmitDefaultValue = false)]
		string P_Name_DataMember {
			get { return ReadDA(ref _name)?.Name; }
			set {
				try {
					WriteDA(
						location: ref _name,
						value: ReferenceEquals(value, null) ? null : new P_Name(name: (MetadataName)value, isAutoName: false));
				}
				catch (Exception exception) {
					throw new MemberDeserializationException(GetType(), nameof(Name), exception);
				}
			}
		}

		[DataMember(Order = 1, Name = nameof(Guid), IsRequired = false, EmitDefaultValue = false)]
		Guid _guid;

		protected MetadataBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		protected MetadataBase(MetadataName name) {
			_name = name is null ? null : new P_Name(name: name, isAutoName: false);
			//
			P_Initialize(resetGuid: true);
		}

		// TODO: Put strings into the resources.
		//
		public MetadataName Name {
			get { return ReadDA(ref _name)?.Name; }
			set {
				P_Name newName = null;
				for (; ; ) {
					EnsureNotReadOnly();
					newName = newName ?? (ReferenceEquals(value, null) ? new P_Name(name: MetadataName.NewName(), isAutoName: true) : new P_Name(name: value, isAutoName: false));
					var currentName = ReadDA(ref _name);
					if (currentName?.Name == newName.Name)
						break;
					else
						try {
							if (ReadDA(ref _treeElementLink) == null) {
								if (ReferenceEquals(currentName, Interlocked.CompareExchange(ref _name, newName, comparand: currentName))) {
									if (ReadDA(ref _treeElementLink) == null)
										break;
									else
										throw
											new EonException(message: $"Недопустимо изменять имя метаданных, если метаданные входят в дерево метаданных (см. св-во '{nameof(TreeElement)}').{Environment.NewLine}\tМетаданные:{this.FmtStr().GNLI2()}");
								}
								else
									continue;
							}
							else
								throw
									new EonException(message: $"Недопустимо изменять имя метаданных, если метаданные входят в дерево метаданных (см. св-во '{nameof(TreeElement)}').{Environment.NewLine}\tМетаданные:{this.FmtStr().GNLI2()}");
						}
						catch {
							Interlocked.CompareExchange(location1: ref _name, value: Disposing || IsDisposed ? null : currentName, comparand: newName);
							throw;
						}
				}
			}
		}

		public bool IsAutoName
			=> ReadDA(ref _name)?.IsAutoName ?? false;

		public Guid Guid
			=> ReadDA(ref _guid);

		public MetadataPathName FullName {
			get {
				var name = Name;
				var parent = Parent;
				if (parent is null)
					return name;
				else
					return parent.FullName + name;
			}
		}

		protected override IReadOnlyScope OuterReadOnlyScope
			=> TreeElement;

		public bool IsValidated
			=> (ReadDA(ref _helpFlags) & 0x1) == 0x1;

		void P_Initialize(bool resetGuid) {
			// Гуид.
			//
			if (resetGuid)
				_guid = Guid.NewGuid();
			// Имя.
			//
			if (ReadDA(ref _name) is null)
				UpdDAIfNullBool(location: ref _name, value: new P_Name(name: MetadataName.NewName(), isAutoName: true));
			// Наборы ссылок.
			//
			WriteDA(location: ref _referenceSetRegistrySpinLock, value: new PrimitiveSpinLock());
			WriteDA(location: ref _referenceSetRegistry, value: new List<MetadataReferenceSetOwnershipRegistration>());
			// Поддержка ф.зависимостей.
			//
			WriteDA(location: ref _dependencySupport, value: P_CreateDependencySupport(metadata: this));
			//
			WriteDA(location: ref _treeElementLink, value: null);
			_helpFlags = 0;
		}

		IList<MetadataReferenceSetOwnershipRegistration> P_GetCopyOfReferenceSetRegistry()
			=>
			ReadDA(location: ref _referenceSetRegistrySpinLock, considerDisposeRequest: true)
			.Invoke(func: () => EnumerateDA(value: ReadDA(location: ref _referenceSetRegistry, considerDisposeRequest: true), considerDisposeRequest: true).ToList());

		public void Validate() {
			if (!IsValidated)
				try {
					OnValidate();
					foreach (var child in Children.EmptyIfNull().SkipNull())
						child.Validate();
					InterlockedUtilities.Or(ref _helpFlags, 1);
				}
				catch (Exception exception) when (!(exception is MetadataValidationException)) {
					throw new MetadataValidationException(metadata: this, innerException: exception);
				}
		}

		public virtual void EnsureValidated() {
			EnsureNotDisposeState();
			if (!IsValidated)
				throw new MetadataNotValidatedYetException(metadata: this);
		}

		// TODO: Put exception messages into the resources.
		//
		protected virtual void OnValidate() {
			if (Name is null)
				throw new MetadataValidationException(message: "Metadata doesn't have a name.", metadata: this);
			foreach (var item in P_GetCopyOfReferenceSetRegistry())
				item?.Set?.EnsureReferencesReachable();
		}

		#region Root, Parent, Children, Siblings, Descendants, Ancestors

		public IMetadata Root
			=> TreeElement?.Node?.Root?.MetadataElement?.Metadata;

		public virtual IMetadata Parent
			=> TreeElement?.Node?.Parent?.MetadataElement?.Metadata;

		public virtual IEnumerable<IMetadata> Children
			=> (TreeElement?.Node?.Children.Select(i => i.MetadataElement?.Metadata).SkipNull()).EmptyIfNull();

		public virtual IEnumerable<IMetadata> Ancestors {
			get {
				var parent = Parent;
				for (; parent != null;) {
					yield return parent;
					parent = parent.Parent;
				}
			}
		}

		public IEnumerable<IMetadata> SelfAndAncestors
			=> Enumerable.Concat(first: this.Sequence(), second: Ancestors.EmptyIfNull());

		public virtual IEnumerable<IMetadata> Siblings {
			get {
				foreach (var childOfParent in (Parent?.Children?.SkipNull()).EmptyIfNull())
					if (ReferenceEquals(childOfParent, this))
						continue;
					else
						yield return childOfParent;
			}
		}

		public IEnumerable<IMetadata> SelfAndSiblings
			=> Enumerable.Concat(first: this.Sequence(), second: Siblings.EmptyIfNull());

		public virtual IEnumerable<IMetadata> Descendants
			=> ((IMetadata)this).TreeNode().Descendants(childrenSelector: locItem => locItem?.Children);

		public IEnumerable<IMetadata> SelfAndDescendants
			=> Enumerable.Concat(first: this.Sequence(), second: Descendants.EmptyIfNull());

		public IEnumerable<T> ChildrentOfType<T>()
			where T : IMetadata
			=> Children.EmptyIfNull().OfType<T>();

		#endregion

		IDependencyScope P_GetOuterDependencyScope()
			=> Parent?.GetDependencyScope();

		protected virtual void BuildDependencyExporter(IOuterDependencyScopeGetter outerScope, out IVh<IDependencyExporter> exporter) {
			outerScope.EnsureNotNull(nameof(outerScope));
			//
			exporter = outerScope.GetOuterScope().ResolveDependency<IMetadataDependencyExporterBuilder>(ensureResolution: true).BuildFor(metadata: this);
		}

		public virtual IEnumerable<IVh<IDependencyHandler2>> LocalDependencies() { yield break; }

		public IDependencyScope GetDependencyScope()
			=> ReadDA(ref _dependencySupport).GetDependencyScope();

		protected sealed override void PopulateCopy(CopyableReadOnlyScopeBase.CopyArgs args, CopyableReadOnlyScopeBase copy) {
			var locArgs = args.Arg(nameof(args)).EnsureOfType<CopyableReadOnlyScopeBase.CopyArgs, CopyArgs>().Value;
			var locCopy = copy.EnsureNotNull(nameof(copy)).EnsureOfType<CopyableReadOnlyScopeBase, MetadataBase>().Value;
			//
			locCopy.P_Initialize(resetGuid: true);
			//
			PopulateCopy(args: args, copy: locCopy);
			//
			base.PopulateCopy(copyArgs: args, copy: copy);
		}

		protected virtual void PopulateCopy(CopyArgs args, MetadataBase copy) {
			copy.EnsureNotNull(nameof(copy));
			//
			if (args.Element != null) {
				var link = new Link<IMetadataTreeElement, IVh<IMetadata>>(source: args.Element, target: copy.ToValueHolder(ownsValue: true));
				copy.WriteDA(location: ref copy._treeElementLink, value: link);
				args.Element.SetMetadata(link: link);
			}
			//
			foreach (var referenceSet in EnumerateDA(value: P_GetCopyOfReferenceSetRegistry().Select(locItem => locItem.Set), considerDisposeRequest: true))
				copy.RegisterReferenceSet(referenceSet: referenceSet.CreateCopy(args: null), useDeserializationContext: true);
		}

		public MetadataBase CreateCopy(IMetadataTreeElement element, ReadOnlyStateTag readOnlyState = default)
			=> (MetadataBase)CreateCopy(copyArgs: new CopyArgs(element: element, readOnlyState: readOnlyState));

		IMetadata IMetadata.CreateCopy(IMetadataTreeElement element, ReadOnlyStateTag readOnlyState)
			=> CreateCopy(element: element, readOnlyState: readOnlyState);

		public new MetadataBase CreateCopy(ReadOnlyStateTag readOnlyState = default)
			=> CreateCopy(element: null, readOnlyState: readOnlyState);

		IMetadata IMetadata.CreateCopy(ReadOnlyStateTag readOnlyState)
			=> CreateCopy(element: null, readOnlyState: readOnlyState);

		// TODO_HIGH: Удалить. Реализовать автоматическую регистрацию (на основе Linq.Expression(s) и рефлексии).
		//
		protected void RegisterReferenceSet(MetadataReferenceSetBase referenceSet, bool useDeserializationContext) {
			referenceSet.EnsureNotNull(nameof(referenceSet));
			//
			var registration = new MetadataReferenceSetOwnershipRegistration(owner: this, set: referenceSet);
			ReadDA(location: ref _referenceSetRegistrySpinLock, considerDisposeRequest: true)
				.Invoke(
					action:
						() => {
							var locRegistry = ReadDA(location: ref _referenceSetRegistry, considerDisposeRequest: true);
							locRegistry.Add(item: registration);
						});
			registration.SetRegistrationCompleteFlag();
		}

		public IMetadataTreeElement TreeElement
			=> TreeElementLink?.Source;

		public ILink<IMetadataTreeElement, IVh<IMetadata>> TreeElementLink
			=> ReadDA(ref _treeElementLink);

		// TODO: Put strings into the resources.
		//
		public void SetTreeElement(ILink<IMetadataTreeElement, IVh<IMetadata>> link) {
			link.EnsureNotNull(nameof(link));
			//
			var existingLink = ReadDA(ref _treeElementLink);
			if (!ReferenceEquals(link, existingLink)) {
				// Проверка аргумента.
				//
				var linkElement = link.Source;
				var linkMetadata = link.Target?.Value;
				if (linkElement == null)
					throw new ArgumentException("Для указанной связи не установлен элемент дерева метаданных.", nameof(link));
				else if (linkMetadata == null)
					throw new ArgumentException("Для указанной связи не установлен объект метаданных.", nameof(link));
				else if (!ReferenceEquals(linkMetadata, this))
					throw new ArgumentException(message: $"Объект метаданных, установленный в указанной связи, не соответствует данному объекту метаданных.{Environment.NewLine}\tОбъект метаданных связи ({nameof(link)}.{nameof(link.Target)}):{linkMetadata.FmtStr().GNLI2()}{Environment.NewLine}\tДанный объект метаданных:{this.FmtStr().GNLI2()}", paramName: nameof(link));
				// Установка связи с элементом дерева метаданных.
				//
				EnsureNotReadOnly();
				if ((existingLink = WriteDA(ref _treeElementLink, link, null)) == null) {
					try {
						linkElement.SetMetadata(link);
					}
					catch {
						Interlocked.CompareExchange(ref _treeElementLink, existingLink, link);
						throw;
					}
				}
				else if (!ReferenceEquals(existingLink, link))
					throw new EonException(message: $"Изменение существующей связи объекта метаданных с элементом дерева метаданных не допускается. Прежде существующая связь должна быть явно удалена методом '{nameof(RemoveTreeElement)}'.");
			}
		}

		public void RemoveTreeElement() {
			var existingLink = ReadDA(ref _treeElementLink);
			if (existingLink != null) {
				this.EnsureNotReadOnly();
				//
				var compareExchangeResult = default(ILink<IMetadataTreeElement, IVh<IMetadata>>);
				try {
					compareExchangeResult = Interlocked.CompareExchange(ref _treeElementLink, null, existingLink);
					if (ReferenceEquals(compareExchangeResult, existingLink))
						existingLink.Source.RemoveMetadata();
				}
				catch {
					if (ReferenceEquals(existingLink, compareExchangeResult))
						Interlocked.CompareExchange(ref _treeElementLink, compareExchangeResult, null);
					throw;
				}
			}
		}

		protected override void OnDeserialized(StreamingContext context) {
			P_Initialize(resetGuid: ReadDA(ref _guid) == Guid.Empty);
			//
			base.OnDeserialized(context);
		}

		// TODO: Put strings into the resources.
		//
		public override string ToString() {
			string fullNameShortView;
			try {
				fullNameShortView = FullName.FmtStr().Short();
			}
			catch (ObjectDisposedException) {
				fullNameShortView = null;
			}
			//
			return $"Full name:{fullNameShortView.FmtStr().GNLI(formatProvider: CultureInfo.InvariantCulture)}";
		}

	}

#pragma warning restore IDE0034 // Simplify 'default' expression

}