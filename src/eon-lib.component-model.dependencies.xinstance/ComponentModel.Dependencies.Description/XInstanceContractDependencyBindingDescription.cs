using System;
using System.Linq;
using System.Runtime.Serialization;

using Eon.Description;
using Eon.Metadata;
using Eon.Runtime.Serialization;
using Newtonsoft.Json;
using static Eon.XInstanceContractUtilities;

namespace Eon.ComponentModel.Dependencies.Description {

	[DataContract]
	public class XInstanceContractDependencyBindingDescription
		:DependencyBindingDescriptionBase, IXInstanceContractDependencyBindingDescription {

		[DataMember(Order = 0, Name = nameof(DependencyTargetDescription), IsRequired = true)]
		MetadataReference<IDescription> _dependencyTargetDescriptionRef;

		[DataMember(Order = 1, Name = nameof(IsDependencyTargetSharingDisabled), IsRequired = true)]
		bool _isDependencyTargetSharingDisabled;

		public XInstanceContractDependencyBindingDescription(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected XInstanceContractDependencyBindingDescription(SerializationContext context)
			: base(context) { }

		public IDescription DependencyTargetDescription
			=> ReadDA(ref _dependencyTargetDescriptionRef).Resolve(this);

		public bool IsDependencyTargetSharingDisabled
			=> ReadDA(ref _isDependencyTargetSharingDisabled);

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			base.OnValidate();
			//
			var dependencyTargetDescription = this.ArgProp(DependencyTargetDescription, nameof(DependencyTargetDescription)).EnsureNotNull().Value;
			//
			foreach (var dependencyId in DependencyIdSet) {
				if (!HasXInstanceContract(description: dependencyTargetDescription, contractCompatibleType: (dependencyId as ITypeDependencyId)?.Type.Resolve()))
					throw
						new MetadataValidationException(
							message: $"Указано недопустимое значение свойства '{nameof(DependencyTargetDescription)}'. Указанное свойством описание не имеет ни одного XInstance-контракта, соотвествующего одному из указанных идентификаторов привязки (байндинга) функиональной зависимости.{Environment.NewLine}\tОписание:{dependencyTargetDescription.FmtStr().GNLI2()}{Environment.NewLine}\tИдентификатор привязки (байндинга):{dependencyId.FmtStr().GNLI2()}",
							metadata: this);
			}
		}

		// TODO: Put strings into the resources.
		//
		public override IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeCtx = null) {
			this.EnsurePermanentReadOnly();
			EnsureValidated();
			//
			// TODO: В зависимости от DependencyId необходимо выбирать наиболее подходящего исполнителя на основе XInstance-контракта.
			//
			var dependencyScopeContextAsIXInstance = dependencyScopeCtx as IXInstance;
			try {
				if (dependencyScopeContextAsIXInstance == null)
					return
						GetOrderedXInstanceContractHandlers(description: DependencyTargetDescription)
						.First();
				else {
					var newInstanceFactoryArgs =
						ArgsTuple
						.Create(
							arg1: dependencyScopeContextAsIXInstance,
							arg2: DependencyTargetDescription);
					return
						RequireSingleContractHandler(
							description: DependencyTargetDescription,
							canShareDependency: !IsDependencyTargetSharingDisabled,
							newInstanceFactoryArgs: newInstanceFactoryArgs);
				}
			}
			catch (Exception firstException) {
				throw
					new EonException(
						message: $"Ошибка получения обработчика функциональной зависимости для указанной цели привязки (байндинга) зависимости.{Environment.NewLine}\tПривязка (байндинг):{this.FmtStr().GNLI2()}{Environment.NewLine}\tЦель привязки:{DependencyTargetDescription.FmtStr().GNLI2()}",
						innerException: firstException);
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_dependencyTargetDescriptionRef = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}