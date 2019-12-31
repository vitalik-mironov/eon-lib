using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using Eon.Threading.Tasks;

namespace Eon {

	public static partial class ValueHolderUtilities {

		#region Nested types

		sealed class P_HolderWithDisposeRegistry<T>
			:Disposable, IVh<T> {

			IVh<T> _realHolder;

			DisposeRegistry _disposeRegistry;

			internal P_HolderWithDisposeRegistry(T value, IEnumerable<IDisposable> disposables) {
				_realHolder = value.ToValueHolder(ownsValue: true);
				_disposeRegistry = new DisposeRegistry(disposables: disposables);
			}

			public bool HasException
				=> false;

			public ExceptionDispatchInfo Exception
				=> ReadDA(ref _realHolder).Exception;

			public T Value
				=> ReadDA(ref _realHolder).Value;

			public bool OwnsValue
				=> true;

			public T ValueDisposeTolerant {
				get {
					var store = TryReadDA(ref _realHolder);
					if (store is null)
						return default(T);
					else
						return store.ValueDisposeTolerant;
				}
			}

			public ITaskWrap<T> Awaitable() {
				try {
					return ReadDA(ref _realHolder).Awaitable();
				} catch (Exception exception) {
					return TaskWrap<T>.Wrap(task: Task.FromException<T>(exception));
				}
			}

			public void RemoveValue()
				=> TryReadDA(ref _realHolder)?.RemoveValue();

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					_realHolder?.Dispose();
					_disposeRegistry?.Dispose();
				}
				_realHolder = null;
				_disposeRegistry = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

	}

}