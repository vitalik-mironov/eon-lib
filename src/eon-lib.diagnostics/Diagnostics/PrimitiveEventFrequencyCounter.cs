using System;
using System.Collections.Immutable;
using System.Threading;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Diagnostics {

	/// <summary>
	/// Special rate (frequency) counter.
	/// </summary>
	public sealed class PrimitiveEventFrequencyCounter {

		#region Static members

		/// <summary>
		/// Value: '00:00:01'.
		/// </summary>
		public static readonly TimeSpan MinTimeUnit = TimeSpan.FromSeconds(value: 1.0D);

		/// <summary>
		/// Value: '262144'.
		/// </summary>
		static readonly int __MaxBufferSize = 262144;

		#endregion

		readonly TimeSpan _timeUnit;

		ImmutableList<TimeSpan> _eventRiseTimepoints;

		/// <summary>
		/// Initializes a new instance of <see cref="PrimitiveEventFrequencyCounter"/>.
		/// <para>As a default value of <see cref="TimeUnit"/> <see cref="MinTimeUnit"/> used.</para>
		/// </summary>
		public PrimitiveEventFrequencyCounter()
			: this(timeUnit: MinTimeUnit) { }

		/// <summary>
		/// Initializes a new instance of <see cref="PrimitiveEventFrequencyCounter"/>.
		/// </summary>
		/// <param name="timeUnit">
		/// The time interval during which the occurrences (events) is counted.
		/// <para>Can't be less than <see cref="MinTimeUnit"/>.</para>
		/// </param>
		public PrimitiveEventFrequencyCounter(TimeSpan timeUnit) {
			timeUnit.Arg(nameof(timeUnit)).EnsureNotLessThan(operand: MinTimeUnit);
			//
			_timeUnit = timeUnit;
			_eventRiseTimepoints = ImmutableList<TimeSpan>.Empty;
		}

		/// <summary>
		/// Gets the time interval during which the occurrences (events) is counted.
		/// </summary>
		public TimeSpan TimeUnit
			=> _timeUnit;

		/// <summary>
		/// Registers occurrence (event).
		/// </summary>
		public void RegisterEvent() {
			var timepoint = StopwatchUtilities.GetTimestampAsTimeSpan();
			for (; ; ) {
				ImmutableList<TimeSpan> newVersion;
				var originalVersion = itrlck.Get(ref _eventRiseTimepoints);
				//
				if (originalVersion.Count > __MaxBufferSize)
					originalVersion = P_Cleanup(timepoint);
				//
				if (originalVersion.Count > 0) {
					var upperBound = originalVersion.Count - 1;
					newVersion = originalVersion;
					for (var y = upperBound; y > -1; y--) {
						if (timepoint >= originalVersion[ y ]) {
							if (y == upperBound)
								newVersion = originalVersion.Add(value: timepoint);
							else
								newVersion = originalVersion.Insert(index: y, item: timepoint);
							break;
						}
					}
				}
				else
					newVersion = originalVersion.Add(timepoint);
				//
				if (!ReferenceEquals(newVersion, originalVersion)) {
					if (ReferenceEquals(originalVersion, Interlocked.CompareExchange(ref _eventRiseTimepoints, value: newVersion, comparand: originalVersion)))
						break;
					else
						Thread.Sleep(1); // Выполнение любого готового к выполнению потока в системе (ОС).
				}
			}
		}

		ImmutableList<TimeSpan> P_Cleanup(TimeSpan timepoint) {
			// TODO: Учет макс. размера буфера (__MaxBufferSize) — макс. значение «скорости».
			//
			var minTimepoint = timepoint.Subtract(_timeUnit);
			ImmutableList<TimeSpan> result;
			for (; ; ) {
				ImmutableList<TimeSpan> newVersion;
				var originalVersion = itrlck.Get(ref _eventRiseTimepoints);
				if (originalVersion.Count > 0) {
					// Поскольку в списке _eventRisingTimepoints все элементы добавляются в порядке их возрастания, то можно воспользоваться бинарным поиском.
					//
					var removeUntilIndex = originalVersion.BinarySearch(item: minTimepoint);
					if (removeUntilIndex < 0) {
						removeUntilIndex = ~removeUntilIndex;
						if (removeUntilIndex == originalVersion.Count)
							// В списке нет элемента, больше, чем  указанный minTimepoint. -1 — очистить список полностью.
							//
							removeUntilIndex = -1;
					}
					//
					ImmutableList<TimeSpan> exchangeResult;
					if (removeUntilIndex == -1) {
						newVersion = ImmutableList<TimeSpan>.Empty;
						exchangeResult = Interlocked.CompareExchange(ref _eventRiseTimepoints, value: newVersion, comparand: originalVersion);
					}
					else if (removeUntilIndex > 0) {
						newVersion = originalVersion.RemoveRange(index: 0, count: removeUntilIndex);
						exchangeResult = Interlocked.CompareExchange(ref _eventRiseTimepoints, value: newVersion, comparand: originalVersion);
					}
					else {
						newVersion = originalVersion;
						exchangeResult = originalVersion;
					}
					//
					if (ReferenceEquals(originalVersion, exchangeResult)) {
						result = newVersion;
						break;
					}
				}
				else {
					result = originalVersion;
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Gets current rate (frequency) value.
		/// </summary>
		public int Frequency() {
			var nowTimepoint = StopwatchUtilities.GetTimestampAsTimeSpan();
			return P_Cleanup(timepoint: nowTimepoint).Count;
		}

		/// <summary>
		/// Gets current rate (frequency) value converted converted to specifed time unit.
		/// </summary>
		/// <param name="unit">
		/// Time unit.
		/// </param>
		public double Frequency(TimeUnit unit) {
			var f = Frequency();
			switch (unit) {
				case Eon.TimeUnit.Tick:
					return f == 0 ? 0.0D : (double)f / _timeUnit.Ticks;
				case Eon.TimeUnit.Millisecond:
					return f == 0 ? 0.0D : f / _timeUnit.TotalMilliseconds;
				case Eon.TimeUnit.Second:
					return f == 0 ? 0.0D : f / _timeUnit.TotalSeconds;
				case Eon.TimeUnit.Minute:
					return f == 0 ? 0.0D : f / _timeUnit.TotalMinutes;
				case Eon.TimeUnit.Hour:
					return f == 0 ? 0.0D : f / _timeUnit.TotalHours;
				case Eon.TimeUnit.Day:
					return f == 0 ? 0.0D : f / _timeUnit.TotalDays;
				default:
					throw new ArgumentOutOfRangeException(message: $"Value is not supported.{Environment.NewLine}\tValue:{unit.FmtStr().GNLI2()}", paramName: nameof(unit)).SetErrorCode(code: GeneralErrorCodes.Operation.Params.Illegal);
			}
		}

		/// <summary>
		/// Clear all registered occurrences (events).
		/// </summary>
		public void Reset()
			=> Interlocked.Exchange(ref _eventRiseTimepoints, ImmutableList<TimeSpan>.Empty);



	}

}