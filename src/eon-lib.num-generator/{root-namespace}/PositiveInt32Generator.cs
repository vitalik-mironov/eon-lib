using System;
using System.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public sealed class PositiveInt32Generator {

		#region Static & constant members

		public const int MinInclusiveValue = 1;

		public static readonly PositiveInt32Generator Default = new PositiveInt32Generator(name: $"{nameof(PositiveInt32Generator)}.{nameof(Default)}", maxInclusiveValue: 1);

		#endregion

		readonly int _maxInclusiveValue;

		int _valueCounter;

		readonly string _name;

		public PositiveInt32Generator()
			: this((string)null) { }

		public PositiveInt32Generator(string name)
			: this(name, int.MaxValue) { }

		public PositiveInt32Generator(string name, int maxInclusiveValue) {
			if (maxInclusiveValue < MinInclusiveValue)
				throw new ArgumentOutOfRangeException(paramName: nameof(maxInclusiveValue), message: FormatXResource(locator: typeof(ArgumentOutOfRangeException), subpath: "CanNotLessThan", args: MinInclusiveValue.ToString("d")));
			//
			_maxInclusiveValue = maxInclusiveValue;
			_valueCounter = maxInclusiveValue;
			_name = name;
		}

		public string Name => _name;

		public bool TryNext(out int value)
			=> P_TryNext(value: out value, error: out _);

		public int Next() {
			if (P_TryNext(value: out var value, error: out var error))
				return value;
			else
				throw error;
		}

		bool P_TryNext(out int value, out Exception error) {
			int valueCounterResult;
			if ((valueCounterResult = Interlocked.Add(ref _valueCounter, 0)) == 0 || (valueCounterResult & 0x80000000) == 0x80000000) {
				value = default;
				error = new EonException(FormatXResource(typeof(PositiveInt32Generator), "ExceptionMessages/ReachedMaxValue", this, _maxInclusiveValue.ToString("d")));
				return false;
			}
			valueCounterResult = 0x7FFFFFFF;
			Exception locError = default;
			try {
				valueCounterResult = Interlocked.Decrement(ref _valueCounter);
			}
			finally {
				if ((valueCounterResult & 0x80000000) == 0x80000000)
					locError = new EonException(FormatXResource(typeof(PositiveInt32Generator), "ExceptionMessages/ReachedMaxValue", this, _maxInclusiveValue.ToString("d")));
			}
			if (locError is null) {
				value = _maxInclusiveValue - valueCounterResult;
				error = null;
				return true;
			}
			else {
				value = default;
				error = locError;
				return false;
			}
		}

		public void Reset()
			=> Interlocked.Exchange(ref _valueCounter, _maxInclusiveValue);

		public override string ToString()
			=> FormatXResource(typeof(PositiveInt32Generator), "ToString", GetType(), _name);

	}

}