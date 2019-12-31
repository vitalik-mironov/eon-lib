using System;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Xml {

	public static class XmlUtilities {

		public const string XmlSchemaTypesNamespace = "http://www.w3.org/2001/XMLSchema";

		public const string XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";

		public const string TempUriXmlNamespace = "http://www.tempuri.org";

		static readonly PositiveInt32Generator __XmlNamespacePrefixPostfixGenerator = new PositiveInt32Generator(name: $"{nameof(XmlUtilities)}.{nameof(__XmlNamespacePrefixPostfixGenerator)}", maxInclusiveValue: 9999);

		public static XmlWriterSettings CreateXmlWriterDefaultSettings(bool closeOutput = true, bool disableFormatting = true) {
			var result = new XmlWriterSettings();
			result.ConformanceLevel = ConformanceLevel.Document;
			result.Encoding = Encoding.UTF8;
			result.CloseOutput = closeOutput;
			result.NamespaceHandling = NamespaceHandling.OmitDuplicates;
			result.OmitXmlDeclaration = false;
			result.NewLineOnAttributes = false;
			if (disableFormatting)
				result.Indent = false;
			else {
				result.Indent = true;
				result.IndentChars = "\x0009";
			}
			return result;
		}

		public static XmlReaderSettings CreateXmlReaderDefaultSettings(bool closeInput = true) {
			var result = new XmlReaderSettings();
			//
			result.CloseInput = closeInput;
			result.IgnoreComments = true;
			result.IgnoreProcessingInstructions = true;
			result.IgnoreWhitespace = true;
			//
			return result;
		}

		public static XmlReaderSettings CopyWith(this XmlReaderSettings originalSettings, bool? closeInput = default) {
			originalSettings.EnsureNotNull(nameof(originalSettings));
			//
			var copy = originalSettings.Clone();
			if (closeInput.HasValue)
				copy.CloseInput = closeInput.Value;
			//
			return copy;
		}

		public static void LookupOrWriteXmlNamespacePrefix(XmlWriter writer, XNamespace @namespace, out string prefix) {
			writer.EnsureNotNull(nameof(writer));
			@namespace.EnsureNotNull(nameof(@namespace));
			//
			if (string.IsNullOrEmpty(@namespace.NamespaceName))
				prefix = string.Empty;
			else {
				prefix = writer.LookupPrefix(@namespace.NamespaceName);
				if (prefix == null) {
					if (writer.WriteState != WriteState.Element)
						throw new EonException(FormatXResource(typeof(XmlWriter), "ExceptionMessages/InvalidWriteState/Expected", WriteState.Element));
					var generatedPrefix = GenerateXmlNamespacePrefix(writer, @namespace);
					writer.WriteAttributeString("xmlns", generatedPrefix, null, @namespace.NamespaceName);
					prefix = generatedPrefix;
				}
			}
		}

		public static string GenerateXmlNamespacePrefix(XmlWriter writer, XNamespace @namespace) {
			writer.EnsureNotNull(nameof(writer));
			@namespace.EnsureNotNull(nameof(@namespace));
			//
			string prefix;
			if (string.IsNullOrEmpty(@namespace.NamespaceName))
				prefix = string.Empty;
			else {
				prefix = writer.LookupPrefix(@namespace.NamespaceName);
				if (prefix is null) {
					if (string.Equals(@namespace.NamespaceName, XmlSchemaInstanceNamespace, StringComparison.Ordinal))
						prefix = "i";
					else {
						int generatedPrefixPostfix;
						if (!__XmlNamespacePrefixPostfixGenerator.TryNext(out generatedPrefixPostfix)) {
							__XmlNamespacePrefixPostfixGenerator.Reset();
							generatedPrefixPostfix = __XmlNamespacePrefixPostfixGenerator.Next();
						}
						prefix = $"ns{generatedPrefixPostfix.ToString("d", CultureInfo.InvariantCulture)}";
					}
				}
			}
			return prefix;
		}

	}

}