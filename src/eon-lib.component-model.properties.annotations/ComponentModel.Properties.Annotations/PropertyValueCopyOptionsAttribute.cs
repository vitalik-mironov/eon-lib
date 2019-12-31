using System;

namespace Eon.ComponentModel.Properties.Annotations {

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class PropertyValueCopyOptionsAttribute
		:Attribute {

		public PropertyValueCopyOptionsAttribute() { }

		public bool IsNotCopyableFrom { get; set; }

		public bool IsNotCopyableTo { get; set; }

	}

}