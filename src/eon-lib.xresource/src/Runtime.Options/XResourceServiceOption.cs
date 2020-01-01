using Eon.Resources.XResource;

namespace Eon.Runtime.Options {

	/// <summary>
	/// Defines EON runtime option of <see cref="IXResourceService"/> (see <see cref="XResourceUtilities.Service"/>).
	/// </summary>
	public sealed class XResourceServiceOption
		:RuntimeOptionBase {

		#region Static members

		public static readonly XResourceServiceOption Fallback;

		static XResourceServiceOption() {
			Fallback = new XResourceServiceOption(service: DefaultXResourceService.Instance);
			RuntimeOptions.Option<XResourceServiceOption>.SetFallback(option: Fallback);
		}

		public static IXResourceService Require()
			=> RuntimeOptions.Option<XResourceServiceOption>.Require().Service;

		#endregion

		readonly IXResourceService _service;

		public XResourceServiceOption(IXResourceService service) {
			service.EnsureNotNull(nameof(service));
			//
			_service = service;
		}

		public IXResourceService Service
			=> _service;

	}

}