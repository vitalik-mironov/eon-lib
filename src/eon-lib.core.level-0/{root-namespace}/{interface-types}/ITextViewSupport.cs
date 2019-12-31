using System;

namespace Eon {

	public interface ITextViewSupport {

		string ToShortTextView(IFormatProvider formatProvider = default);

		string ToLongTextView(IFormatProvider formatProvider = default);

	}

}