using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Reflection {

	[DataContract(Namespace = Xml.Schema.EonXmlNamespaces.Core)]
	public sealed class DotNetAssemblyName
		:AsReadOnlyValidatableBase {

		string _nameString;

		public DotNetAssemblyName(DotNetAssemblyName other, bool isReadOnly = false)
			: this(
				 name: other.EnsureNotNull(nameof(other)).Value.Name,
				 isReadOnly: isReadOnly) { }

		public DotNetAssemblyName(string name, bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			_nameString = name;
		}

		[JsonConstructor]
		DotNetAssemblyName(SerializationContext context)
			: base(ctx: context) { }

		[DataMember(Order = 0, IsRequired = true)]
		public string Name {
			get { return _nameString; }
			set {
				EnsureNotReadOnly();
				_nameString = value;
			}
		}

		// TODO: Put strings into the resources.
		//
		public AssemblyName GetAssemblyName() {
			var name = Name;
			try {
				return new AssemblyName(name);
			}
			catch (Exception exception) {
				throw new EonException(message: $"Преобразование значения в объект типа '{typeof(AssemblyName)}' вызвало исключение.{Environment.NewLine}\tЗначение:{Environment.NewLine}{name.IndentLines2()}", innerException: exception);
			}
		}

		protected override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy)
			=> copy = new DotNetAssemblyName(this, isReadOnly: true);

		public new DotNetAssemblyName AsReadOnly() {
			if (IsReadOnly)
				return this;
			else
				return new DotNetAssemblyName(this, isReadOnly: true);
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			var name = Name;
			if (name == null)
				throw new ValidationException($"Не указано значение свойства '{nameof(Name)}'.");
			else if (string.IsNullOrWhiteSpace(name))
				throw new ValidationException($"Указано недопустимое значение свойства '{nameof(Name)}'. {FormatXResource(typeof(string), "CanNotEmptyOrWhiteSpace")}");
		}

	}

}