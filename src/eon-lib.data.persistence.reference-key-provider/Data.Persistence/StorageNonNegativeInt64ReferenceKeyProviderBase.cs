using System;
using System.Collections.Immutable;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Threading;
using Eon.Threading.Tasks;

namespace Eon.Data.Persistence {

	public abstract class StorageNonNegativeInt64ReferenceKeyProviderBase
		:DisposeNotifying, IReferenceKeyProvider<long> {

		#region Nested types 

		protected enum FillUpKeyBufferResult {

			BufferFilledUp = 0,

			BufferNotEmpty,

			NoMoreKeys

		}

		protected readonly struct TakeNextKeyResult {

			public readonly bool HasTaken;

			public readonly long? LowerTakenKey;

			public readonly long? UpperTakenKey;

			public TakeNextKeyResult(bool hasTaken, long? lowerTakenKey, long? upperTakenKey) {
				HasTaken = hasTaken;
				LowerTakenKey = lowerTakenKey;
				UpperTakenKey = upperTakenKey;
			}

		}

		#endregion

		#region Static & constant members

		public const int MaxSizeOfKeyBuffer = 16384;

		public const int DefaultSizeOfKeyBuffer = 512;

		static readonly Type __KeyType = typeof(long);

		#endregion

		IStorageNonNegativeInt64ReferenceKeyProviderSettings _settings;

		ImmutableQueue<long> _keyBuffer;

		SemaphoreSlim _keyBufferFillUpLock;

		readonly int _maxSizeOfKeyBuffer;

		public StorageNonNegativeInt64ReferenceKeyProviderBase(IStorageNonNegativeInt64ReferenceKeyProviderSettings settings) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid();
			//
			InspectOfSettings(settings: settings);
			//
			_settings = settings;
			_keyBuffer = ImmutableQueue<long>.Empty;
			_keyBufferFillUpLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
			_maxSizeOfKeyBuffer = Math.Min(val1: MaxSizeOfKeyBuffer, val2: settings.KeyBufferPreferredSize ?? DefaultSizeOfKeyBuffer);
		}

		Type IReferenceKeyProvider.KeyType
			=> __KeyType;

		public IStorageNonNegativeInt64ReferenceKeyProviderSettings Settings
			=> ReadDA(ref _settings);

		protected ImmutableQueue<long> P_KeyBuffer
			=> ReadDA(ref _keyBuffer);

		protected SemaphoreSlim P_KeyBufferFillUpLock
			=> ReadDA(ref _keyBufferFillUpLock);

		protected virtual void InspectOfSettings(IStorageNonNegativeInt64ReferenceKeyProviderSettings settings) { }

		protected abstract Task<DbConnection> CreateStorageConnectionAsync(IContext ctx = default);

		public async Task<long> NextKeyAsync(IContext ctx = default) {
			for (; ; ) {
				ctx.ThrowIfCancellationRequested();
				// Попытка взять ключ из буфера.
				//
				var nextKey = 0L;
				var isNextKeyTakenFromBuffer = UpdDABool(location: ref _keyBuffer, transform: locCurrent => locCurrent.IsEmpty ? locCurrent : locCurrent.Dequeue(out nextKey));
				//
				if (isNextKeyTakenFromBuffer)
					return nextKey;
				else {
					// Буфер пуст, необходимо его заполнить.
					//
					var fillUpKeyBufferResult = await fillUpEmptyKeyBufferAsync().ConfigureAwait(false);
					if (fillUpKeyBufferResult == FillUpKeyBufferResult.NoMoreKeys)
						throw
							new EonException(message: $"Cannot take the next reference key. All keys from the specified range was taken already.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}{Environment.NewLine}\tRange:{Environment.NewLine}{$"[{Settings.ScopeLowerKey.FmtStr().Decimal()}; {Settings.ScopeUpperKey.FmtStr().Decimal()}]".IndentLines2()}");
				}
			}
			//
			async Task<FillUpKeyBufferResult> fillUpEmptyKeyBufferAsync() {
				if (P_KeyBuffer.IsEmpty) {
					var locCt = ctx.Ct();
					var locLck = ReadDA(ref _keyBufferFillUpLock, considerDisposeRequest: true);
					var locLckAcquired = false;
					try {
						locLckAcquired = await locLck.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds).ConfigureAwait(false);
						if (!locLckAcquired)
							throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
						//
						if (P_KeyBuffer.IsEmpty) {
							var locTakeKeyResult = await TakeNextKeyAsync(count: _maxSizeOfKeyBuffer, ctx: ctx).ConfigureAwait(false);
							FillUpKeyBufferResult locResult;
							if (locTakeKeyResult.HasTaken) {
								for (var locTakenKey = locTakeKeyResult.LowerTakenKey.Value; locTakenKey <= locTakeKeyResult.UpperTakenKey.Value; locTakenKey++)
									UpdDABool(location: ref _keyBuffer, transform: locCurrent => locCurrent.Enqueue(value: locTakenKey));
								locResult = FillUpKeyBufferResult.BufferFilledUp;
							}
							else
								locResult = FillUpKeyBufferResult.NoMoreKeys;
							return locResult;
						}
						else
							return FillUpKeyBufferResult.BufferNotEmpty;
					}
					finally {
						if (locLckAcquired)
							try { locLck.Release(); }
							catch (ObjectDisposedException) { }
					}
				}
				else
					return FillUpKeyBufferResult.BufferNotEmpty;
			}
		}

		protected virtual async Task<TakeNextKeyResult> TakeNextKeyAsync(int count, IContext ctx = default) {
			count.EnsureNotLessThanOne(nameof(count));
			//
			using (var storageConnection = await CreateStorageConnectionAsync(ctx: ctx).ConfigureAwait(false))
				return await TakeNextKeyAsync(storeConnection: storageConnection, count: count, ctx: ctx).ConfigureAwait(false);
		}

		protected abstract Task<TakeNextKeyResult> TakeNextKeyAsync(DbConnection storeConnection, int count, IContext ctx = default);

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_keyBufferFillUpLock?.Dispose();
			}
			_keyBufferFillUpLock = null;
			_keyBuffer = null;
			//
			base.Dispose(explicitDispose);
		}

	}
}