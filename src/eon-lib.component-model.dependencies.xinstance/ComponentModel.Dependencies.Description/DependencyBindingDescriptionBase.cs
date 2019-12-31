using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Description;
using Eon.Linq;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.ComponentModel.Dependencies.Description {

	[DataContract]
	public abstract class DependencyBindingDescriptionBase
		:DescriptionBase, IDependencyBindingDescriptionBase {

		ISet<DependencyId> _dependencyIdSet;
		[DataMember(Order = 0, Name = nameof(DependencyIdSet), IsRequired = true)]
		IEnumerable<DependencyId> P_DependencyIdSet_DataMember {
			get => _dependencyIdSet;
			set {
				if (value is null)
					_dependencyIdSet = null;
				else
					try {
						var set = new HashSet<DependencyId>();
						var itemIndex = -1;
						foreach (var item in value.Arg(nameof(value)).EnsureNoNullElements().Value) {
							itemIndex++;
							if (!set.Add(item))
								throw
									new ArgumentException(
										paramName: nameof(value),
										message: FormatXResource(typeof(Array), "CanNotContainNonUnique/NonUniqueValueAt", item, itemIndex.ToString("d")));
						}
						_dependencyIdSet = set;
					}
					catch (Exception firstException) {
						throw new MemberDeserializationException(GetType(), nameof(DependencyIdSet), firstException);
					}
			}
		}


		protected DependencyBindingDescriptionBase(MetadataName name)
			: base(name) { }

		protected DependencyBindingDescriptionBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public IEnumerable<IDependencyId> DependencyIdSet
			=> ReadDA(ref _dependencyIdSet);

		// TODO: Put strings into the resources.
		//
		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			base.OnSetReadOnly(previousState, newState);
			//
			if (newState.IsReadOnly && !newState.IsPermanent)
				throw
					new NotSupportedException(
						$"Изменение состояния доступности редактирования данного объекта согласно заданным параметрам не поддерживается.{Environment.NewLine}\tОбъект:{Environment.NewLine}{this.FmtStr().GI2()}{Environment.NewLine}\tПараметры:{Environment.NewLine}{newState.FmtStr().GI2()}");
			if (newState.IsReadOnly)
				WriteDA(
					ref _dependencyIdSet,
					ReadDA(ref _dependencyIdSet)?.AsReadOnly(transform: i => i?.AsReadOnly()));
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			this.ArgProp(DependencyIdSet, nameof(DependencyIdSet)).EnsureNotNull().EnsureNotEmpty();
		}

		public abstract IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeContext = null);

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}Идентификаторы функциональных зависимостей:{_dependencyIdSet.FmtStr().GCollectionNlI(formatProvider: null)}";

		protected override void Dispose(bool explicitDispose) {
			_dependencyIdSet = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}