using System;

namespace Eon.ComponentModel.Properties.Annotations {

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public class PropertyValueCopyTypeOptionsAttribute
		:Attribute {

		public PropertyValueCopyTypeOptionsAttribute() { }

		public bool IsNotCopyableFrom { get; set; }

		public bool IsNotCopyableTo { get; set; }

	}

}