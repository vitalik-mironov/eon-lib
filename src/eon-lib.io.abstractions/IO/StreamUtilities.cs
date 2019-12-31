using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Eon.IO {

	public static class StreamUtilities {

		/// <summary>
		/// Значение: <see langword="81920"/>.
		/// </summary>
		public static readonly int DefaultBufferSize = 81920;

		public static async Task CopyToAsync(this Stream source, Stream destination, CancellationToken cancellationToken = default) {
			source.EnsureNotNull(nameof(source));
			//
			await
				source
				.CopyToAsync(destination: destination, bufferSize: DefaultBufferSize, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public static async Task WriteAsync(this Stream stream, byte[ ] buffer, CancellationToken ct = default) {
			stream.EnsureNotNull(nameof(stream));
			buffer.EnsureNotNull(nameof(buffer));
			//
			await
				stream
				.WriteAsync(buffer: buffer, offset: 0, count: buffer.Length, cancellationToken: ct)
				.ConfigureAwait(false);
		}

		public static void Write(this Stream stream, byte[ ] buffer) {
			stream.EnsureNotNull(nameof(stream));
			buffer.EnsureNotNull(nameof(buffer));
			//
			stream.Write(buffer: buffer, offset: 0, count: buffer.Length);
		}

	}

}