using System;

namespace Eon {

	public sealed class StringCompareOp {

		#region Static & constant members

		/// <summary>
		/// Соответствует <see cref="StringComparison.Ordinal"/>.
		/// </summary>
		public static readonly StringCompareOp OrdinalCS = new StringCompareOp(comparison: StringComparison.Ordinal);

		/// <summary>
		/// Соответствует <see cref="StringComparison.OrdinalIgnoreCase"/>.
		/// </summary>
		public static readonly StringCompareOp OrdinalCI = new StringCompareOp(comparison: StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Соответствует <see cref="StringComparison.InvariantCulture"/>.
		/// </summary>
		public static readonly StringCompareOp InvariantCultureCS = new StringCompareOp(comparison: StringComparison.InvariantCulture);

		/// <summary>
		/// Соответствует <see cref="StringComparison.InvariantCultureIgnoreCase"/>.
		/// </summary>
		public static readonly StringCompareOp InvariantCultureCI = new StringCompareOp(comparison: StringComparison.InvariantCultureIgnoreCase);

		/// <summary>
		/// Соответствует <see cref="StringComparison.CurrentCulture"/>.
		/// </summary>
		public static readonly StringCompareOp CurrentCultureCS = new StringCompareOp(comparison: StringComparison.CurrentCulture);

		/// <summary>
		/// Соответствует <see cref="StringComparison.CurrentCultureIgnoreCase"/>.
		/// </summary>
		public static readonly StringCompareOp CurrentCultureCI = new StringCompareOp(comparison: StringComparison.CurrentCultureIgnoreCase);

		#endregion

		public readonly StringComparison Type;

		public readonly StringComparer Comparer;

		public readonly new Func<string, string, bool> Equals;

		StringCompareOp(StringComparison comparison) {
			switch (comparison) {
				case StringComparison.CurrentCulture:
					Comparer = StringComparer.CurrentCulture;
					Equals = (locA, locB) => string.Equals(a: locA, b: locB, StringComparison.CurrentCulture);
					break;
				case StringComparison.CurrentCultureIgnoreCase:
					Comparer = StringComparer.CurrentCultureIgnoreCase;
					Equals = (locA, locB) => string.Equals(a: locA, b: locB, StringComparison.CurrentCultureIgnoreCase);
					break;
				case StringComparison.InvariantCulture:
					Comparer = StringComparer.InvariantCulture;
					Equals = (locA, locB) => string.Equals(a: locA, b: locB, StringComparison.InvariantCulture);
					break;
				case StringComparison.InvariantCultureIgnoreCase:
					Comparer = StringComparer.InvariantCultureIgnoreCase;
					Equals = (locA, locB) => string.Equals(a: locA, b: locB, StringComparison.InvariantCultureIgnoreCase);
					break;
				case StringComparison.Ordinal:
					Comparer = StringComparer.Ordinal;
					Equals = (locA, locB) => string.Equals(a: locA, b: locB, StringComparison.Ordinal);
					break;
				case StringComparison.OrdinalIgnoreCase:
					Comparer = StringComparer.OrdinalIgnoreCase;
					Equals = (locA, locB) => string.Equals(a: locA, b: locB, StringComparison.OrdinalIgnoreCase);
					break;
				default:
					throw new ArgumentOutOfRangeException(paramName: nameof(comparison), message: $"Значение не поддерживается.{Environment.NewLine}\tЗначение:{comparison.ToString().FmtStr().GNLI2()}");
			}
			Type = comparison;
		}

	}

}