using System;
using System.Net;
using System.Net.Http;

namespace Eon {

	public static partial class FormatStringUtilitiesNetHttp {

		public static string G(this FormatStringUtilitiesHandle<WebExceptionStatus> hnd)
			=> $"{hnd.Object.ToString("d")} ({hnd.Object.ToString("g")})";

		public static string G(this FormatStringUtilitiesHandle<HttpStatusCode> hnd)
			=> $"{hnd.Object.ToString("d")} ({hnd.Object.ToString("g")})";

		public static string Status(this FormatStringUtilitiesHandle<HttpResponseMessage> hnd)
			=> hnd.Object is null ? FormatStringUtilitiesCoreL0.GetNullValueText() : $"Статус-код:{Environment.NewLine}{hnd.Object.StatusCode.FmtStr().G().IndentLines()}{Environment.NewLine}Сообщение ({nameof(hnd.Object.ReasonPhrase)}):{hnd.Object.ReasonPhrase.FmtStr().GNLI()}";

	}

}