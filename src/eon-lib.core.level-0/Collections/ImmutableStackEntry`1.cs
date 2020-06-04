using System;

namespace Eon.Collections {

	public readonly struct ImmutableStackEntry<T> {

		public readonly T Value;

		public readonly int Position;

		// TODO: Put strings into the resources.
		//
		public ImmutableStackEntry(T value, int position) {
			if (position < 0)
				throw new ArgumentOutOfRangeException(paramName: nameof(position), message: $"Value cannot be less than zero.{Environment.NewLine}\tValue:{Environment.NewLine}\t\t{position:d}");
			//
			Value = value;
			Position = position;
		}

	}

}