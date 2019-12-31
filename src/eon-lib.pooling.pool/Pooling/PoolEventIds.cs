using Microsoft.Extensions.Logging;

namespace Eon.Pooling {

	public static class PoolEventIds {

		/// <summary>
		/// 50400, pool_take_existing.
		/// </summary>
		public static readonly EventId TakeExisting = new EventId(id: 50400, name: "pool_take_existing");

		/// <summary>
		/// 50401, pool_take_new.
		/// </summary>
		public static readonly EventId TakeNew = new EventId(id: 50401, name: "pool_take_new");

		/// <summary>
		/// 50402, pool_take_exhausted.
		/// </summary>
		public static readonly EventId TakeExhausted = new EventId(id: 50402, name: "pool_take_exhausted");

	}

}