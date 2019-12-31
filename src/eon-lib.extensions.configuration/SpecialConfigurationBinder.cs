using System;

using Eon;

namespace Microsoft.Extensions.Configuration {

#if DEBUG

	public class SpecialConfigurationBinder {

		public T Bind<T>(IConfiguration config) {
			config.EnsureNotNull(nameof(config));
			//
			throw new NotImplementedException();
		}

	}

#endif

}