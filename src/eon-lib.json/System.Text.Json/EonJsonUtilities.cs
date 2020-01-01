using System.IO;
using System.Threading.Tasks;

using Eon;
using Eon.ComponentModel.Dependencies;
using Eon.IO;
using Eon.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace System.Text.Json {

	public static class EonJsonUtilities {

		static readonly DisposableLazy<JsonSerializer> __DefaultSerializer;

		static EonJsonUtilities() {
			__DefaultSerializer = new DisposableLazy<JsonSerializer>(factory: CreateDefaultSerializer, ownsValue: true);
		}

		public static string SerializeToJson<T>(this T @object, bool indent)
			=> SerializeToJson(@object: @object, serializer: DefaultSerializer, indent: indent);

		public static string SerializeToJson<T>(this T @object, JsonSerializer serializer, bool indent) {
			@object.EnsureNotNull(nameof(@object));
			serializer.EnsureNotNull(nameof(serializer));
			//
			using (var acquiredBuffer = EonStringBuilderUtilities.AcquireBufferUnlimCapacity())
			using (var stringWriter = new StringWriter(sb: acquiredBuffer.StringBuilder))
			using (var jsonWriter = new JsonTextWriter(textWriter: stringWriter)) {
				if (indent) {
					jsonWriter.Formatting = Formatting.Indented;
					jsonWriter.Indentation = 1;
					jsonWriter.IndentChar = '\x0009';
				}
				else
					jsonWriter.Formatting = Formatting.None;
				serializer.Serialize(jsonWriter, @object);
				jsonWriter.Flush();
				return acquiredBuffer.StringBuilder.ToString();
			}
		}

		public static Task<string> SerializeToJsonAsync<T>(this T @object) {
			try {
				@object.EnsureNotNull(nameof(@object));
				//
				return Task.FromResult(result: SerializeToJson(@object: @object, serializer: DefaultSerializer, indent: false));
			}
			catch (Exception exception) {
				return Task.FromException<string>(exception: exception);
			}
		}

		public static string SerializeToJson<T>(this T @object)
			=> SerializeToJson(@object: @object, serializer: DefaultSerializer, indent: false);

		public static T DeserializeFromJson<T>(string json) {
			json.EnsureNotNullOrEmpty(nameof(json));
			//
			using (var stringReader = new StringReader(s: json))
			using (var jsonReader = new JsonTextReader(reader: stringReader))
				return __DefaultSerializer.Value.Deserialize<T>(jsonReader);
		}

		public static JsonSerializerSettings CreateDefaultSerializerSettings() {
			var settings = new JsonSerializerSettings();
			var stringEnumConverter = new StringEnumConverter();
			settings.Converters.Add(stringEnumConverter);
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.None;
			return settings;
		}

		public static JsonSerializer CreateDefaultSerializer()
			=> JsonSerializer.Create(settings: CreateDefaultSerializerSettings());

		public static JsonSerializer DefaultSerializer
			=> __DefaultSerializer.Value;

		public static T Deserialize<T>(this JsonSerializer serializer, Stream input, Encoding encoding = default) {
			serializer.EnsureNotNull(nameof(serializer));
			input.EnsureNotNull(nameof(input));
			//
			using (var textReader = new StreamReader(stream: input, encoding: encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: TextReaderWriterUtilities.DefaultBufferSize, leaveOpen: true))
			using (var jsonReader = new JsonTextReader(textReader))
				return serializer.Deserialize<T>(reader: jsonReader);
		}

		public static JsonSerializer SelectSerializer(JsonSerializer serializer = default, IDependencySupport dependencies = default) {
			if (serializer is null)
				return dependencies?.GetService<JsonSerializer>() ?? DefaultSerializer;
			else
				return serializer;
		}

	}

}