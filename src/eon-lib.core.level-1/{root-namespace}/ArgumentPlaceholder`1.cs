using System;

namespace Eon {

	/// <summary>
	/// Provides special argument placeholder.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public readonly struct ArgumentPlaceholder<TValue> {

		public readonly bool HasExplicitValue;

		readonly TValue _explicitValue;

		public ArgumentPlaceholder(TValue value) {
			_explicitValue = value;
			HasExplicitValue = true;
		}

		// TODO: Put strings into the resources.
		//
		public TValue ExplicitValue {
			get {
				if (HasExplicitValue)
					return _explicitValue;
				else
					throw
						new InvalidOperationException(
							message: $"Для аргумента явно не установлено какое-либо значение (см. '{nameof(HasExplicitValue)}').");
			}
		}

		/// <summary>
		/// Returns explicit value spcified by this placeholder (see <see cref="HasExplicitValue"/>, <see cref="ExplicitValue"/>) or value specified by <paramref name="value"/>.
		/// </summary>
		/// <param name="value">Substitution value.</param>
		public TValue Substitute(TValue value)
			=> HasExplicitValue ? _explicitValue : value;

		public static implicit operator ArgumentPlaceholder<TValue>(TValue value)
			=> new ArgumentPlaceholder<TValue>(value: value);

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=> $"{(HasExplicitValue ? _explicitValue.FmtStr().G() : "<значение отсутствует>")}";

	}

}