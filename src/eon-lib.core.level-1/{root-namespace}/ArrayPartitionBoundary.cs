using System;
using System.Diagnostics;

namespace Eon {

	[DebuggerDisplay("{ToString(),nq}")]
	public readonly struct ArrayPartitionBoundary {

		#region Static members

		// TODO: Put strings into the resources.
		//
		public static ArrayPartitionBoundary Get(int arrayLength, ArgumentUtilitiesHandle<int?> offset, ArgumentUtilitiesHandle<int?> count) {
			arrayLength
				.Arg(nameof(arrayLength))
				.EnsureNotLessThan(0);
			var locOffset =
				offset
				.EnsureNotLessThan(0)
				.EnsureNotGreaterThan(
					operand: arrayLength - 1,
					exceptionMessageFirstLineFactory:
						locHnd => $"Указанное смещение не соответствует длине массива (или к.л. другой последовательности).{Environment.NewLine}\tДлина массива:{Environment.NewLine}{arrayLength.ToString("d").FmtStr().GNLI2()}{Environment.NewLine}\tУказанное смещение:{Environment.NewLine}{locHnd.Value.Value.ToString("d").FmtStr().GNLI2()}")
				.Value
				?? 0;
			var maxCount = arrayLength - locOffset;
			var locCount =
				count
				.EnsureNotLessThan(0)
				.EnsureNotGreaterThan(
					operand: maxCount,
					exceptionMessageFirstLineFactory:
						locHnd => $"Указанная длина сегмента массива не соответствует указанному смещению и всей длине массива (или к.л. другой последовательности).{Environment.NewLine}\tДлина массива:{Environment.NewLine}{arrayLength.ToString("d").FmtStr().GNLI2()}{Environment.NewLine}\tУказанное смещение:{Environment.NewLine}{locOffset.ToString("d").FmtStr().GNLI2()}")
				.Value
				??
				maxCount;
			//
			return
				new ArrayPartitionBoundary(
					offset: locOffset,
					count: locCount);
		}

		public static ArrayPartitionBoundary Get(int arrayLength, int? offset = null, int? count = null)
			=> Get(arrayLength: arrayLength, offset: offset.Arg(nameof(offset)), count: count.Arg(nameof(count)));

		#endregion

		public readonly int Offset;

		public readonly int Count;

		public ArrayPartitionBoundary(int offset, int count)
			: this(offset: offset.Arg(nameof(offset)), count: count.Arg(nameof(count))) { }

		public ArrayPartitionBoundary(ArgumentUtilitiesHandle<int> offset, ArgumentUtilitiesHandle<int> count) {
			offset.EnsureNotLessThan(0);
			count.EnsureNotLessThan(0);
			//
			Offset = offset.Value;
			Count = count.Value;
		}

		// TODO: Put strings into the resources.
		//
		public ArrayPartitionBoundary Take(int count) {
			count.Arg(nameof(count)).EnsureNotLessThanZero();
			//
			if (count > Count)
				throw
					new ArgumentOutOfRangeException(paramName: nameof(count), message: $"Значение не может быть больше, чем размер секции (св-во '{nameof(Count)}').{Environment.NewLine}\tЗначение:{count.ToString("d").FmtStr().GNLI2()}{Environment.NewLine}\tРазмер секции:{Count.ToString("d").FmtStr().GNLI2()}");
			else
				return new ArrayPartitionBoundary(offset: Offset, count: count);
		}

		// TODO: Put strings into the resources.
		//
		public ArrayPartitionBoundary Skip(int count) {
			count.Arg(nameof(count)).EnsureNotLessThanZero();
			//
			if (count > Count)
				throw
					new ArgumentOutOfRangeException(paramName: nameof(count), message: $"Значение не может быть больше, чем размер секции (св-во '{nameof(Count)}').{Environment.NewLine}\tЗначение:{count.ToString("d").FmtStr().GNLI2()}{Environment.NewLine}\tРазмер секции:{Count.ToString("d").FmtStr().GNLI2()}");
			else
				checked {
					return new ArrayPartitionBoundary(offset: Offset + count, count: Count - count);
				}
		}

		public override string ToString()
			=> $"offset: {Offset:d}, count: {Count:d}";

	}

}