using System;

namespace Eon {

	public static class GuidUtilities {

		/// <summary>
		/// Возвращает строку нового GUID'a в hex-формате — 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx'.
		/// </summary>
		/// <param name="offset">
		/// Смещение в массиве байтов GUID — начало сегмента GUID, который будет преобразован в hex-строку.
		/// </param>
		/// <param name="count">
		/// Количество байтов GUID — длина сегмента GUID, который будет преобразован в hex-строку.
		/// </param>
		/// <returns>Объект <seealso cref="string"/>.</returns>
		public static string NewGuidString(int? offset = null, int? count = null)
			=> ByteArrayUtilities.ToHexString(inArray: Guid.NewGuid().ToByteArray(), offset: offset, count: count);

		public static string NewGuidBase64String(int? offset = null, int? count = null)
			=> Convert.ToBase64String(inArray: Guid.NewGuid().ToByteArray(), offset: offset ?? 0, length: count ?? 16);

		public static Guid ToGuid(this int value) {
			var guidBytes = new byte[ 16 ];
			var valueBytes = BitConverter.GetBytes(value);
			guidBytes[ 15 ] = valueBytes[ 0 ];
			guidBytes[ 14 ] = valueBytes[ 1 ];
			guidBytes[ 13 ] = valueBytes[ 2 ];
			guidBytes[ 12 ] = valueBytes[ 3 ];
			return new Guid(guidBytes);
		}

		public static Guid? ToGuid(this int? value)
			=> value.HasValue ? ToGuid(value.Value) : default(Guid?);

		public static Guid? ToGuid(this byte[ ] input, int startIndex = 0) {
			input
				.EnsureHasMinLength(nameof(input), 16);
			//
			if (input == null)
				return null;
			else {
				startIndex
					.EnsureNotLessThan(nameof(startIndex), input.GetLowerBound(0));
				//
				var buffer = new byte[ 16 ];
				Array.Copy(input, startIndex, buffer, 0, 16);
				return new Guid(buffer);
			}
		}

	}

}