using System;
using System.Diagnostics;

namespace Eon.Text {

	[DebuggerDisplay("{ToString(),nq}")]
	public readonly struct CIStringWrapStruct
		:IEquatable<CIStringWrapStruct> {

		readonly string _value;

		CIStringWrapStruct(string wrappedString) {
			_value = wrappedString;
		}

		public static implicit operator CIStringWrapStruct(string value)
			=> value is null ? default : new CIStringWrapStruct(value);

		public static explicit operator string(CIStringWrapStruct value)
			=> value._value;

		public string Value
			=> _value;

		public override bool Equals(object obj)
			=> Equals(this, obj as CIStringWrap);

		public override int GetHashCode()
			=> _value.GetHashCode();

		public override string ToString()
			=> _value;

		public bool Equals(CIStringWrapStruct other)
			=> Equals(this, other);

		public static bool Equals(CIStringWrapStruct a, CIStringWrapStruct b) {
			if (ReferenceEquals(a, b))
				return true;
			else
				return string.Equals(a._value, b._value, StringComparison.OrdinalIgnoreCase);
		}

	}

}