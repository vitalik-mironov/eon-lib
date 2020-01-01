using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using Eon;
using Eon.Diagnostics.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Microsoft.Extensions.Configuration {

	public static class ConfigurationUtilities {

		// TODO: Put strings into the resources.
		//
		public static string GetPathInfo(this IConfiguration config) {
			if (config is null)
				return null;
			else
				return $"Configuration:{((config as IConfigurationSection)?.Path ?? "Root").FmtStr().GNLI()}";
		}

		// TODO: Put strings into the resources.
		//
		public static IConfigurationSection RequireSection(this IConfiguration config, string name) {
			config.EnsureNotNull(nameof(config));
			name.EnsureNotNull(nameof(name)).EnsureNotEmpty();
			//
			var section = config.GetChildren().Where(locItem => locItem.Key.EqualsOrdinalCI(otherString: name)).SingleOrDefault();
			if (section is null)
				throw new EonException(message: $"Required configuration section is missing.{Environment.NewLine}\tSection name:{name.FmtStr().GNLI2()}{GetPathInfo(config: config).FmtStr().GNLI()}");
			else
				return section;
		}

		public static T Deserialize<T>(this IConfiguration config, IServiceProvider serviceProvider = default) {
			config.EnsureNotNull(nameof(config));
			//
			T result;
			using (var reader = ToJsonReader(config: config, serviceProvider: serviceProvider)) {
				var serializerSettings = EonJsonUtilities.CreateDefaultSerializerSettings();
				serializerSettings.TypeNameHandling = TypeNameHandling.All;
				serializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.All;
				var serializer = JsonSerializer.Create(settings: serializerSettings);
				result = serializer.Deserialize<T>(reader: reader.Value);
			}
			return result;
		}

		// TODO: Put strings into the resources.
		//
		public static T DeserializeJson<T>(this IConfiguration config, IServiceProvider serviceProvider = default, Func<JsonSerializerSettings, JsonSerializerSettings> configSettings = default) {
			config.EnsureNotNull(nameof(config));
			//
			T result;
			using (var reader = ToJsonReader(config: config, serviceProvider: serviceProvider)) {
				var serializerSettings = EonJsonUtilities.CreateDefaultSerializerSettings();
				serializerSettings.TypeNameHandling = TypeNameHandling.All;
				serializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.All;
				if (!(configSettings is null)) {
					serializerSettings = configSettings(arg: serializerSettings);
					if (serializerSettings is null)
						throw new EonException(message: $"The method has returned an invalid result '{serializerSettings.FmtStr().G()}'.{Environment.NewLine}\tMethod:{configSettings.FmtStr().GNLI2()}");
				}
				var serializer = JsonSerializer.Create(settings: serializerSettings);
				result = serializer.Deserialize<T>(reader: reader.Value);
			}
			return result;
		}

		public static IVh<JsonReader> ToJsonReader(this IConfiguration config, IServiceProvider serviceProvider = default) {
			config.EnsureNotNull(nameof(config));
			//
			const int writeStartObjectCtrlFlag = 1;
			const int writeEndObjectCtrlFlag = 2;
			const int writeStartArrayCtrlFlag = 3;
			const int writeEndArrayCtrlFlag = 4;
			const int writeNullCtrlFlag = 5;
			const int arrayContextCtrlFlag = 6;
			const int nodeType_Array = 1;
			const int nodeType_Object = 2;
			const int nodeType_Undefined = 0;
			//
			var configInfo = (config as IConfigurationSection)?.Path ?? "Root";
			var logger = serviceProvider?.GetService<ILogger<DefaultCategory>>();
			var buffer = default(MemoryStream);
			var bufferTextWriter = default(StreamWriter);
			var bufferJsonWriter = default(JsonTextWriter);
			var resultTextReader = default(StreamReader);
			var resultJsonReader = default(JsonTextReader);
			try {
				buffer = new MemoryStream(capacity: 81920);
				bufferTextWriter = new StreamWriter(stream: buffer, encoding: Encoding.UTF8, bufferSize: 81920, leaveOpen: true) { AutoFlush = false };
				bufferJsonWriter = new JsonTextWriter(textWriter: bufferTextWriter);
				var ctrl = new Stack<(IConfigurationSection section, int ctrlFlag)>(capacity: 1024);
				if (config is IConfigurationSection configAsSection) {
					if (int.TryParse(s: configAsSection.Key, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out _)) {
						ctrl.Push(item: (section: null, ctrlFlag: writeEndArrayCtrlFlag));
						ctrl.Push(item: (section: configAsSection, ctrlFlag: arrayContextCtrlFlag));
						ctrl.Push(item: (section: null, ctrlFlag: writeStartArrayCtrlFlag));
					}
					else if (configAsSection.Value is null) {
						ctrl.Push(item: (section: null, ctrlFlag: writeEndObjectCtrlFlag));
						foreach (var child in configAsSection.GetChildren().Reverse())
							ctrl.Push(item: (section: child, ctrlFlag: default));
						ctrl.Push(item: (section: null, ctrlFlag: writeStartObjectCtrlFlag));
					}
					else if (string.IsNullOrEmpty(configAsSection.Value))
						bufferJsonWriter.WriteNull();
					else
						bufferJsonWriter.WriteValue(value: configAsSection.Value);
				}
				else {
					ctrl.Push(item: (section: null, ctrlFlag: writeEndObjectCtrlFlag));
					foreach (var child in config.GetChildren().Reverse())
						ctrl.Push(item: (section: child, ctrlFlag: default));
					ctrl.Push(item: (section: null, ctrlFlag: writeStartObjectCtrlFlag));
				}
				while (ctrl.Count != 0) {
					var frame = ctrl.Pop();
					switch (frame.ctrlFlag) {
						case writeStartObjectCtrlFlag:
							bufferJsonWriter.WriteStartObject();
							break;
						case writeEndObjectCtrlFlag:
							bufferJsonWriter.WriteEndObject();
							break;
						case writeStartArrayCtrlFlag:
							bufferJsonWriter.WriteStartArray();
							break;
						case writeEndArrayCtrlFlag:
							bufferJsonWriter.WriteEndArray();
							break;
						case writeNullCtrlFlag:
							bufferJsonWriter.WriteNull();
							break;
						default:
							var section = frame.section;
							if (frame.ctrlFlag != arrayContextCtrlFlag)
								bufferJsonWriter.WritePropertyName(name: section.Key);
							if (section.Value is null) {
								var nodeType = nodeType_Undefined;
								var previousArrayIndex = int.MaxValue;
								foreach (var child in section.GetChildren().Reverse()) {
									switch (nodeType) {
										case nodeType_Undefined:
											if (int.TryParse(s: child.Key, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out var arrayInitialIndex)) {
												nodeType = nodeType_Array;
												previousArrayIndex = arrayInitialIndex;
												ctrl.Push(item: (section: null, ctrlFlag: writeEndArrayCtrlFlag));
												ctrl.Push(item: (section: child, ctrlFlag: arrayContextCtrlFlag));
											}
											else {
												nodeType = nodeType_Object;
												ctrl.Push(item: (section: null, ctrlFlag: writeEndObjectCtrlFlag));
												ctrl.Push(item: (section: child, ctrlFlag: default));
											}
											break;
										case nodeType_Array:
											if (int.TryParse(s: child.Key, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out var arrayIndex)) {
												if (arrayIndex < previousArrayIndex) {
													var skippedItemsCount = arrayIndex - previousArrayIndex - 1;
													for (var y = 0; y < skippedItemsCount; y++)
														ctrl.Push(item: (section: null, ctrlFlag: writeNullCtrlFlag));
													previousArrayIndex = arrayIndex;
													ctrl.Push(item: (section: child, ctrlFlag: arrayContextCtrlFlag));
												}
												else
													throw new EonException();
											}
											else
												throw new EonException();
											break;
										case nodeType_Object:
											ctrl.Push(item: (section: child, ctrlFlag: default));
											break;
									}
								}
								switch (nodeType) {
									case nodeType_Undefined:
										bufferJsonWriter.WriteNull();
										break;
									case nodeType_Array:
										ctrl.Push(item: (section: null, ctrlFlag: writeStartArrayCtrlFlag));
										break;
									case nodeType_Object:
										ctrl.Push(item: (section: null, ctrlFlag: writeStartObjectCtrlFlag));
										break;
								}
							}
							else if (string.IsNullOrEmpty(section.Value))
								bufferJsonWriter.WriteNull();
							else
								bufferJsonWriter.WriteValue(value: section.Value);
							break;
					}
				}
				//
				bufferJsonWriter.Flush();
				bufferTextWriter.Flush();
				buffer.Flush();
				//
				bufferJsonWriter.Close();
				bufferTextWriter.Close();
				bufferTextWriter.Dispose();
				//
				var jsonText = Encoding.UTF8.GetString(bytes: buffer.ToArray());
				logger
					?.LogDebug(
						eventId: GenericEventIds.ConfigSerialization,
						message: $"{typeof(ConfigurationUtilities).FullName}.{nameof(ToJsonReader)}({typeof(IConfiguration).FullName}, {typeof(IServiceProvider).FullName}): The specified configuration object successfully serialized to JSON.{Environment.NewLine}({configInfo}){Environment.NewLine}{{config_as_json}}",
						args: new object[ ] { jsonText });
				Debug
					.WriteLine(
						message: $"{typeof(ConfigurationUtilities).FullName}.{nameof(ToJsonReader)}({typeof(IConfiguration).FullName}, {typeof(IServiceProvider).FullName}): The specified configuration object successfully serialized to JSON.{Environment.NewLine}({configInfo}){Environment.NewLine}{jsonText}{Environment.NewLine}");
				//
				buffer.Position = 0L;
				resultTextReader = new StreamReader(stream: buffer, detectEncodingFromByteOrderMarks: false, encoding: Encoding.UTF8, bufferSize: 81920, leaveOpen: true);
				resultJsonReader = new JsonTextReader(reader: resultTextReader);
				return resultJsonReader.ToValueHolder(disposable1: resultTextReader, disposable2: buffer);
			}
			catch {
				resultJsonReader.Dispose(exception: null);
				resultTextReader.Dispose(exception: null);
				bufferJsonWriter.Dispose(exception: null);
				bufferTextWriter.Dispose(exception: null);
				buffer.Dispose(exception: null);
				throw;
			}
		}

	}

}