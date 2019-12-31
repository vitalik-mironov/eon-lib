using System;

namespace Eon.Threading {

	public class LockReleaseFailException
		:InvalidOperationException {

		readonly LockReleaseFailReason _reason;

		readonly object _resource;

		public LockReleaseFailException()
			: this(resource: null, reason: LockReleaseFailReason.Unknown, innerException: null) { }

		public LockReleaseFailException(LockReleaseFailReason reason)
			: this(resource: null, reason: reason, innerException: null) { }

		public LockReleaseFailException(object resource, LockReleaseFailReason reason)
			: this(resource: resource, reason: reason, innerException: null) { }

		// TODO: Put strings into the resources.
		//
		public LockReleaseFailException(object resource, LockReleaseFailReason reason, Exception innerException)
			: base(
					message: $"Lock release operation failed.{Environment.NewLine}\tResource:{Environment.NewLine}\t\t{resource}{Environment.NewLine}\tReason:{Environment.NewLine}\t\t{reason}.",
					innerException: innerException) {
			//
			_reason = reason;
			_resource = resource;
		}

		public LockReleaseFailReason Reason => _reason;

		public object Resource => _resource;

	}

}