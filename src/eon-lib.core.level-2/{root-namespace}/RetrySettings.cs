using System.Runtime.Serialization;

using Eon.Description;
using Eon.Runtime.Serialization;
using Eon.Threading;

namespace Eon {

	[DataContract]
	public class RetrySettings
		:AsReadOnlyValidatableBase, ISettings, IAsReadOnly<RetrySettings> {

		bool _isDisabled;

		int _maxCount;

		TimeoutDuration _interval;

		public RetrySettings()
			: this(isDisabled: default) { }

		public RetrySettings(bool isDisabled = default, int maxCount = default, TimeoutDuration interval = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			//
			_isDisabled = isDisabled;
			_maxCount = maxCount;
			_interval = interval;
		}

		public RetrySettings(RetrySettings other, bool isReadOnly = default)
			: this(isDisabled: other.EnsureNotNull(nameof(other)).Value.IsDisabled, maxCount: other.EnsureNotNull(nameof(other)).Value.MaxCount, interval: other.Interval, isReadOnly: isReadOnly) { }

		protected RetrySettings(SerializationContext ctx)
			: base(ctx: ctx) { }

		[DataMember(Order = 0, IsRequired = false)]
		public bool IsDisabled {
			get => _isDisabled;
			set {
				EnsureNotReadOnly();
				_isDisabled = value;
			}
		}

		[DataMember(Order = 1, IsRequired = true)]
		public int MaxCount {
			get => _maxCount;
			set {
				EnsureNotReadOnly();
				_maxCount = value;
			}
		}

		[DataMember(Order = 2, IsRequired = true)]
		public TimeoutDuration Interval {
			get => _interval;
			set {
				EnsureNotReadOnly();
				_interval = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out RetrySettings readOnlyCopy)
			=> readOnlyCopy = new RetrySettings(other: this, isReadOnly: true);

		public new RetrySettings AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				RetrySettings readOnlyCopy;
				CreateReadOnlyCopy(out readOnlyCopy);
				return readOnlyCopy;
			}
		}

		protected override void OnValidate() {
			MaxCount
				.ArgProp(nameof(MaxCount))
				.EnsureNotLessThan(-1);
			//
			Interval
				.ArgProp(nameof(Interval))
				.EnsureNotNull()
				.EnsureNotInfinite();
		}

	}

}