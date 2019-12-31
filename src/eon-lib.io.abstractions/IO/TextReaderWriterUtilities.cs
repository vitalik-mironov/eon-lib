using System.IO;
using System.Text;

namespace Eon.IO {

	public static class TextReaderWriterUtilities {

		/// <summary>
		/// Значение: <see cref="StreamUtilities.DefaultBufferSize"/>.
		/// </summary>
		public static readonly int DefaultBufferSize = StreamUtilities.DefaultBufferSize;

		public static void Write(this TextWriter writer, StringBuilder buffer) {
			writer.EnsureNotNull(nameof(writer));
			buffer.EnsureNotNull(nameof(buffer));
			//
			//
			writer.Write(buffer.ToString());
		}

		public static TextReader CreateTextReader(this Stream input, Encoding encoding) {
			input.EnsureNotNull(nameof(input));
			encoding.EnsureNotNull(nameof(encoding));
			//
			return new StreamReader(stream: input, encoding: encoding, detectEncodingFromByteOrderMarks: false, bufferSize: DefaultBufferSize, leaveOpen: true);
		}

	}

}