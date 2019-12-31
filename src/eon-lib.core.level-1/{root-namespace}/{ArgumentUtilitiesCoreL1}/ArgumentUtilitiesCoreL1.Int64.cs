using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	// TODO_HIGH: Использование фабрики исключения, текст сообщения для свойств.

	public static partial class ArgumentUtilitiesCoreL1 {

		public static long EnsureGreaterThanOrEqual(this long value, long operand, string name) {
			if (value < operand)
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(value) : name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("d")));
			return value;
		}

		public static long EnsureNotLessThanOne(this long value, string name) {
			if (value < 1)
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(value) : name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanOne"));
			return value;
		}

		public static ArgumentUtilitiesHandle<long> EnsureNotZero(this ArgumentUtilitiesHandle<long> hnd) {
			if (hnd.Value == 0L)
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotEqualTo", 0L));
			return hnd;
		}

		public static ArgumentUtilitiesHandle<long?> EnsureNotZero(this ArgumentUtilitiesHandle<long?> hnd) {
			if (hnd.Value.Value == 0L)
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotEqualTo", 0L));
			return hnd;
		}

		public static ArgumentUtilitiesHandle<long> EnsureNotLessThanZero(this long value, string name)
			=> new ArgumentUtilitiesHandle<long>(value, name).EnsureNotLessThanZero();

		public static ArgumentUtilitiesHandle<long?> EnsureNotLessThanZero(this ArgumentUtilitiesHandle<long?> hnd) {
			if (hnd.Value < 0L)
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			return hnd;
		}

		public static ArgumentUtilitiesHandle<long> EnsureNotLessThanZero(this ArgumentUtilitiesHandle<long> hnd) {
			if (hnd.Value < 0L)
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			return hnd;
		}

		public static ArgumentUtilitiesHandle<long> EnsureNotLessThan(this ArgumentUtilitiesHandle<long> hnd, long operand) {
			if (hnd.Value < operand)
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("d")));
			return hnd;
		}

		public static ArgumentUtilitiesHandle<long?> EnsureNotLessThan(this ArgumentUtilitiesHandle<long?> hnd, long operand) {
			if (hnd.Value < operand)
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("d")));
			return hnd;
		}

		public static ArgumentUtilitiesHandle<long> EnsureNotGreaterThan(this ArgumentUtilitiesHandle<long> hnd, long operand) {
			if (hnd.Value > operand)
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", operand.ToString("d")));
			return hnd;
		}

		public static ArgumentUtilitiesHandle<long?> EnsureBetween(this ArgumentUtilitiesHandle<long?> hnd, long bound1Inclusive, long bound2Inclusive) {
			if (hnd.Value.HasValue && !hnd.Value.Value.Between(bound1Inclusive, bound2Inclusive))
				throw
					new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: FormatXResource(typeof(ArgumentOutOfRangeException), "OutOfRange/WithRange", (bound1Inclusive > bound2Inclusive ? bound2Inclusive : bound1Inclusive).FmtStr().Decimal(), (bound2Inclusive > bound1Inclusive ? bound2Inclusive : bound1Inclusive).FmtStr().Decimal()));
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<long> EnsureNotLessThan(this ArgumentUtilitiesHandle<long> hnd, ArgumentUtilitiesHandle<long> otherArgument) {
			if (hnd.Value < otherArgument.Value)
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan/OtherArg2", hnd.Value.ToString("d"), otherArgument.Value.ToString("d"), otherArgument.Name));
			return hnd;
		}

		public static ArgumentUtilitiesHandle<long> EnsureNotEqualTo(this ArgumentUtilitiesHandle<long> hnd, long operand) {
			if (hnd.Value == operand)
				throw
					new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotEqualTo", operand.ToString("d")));
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<long?> EnsureNotEqualTo(this ArgumentUtilitiesHandle<long?> hnd, long operand) {
			if (hnd.Value == operand)
				throw
					new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotEqualTo", operand.ToString("d")));
			else
				return hnd;
		}

	}

}