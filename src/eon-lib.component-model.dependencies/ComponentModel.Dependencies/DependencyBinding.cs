using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eon.ComponentModel.Dependencies {

	[DataContract]
	public class DependencyBinding
		:AsReadOnlyValidatableBase, IDependencyBinding {

		DependencyId _dependencyId;

		DependencyTarget _target;

		public DependencyBinding()
			: this(dependencyId: null) { }

		public DependencyBinding(bool isReadOnly)
			: this(dependencyId: null, isReadOnly: isReadOnly) { }

		public DependencyBinding(DependencyId dependencyId = default, DependencyTarget target = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			//
			_dependencyId = isReadOnly ? dependencyId?.AsReadOnly() : dependencyId;
			_target = isReadOnly ? target?.AsReadOnly() : target;
		}

		public DependencyBinding(DependencyBinding other, bool isReadOnly = false)
			: this(
					dependencyId: other.EnsureNotNull(nameof(other)).Value.DependencyId,
					target: other.Target,
					isReadOnly: isReadOnly) { }

		[DataMember(Order = 0, IsRequired = true)]
		public DependencyId DependencyId {
			get { return _dependencyId; }
			set {
				EnsureNotReadOnly();
				_dependencyId = value;
			}
		}

		IDependencyId IDependencyBinding.DependencyId {
			get { return DependencyId; }
			set {
				DependencyId =
					value
					.Arg(nameof(value))
					.EnsureOfType<IDependencyId, DependencyId>()
					.Value;
			}
		}

		[DataMember(Order = 1, IsRequired = true)]
		public DependencyTarget Target {
			get { return _target; }
			set {
				EnsureNotReadOnly();
				_target = value;
			}
		}

		IDependencyTarget IDependencyBinding.Target {
			get { return _target; }
			set {
				Target =
					value
					.Arg(nameof(value))
					.EnsureOfType<IDependencyTarget, DependencyTarget>()
					.Value;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			var id = DependencyId;
			var target = Target;
			if (id is null)
				throw new ValidationException($"Не указано значение свойства '{nameof(DependencyId)}'.");
			else if (target is null)
				throw new ValidationException($"Не указано значение свойства '{nameof(Target)}'.");
			else if (!id.IsTargetMatch(target))
				throw new ValidationException($"Указанная цель '{target}' привязки функциональной зависимости '{id}' не соответствует указанной зависимости.");
			try {
				id.Validate();
			}
			catch (Exception firstException) {
				throw new ValidationException($"Ошибка валидации значения свойства '{nameof(DependencyId)}'.", firstException);
			}
			try {
				target.Validate();
			}
			catch (Exception exception) {
				throw new ValidationException(message: $"Ошибка валидации значения свойства '{nameof(Target)}'.", innerException: exception);
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			DependencyBinding locReadOnlyCopy;
			CreateReadOnlyCopy(out locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out DependencyBinding readOnlyCopy)
			=> readOnlyCopy = new DependencyBinding(other: this, isReadOnly: true);

		public new DependencyBinding AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				DependencyBinding readOnlyCopy;
				CreateReadOnlyCopy(out readOnlyCopy);
				return readOnlyCopy;
			}
		}

		IDependencyBinding IAsReadOnlyMethod<IDependencyBinding>.AsReadOnly()
			=> AsReadOnly();

		// TODO: Put strings into the resources.
		//
		public virtual IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeContext = null) {
			var target = Target;
			if (target is null)
				throw
					new InvalidOperationException(
						message: $"Для данного объекта привязки не определена цель (свойство '{nameof(Target)}').{Environment.NewLine}\tОбъект привязки:{this.FmtStr().GNLI2()}");
			else
				return target.GetDependencyHandler(dependencyScopeContext: dependencyScopeContext);
		}

	}

}