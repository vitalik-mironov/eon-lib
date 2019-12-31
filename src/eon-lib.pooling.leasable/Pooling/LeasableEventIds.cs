using Microsoft.Extensions.Logging;

namespace Eon.Pooling {

	public static class LeasableEventIds {

		/// <summary>
		/// 50200, lease_start.
		/// </summary>
		public static readonly EventId LeaseStart = new EventId(id: 50200, name: "lease_start");

		/// <summary>
		/// 50201, lease_end.
		/// </summary>
		public static readonly EventId LeaseEnd = new EventId(id: 50201, name: "lease_end");

	}

}