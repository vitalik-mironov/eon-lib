using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		public static ArgumentUtilitiesHandle<double> EnsureNotNaN(this ArgumentUtilitiesHandle<double> hnd) {
			if (double.IsNaN(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNaN"));
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<double> EnsureNotNaN(this double value, string name) {
			if (double.IsNaN(value))
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(name) : name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNaN"));
			else
				return new ArgumentUtilitiesHandle<double>(value, string.IsNullOrEmpty(name) ? nameof(name) : name);
		}

		public static ArgumentUtilitiesHandle<double> EnsureNotPositiveInfinity(this ArgumentUtilitiesHandle<double> hnd) {
			if (double.IsPositiveInfinity(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotPositiveInfinity"));
			else
				return hnd;
		}
		public static ArgumentUtilitiesHandle<double> EnsureNotNegativeInfinity(this ArgumentUtilitiesHandle<double> hnd) {
			if (double.IsNegativeInfinity(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNegativeInfinity"));
			else
				return hnd;
		}
		public static ArgumentUtilitiesHandle<double> EnsureNumber(this ArgumentUtilitiesHandle<double> hnd) {
			if (double.IsNaN(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNaN"));
			else if (double.IsPositiveInfinity(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotPositiveInfinity"));
			else if (double.IsNegativeInfinity(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNegativeInfinity"));
			else
				return hnd;
		}
		public static ArgumentUtilitiesHandle<double> EnsureNumber(this double value, string name)
			=> new ArgumentUtilitiesHandle<double>(value, string.IsNullOrEmpty(name) ? nameof(name) : name).EnsureNumber();
		public static ArgumentUtilitiesHandle<double> EnsureNotGreaterThan(this ArgumentUtilitiesHandle<double> argument, double operand) {
			operand.EnsureNotNaN(nameof(operand));
			//
			if (double.IsNaN(argument.Value) || argument.Value <= operand)
				return argument;
			else
				throw new ArgumentOutOfRangeException(argument.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", operand.ToString("r")));
		}
		public static ArgumentUtilitiesHandle<double> EnsureNotLessThan(this ArgumentUtilitiesHandle<double> hnd, double operand) {
			operand.EnsureNotNaN(nameof(operand));
			//
			if (double.IsNaN(hnd.Value) || hnd.Value >= operand)
				return hnd;
			else
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("r")));
		}
		public static ArgumentUtilitiesHandle<double> EnsureGreaterThan(this ArgumentUtilitiesHandle<double> hnd, double operand) {
			operand.EnsureNotNaN(nameof(operand));
			//
			if (double.IsNaN(hnd.Value) || hnd.Value > operand)
				return hnd;
			else
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "MustGreaterThan", operand.ToString("r")));
		}

	}

}