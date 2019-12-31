using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Eon {

	public static class NumberUtilities {

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool Between(this decimal number, decimal a, decimal b)
			=> a > b ? b <= number && number <= a : a <= number && number <= b;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool Between(this decimal? number, decimal a, decimal b)
			=> number.HasValue ? (a > b ? b <= number.Value && number.Value <= a : a <= number.Value && number.Value <= b) : false;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool Between(this int number, int bound1Inclusive, int bound2Inclusive)
			=> bound1Inclusive > bound2Inclusive ? bound2Inclusive <= number && number <= bound1Inclusive : bound1Inclusive <= number && number <= bound2Inclusive;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool Between(this long number, long a, long b) {
			if (a > b)
				return b <= number && number <= a;
			else
				return a <= number && number <= b;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool Between(this int? number, int a, int b)
			=> number.HasValue ? Between(number.Value, a, b) : false;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool Between(this long? number, long a, long b) {
			if (number.HasValue)
				return Between(number.Value, a, b);
			else
				return false;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool HasMaxScaleOf(this decimal number, int scale) {
			scale.EnsureNotLessThanZero(nameof(scale));
			//
			return number == Math.Round(d: number, decimals: scale);
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static decimal? ZeroIfNegative(this decimal? number)
			=> number < decimal.Zero ? decimal.Zero : number;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static decimal ZeroIfNegative(this decimal number)
			=> number < decimal.Zero ? decimal.Zero : number;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static long? Max(this long? a, long? b) {
			if (a.HasValue && b.HasValue)
				return a.Value > b.Value ? a : b;
			else
				return default;
		}

		public static bool TryParseInvariant(string stringValue, out long? result)
			=> TryParse(stringValue, CultureInfo.InvariantCulture, out result);

		public static bool TryParse(string stringValue, out long? result)
			=> TryParse(stringValue, null, out result);

		public static bool TryParse(string stringValue, IFormatProvider formatProvider, out long? result) {
			if (stringValue is null) {
				result = null;
				return true;
			}
			else if (stringValue == string.Empty) {
				result = null;
				return false;
			}
			else {
				long locResult;
				if (long.TryParse(stringValue, NumberStyles.Integer, formatProvider, out locResult)) {
					result = locResult;
					return true;
				}
				else {
					result = null;
					return false;
				}
			}
		}

		public static bool TryParseInvariant(string stringValue, out int? result)
			=> TryParse(stringValue, CultureInfo.InvariantCulture, out result);

		public static bool TryParse(string stringValue, out int? result)
			=> TryParse(stringValue, null, out result);

		public static bool TryParse(string stringValue, IFormatProvider formatProvider, out int? result) {
			if (stringValue is null) {
				result = null;
				return true;
			}
			else if (stringValue == string.Empty) {
				result = null;
				return false;
			}
			else {
				int locResult;
				if (int.TryParse(stringValue, NumberStyles.Integer, formatProvider, out locResult)) {
					result = locResult;
					return true;
				}
				else {
					result = null;
					return false;
				}
			}
		}

		public static void Split(this long value, out int left, out int right) {
			int locLeft;
			int locRight;
			if (BitConverter.IsLittleEndian) {
				locLeft = (int)(value >> 32);
				locRight = (int)((value << 32) >> 32);
			}
			else {
				locLeft = (int)((value >> 32) << 32);
				locRight = (int)(value << 32);
			}
			left = locLeft;
			right = locRight;
		}

		public static int Clamp(int value, int min, int max)
			=> Math.Min(Math.Max(value, min), max);

		public static long Clamp(long value, long min, long max)
			=> Math.Min(Math.Max(value, min), max);

		public static decimal Clamp(decimal value, decimal min, decimal max)
			=> Math.Min(val1: Math.Max(val1: value, val2: min), val2: max);

	}

}