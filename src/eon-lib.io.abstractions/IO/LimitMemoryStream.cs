using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Eon.Threading.Tasks;

namespace Eon.IO {

	public class LimitMemoryStream
		:MemoryStream {

		readonly int _maxLength;

		readonly bool _throwOnExceedWrite;

		public LimitMemoryStream(int maxLength, bool throwOnExceedWrite) {
			maxLength
				.Arg(nameof(maxLength))
				.EnsureNotLessThanZero();
			//
			_maxLength = maxLength;
			_throwOnExceedWrite = throwOnExceedWrite;
		}

		public override long Position {
			get => base.Position;
			set {
				value
					.Arg(nameof(value))
					.EnsureNotGreaterThan(operand: _maxLength - 1);
				//
				base.Position = value;
			}
		}

		public override void Write(byte[ ] buffer, int offset, int count) {
			count = P_EnureAvailableLength(requestedLengthAddition: count);
			if (count > 0)
				base.Write(buffer, offset, count);
		}

		public override void WriteByte(byte value) {
			if (P_EnureAvailableLength(requestedLengthAddition: 1) == 1)
				base.WriteByte(value);
		}

		public override Task WriteAsync(byte[ ] buffer, int offset, int count, CancellationToken ct) {
			try {
				count = P_EnureAvailableLength(requestedLengthAddition: count);
				if (count > 0)
					return base.WriteAsync(buffer, offset, count, ct);
				else
					return TaskUtilities.FromVoidResult();
			}
			catch (Exception exception) {
				return TaskUtilities.FromError(error: exception);
			}
		}

		// TODO: Put strings into the resources.
		//
		int P_EnureAvailableLength(int requestedLengthAddition) {
			if (requestedLengthAddition < 1)
				return requestedLengthAddition;
			else {
				var availableAdditionLength = Math.Min(_maxLength - (int)Length, requestedLengthAddition);
				if (availableAdditionLength < 1 && _throwOnExceedWrite)
					throw new EonException(message: $"Невозможно выполнить запись в данный поток, так как длина потока достигла установленного предела.{Environment.NewLine}\tПредел:{_maxLength.ToString("d").FmtStr().GNLI2()}");
				return availableAdditionLength;
			}
		}

		// TODO: Put strings into the resources.
		//
		public override void SetLength(long value) {
			value
				.Arg(nameof(value))
				.EnsureNotGreaterThan(operand: int.MaxValue)
				.EnsureNotGreaterThan(operand: 0L);
			if (value > _maxLength) {
				if (_throwOnExceedWrite)
					throw new EonException(message: $"Невозможно установить требуемую длину потока, так как устанавливаемая длина больше установленного предела.{Environment.NewLine}\tУстанавливаемая длина:{value.ToString("d").FmtStr().GNLI2()}{Environment.NewLine}\tПредел:{_maxLength.ToString("d").FmtStr().GNLI2()}");
				else
					value = _maxLength;
			}
			base.SetLength(value);
		}

	}

}