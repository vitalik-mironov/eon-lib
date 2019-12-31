using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Eon.Reflection;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.ComponentModel.Dependencies {

	[DataContract]
	public class TypeDependencyId
		:DependencyId, ITypeDependencyId {

		volatile TypeNameReference _type;

		[JsonConstructor]
		protected TypeDependencyId(SerializationContext context)
			: base(context) { }

		public TypeDependencyId()
			: base(isReadOnly: false) { }

		public TypeDependencyId(bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) { }

		public TypeDependencyId(
			ITypeDependencyId other,
			bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			//
			other.EnsureNotNull(nameof(other));
			//
			_type = other.Type;
		}

		[DataMember(Order = 0, IsRequired = true)]
		public TypeNameReference Type {
			get { return _type; }
			set {
				EnsureNotReadOnly();
				_type = value;
			}
		}

		public override bool IsTargetMatch(IDependencyTarget target) {
			target.EnsureNotNull(nameof(target));
			//
			var targetType = (target as ITypeDependencyTarget)?.TargetType?.Resolve();
			if (targetType == null)
				return false;
			else {
				var dependencyType = Type?.Resolve();
				return dependencyType != null && dependencyType.IsAssignableFrom(targetType);
			}
		}

		#region Equality impls.

		public override bool Equals(object other) {
			return Equals(other as TypeDependencyId);
		}

		public override int GetHashCode() {
			return _type?.GetHashCode() ?? 0;
		}

		public static bool Equals(TypeDependencyId a, TypeDependencyId b) {
			if (ReferenceEquals(a, b))
				return true;
			else if (ReferenceEquals(a, null))
				return false;
			else
				return a.Equals(b);
		}

		public virtual bool Equals(TypeDependencyId other) {
			if (ReferenceEquals(other, null))
				return false;
			else
				return TypeNameReference.Equals(_type, other._type);
		}

		public sealed override bool Equals(DependencyId other) {
			return base.Equals(other) || Equals(other as TypeDependencyId);
		}

		public static bool operator !=(TypeDependencyId a, TypeDependencyId b) {
			return !Equals(a, b);
		}

		public static bool operator ==(TypeDependencyId a, TypeDependencyId b) {
			return Equals(a, b);
		}

		#endregion

		protected sealed override void CreateReadOnlyCopy(out DependencyId readOnlyCopy) {
			TypeDependencyId locReadOnlyCopy;
			CreateReadOnlyCopy(out locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out TypeDependencyId readOnlyCopy)
			=> readOnlyCopy = new TypeDependencyId(this, isReadOnly: true);

		public new TypeDependencyId AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				TypeDependencyId readOnlyCopy;
				CreateReadOnlyCopy(out readOnlyCopy);
				return readOnlyCopy;
			}
		}

		ITypeDependencyId IAsReadOnlyMethod<ITypeDependencyId>.AsReadOnly()
			=> AsReadOnly();

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			base.OnValidate();
			//
			if (Type is null)
				throw new ValidationException($"Не указано значение свойства '{nameof(Type)}'.");
		}

	}

}