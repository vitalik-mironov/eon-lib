using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Eon.Net.Http {

	public class CustomDelegatingHandler
		:DelegatingHandler {

		public CustomDelegatingHandler()
			: this(innerHandler: null) { }

		public CustomDelegatingHandler(HttpMessageHandler innerHandler) {
			if (!(innerHandler is null))
				InnerHandler = innerHandler;
		}

		// TODO: Put strings into the resources.
		//
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
			=> InnerHandler is null ? throw new EonException(message: $"Inner handler not set for this delegating handler.{Environment.NewLine}\tHandler:{this.FmtStr().GNLI2()}") : await base.SendAsync(request: request, cancellationToken: ct).ConfigureAwait(false);

	}

}