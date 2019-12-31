using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Threading {

	[DataContract]
	public sealed class TimeoutDuration
		:AsReadOnlyValidatableBase, IEquatable<TimeoutDuration>, IAsReadOnly<TimeoutDuration> {

		#region Static & constant members

		public const int SecondsMaxValue = 2147483;

		public const int MillisecondsMaxValue = int.MaxValue - 1;

		const int __MillisecondsMaxValuePlusOne = int.MaxValue;

		public static readonly TimeSpan InfiniteTimeSpan = TimeSpan.FromMilliseconds(-1.0D);

		public static readonly TimeoutDuration Infinite = new TimeoutDuration(milliseconds: Timeout.Infinite, isReadOnly: true, skipValidation: false);

		public static readonly TimeoutDuration Zero = new TimeoutDuration(milliseconds: 0, isReadOnly: true, skipValidation: false);

		public static readonly TimeoutDuration One = new TimeoutDuration(milliseconds: 1, isReadOnly: true, skipValidation: false);

		public static bool operator ==(TimeoutDuration timeoutDurationA, TimeoutDuration timeoutDurationB) {
			if (ReferenceEquals(timeoutDurationA, timeoutDurationB))
				return true;
			else if (ReferenceEquals(timeoutDurationA, null) || ReferenceEquals(timeoutDurationB, null))
				return false;
			else
				return timeoutDurationA._milliseconds == timeoutDurationB._milliseconds;
		}

		public static bool operator !=(TimeoutDuration timeoutDurationA, TimeoutDuration timeoutDurationB) {
			if (ReferenceEquals(timeoutDurationA, timeoutDurationB))
				return false;
			else if (timeoutDurationA is null || timeoutDurationB is null)
				return true;
			else
				return timeoutDurationA._milliseconds != timeoutDurationB._milliseconds;
		}

		public static bool Equals(TimeoutDuration timeoutDurationA, TimeoutDuration timeoutDurationB)
			=> timeoutDurationA == timeoutDurationB;

		// TODO: Put strings into the resources.
		//
		public static TimeoutDuration FromSeconds(int seconds) {
			seconds
				.Arg(nameof(seconds))
				.EnsureNotGreaterThan(operand: SecondsMaxValue);
			if (!TimeoutUtilities.IsValidTimeout(seconds))
				throw
					new ArgumentOutOfRangeException(paramName: nameof(seconds), message: $"Указанное значение '{seconds:d}' не представляет допустимое значение таймаута.");
			else if (seconds == Timeout.Infinite)
				return Infinite;
			else
				return
					new TimeoutDuration(isReadOnly: true) {
						_milliseconds = seconds * 1000
					};
		}

		// TODO: Put strings into the resources.
		//
		public static TimeoutDuration FromMilliseconds(int milliseconds) {
			milliseconds
				.Arg(nameof(milliseconds))
				.EnsureNotGreaterThan(operand: MillisecondsMaxValue);
			if (!TimeoutUtilities.IsValidTimeout(milliseconds))
				throw
					new ArgumentOutOfRangeException(paramName: nameof(milliseconds), message: $"Указанное значение '{milliseconds:d}' не представляет допустимое значение таймаута.");
			else if (milliseconds == Timeout.Infinite)
				return Infinite;
			else
				return
					new TimeoutDuration(isReadOnly: true) {
						_milliseconds = milliseconds
					};
		}

		public static TimeoutDuration FromTimeSpan(TimeSpan ts)
			=> FromTimeSpan(ts: ts.Arg(nameof(ts)));

		public static TimeoutDuration FromTimeSpan(ArgumentUtilitiesHandle<TimeSpan> ts) {
			if (ts.Value == InfiniteTimeSpan)
				return Infinite;
			else if (ts.Value < TimeSpan.Zero)
				throw
					new ArgumentOutOfRangeException(paramName: ts.Name, message: FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			var tsMilliseconds = Math.Ceiling(ts.Value.TotalMilliseconds);
			if (tsMilliseconds > MillisecondsMaxValue)
				throw
					new ArgumentOutOfRangeException(paramName: ts.Name, message: FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", TimeSpan.FromMilliseconds(MillisecondsMaxValue).ToString("c")));
			return
				new TimeoutDuration(isReadOnly: true) {
					_milliseconds = (int)tsMilliseconds
				};
		}

		public static TimeoutDuration Random(int minInclusiveMilliseconds, int maxExclusiveMilliseconds) {
			minInclusiveMilliseconds
				.Arg(nameof(minInclusiveMilliseconds))
				.EnsureNotLessThanZero();
			maxExclusiveMilliseconds
				.Arg(nameof(maxExclusiveMilliseconds))
				.EnsureGreaterThan(operand: minInclusiveMilliseconds)
				.EnsureNotGreaterThan(operand: __MillisecondsMaxValuePlusOne);
			//
			return new TimeoutDuration(milliseconds: RandomUtilities.Next(minInclusive: minInclusiveMilliseconds, maxExclusive: maxExclusiveMilliseconds), isReadOnly: true, skipValidation: false);
		}

		// TODO: Put strings into the resources.
		//
		public static bool TryParseMilliseconds(string millisecondsString, out TimeoutDuration result, out Exception error) {
			if (millisecondsString == null) {
				result = null;
				error = null;
				return true;
			}
			else {
				TimeoutDuration locResult;
				try {
					locResult = new TimeoutDuration(milliseconds: int.Parse(s: millisecondsString, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture), isReadOnly: true, skipValidation: false);
				}
				catch (Exception firstException) {
					result = null;
					error = new FormatException(message: $"Ошибка преобразования строки в объект '{nameof(TimeoutDuration)}'.{Environment.NewLine}\tСтрока:{millisecondsString.FmtStr().GNLI2()}", innerException: firstException);
					return false;
				}
				result = locResult;
				error = null;
				return true;
			}
		}

		#endregion

		int _milliseconds;
		// TODO: Put exception messages into the resources.
		//
		[DataMember(Order = 0, Name = nameof(Milliseconds), IsRequired = true)]
		int P_Milliseconds_DataMember {
			get => _milliseconds;
			set {
				if (!TimeoutUtilities.IsValidTimeout(value))
					throw
						new MemberDeserializationException(
							message: $"Указанное значение '{value:d}' не представляет допустимое значение таймаута.",
							type: GetType(),
							memberName: nameof(Milliseconds));
				_milliseconds = value;
			}
		}

		public TimeoutDuration()
			: base(isReadOnly: false) {
			_milliseconds = 0;
		}

		TimeoutDuration(bool isReadOnly)
			: base(isReadOnly: isReadOnly, isValid: true) {
			_milliseconds = Infinite._milliseconds;
		}

		public TimeoutDuration(int milliseconds)
			: this(milliseconds: milliseconds, isReadOnly: true, skipValidation: false) { }

		// TODO: Put exception messages into the resources.
		//
		public TimeoutDuration(int milliseconds, bool isReadOnly, bool skipValidation)
			: base(isReadOnly: isReadOnly, isValid: !skipValidation) {
			if (!skipValidation && !TimeoutUtilities.IsValidTimeout(milliseconds))
				throw
					new ArgumentOutOfRangeException(
						paramName: nameof(milliseconds),
						message: $"Указанное значение '{milliseconds:d}' не представляет допустимое значение таймаута.");
			//
			_milliseconds = milliseconds;
		}

		[JsonConstructor]
		TimeoutDuration(SerializationContext ctx)
			: base(ctx: ctx, isReadOnly: true, isValid: true) { }

		public int Milliseconds {
			get => _milliseconds;
			set {
				EnsureNotReadOnly();
				_milliseconds = value;
			}
		}

		public bool IsInfinite
			=> _milliseconds == Timeout.Infinite;

		// TODO: Put exception messages into the resources.
		//
		public TimeSpan TimeSpan {
			get {
				if (IsInfinite)
					return InfiniteTimeSpan;
				else
					return TimeSpan.FromMilliseconds(_milliseconds);
			}
		}

		public override string ToString() {
			if (IsInfinite)
				return FormatXResource(locator: typeof(TimeoutDuration), subpath: "ToString/Infinite");
			else
				return TimeSpan.ToString("c");
		}

		public override bool Equals(object obj) => Equals(obj as TimeoutDuration);

		public override int GetHashCode() => _milliseconds;

		public bool Equals(TimeoutDuration other) {
			if (other is null)
				return false;
			else
				return _milliseconds == other._milliseconds;
		}

		protected override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy)
			=> copy = new TimeoutDuration(milliseconds: _milliseconds, isReadOnly: true, skipValidation: true);

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			var ms = _milliseconds;
			if (!TimeoutUtilities.IsValidTimeout(timeout: ms))
				throw new ValidationException(message: $"Value '{ms:d}' of '{nameof(Milliseconds)}' property  is not a valid timeout value.");
		}

		public new TimeoutDuration AsReadOnly() {
			if (IsReadOnly)
				return this;
			else
				return new TimeoutDuration(milliseconds: _milliseconds, isReadOnly: true, skipValidation: true);
		}

	}

}