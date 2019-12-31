using System;

namespace Eon.ComponentModel {

	public sealed class RunControlAttemptSuccess
		:RunControlAttemptPropertiesBase, IRunControlAttemptSuccess {

		internal RunControlAttemptSuccess(IRunControlAttemptProperties props, TimeSpan duration, bool isMaster)
			: base(props: props) {
			if (duration < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(paramName: nameof(duration));
			//
			Duration = duration;
			IsMaster = isMaster;
		}

		internal RunControlAttemptSuccess(IRunControlAttemptSuccess other, ArgumentPlaceholder<bool> isMaster = default)
			: this(props: other.EnsureNotNull(nameof(other)).Value, duration: other.Duration, isMaster: isMaster.Substitute(value: other.IsMaster)) { }

		public TimeSpan Duration { get; }

		public bool IsMaster { get; }

	}

}