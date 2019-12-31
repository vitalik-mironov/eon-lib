using System;
using System.Diagnostics;

namespace Eon.Text {

	[DebuggerDisplay("{ToString(),nq}")]
	public sealed class CIStringWrap
		:IEquatable<CIStringWrap> {

		readonly string _wrappedString;

		CIStringWrap(string wrappedString) {
			_wrappedString = wrappedString;
		}

		public static implicit operator CIStringWrap(string value)
			=> value is null ? null : new CIStringWrap(value);

		public static explicit operator string(CIStringWrap value)
			=> value?._wrappedString;

		public string WrappedString
			=> _wrappedString;

		public override bool Equals(object obj)
			=> Equals(this, obj as CIStringWrap);

		public override int GetHashCode()
			=> _wrappedString.GetHashCode();

		public override string ToString()
			=> _wrappedString;

		public bool Equals(CIStringWrap other)
			=> Equals(this, other);

		public static bool Equals(CIStringWrap a, CIStringWrap b) {
			if (ReferenceEquals(a, b))
				return true;
			else if (a is null || b is null)
				return false;
			else
				return string.Equals(a._wrappedString, b._wrappedString, StringComparison.OrdinalIgnoreCase);
		}

	}

}