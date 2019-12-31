using System;
using System.Runtime.CompilerServices;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static class ByteArrayUtilities {

		static readonly char[ ] __HexAlphabet = new char[ ] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', };

		static readonly int[ ] _hexCharMap;

		static ByteArrayUtilities() {
			_hexCharMap = new int[ 103 ];
			for (var i = 0; i < _hexCharMap.Length; i++)
				_hexCharMap[ i ] = -256;
			for (var i = 48; i < 58; i++)
				_hexCharMap[ i ] = i - 48;
			for (var i = 65; i < 71; i++)
				_hexCharMap[ i ] = i - 55;
			for (var i = 97; i < 103; i++)
				_hexCharMap[ i ] = i - 87;
		}

		public static bool IsEqual(this byte[ ] array1, byte[ ] array2) {
			if (ReferenceEquals(array1, array2))
				return true;
			else if (array1 is null || array2 is null || array1.Length != array2.Length)
				return false;
			var arrayLength = array1.Length;
			var array1LowerBound = array1.GetLowerBound(0);
			var array2LowerBound = array2.GetLowerBound(0);
			var result = true;
			for (var offset = 0; result && offset < arrayLength; offset++)
				result = array1[ offset + array1LowerBound ] == array2[ offset + array2LowerBound ];
			return result;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static string ToBase64String(this byte[ ] array)
			=> array == null ? null : Convert.ToBase64String(inArray: array);

		public static byte[ ] FromBase64String(string input) {
			if (input is null)
				return null;
			else
				return Convert.FromBase64String(s: input);
		}

		public static string ToHexString(this byte[ ] inArray, int? offset = default, int? count = default, char? delimiter = default) {
			if (inArray is null) {
				offset.Arg(nameof(offset)).EnsureNotLessThan(0);
				count.Arg(nameof(count)).EnsureNotLessThan(0);
				//
				return null;
			}
			else {
				var inArraySegment = ArrayPartitionBoundary.Get(arrayLength: inArray.Length, offset: offset, count: count);
				if (inArraySegment.Count < 1)
					return string.Empty;
				else {
					var maxInputLength = delimiter.HasValue ? int.MaxValue / 3 : int.MaxValue / 2 - 1;
					if (inArraySegment.Count > maxInputLength)
						throw
							new ArgumentException(
								message: FormatXResource(typeof(Array), "TooBig/ExpectedMaxLength", maxInputLength),
								paramName: nameof(inArray));
					//
					var inArrayLowerBound = inArray.GetLowerBound(0);
					byte b;
					if (delimiter.HasValue) {
						var delimiterValue = delimiter.Value;
						var outputBuffer = new char[ inArraySegment.Count * 3 ];
						var counter = 0;
						for (var i = 0; i < outputBuffer.Length; i++) {
							b = inArray[ inArrayLowerBound + inArraySegment.Offset + counter++ ];
							//
							outputBuffer[ i ] = __HexAlphabet[ b >> 4 ];
							outputBuffer[ ++i ] = __HexAlphabet[ b & 15 ];
							outputBuffer[ ++i ] = delimiterValue;
						}
						return new string(value: outputBuffer, startIndex: 0, length: outputBuffer.Length - 1);
					}
					else {
						var outputBuffer = new char[ inArraySegment.Count * 2 ];
						for (var i = 0; i < outputBuffer.Length; i++) {
							b = inArray[ inArrayLowerBound + inArraySegment.Offset + (i >> 1) ];
							//
							outputBuffer[ i ] = __HexAlphabet[ b >> 4 ];
							outputBuffer[ ++i ] = __HexAlphabet[ b & 15 ];
						}
						return new string(value: outputBuffer);
					}
				}
			}
		}

		public static byte[ ] FromHexString(string input) {
			if (input == null)
				return null;
			else if (input == string.Empty)
				return new byte[ 0 ];
			else if (input.Length % 2 != 0)
				throw new ArgumentException(message: FormatXResource(typeof(string), "NonValidHexString/WithHexString", input.FmtStr().G()), paramName: nameof(input));
			var resultLength = input.Length / 2;
			var result = new byte[ resultLength ];
			int mappedByte;
			for (var i = 0; i < resultLength; i++) {
				try {
					mappedByte = _hexCharMap[ input[ i * 2 ] ] * 16 + _hexCharMap[ input[ i * 2 + 1 ] ];
				}
				catch (IndexOutOfRangeException) {
					throw new ArgumentException(message: FormatXResource(typeof(string), "NonValidHexString/WithHexString", input.FmtStr().G()), paramName: nameof(input));
				}
				if ((mappedByte & 0xFFFFFF00) != 0)
					throw new ArgumentException(FormatXResource(locator: typeof(string), subpath: "NonValidHexString/WithHexString", args: input.FmtStr().G()), nameof(input));
				result[ i ] = (byte)mappedByte;
			}
			return result;
		}

		public static Guid? ToGuid(this byte[ ] input, int startIndex = 0)
			=> GuidUtilities.ToGuid(input, startIndex: startIndex);

		public static int ToInt32(this byte[ ] input, int startIndex, int length) {
			input.EnsureNotNull(nameof(input));
			startIndex.Arg(nameof(startIndex)).EnsureNotLessThanZero();
			length
				.Arg(nameof(length))
				.EnsureNotLessThan(1)
				.EnsureNotGreaterThan(4);
			if (length > input.Length - startIndex)
				throw new ArgumentException(message: FormatXResource(locator: typeof(Array), subpath: "TooSmall"), paramName: nameof(input));
			var result = 0;
			for (var i = 0; i < length; i++)
				result |= input[ startIndex + i ] << (8 * i);
			return result;
		}

		public static long? ToInt64(this byte[ ] input, int startIndex = 0, int? length = null) {
			if (input is null)
				return null;
			startIndex.Arg(nameof(startIndex)).EnsureNotLessThanZero();
			int locLength;
			if (length.HasValue) {
				length.Arg(nameof(length)).EnsureBetween(1, 8);
				locLength = length.Value;
			}
			else
				locLength = Math.Max(Math.Min(8, input.Length - startIndex), 1);
			if (locLength > input.Length - startIndex)
				throw new ArgumentException(message: FormatXResource(typeof(Array), "TooSmall"), paramName: nameof(input));
			long result = 0;
			for (var i = 0; i < locLength; i++)
				result |= (long)input[ startIndex + i ] << (8 * i);
			return result;
		}

		public static int? ToInt32BigEndian(this byte[ ] input, int startIndex = 0, int? length = null) {
			if (input == null)
				return null;
			startIndex.Arg(nameof(startIndex)).EnsureNotLessThanZero();
			int locLength;
			if (length.HasValue) {
				length.Arg(nameof(length)).EnsureBetween(1, 4);
				locLength = length.Value;
			}
			else
				locLength = Math.Max(Math.Min(4, input.Length - startIndex), 1);
			if (locLength > input.Length - startIndex)
				throw new ArgumentException(message: FormatXResource(locator: typeof(Array), subpath: "TooSmall"), paramName: nameof(input));
			var result = 0;
			for (int i = 0, k = locLength - 1; i < locLength; i++, k--)
				result |= input[ startIndex + i ] << (8 * k);
			return result;
		}

		public static long? ToInt64BigEndian(this byte[ ] input, int startIndex, int? length = default) {
			if (input is null)
				return null;
			else {
				startIndex.Arg(nameof(startIndex)).EnsureNotLessThanZero();
				int locLength;
				if (length.HasValue) {
					length
						.Arg(nameof(length))
						.EnsureBetween(1, 8);
					locLength = length.Value;
				}
				else
					locLength = Math.Max(Math.Min(8, input.Length - startIndex), 1);
				if (locLength > input.Length - startIndex)
					throw new ArgumentException(FormatXResource(typeof(Array), "TooSmall"), nameof(input));
				long result = 0;
				for (int i = 0, k = locLength - 1; i < locLength; i++, k--)
					result |= (long)input[ startIndex + i ] << (8 * k);
				return result;
			}
		}

		public static long? ToInt64BigEndian(this byte[ ] input) {
			if (input is null)
				return null;
			else {
				var length = input.Length < 8 ? input.Length : 8;
				long result = 0;
				for (int i = 0, k = 8 - 1; i < 8; i++, k--)
					result |= (long)input[ i ] << (8 * k);
				return result;
			}
		}

		public static byte[ ] GetBytesBigEndian(int? value) {
			if (value.HasValue) {
				if (BitConverter.IsLittleEndian) {
					var result = new byte[ 4 ];
					var valueAsUInt32 = (uint)value;
					result[ 3 ] = (byte)value;
					result[ 2 ] = (byte)((valueAsUInt32 >> 8) & 0xFF);
					result[ 1 ] = (byte)((valueAsUInt32 >> 16) & 0xFF);
					result[ 0 ] = (byte)((valueAsUInt32 >> 24) & 0xFF);
					return result;
				}
				else
					return BitConverter.GetBytes(value.Value);
			}
			else
				return null;
		}

		public static byte[ ] GetBytesBigEndian(long? value) {
			if (value.HasValue) {
				if (BitConverter.IsLittleEndian) {
					var result = new byte[ 8 ];
					var valueAsUInt64 = (ulong)value;
					result[ 7 ] = (byte)value;
					result[ 6 ] = (byte)((valueAsUInt64 >> 8) & 0xFF);
					result[ 5 ] = (byte)((valueAsUInt64 >> 16) & 0xFF);
					result[ 4 ] = (byte)((valueAsUInt64 >> 24) & 0xFF);
					result[ 3 ] = (byte)((valueAsUInt64 >> 32) & 0xFF);
					result[ 2 ] = (byte)((valueAsUInt64 >> 40) & 0xFF);
					result[ 1 ] = (byte)((valueAsUInt64 >> 48) & 0xFF);
					result[ 0 ] = (byte)((valueAsUInt64 >> 56) & 0xFF);
					return result;
				}
				else {
					unsafe {
						var result = new byte[ 8 ];
						fixed (byte* bPtr = result)
							*(long*)bPtr = value.Value;
						return result;
					}
				}
			}
			else
				return null;
		}

		public static int EvaluateHashCode(this byte[ ] array) {
			if (array is null)
				return 0;
			else if (array.Length < 1)
				return -1;
			else {
				var result = int.MaxValue;
				for (var i = 0; i < array.Length; i++)
					result ^= (array[ i ] << (8 * (i & 3)));
				return result;
			}
		}

		public static int GetBase64Length(int sourceLength) {
			sourceLength.Arg(nameof(sourceLength)).EnsureNotLessThanZero();
			//
			int length;
			checked {
				length = (int)((4.0D / 3.0D) * sourceLength);
				if (length % 4 != 0)
					length += 4 - length % 4;
			}
			return length;
		}

	}

}