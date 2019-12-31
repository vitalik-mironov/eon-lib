using System;
using System.Collections;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static class BitArrayUtilities {

		static readonly byte[ ] __LittleEndianMap = new byte[ ] { 128, 64, 32, 16, 8, 4, 2, 1 };

		static readonly byte[ ] __BigEndianMap = new byte[ ] { 1, 2, 4, 8, 16, 32, 64, 128 };

		public static BitArray ToBits(this int number) { return new BitArray(new int[ ] { number }); }

		public static byte[ ] ToBytes(this BitArray bits) { return ToBytes(bits, false); }

		public static byte[ ] ToBytes(this BitArray bits, bool littleEndian) {
			bits.EnsureNotNull(nameof(bits));
			//
			return ToBytes(bits: bits, startIndex: 0, length: bits.Length, littleEndian: littleEndian);
		}

		public static byte[ ] ToBytes(this BitArray bits, int startIndex, int length)
			=> ToBytes(bits: bits, startIndex: startIndex, length: length, littleEndian: false);

		public static byte[ ] ToBytesRight(this BitArray bits, int length) {
			int bitsLength;
			if (bits is null)
				throw new ArgumentNullException(nameof(bits));
			else if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			else if (length == 0)
				return new byte[ ] { };
			else if (length > (bitsLength = bits.Length))
				throw new ArgumentOutOfRangeException(nameof(length), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", bitsLength.ToString("d")));
			return ToBytes(bits, bitsLength - length, length, false);
		}

		public static byte[ ] ToBytes(this BitArray bits, int startIndex, int length, bool littleEndian) {
			if (bits is null)
				throw new ArgumentNullException(nameof(bits));
			var bitsLength = bits.Length;
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(startIndex), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			else if (startIndex > (bitsLength - 1))
				throw new ArgumentOutOfRangeException(nameof(startIndex), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", (bitsLength - 1).ToString("d")));
			else if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			else if (length == 0)
				return new byte[ ] { };
			else if (length > (bitsLength - startIndex))
				throw new ArgumentOutOfRangeException(nameof(length), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", (bitsLength - startIndex).ToString("d")));
			byte[ ] map;
			if (littleEndian)
				map = __LittleEndianMap;
			else
				map = __BigEndianMap;
			var result = new byte[ length / 8 + ((length & 7) > 0 ? 1 : 0) ];
			var bitsIndex = startIndex;
			var bitsLengthLimit = startIndex + length;
			for (var i = 0; i < result.Length; i++) {
				for (var m = 0; m < 8 && bitsIndex < bitsLengthLimit; m++, bitsIndex++)
					result[ i ] += (bits[ bitsIndex ] ? map[ m ] : byte.MinValue);
			}
			return result;
		}

	}

}