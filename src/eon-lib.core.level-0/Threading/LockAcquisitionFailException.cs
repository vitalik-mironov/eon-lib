using System;

namespace Eon.Threading {

	public class LockAcquisitionFailException
		:InvalidOperationException {

		readonly LockAcquisitionFailReason _reason;

		readonly object _resource;

		public LockAcquisitionFailException()
			: this(resource: null, reason: LockAcquisitionFailReason.Unknown, innerException: null) { }

		public LockAcquisitionFailException(LockAcquisitionFailReason reason)
			: this(resource: null, reason: reason, innerException: null) { }

		public LockAcquisitionFailException(string message, object resource, LockAcquisitionFailReason reason)
			: this(message: message, resource: resource, reason: reason, innerException: null) { }

		public LockAcquisitionFailException(object resource, LockAcquisitionFailReason reason)
			: this(resource: resource, reason: reason, innerException: null) { }

		public LockAcquisitionFailException(object resource, LockAcquisitionFailReason reason, Exception innerException)
			: this(message: null, resource: resource, reason: reason, innerException: innerException) { }

		public LockAcquisitionFailException(string message)
			: this(message: message, resource: null, reason: LockAcquisitionFailReason.Unknown, innerException: null) { }

		// TODO: Put strings into the resources.
		//
		public LockAcquisitionFailException(string message, object resource, LockAcquisitionFailReason reason, Exception innerException)
			: base(
					message: $"Lock acquisition operation failed.{Environment.NewLine}\tResource:{Environment.NewLine}\t\t{resource}{Environment.NewLine}\tReason:{Environment.NewLine}\t\t{reason}.{(string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine + message)}",
					innerException: innerException) {
			//
			_reason = reason;
			_resource = resource;
		}

		public LockAcquisitionFailReason Reason
			=> _reason;

		public object Resource
			=> _resource;

	}

}