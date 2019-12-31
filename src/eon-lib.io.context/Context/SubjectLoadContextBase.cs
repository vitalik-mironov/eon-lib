using System;
using System.Runtime.Serialization;
using System.Xml;

using Eon.ComponentModel.Dependencies;
using Eon.Net.Mime;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Context {
#pragma warning disable CS3001 // Argument type is not CLS-compliant

	/// <summary>
	/// Базовая реализация контекста какого-либо предмета загрузки (см. <seealso cref="ISubjectLoadContext"/>).
	/// </summary>
	public abstract class SubjectLoadContextBase
		:GenericContext, ISubjectLoadContext {

		protected SubjectLoadContextBase(Uri baseUri, string mediaType = default, Uri siteOrigin = default, XFullCorrelationId fullCorrelationId = default, ArgumentPlaceholder<XCorrelationId> correlationId = default, object localTag = default, IContext outerCtx = default)
			: base(fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, outerCtx: outerCtx) {
			//
			SubjectLoadContextProps.SubjectLoadBaseUriProp.SetLocalValue(ctx: this, baseUri.Arg(nameof(baseUri)));
			SubjectLoadContextProps.SubjectLoadSiteOriginProp.SetLocalValue(ctx: this, siteOrigin is null ? baseUri.Arg(nameof(baseUri)) : siteOrigin.Arg(nameof(baseUri)));
			SubjectLoadContextProps.SubjectLoadMediaTypeProp.SetLocalValue(ctx: this, value: mediaType ?? MediaTypeNameUtilities.AppOctetStream);
		}

		public Uri BaseUri
			=> this.SubjectLoadBaseUri();

		public Uri SiteOrigin
			=> this.SubjectLoadSiteOrigin();

		public string MediaType
			=> this.SubjectLoadMediaType();

		public virtual XmlObjectSerializer CreateXmlObjectSerializer(Type type)
			=> this.Dependencies().RequireService<IDataContractServiceProvider>().CreateDefaultXmlDataContractSerializer(type: type);

		public virtual JsonSerializer CreateJsonSerializer() {
			var result = this.Dependencies().RequireServiceNew<JsonSerializer>();
			result.TypeNameHandling = TypeNameHandling.Auto;
			result.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
			result.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
			return result;
		}

		public virtual XmlReaderSettings CreateXmlReaderSettings()
			=>
			new XmlReaderSettings() {
				CheckCharacters = true,
				CloseInput = false,
				ConformanceLevel = ConformanceLevel.Document,
				DtdProcessing = DtdProcessing.Prohibit,
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true
			};

		public override string ToString()
			=> base.ToString() + $"{Environment.NewLine}Base URI:{this.SubjectLoadBaseUri(disposeTolerant: true).FmtStr().GNLI()}" + $"{Environment.NewLine}Media type:{this.SubjectLoadMediaType(disposeTolerant: true).FmtStr().GNLI()}";

	}

#pragma warning restore CS3001 // Argument type is not CLS-compliant
}