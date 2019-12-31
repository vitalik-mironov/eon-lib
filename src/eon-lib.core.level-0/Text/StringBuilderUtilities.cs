using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eon.Collections;
using Eon.Runtime.Options;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Text {

	public static partial class StringBuilderUtilities {

		#region Nested types

		public readonly struct AcquiredStringBuilderCookie
			:IDisposable {

			readonly bool _unlimCapacity;

			readonly StringBuilder _sb;

			internal AcquiredStringBuilderCookie(StringBuilder sb, bool unlimCapacity) {
				if (sb is null)
					throw new ArgumentNullException(paramName: nameof(sb));
				//
				_unlimCapacity = unlimCapacity;
				_sb = sb;
			}

			public StringBuilder StringBuilder
				=> _sb;

			public void Dispose() {
				if (_unlimCapacity) {
					_sb.Clear();
					_sb.Capacity = 0;
				}
				else
					P_ReleaseBuffer(cookie: this);
			}

		}

		#endregion

		/// <summary>
		/// Gets the current settings of <see cref="StringBuilder"/>-buffer set that specified by <see cref="StringBuilderCacheSettingsOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static IStringBuilderBufferSetSettings BufferSettings
			=> StringBuilderCacheSettingsOption.Require();

		static ImmutableStack<ImmutableStackEntry<StringBuilder>> __SbCache;

		static readonly int __SbCacheMaxSizeMinusOne;

		static StringBuilderUtilities() {
			__SbCache = ImmutableStack<ImmutableStackEntry<StringBuilder>>.Empty;
			__SbCacheMaxSizeMinusOne = BufferSettings.BufferSetMaxSize.Value - 1;
		}

		public static AcquiredStringBuilderCookie AcquireBuffer() {
			var updateResult =
				itrlck
				.Update(
					location: ref __SbCache,
					transform:
						(locCurrent) => {
							if (locCurrent.IsEmpty)
								return locCurrent;
							else {
								return locCurrent.Pop();
							}
						});
			if (updateResult.IsUpdated)
				return new AcquiredStringBuilderCookie(sb: updateResult.Original.Peek().Value, unlimCapacity: false);
			else
				return new AcquiredStringBuilderCookie(sb: new StringBuilder(capacity: BufferSettings.BufferMinCapacity.Value, maxCapacity: BufferSettings.BufferMaxCapacity.Value), unlimCapacity: false);
		}

		static void P_ReleaseBuffer(AcquiredStringBuilderCookie cookie) {
			var sb = cookie.StringBuilder.ArgProp($"{nameof(cookie)}.{nameof(cookie.StringBuilder)}").EnsureNotNull().Value;
			//
			sb.Clear();
			var updateResult =
				itrlck
				.Update(
					location: ref __SbCache,
					transform:
						(locCurrent) => {
							int locPeekedPosition;
							if (locCurrent.IsEmpty)
								return locCurrent.Push(new ImmutableStackEntry<StringBuilder>(value: sb, position: 0));
							else if ((locPeekedPosition = locCurrent.Peek().Position) < __SbCacheMaxSizeMinusOne) {
								return locCurrent.Push(new ImmutableStackEntry<StringBuilder>(value: sb, position: locPeekedPosition + 1));
							}
							else
								return locCurrent;
						});
			if (!updateResult.IsUpdated)
				sb.Capacity = 0;
		}

		public static AcquiredStringBuilderCookie AcquireBufferUnlimCapacity()
			=> new AcquiredStringBuilderCookie(sb: new StringBuilder(capacity: BufferSettings.BufferMinCapacity.Value), unlimCapacity: true);

		// TODO: Put strings into the resources.
		//
		public static char Last(this StringBuilder sb, int offset = default) {
			if (sb is null)
				throw new ArgumentNullException(paramName: nameof(sb));
			else if (offset < 0)
				throw new ArgumentOutOfRangeException(paramName: nameof(offset), message: $"Value cannot be less than zero.{Environment.NewLine}\tValue:{Environment.NewLine}\t\t{offset:d}");
			//
			if (sb.Length == 0)
				throw new ArgumentException(paramName: nameof(sb), message: "Cannot be empty.");
			else if (sb.Length - 1 - offset < 0)
				throw new IndexOutOfRangeException();
			else
				return sb[ sb.Length - 1 - offset ];
		}

		public static void NewLineThenAppend(this StringBuilder sb, string value) {
			if (sb is null)
				throw new ArgumentNullException(paramName: nameof(sb));
			//
			if (sb.Length > 0)
				sb.Append(value: Environment.NewLine);
			sb.Append(value);
		}

		// TODO: Put exception messages into the resources.
		//
		/// <summary>
		/// Добавляет в конец изменяемой строки сначала символы перевода строки (<seealso cref="Environment.NewLine"/>), а затем указанное содержимое в соответствии с указанным форматом.
		/// <para>Данный метод операцию добавления выполняет с использованием распараллеливания. Т.е. исходный массив данных <paramref name="source"/> разделяется на несколько секций и каждая секция обрабатывается в отдельном потоке выполнения.</para>
		/// </summary>
		/// <typeparam name="T">Тип элемента содержимого.</typeparam>
		/// <param name="output">Изменяемая строка вывода.</param>
		/// <param name="source">Данные содержимого.</param>
		/// <param name="format">Функция, форматирующая вывод каждого элемента содержимого.</param>
		public static void AppendNewLineThen<T>(this StringBuilder output, T[ ] source, Func<T, string> format) {
			var maxDop = Math.Min(Environment.ProcessorCount, int.MaxValue / 3);
			//
			if (output is null)
				throw new ArgumentNullException(paramName: nameof(output));
			else if (format is null)
				throw new ArgumentNullException(paramName: nameof(format));
			//
			int sourceLength;
			if (source is null)
				return;
			else if (source.GetLowerBound(0) < 0)
				throw new NotSupportedException($"Array with negative lower bound is not supported.{Environment.NewLine}\tLower bound:{Environment.NewLine}\t\t{source.GetLowerBound(0):d}");
			else if ((sourceLength = source.Length) < 1)
				return;
			else if (sourceLength < maxDop * maxDop * maxDop) {
				for (var i = 0; i < sourceLength; i++) {
					output.Append(Environment.NewLine);
					output.Append(format(source[ i ]));
				}
				return;
			}
			var partitioner = Partitioner.Create(fromInclusive: 0, toExclusive: sourceLength, rangeSize: Math.Max(sourceLength / maxDop, maxDop));
			var newLine = Environment.NewLine;
			var buffer = new ConcurrentDictionary<Tuple<int, int>, char[ ]>();
			Parallel
				.ForEach(
					source: partitioner,
					parallelOptions:
						new ParallelOptions() {
							MaxDegreeOfParallelism = maxDop,
							TaskScheduler = TaskScheduler.Current ?? TaskScheduler.Default
						},
					body:
						(locPartition, locLoopState) => {
							if (locLoopState.IsExceptional || locLoopState.ShouldExitCurrentIteration)
								return;
							using (var locBuffer = AcquireBufferUnlimCapacity()) {
								var locSb = locBuffer.StringBuilder;
								for (var i = locPartition.Item1; i < locPartition.Item2; i++) {
									locSb.Append(newLine);
									locSb.Append(format(source[ i ]));
								}
								var locSbChars = locSb.ToCharArray();
								buffer
									.AddOrUpdate(
										key: locPartition,
										addValue: locSbChars,
										updateValueFactory: (locKey, locCurrentValue) => locCurrentValue);
							}
						});
			foreach (var key in buffer.Keys.OrderBy(i => i.Item1))
				output.Append(buffer[ key ]);
		}

		public static char[ ] ToCharArray(this StringBuilder sb) {
			if (sb is null)
				return null;
			else if (sb.Length > 0) {
				var charArray = new char[ sb.Length ];
				sb.CopyTo(sourceIndex: 0, destination: charArray, destinationIndex: 0, count: sb.Length);
				return charArray;
			}
			else
				return new char[ ] { };
		}

		public static void Append(this StringBuilder sb, StringBuilder value) {
			if (sb is null)
				throw new ArgumentNullException(paramName: nameof(sb));
			//
			if (value?.Length > 0) {
				var buffer = new char[ value.Length ];
				value.CopyTo(0, buffer, 0, value.Length);
				sb.Append(buffer, 0, buffer.Length);
			}
		}

		public static void AppendTerminating(this StringBuilder sb, char terminator) {
			if (sb?.Length > 0 && sb[ sb.Length - 1 ] != terminator)
				sb.Append(terminator);
		}

	}

}