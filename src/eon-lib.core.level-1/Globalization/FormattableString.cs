using System;

namespace Eon.Globalization {

	public class FormattableString
		:IFormattable {

		static readonly object[ ] __EmptyArgsArray = new object[ ] { };

		readonly string _format;

		readonly object[ ] _args;

		public FormattableString() {
			_format = string.Empty;
			_args = __EmptyArgsArray;
		}

		public FormattableString(string format, object[ ] args) {
			_format = format.EnsureNotNull(nameof(format));
			_args = args ?? __EmptyArgsArray;
		}

		public FormattableString(string format, object arg1, params object[ ] args) {
			_format = format.EnsureNotNull(nameof(format));
			_args = ArrayUtilities.ConcatArray(arg1, args ?? __EmptyArgsArray);
		}

		public FormattableString(string format, object arg1, object arg2, params object[ ] args) {
			_format = format.EnsureNotNull(nameof(format));
			_args = ArrayUtilities.ConcatArray(new[ ] { arg1, arg2 }, args ?? __EmptyArgsArray);
		}

		public FormattableString(string format, object arg1, object arg2, object arg3, params object[ ] args) {
			_format = format.EnsureNotNull(nameof(format));
			_args = ArrayUtilities.ConcatArray(new[ ] { arg1, arg2, arg3 }, args ?? __EmptyArgsArray);
		}

		public FormattableString(string format, object arg1, object arg2, object arg3, object arg4, params object[ ] args) {
			_format = format.EnsureNotNull(nameof(format));
			_args = ArrayUtilities.ConcatArray(new[ ] { arg1, arg2, arg3, arg4 }, args ?? __EmptyArgsArray);
		}

		string IFormattable.ToString(string format, IFormatProvider formatProvider)
			=> ToString(formatProvider);

		public override string ToString()
			=> ToString(formatProvider: null);

		public virtual string ToString(IFormatProvider formatProvider)
			=> string.Format(provider: formatProvider, format: _format, args: _args);

	}

}