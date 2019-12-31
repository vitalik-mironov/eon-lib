using System;

namespace Eon.Diagnostics {

	public interface IEnvironmentStateSnapshot {

		int? AppDomainId { get; }

		int? ManagedThreadId { get; }

		int? ProcessId { get; }

		string ProcessName { get; }

		DateTimeOffset? ProcessStartTime { get; }

		long? GCTotalMemory { get; }

		long? XDisposableCount { get; }

		string UserIdentityName { get; }

		TimeSpan? OSUpTime { get; }

	}

}