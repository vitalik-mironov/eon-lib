using System;

namespace Eon {

	public static class RandomUtilities {

		static readonly Random[ ] __Randoms;

		static RandomUtilities() {
			__Randoms = new Random[ 128 ];
			var random = new Random(Seed: Environment.TickCount);
			for (var i = 0; i < __Randoms.Length; i++)
				__Randoms[ i ] = new Random(random.Next());
		}

		[Obsolete(message: "Not implemented yet.", error: true)]
		public static int NextPrimeInt32() {
			throw new NotImplementedException();
			//return RandomPrimeNumberGenerator.Singleton.NextPrimeInt32();
		}

		[Obsolete(message: "Not implemented yet.", error: true)]
		public static int NextPrimeInt32(int maxExclusive) {
			throw new NotImplementedException();
			//return RandomPrimeNumberGenerator.Singleton.NextPrimeInt32(maxExclusive);
		}

		public static int Next()
			=> __Randoms[ __Randoms[ 0 ].Next(0, __Randoms.Length) ].Next();

		public static int Next(int minInclusive)
			=> __Randoms[ __Randoms[ (minInclusive & int.MaxValue) % __Randoms.Length ].Next(0, __Randoms.Length) ].Next(minInclusive);

		public static int Next(int minInclusive, int maxExclusive)
			=> __Randoms[ __Randoms[ ((minInclusive ^ maxExclusive) & int.MaxValue) % __Randoms.Length ].Next(0, __Randoms.Length) ].Next(minInclusive, maxExclusive);

		public static int NextBytesRight(byte[ ] buffer, int maxCount) {
			buffer.EnsureNotNull(nameof(buffer));
			maxCount
				.Arg(nameof(maxCount))
				.EnsureNotLessThanZero();
			maxCount = Math.Min(maxCount, buffer.Length);
			if (maxCount == 0)
				return 0;
			else {
				var bufferMinBound = buffer.Length - maxCount - 1;
				var locBuffer = new byte[ 8 ];
				for (var y = buffer.Length - 1; y > bufferMinBound;) {
					__Randoms[ ((BitConverter.ToInt32(locBuffer, locBuffer[ 0 ] % 5) ^ maxCount) & int.MaxValue) % __Randoms.Length ].NextBytes(locBuffer);
					for (var n = 0; n < locBuffer.Length && y > bufferMinBound; n++)
						buffer[ y-- ] = locBuffer[ n ];
				}
				return maxCount;
			}
		}

		public static int NextBytesLeft(byte[ ] buffer, int maxCount) {
			buffer.EnsureNotNull(nameof(buffer));
			maxCount
				.Arg(nameof(maxCount))
				.EnsureNotLessThanZero();
			maxCount = Math.Min(maxCount, buffer.Length);
			if (maxCount == 0)
				return 0;
			else {
				var locBuffer = new byte[ 8 ];
				for (var y = 0; y < maxCount;) {
					__Randoms[ ((BitConverter.ToInt32(locBuffer, locBuffer[ 7 ] % 5) ^ maxCount) & int.MaxValue) % __Randoms.Length ].NextBytes(locBuffer);
					for (var n = 0; n < locBuffer.Length && y < maxCount; n++)
						buffer[ y++ ] = locBuffer[ n ];
				}
				return maxCount;
			}
		}

		// TODO: Put strings into the resources.
		//
		public static long NextInt64(long minInclusive, long maxExclusive) {
			minInclusive.Arg(nameof(minInclusive)).EnsureNotEqualTo(long.MinValue);
			if (maxExclusive <= minInclusive)
				throw
					new ArgumentOutOfRangeException(
						paramName: nameof(maxExclusive),
						message: $"Значение аргумента должно быть больше значения аргумента '{nameof(minInclusive)}'.{Environment.NewLine}\tЗначение:{Environment.NewLine}{maxExclusive.FmtStr().Decimal().IndentLines2()}{Environment.NewLine}\tЗначение аргумента '{nameof(minInclusive)}':{Environment.NewLine}{minInclusive.FmtStr().Decimal().IndentLines2()}");
			//
			if (minInclusive < 0L && maxExclusive > 0) {
				var randomInt64 = NextInt64(0, long.MaxValue);
				if (randomInt64 % 2L == 0L)
					return
						randomInt64 < maxExclusive
						? randomInt64
						: randomInt64 % maxExclusive;
				else
					return
						randomInt64 <= -minInclusive
						? -randomInt64
						: -(randomInt64 % -minInclusive) - 1L;
			}
			else {
				var rangeSize = maxExclusive - minInclusive;
				if (rangeSize == 1)
					return minInclusive;
				else if (rangeSize <= int.MaxValue) {
					var randomInt32 =
						Next(
							minInclusive: 0,
							maxExclusive: (int)rangeSize);
					return minInclusive + randomInt32;
				}
				else {
					var randomInt64Bytes = new byte[ 8 ];
					NextBytesLeft(randomInt64Bytes, 8);
					var randomInt64 = BitConverter.ToInt64(randomInt64Bytes, 0);
					if (randomInt64 < 0L)
						randomInt64 ^= -1L;
					randomInt64 = randomInt64 % rangeSize;
					return minInclusive + randomInt64;
				}
			}
		}

	}

}