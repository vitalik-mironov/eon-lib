using Microsoft.Extensions.Logging;

namespace Eon.Diagnostics.Logging {

	public static class GenericEventIds {

		/// <summary>
		/// 50300, config_serialization.
		/// </summary>
		public static readonly EventId ConfigSerialization = new EventId(id: 50300, name: "config_serialization");

		/// <summary>
		/// 50301, explicit_dispose.
		/// </summary>
		public static readonly EventId ExplicitDispose = new EventId(id: 50301, name: "explicit_dispose");

	}

}