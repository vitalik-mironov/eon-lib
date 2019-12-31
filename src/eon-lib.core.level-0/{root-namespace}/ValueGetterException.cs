using System;

namespace Eon {

	public class ValueGetterException
		:InvalidOperationException {

		readonly object _item;

		// TODO: Put strings into the resources.
		//
		public ValueGetterException(Exception innerException)
			: this(item: null, innerException: innerException) { }

		public ValueGetterException(object item, Exception innerException)
			: base(message: $"Объект хранения значения не содержит значение, а содержит ошибку, которая была установлена как результат вычисления значения.{(item is null ? string.Empty : $"{Environment.NewLine}\tОбъект хранения значения:{Environment.NewLine}\t\t{item}")}", innerException: innerException) {
			_item = item;
		}

		public object Item
			=> _item;

	}

}