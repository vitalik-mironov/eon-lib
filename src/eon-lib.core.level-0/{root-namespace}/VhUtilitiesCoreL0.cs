using System;

namespace Eon {

	public static class VhUtilitiesCoreL0 {

		public static void Dispose<T>(this ValueHolderClass<T> holder, Exception exception = default)
			where T : IDisposable {
			if (!(holder is null))
				try {
					holder.ValueExceptionTolerant?.Dispose();
				}
				catch (Exception secondException) {
					if (exception is null)
						throw;
					else
						throw new AggregateException(exception, secondException);
				}
		}

		public static void Dispose<T>(this ValueHolderStruct<T> holder, Exception exception = default)
			where T : IDisposable {
			try {
				holder.ValueExceptionTolerant?.Dispose();
			}
			catch (Exception secondException) {
				if (exception is null)
					throw;
				else
					throw new AggregateException(exception, secondException);
			}
		}

		public static void Dispose<T>(this ValueHolderStruct<T>? holder, Exception exception = default)
			where T : IDisposable {
			if (holder.HasValue)
				try {
					holder.Value.ValueExceptionTolerant?.Dispose();
				}
				catch (Exception secondException) {
					if (exception is null)
						throw;
					else
						throw new AggregateException(exception, secondException);
				}
		}

	}

}