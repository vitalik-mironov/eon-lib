using System;
using System.Collections.Generic;
using System.Linq;
using Eon.Collections;
using Eon.Linq;
using Eon.Threading;

namespace Eon {

	using StrictedWeakReference = WeakReference<IDisposable>;

	public class DisposeRegistry
		:Disposable, IDisposeRegistry {

		/// <summary>
		/// <para>Value: '1024'.</para>
		/// </summary>
		const int __DefaultCleanupThreshold = 1024;

		List<StrictedWeakReference> _disposableWeakReferences;

		PrimitiveSpinLock _disposablesSpinLock;

		readonly int _cleanupThreshold;

		public DisposeRegistry(int cleanupThreshold = default) {
			cleanupThreshold.Arg(nameof(cleanupThreshold)).EnsureNotLessThanZero();
			//
			_disposablesSpinLock = new PrimitiveSpinLock();
			_disposableWeakReferences = new List<StrictedWeakReference>();
			_cleanupThreshold = cleanupThreshold == default ? __DefaultCleanupThreshold : cleanupThreshold;
		}

		public DisposeRegistry(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4, int cleanupThreshold = default)
			: this(disposables: disposable1.Sequence().Concat(item: disposable2).Concat(item: disposable3).Concat(item: disposable4), cleanupThreshold: cleanupThreshold) { }

		public DisposeRegistry(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, int cleanupThreshold = default)
			: this(disposables: disposable1.Sequence().Concat(item: disposable2).Concat(item: disposable3), cleanupThreshold: cleanupThreshold) { }

		public DisposeRegistry(IDisposable disposable1, IDisposable disposable2, int cleanupThreshold = default)
			: this(disposables: disposable1.Sequence().Concat(item: disposable2), cleanupThreshold: cleanupThreshold) { }

		public DisposeRegistry(IDisposable disposable, int cleanupThreshold = default)
			: this(disposables: disposable.Sequence(), cleanupThreshold: cleanupThreshold) { }

		public DisposeRegistry(IEnumerable<IDisposable> disposables, int cleanupThreshold = default) {
			cleanupThreshold.Arg(nameof(cleanupThreshold)).EnsureNotLessThanZero();
			//
			_disposablesSpinLock = new PrimitiveSpinLock();
			_disposableWeakReferences = new List<StrictedWeakReference>(collection: disposables.EmptyIfNull().SkipNull().Select(locItem => new StrictedWeakReference(target: locItem, trackResurrection: false)));
			_cleanupThreshold = cleanupThreshold == default ? __DefaultCleanupThreshold : cleanupThreshold;
		}

		public void Register(IDisposable disposable) {
			disposable.EnsureNotNull(nameof(disposable));
			//
			var spinLock = ReadDA(ref _disposablesSpinLock);
			var weakReference = new StrictedWeakReference(target: disposable, trackResurrection: false);
			spinLock
				.Invoke(
					action:
						() => {
							var locRegistry = ReadDA(ref _disposableWeakReferences, considerDisposeRequest: true);
							if (locRegistry.Count % _cleanupThreshold == 0) {
								for (var i = locRegistry.Count - 1; i > -1; i--) {
									if (!locRegistry[ i ].TryGetTarget(out _))
										locRegistry.RemoveAt(i);
								}
							}
							locRegistry.Add(item: weakReference);
						});
		}

		public IReadOnlyList<IDisposable> GetAlive(bool disposeTolerant = default)
			=> GetAlive<IDisposable>();

		public IReadOnlyList<T> GetAlive<T>(bool disposeTolerant = default)
			where T : class, IDisposable {
			var spinLock = disposeTolerant ? TryReadDA(location: ref _disposablesSpinLock) : ReadDA(location: ref _disposablesSpinLock);
			if (spinLock is null) {
				if (disposeTolerant)
					return ListReadOnlyWrap<T>.Empty;
				else {
					EnsureNotDisposeState(); // ObjectDisposedException will be thrown here.
					return default;
				}
			}
			else {
				var buffer =
					spinLock
					.Invoke(
						func:
							() => {
								var locReferences = disposeTolerant ? TryReadDA(location: ref _disposableWeakReferences) : ReadDA(location: ref _disposableWeakReferences);
								if (locReferences is null) {
									if (!disposeTolerant)
										EnsureNotDisposeState(); // ObjectDisposedException will be thrown here.
									return null;
								}
								else {
									var locBuffer = new List<T>();
									for (var locY = locReferences.Count - 1; locY > -1; locY--) {
										if (locReferences[ locY ].TryGetTarget(target: out var locDisposable) && !((locDisposable as IEonDisposable)?.IsDisposeRequested ?? false)) {
											if (locDisposable is T locDisposableStricted)
												locBuffer.Add(item: locDisposableStricted);
										}
										else
											locReferences.RemoveAt(index: locY);
									}
									return locBuffer;
								}
							});
				if (buffer is null || buffer.Count == 0)
					return ListReadOnlyWrap<T>.Empty;
				else if (buffer.Count == 1)
					return new ListReadOnlyWrap<T>(list: buffer);
				else {
					var uniqueness = new HashSet<T>(comparer: ReferenceEqualityComparer<T>.Instance);
					for (var y = buffer.Count - 1; y > -1; y--) {
						if (!uniqueness.Add(item: buffer[ y ]))
							buffer.RemoveAt(index: y);
					}
					buffer.TrimExcess();
					return new ListReadOnlyWrap<T>(list: buffer);
				}
			}
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_disposablesSpinLock?.EnterAndExitLock();
				_disposableWeakReferences?.ForEach(DisposableUtilities.DisposeTarget);
				_disposableWeakReferences?.Clear();
			}
			_disposableWeakReferences = null;
			_disposablesSpinLock = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}
