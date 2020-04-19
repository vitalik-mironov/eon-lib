using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Eon {

	public static class DisposableUtilities {

		#region Nested types

		public delegate void EnsureAllowedDisposeState(bool considerDisposeRequest);

		#endregion

		public static void DisposeAndClearArray<T>(this T[ ] array)
			where T : class, IDisposable
			=> DisposeAndClearArray(array: array, exception: null);

		public static void DisposeAndClearArray<T>(this T[ ] array, Exception exception)
			where T : class, IDisposable {
			if (array is null)
				return;
			else
				try {
					var lowerBound = array.GetLowerBound(0);
					var upperBound = array.GetUpperBound(0);
					if (lowerBound <= upperBound) {
						for (var x = lowerBound; x <= upperBound; x++)
							array[ x ]?.Dispose();
						Array.Clear(array, lowerBound, array.Length);
					}
				}
				catch (Exception caughtException) {
					if (exception is null)
						throw;
					else
						throw new AggregateException(exception, caughtException);
				}
		}

		public static void DisposeMany(this IEnumerable<IDisposable> disposables)
			=> DisposeMany(disposables: disposables, exception: null);

		public static void DisposeMany(this IEnumerable<IDisposable> disposables, Exception exception) {
			try {
				if (!(disposables is null))
					foreach (var disposable in disposables)
						disposable?.Dispose();
			}
			catch (Exception locException) {
				if (exception is null)
					throw;
				else
					throw new AggregateException(exception, locException);
			}
		}

		public static void DisposeMany(params IDisposable[ ] disposables)
			=> DisposeMany(disposables: (IEnumerable<IDisposable>)disposables, exception: null);

		public static void DisposeMany(Exception exception, params IDisposable[ ] disposables)
			=> DisposeMany(exception: exception, disposables: (IEnumerable<IDisposable>)disposables);

		public static void DisposeManyDeep(Exception exception, params object[ ] disposables) {
			if (!(disposables == null || disposables.Length < 1))
				try {
					var buffer = new List<object>();
					var bufferUniqueness = new HashSet<object>();
					//
					Action<object> disposeItemOrAddToBuffer =
						locItem => {
							var locItemAsDisposable = locItem as IDisposable;
							if (locItemAsDisposable == null) {
								// Объект не реализует интерфейс IDisposable.
								//
								var locItemAsArray = locItem as object[ ];
								if (locItemAsArray == null) {
									// Объект не является массивом.
									//
									var locItemAsIEnumerable = locItem as IEnumerable;
									if (locItemAsIEnumerable != null && bufferUniqueness.Add(locItemAsIEnumerable))
										buffer.Insert(0, locItemAsIEnumerable);
								}
								else if (bufferUniqueness.Add(locItemAsArray))
									buffer.Insert(0, locItemAsArray);
							}
							else
								locItemAsDisposable.Dispose();
						};
					//
					buffer.Insert(0, disposables);
					//
					for (; buffer.Count > 0;) {
						//
						var bufferItemAsArray = buffer[ buffer.Count - 1 ] as object[ ];
						if (bufferItemAsArray == null) {
							foreach (var item in (IEnumerable)buffer[ buffer.Count - 1 ]) {
								if (item != null)
									disposeItemOrAddToBuffer(item);
							}
						}
						else {
							var startIndex = bufferItemAsArray.GetLowerBound(0);
							for (var i = 0; i < bufferItemAsArray.Length; i++) {
								var item = bufferItemAsArray[ i + startIndex ];
								if (item != null)
									disposeItemOrAddToBuffer(item);
							}
						}
						//
						buffer.RemoveAt(buffer.Count - 1);
					}
				}
				catch (Exception firstException) {
					if (exception == null)
						throw;
					else
						throw new AggregateException(exception, firstException);
				}
		}

		public static T WriteDA<T>(EnsureAllowedDisposeState ensureAllowedDisposeState, Func<bool> isDisposed, ref T location, T value, T comparand)
			where T : class {
			//
			T originalValue;
			ensureAllowedDisposeState(considerDisposeRequest: true);
			originalValue = Interlocked.CompareExchange(location1: ref location, value: value, comparand: comparand);
			if (ReferenceEquals(objA: originalValue, objB: comparand) && !ReferenceEquals(objA: value, objB: originalValue)) {
				// Value in location changed.
				//
				try { ensureAllowedDisposeState(considerDisposeRequest: true); }
				catch (ObjectDisposedException) {
					if (isDisposed())
						Interlocked.Exchange(ref location, null);
					throw;
				}
			}
			return originalValue;
		}

		public static void WriteDA<T>(EnsureAllowedDisposeState ensureAllowedDisposeState, Func<bool> isDisposed, ref T location, T value, T comparand, out bool result)
			where T : class {
			//
			T exchangeResult;
			bool locResult;
			ensureAllowedDisposeState(considerDisposeRequest: true);
			exchangeResult = Interlocked.CompareExchange(location1: ref location, value: value, comparand: comparand);
			if ((locResult = ReferenceEquals(exchangeResult, comparand)) && !ReferenceEquals(value, exchangeResult)) {
				// Установлено другое значение.
				//
				try {
					ensureAllowedDisposeState(considerDisposeRequest: true);
				}
				catch (ObjectDisposedException) {
					if (isDisposed())
						// Поскольку выгрузка завершена...
						//
						Interlocked.Exchange(ref location, null);
					throw;
				}
			}
			result = locResult;
		}

		public static T WriteDA<T>(EnsureAllowedDisposeState ensureAllowedDisposeState, Func<bool> isDisposed, ref T location, T value)
			where T : class {
			//
			T originalValue;
			ensureAllowedDisposeState(considerDisposeRequest: true);
			originalValue = Interlocked.Exchange(ref location, value);
			if (!ReferenceEquals(value, originalValue)) {
				// Установлено другое значение.
				//
				try {
					ensureAllowedDisposeState(considerDisposeRequest: true);
				}
				catch (ObjectDisposedException) {
					if (isDisposed())
						// Поскольку выгрузка завершена...
						//
						Interlocked.Exchange(ref location, null);
					throw;
				}
			}
			return originalValue;
		}

		public static T WriteDA<T>(IEonDisposable disposable, ref T location, T value, T comparand)
			where T : class
			=>
			WriteDA(
				ensureAllowedDisposeState: disposable.EnsureNotNull(nameof(disposable)).Value.EnsureNotDisposeState,
				isDisposed: () => disposable.IsDisposed,
				location: ref location,
				value: value,
				comparand: comparand);

		public static T WriteDA<T>(IEonDisposable disposable, ref T location, T value)
			where T : class
			=>
			WriteDA(
				ensureAllowedDisposeState: disposable.EnsureNotNull(nameof(disposable)).Value.EnsureNotDisposeState,
				isDisposed: () => disposable.IsDisposed,
				location: ref location,
				value: value);

		// TODO: Put strings into the resources.
		//
		public static ObjectDisposedException NewObjectDisposedException(object disposable, bool disposeRequestedException, string disposableName = default)
			=> ExceptionUtilities.NewObjectDisposedException(disposable: disposable, disposeRequestedException: disposeRequestedException, disposableName: disposableName);

		public static T DisposeFunc<T>(this T disposable)
			where T : IDisposable {
			disposable?.Dispose();
			return disposable;
		}

		public static void DisposeTarget(this WeakReference weakReference) {
			if (weakReference?.IsAlive == true)
				(weakReference.Target as IDisposable)?.Dispose();
		}

		public static void DisposeTarget(this WeakReference<IDisposable> weakReference) {
			if (!(weakReference is null) && weakReference.TryGetTarget(target: out var target))
				target?.Dispose();
		}

		public static void Dispose(Exception exception, IDisposable disposable)
			=> Dispose(disposable, exception: exception);

		public static void Dispose(this IDisposable disposable, Exception exception) {
			if (disposable is null)
				return;
			else
				try {
					disposable.Dispose();
				}
				catch (Exception locException) {
					if (exception is null)
						throw;
					else
						throw new AggregateException(exception, locException);
				}
		}

	}

}