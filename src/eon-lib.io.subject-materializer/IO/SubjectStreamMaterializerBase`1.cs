using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Eon.ComponentModel;
using Eon.Context;
using Eon.Threading.Tasks;
using Eon.Xml;

using Newtonsoft.Json;

using static Eon.DisposableUtilities;
using static Eon.Net.Mime.MediaTypeNameUtilities;

namespace Eon.IO {

	public abstract class SubjectStreamMaterializerBase<TSubject>
		:Disposable, ISubjectMaterializer<TSubject>
		where TSubject : class {

		#region Static members

		static bool P_TryRecognizeStreamContentAsJson(Stream stream) {
			stream.EnsureNotNull(nameof(stream));
			//
			if (stream.CanSeek) {
				var jsonReader = default(JsonTextReader);
				var textReader = default(TextReader);
				var initialInputStreamPosition = default(long?);
				var error = default(Exception);
				try {
					TextReader createTextReader(bool locLeaveInputStreamOpen) => new StreamReader(stream: stream, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: TextReaderWriterUtilities.DefaultBufferSize, leaveOpen: locLeaveInputStreamOpen);
					//
					initialInputStreamPosition = stream.Position;
					textReader = createTextReader(true);
					jsonReader =
						new JsonTextReader(textReader) {
							CloseInput = false
						};
					//
					bool isJson;
					try {
						isJson = jsonReader.Read() && jsonReader.TokenType == JsonToken.StartObject;
						stream.Position = initialInputStreamPosition.Value;
						initialInputStreamPosition = null;
					}
					catch (JsonReaderException) {
						isJson = false;
					}
					//
					textReader.Dispose();
					(jsonReader as IDisposable)?.Dispose();
					//
					return isJson;
				}
				catch (Exception exception) {
					error = exception;
					throw;
				}
				finally {
					try {
						if (initialInputStreamPosition.HasValue)
							try { stream.Position = initialInputStreamPosition.Value; }
							catch (ObjectDisposedException) { }
						if (error != null) {
							textReader?.Dispose();
							(jsonReader as IDisposable)?.Dispose();
						}
					}
					catch (Exception firstException) {
						if (error == null)
							throw;
						else
							throw error.ToAggregateException(firstException);
					}
				}
			}
			else
				return false;
		}

		static async Task<bool> P_TryRecognizeStreamContentAsXmlAsync(Stream stream, XmlReaderSettings xmlReaderSettings) {
			stream.EnsureNotNull(nameof(stream));
			xmlReaderSettings.EnsureNotNull(nameof(xmlReaderSettings));
			//
			if (stream.CanSeek) {
				var xmlReader = default(XmlReader);
				var textReader = default(TextReader);
				var initialInputStreamPosition = default(long?);
				var tryException = default(Exception);
				try {
					Func<Stream, bool, TextReader> createTextReader =
						(locInputStream, locLeaveInputStreamOpen)
						 =>
						new StreamReader(
							stream: locInputStream,
							encoding: Encoding.UTF8,
							detectEncodingFromByteOrderMarks: true,
							bufferSize: TextReaderWriterUtilities.DefaultBufferSize,
							leaveOpen: locLeaveInputStreamOpen);
					//
					initialInputStreamPosition = stream.Position;
					textReader = createTextReader(stream, true);
					var xmlReaderSettingsClone = xmlReaderSettings.Clone();
					xmlReaderSettingsClone.DtdProcessing = DtdProcessing.Prohibit;
					xmlReaderSettingsClone.CloseInput = false;
					xmlReaderSettingsClone.IgnoreComments
						= xmlReaderSettingsClone.IgnoreProcessingInstructions
						= xmlReaderSettingsClone.IgnoreWhitespace
						= true;
					xmlReader = XmlReader.Create(textReader, xmlReaderSettingsClone);
					//
					bool isXml;
					try {
						isXml =
							await
							xmlReader
							.ReadAsync()
							.ConfigureAwait(false);
						isXml = isXml && xmlReader.NodeType == XmlNodeType.XmlDeclaration;
						//
						stream.Position = initialInputStreamPosition.Value;
						initialInputStreamPosition = null;
					}
					catch (XmlException) {
						isXml = false;
					}
					//
					textReader.Dispose();
					(xmlReader as IDisposable)?.Dispose();
					//
					return isXml;
				}
				catch (Exception firstException) {
					tryException = firstException;
					throw;
				}
				finally {
					try {
						if (initialInputStreamPosition.HasValue)
							try { stream.Position = initialInputStreamPosition.Value; }
							catch (ObjectDisposedException) { }
						if (tryException != null) {
							textReader?.Dispose();
							(xmlReader as IDisposable)?.Dispose();
						}
					}
					catch (Exception firstException) {
						if (tryException == null)
							throw;
						else
							throw tryException.ToAggregateException(firstException);
					}
				}
			}
			else
				return false;
		}

		#endregion

		protected SubjectStreamMaterializerBase() { }

		// TODO: Put strings into the resources.
		//
		public async Task<TSubject> MaterializeAsync(ISubjectLoadContext ctx) {
			var gotStreamInfoStore = default(IVh<StreamInfo>);
			var recognizedStreamInfoStore = default(IVh<StreamInfo>);
			var materializedSubject = default(TSubject);
			var caughtException = default(Exception);
			try {
				// Получить поток загрузки.
				//
				gotStreamInfoStore = await GetSourceStreamAsync(context: ctx).ConfigureAwait(false);
				var useStreamInfo = gotStreamInfoStore.Value;
				//
				if (IsGenericMediaType(useStreamInfo.ContentMediaType)) {
					// Распознать конкретный медиа-тип содержимого потока загрузки (т.е. отличный от универсального).
					//
					recognizedStreamInfoStore = await RecognizeMediaFormatAsync(ctx, useStreamInfo.Stream).ConfigureAwait(false);
					useStreamInfo = recognizedStreamInfoStore.Value;
				}
				// Если контекстом загрузки указан медиа-тип, отличный от универсального, то медиа-тип потока загрузки должен соответствовать указанному в контексте загрузки.
				//
				if (!IsGenericMediaType(ctx.MediaType) && !useStreamInfo.ContentMediaType.EqualsOrdinalCI(ctx.MediaType))
					throw new EonException($"Медиа-тип содержимого потока загрузки не соответствует требуемому медиа-типу контекста загрузки.{Environment.NewLine}\tМедиа-тип потока загрузки:{Environment.NewLine}{useStreamInfo.ContentMediaType.IndentLines2()}{Environment.NewLine}\tМедиа-тип контекста загрузки:{ctx.MediaType.IndentLines2()}.");
				// Материализация (десериализация).
				//
				materializedSubject = await DoMaterializeAsync(ctx: ctx, stream: useStreamInfo).ConfigureAwait(false);
				//
				var siteOriginSupport = materializedSubject as ISiteOriginSupport;
				if (siteOriginSupport?.HasSiteOrigin == false)
					siteOriginSupport.SetSiteOrigin(siteOrigin: ctx.SiteOrigin);
				//
				return materializedSubject;
			}
			catch (Exception exception) {
				caughtException = exception;
				throw;
			}
			finally {
				if (caughtException != null)
					DisposeManyDeep(caughtException, materializedSubject);
				try {
					recognizedStreamInfoStore?.Dispose();
					gotStreamInfoStore?.Dispose();
				}
				catch (Exception exception) {
					if (caughtException == null)
						throw;
					else
						throw caughtException.ToAggregateException(exception);
				}
			}
		}

		ITaskWrap<TSubject> ISubjectMaterializer<TSubject>.MaterializeAsync(ISubjectLoadContext ctx)
			=> MaterializeAsync(ctx).Wrap();

		// TODO: Put strings into the resources.
		//
		protected virtual async Task<TSubject> DoMaterializeAsync(ISubjectLoadContext ctx, StreamInfo stream) {
			ctx.EnsureNotNull(nameof(ctx));
			stream.EnsureNotNull(nameof(stream));
			//
			await Task.CompletedTask;
			TSubject subject;
			var deserializedObjectAsIDisposable = default(IDisposable);
			try {
				object deserializedObject;
				if (IsJsonMediaType(mediaTypeName: stream.ContentMediaType)) {
					// JSON.
					//
					var jsonSerializer = ctx.CreateJsonSerializer();
					using (var textReader = new StreamReader(stream.Stream, Encoding.UTF8, true, TextReaderWriterUtilities.DefaultBufferSize, true))
					using (var jsonReader = new JsonTextReader(textReader))
						deserializedObjectAsIDisposable = (deserializedObject = jsonSerializer.Deserialize(jsonReader)) as IDisposable;
				}
				else if (IsXmlMediaType(mediaTypeName: stream.ContentMediaType)) {
					// XML.
					//
					var xmlObjectSerializer = ctx.CreateXmlObjectSerializer(type: typeof(object));
					using (var xmlReader = XmlReader.Create(input: stream.Stream, settings: ctx.CreateXmlReaderSettings().CopyWith(closeInput: false)))
						deserializedObjectAsIDisposable = (deserializedObject = xmlObjectSerializer.ReadObject(xmlReader, false)) as IDisposable;
				}
				else
					throw new NotSupportedException($"Медиа-тип содержимого потока не поддерживается.{Environment.NewLine}\tМедиа-тип:{stream.ContentMediaType.FmtStr().GNLI2()}");
				//
				subject = deserializedObject as TSubject;
				if (subject is null && !(deserializedObject is null))
					throw new EonException(message: $"Десериализованный объект имеет тип, не совместимый с требуемым.{Environment.NewLine}\tТип десериализованного объекта:{deserializedObject.GetType().FmtStr().GNLI2()}{Environment.NewLine}\tТребуемый тип:{typeof(TSubject).FmtStr().GI2()}.");
				//
				return subject;
			}
			catch (Exception exception) {
				DisposeMany(exception: exception, disposables: deserializedObjectAsIDisposable);
				throw;
			}
		}

		protected abstract Task<IVh<StreamInfo>> GetSourceStreamAsync(ISubjectLoadContext context);

		// TODO: Put strings into the resources.
		//
		protected virtual async Task<IVh<StreamInfo>> RecognizeMediaFormatAsync(ISubjectLoadContext ctx, Stream stream) {
			var isJson = P_TryRecognizeStreamContentAsJson(stream);
			if (isJson)
				return new StreamInfo(stream, false, contentMediaType: AppJson).ToValueHolder(true);
			else {
				var isXml = await P_TryRecognizeStreamContentAsXmlAsync(stream, ctx.CreateXmlReaderSettings()).ConfigureAwait(false);
				if (isXml)
					return new StreamInfo(stream, false, contentMediaType: AppXml).ToValueHolder(true);
				else
					throw
						new NotSupportedException(message: $"Формат содержимого потока не распознан или не поддерживается.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}{Environment.NewLine}\tКонтекст:{ctx.FmtStr().GNLI2()}{Environment.NewLine}\tПоток:{stream.FmtStr().GNLI2()}");
			}
		}

	}

}