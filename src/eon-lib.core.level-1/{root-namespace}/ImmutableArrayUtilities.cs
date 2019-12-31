using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static class ImmutableArrayUtilities {

		static readonly char[ ] __HexAlphabet = new char[ ] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', };

		public static ImmutableArray<T> ToImmutableArray<T>(this T[ ] array) {
			if (array is null)
				return default;
			else if (array.Length < 1)
				return ImmutableArray<T>.Empty;
			else
				return ImmutableArray.Create(array);
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static T[ ] ToArray<T>(this in ImmutableArray<T>? immutableArray)
			=> immutableArray.HasValue ? ToArray(immutableArray.Value) : null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static T[ ] ToArray<T>(this ImmutableArray<T> immutableArray) {
			if (immutableArray.IsDefault)
				return null;
			else if (immutableArray.IsEmpty)
				return new T[ ] { };
			else {
				var result = new T[ immutableArray.Length ];
				immutableArray.CopyTo(destination: result);
				return result;
			}
		}

		public static string ToHexString(this in ImmutableArray<byte> immutableArray) {
			if (immutableArray.IsDefault)
				return null;
			else if (immutableArray.IsEmpty)
				return string.Empty;
			else {
				var arrayLength = immutableArray.Length;
				if (arrayLength > int.MaxValue / 2 - 1)
					throw new ArgumentException(message: FormatXResource(typeof(Array), "TooBig/ExpectedMaxLength", int.MaxValue / 2 - 1), paramName: nameof(immutableArray));
				var buffer = new char[ arrayLength * 2 ];
				byte b;
				for (var i = 0; i < buffer.Length; i++) {
					buffer[ i ] = __HexAlphabet[ (b = immutableArray[ i >> 1 ]) >> 4 ];
					buffer[ ++i ] = __HexAlphabet[ b & 15 ];
				}
				return new string(buffer);
			}
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static string ToBase64String(this in ImmutableArray<byte> immutableArray)
			=> immutableArray.IsDefault ? null : immutableArray.ToArray().ToBase64String();

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ImmutableArray<byte> FromBase64String(string s)
			=> s is null ? default : ImmutableArray.Create(items: Convert.FromBase64String(s: s));

	}

}