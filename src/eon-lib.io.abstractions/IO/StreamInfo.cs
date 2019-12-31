using System.IO;

using Eon.Net.Mime;

namespace Eon.IO {

	/// <summary>
	/// Сведения о потоке <see cref="System.IO.Stream"/>.
	/// </summary>
	public class StreamInfo
		:Disposable {

		IVh<Stream> _streamStore;

		string _contentMediaType;

		/// <summary>
		/// Инициализирует новый экземпляр типа <see cref="StreamInfo"/>.
		/// </summary>
		/// <param name="stream">Поток.</param>
		/// <param name="ownsStream">Признак, указывающий, должен ли выгружаться поток <paramref name="stream"/> при выгрузке созданного объекта <see cref="StreamInfo"/>.</param>
		/// <param name="contentMediaType">
		/// Медиа-тип содержимого потока.
		/// <para>По умолчанию (в том числе, когда <paramref name="contentMediaType"/> равен null) используется медиа-тип <see cref="MediaTypeNameUtilities.AppOctetStream"/>.</para>
		/// </param>
		public StreamInfo(Stream stream, bool ownsStream, string contentMediaType = null)
			: this(stream: stream.EnsureNotNull(nameof(stream)).Value.ToValueHolder(ownsValue: ownsStream), contentMediaType: contentMediaType) { }

		/// <summary>
		/// Инициализирует новый экземпляр типа <see cref="StreamInfo"/>.
		/// </summary>
		/// <param name="stream">Поток.</param>
		/// <param name="contentMediaType">
		/// Медиа-тип содержимого потока.
		/// <para>По умолчанию (в том числе, когда <paramref name="contentMediaType"/> равен null) используется медиа-тип <see cref="MediaTypeNameUtilities.AppOctetStream"/>.</para>
		/// </param>
		public StreamInfo(IVh<Stream> stream, string contentMediaType = null) {
			stream.Arg(nameof(stream)).EnsureSelfAndValueNotNull();
			contentMediaType.Arg(nameof(contentMediaType)).EnsureHasMaxLength(maxLength: MediaTypeNameUtilities.MediaTypeNameDefaultMaxLength).EnsureNotEmptyOrWhiteSpace();
			//
			_streamStore = stream;
			_contentMediaType = contentMediaType ?? MediaTypeNameUtilities.AppOctetStream;
		}

		/// <summary>
		/// Возвращает поток, указанный при создании данного экземпляра.
		/// </summary>
		public Stream Stream
			=> ReadDA(ref _streamStore).Value;

		/// <summary>
		/// Возвращает медиа-тип содержимого потока.
		/// <para>По умолчанию равен <see cref="MediaTypeNameUtilities.AppOctetStream"/>.</para>
		/// </summary>
		public string ContentMediaType
			=> ReadDA(ref _contentMediaType);

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_streamStore?.Dispose();
			}
			_streamStore = null;
			_contentMediaType = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}