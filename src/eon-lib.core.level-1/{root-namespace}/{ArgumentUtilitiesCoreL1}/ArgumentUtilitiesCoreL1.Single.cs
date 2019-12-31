using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	// TODO_HIGH: Использование фабрики исключения, текст сообщения для свойств.

	public static partial class ArgumentUtilitiesCoreL1 {

		public static ArgumentUtilitiesHandle<float> EnsureNotNaN(this ArgumentUtilitiesHandle<float> hnd) {
			if (float.IsNaN(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNaN"));
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<float> EnsureNotNaN(this float value, string name) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(name) : name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNaN"));
			else
				return new ArgumentUtilitiesHandle<float>(value, string.IsNullOrEmpty(name) ? nameof(name) : name);
		}

		public static ArgumentUtilitiesHandle<float> EnsureNotPositiveInfinity(this ArgumentUtilitiesHandle<float> hnd) {
			if (float.IsPositiveInfinity(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotPositiveInfinity"));
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<float> EnsureNotNegativeInfinity(this ArgumentUtilitiesHandle<float> hnd) {
			if (float.IsNegativeInfinity(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNegativeInfinity"));
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<float> EnsureNumber(this ArgumentUtilitiesHandle<float> hnd) {
			if (float.IsNaN(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNaN"));
			else if (float.IsPositiveInfinity(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotPositiveInfinity"));
			else if (float.IsNegativeInfinity(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotNegativeInfinity"));
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<float> EnsureNumber(this float value, string name)
			=> new ArgumentUtilitiesHandle<float>(value, string.IsNullOrEmpty(name) ? nameof(name) : name).EnsureNumber();

		public static ArgumentUtilitiesHandle<float> EnsureNotGreaterThan(this ArgumentUtilitiesHandle<float> argument, float operand) {
			operand.EnsureNotNaN(nameof(operand));
			//
			if (float.IsNaN(argument.Value) || argument.Value <= operand)
				return argument;
			else
				throw new ArgumentOutOfRangeException(argument.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", operand.ToString("r")));
		}

		public static ArgumentUtilitiesHandle<float> EnsureNotLessThan(this ArgumentUtilitiesHandle<float> hnd, float operand) {
			operand.EnsureNotNaN(nameof(operand));
			//
			if (float.IsNaN(hnd.Value) || hnd.Value >= operand)
				return hnd;
			else
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("r")));
		}

	}

}