using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

using Eon.Collections.Trees;
using Eon.Runtime.Serialization;

namespace Eon.Metadata.Tree {
#pragma warning disable IDE0034 // Simplify 'default' expression

	[DataContract(Name = DataContractName)]
	public abstract class MetadataTreeStructureComponentBase
		:CopyableReadOnlyScopeBase, IMetadataTreeStructureComponent {

		public const string DataContractName = "MetadataTreeStructureComponent";

		ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> _parentLink;

		[DataMember(Order = 0, Name = nameof(Caption), IsRequired = false, EmitDefaultValue = false)]
		string _caption;

		protected MetadataTreeStructureComponentBase(string caption = default(string), ReadOnlyStateTag readOnlyState = null)
			: base(readOnlyState: readOnlyState) {
			_caption = caption;
			_parentLink = null;
		}

		protected MetadataTreeStructureComponentBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public string Caption {
			get { return ReadDA(ref _caption); }
			set {
				EnsureNotReadOnly();
				WriteDA(ref _caption, value);
			}
		}

		public IMetadataTreeStructureComponent Root {
			get {
				IMetadataTreeStructureComponent previousAscendant, nextAscendant;
				previousAscendant = nextAscendant = this;
				for (; nextAscendant != null;) {
					previousAscendant = nextAscendant;
					nextAscendant = nextAscendant.Parent;
				}
				return previousAscendant;
			}
		}

		public IMetadataTreeStructureComponent Parent
			=> ParentLink?.Source;

		public ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> ParentLink
			=> ReadDA(ref _parentLink);

		protected override IReadOnlyScope OuterReadOnlyScope
			=> Parent;

		// TODO: Put strings into the resources.
		//
		public void SetParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> link) {
			link.EnsureNotNull(nameof(link));
			//
			var existingLink = ReadDA(ref _parentLink);
			if (!ReferenceEquals(existingLink, link)) {
				if (existingLink == null) {
					// Проверка аргумента.
					//
					var linkParent = link.Source;
					var linkChild = link.Target;
					if (linkParent == null)
						throw new ArgumentException($"Для указанной связи не установлен компонент-родитель (свойство '{nameof(link.Source)}').", nameof(link));
					else if (linkChild == null)
						throw new ArgumentException($"Для указанной связи не установлен дочерний компонент (свойство '{nameof(link.Target)}').", nameof(link));
					else if (!ReferenceEquals(linkChild, this))
						throw new ArgumentException($"В указанной связи дочерний компонент (свойство '{nameof(link.Target)}') не соответствует данному компоненту.{Environment.NewLine}\tДочерний компонент связи:{Environment.NewLine}{linkChild.FmtStr().G().IndentLines2()}{Environment.NewLine}\tКомпонент:{Environment.NewLine}{this.FmtStr().G().IndentLines2()}", nameof(link));
					else if (linkParent.IsDescendantOf(this))
						throw new ArgumentException($"Указанный в связи компонент-родитель (свойство '{nameof(link.Source)}') уже является частью структуры данного компонента.{Environment.NewLine}\tКомпонент-родитель связи:{Environment.NewLine}{linkParent.FmtStr().G().IndentLines2()}{Environment.NewLine}\tКомпонент:{Environment.NewLine}{this.FmtStr().G().IndentLines2()}", nameof(link));
					// Проверка доступности редактирования.
					//
					EnsureNotReadOnly();
					//
					OnSettingParent(link);
					//
					try {
						if ((existingLink = WriteDA(location: ref _parentLink, value: link, comparand: null)) == null) {
							if (!linkParent.HasChildComponent(linkChild))
								throw
									new EonException($"Компонент не входит в число дочерних компонентов компонента-родителя.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{this.FmtStr().G().IndentLines2()}{Environment.NewLine}\tКомпонент-родитель:{Environment.NewLine}{linkParent.FmtStr().G().IndentLines2()}");
						}
						else if (!ReferenceEquals(existingLink, link))
							throw
								new EonException($"Изменение существующей связи с родительским компонентом не допускается. Прежде существующая связь должна быть явно удалена методом '{nameof(RemoveParent)}'.");
					} catch {
						Interlocked.CompareExchange(ref _parentLink, null, link);
						throw;
					}
				}
				else
					throw new EonException($"Изменение существующей связи с родительским компонентом не допускается. Прежде существующая связь должна быть явно удалена методом '{nameof(RemoveParent)}'.");
			}
		}

		protected abstract void OnSettingParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> link);

		public void RemoveParent() {
			var existingLink = ReadDA(ref _parentLink);
			if (existingLink != null) {
				EnsureNotReadOnly();
				//
				OnRemovingParent(existingLink);
				// Здесь необходимо учитывать, что existingLink.Source == this.Parent, а existingLink.Target == this.
				//
				if (existingLink.Source.HasChildComponent(existingLink.Target))
					throw
						new EonException($"Невозможно удалить связь данного компонента с компонентом-родителем, так как данный компонент входит в число дочерних компонентов компонента-родителя.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{existingLink.Target.FmtStr().G().IndentLines2()}{Environment.NewLine}\tКомпонент-родитель:{Environment.NewLine}{existingLink.Source.FmtStr().G().IndentLines2()}");
				else
					Interlocked.CompareExchange(ref _parentLink, null, existingLink);
			}
		}

		protected abstract void OnRemovingParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> link);

		public abstract bool HasChildComponent(IMetadataTreeStructureComponent component);

		public IEnumerable<IMetadataTreeStructureComponent> ChildComponents {
			get {
				IEnumerable<IMetadataTreeStructureComponent> components;
				GetChildComponents(out components);
				return EnumerateDA(components);
			}
		}

		protected abstract void GetChildComponents(out IEnumerable<IMetadataTreeStructureComponent> components);

		protected sealed override void PopulateCopy(CopyArgs args, CopyableReadOnlyScopeBase copy) {
			var locCopy =
				copy
				.EnsureNotNull(nameof(copy))
				.EnsureOfType<CopyableReadOnlyScopeBase, MetadataTreeStructureComponentBase>()
				.Value;
			//
			locCopy._parentLink = null;
			//
			PopulateCopy(args, locCopy);
			//
			base.PopulateCopy(args, copy);
		}

		protected abstract void PopulateCopy(CopyArgs args, MetadataTreeStructureComponentBase copy);

		public override string ToString()
			=>
			$"Тип:{GetType().FmtStr().GNLI()}"
			+ $"{Environment.NewLine}Наименование:{_caption.FmtStr().GNLI()}";

		protected override void Dispose(bool explicitDispose) {
			_parentLink = null;
			_caption = null;
			//
			base.Dispose(explicitDispose);
		}

	}

#pragma warning restore IDE0034 // Simplify 'default' expression
}