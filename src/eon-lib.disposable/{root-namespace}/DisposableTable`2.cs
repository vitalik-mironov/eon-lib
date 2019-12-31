using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	/// <summary>
	/// Disposable instance table.
	/// <para>Main goal of this table likewise the idea of <see cref="System.Runtime.CompilerServices.ConditionalWeakTable{TKey, TValue}"/>, but instances leaves this table on special event — <see cref="IDisposeNotifying.AfterDisposed"/>.</para>
	/// <para>Built upon <see cref="ImmutableDictionary{TKey, TValue}"/>.</para>
	/// </summary>
	/// <typeparam name="TKey">Key type constraint.</typeparam>
	/// <typeparam name="TValue">Value type constraint.</typeparam>
	public class DisposableTable<TKey, TValue>
		:IReadOnlyDictionary<TKey, TValue>
		where TKey : class, IDisposeNotifying {

		#region Nested types

		/// <summary>
		/// Value holder.
		/// </summary>
		sealed class P_ValueHolder {

			public TValue Value;

			public Action<TKey, TValue> ValueCleanup;

		}

		#endregion

		#region Static & constant members

		static readonly Action<TKey, TValue> __NopValueCleanup = P_NopValueCleanup;

		static void P_NopValueCleanup(TKey key, TValue value) { }

		#endregion

		ImmutableDictionary<TKey, P_ValueHolder> _innerTable;

		public DisposableTable() {
			_innerTable = ImmutableDictionary<TKey, P_ValueHolder>.Empty;
		}

		public int Count
			=> itrlck.Get(ref _innerTable).Count;

		public IEnumerable<TKey> Keys
			=> itrlck.Get(ref _innerTable).Keys;

		public IEnumerable<TValue> Values
			=> itrlck.Get(ref _innerTable).Values.Select(selector: locItem => locItem.Value);

		public TValue this[ TKey key ]
			=> itrlck.Get(ref _innerTable)[ key ].Value;

		// TODO: Put strings into the resources.
		//
		TValue P_GetOrAdd(TKey key, Func<TKey, TValue> valueFactory, Action<TKey, TValue> valueCleanup, bool keyAbsenceRequired, out bool added, bool keyDisposeTolerant = default, TValue keyDisposeSentinel = default)
			=> P_GetOrAdd(key: key.Arg(nameof(key)), valueFactory: valueFactory, valueCleanup: valueCleanup, keyAbsenceRequired: keyAbsenceRequired, keyDisposeTolerant: keyDisposeTolerant, keyDisposeSentinel: keyDisposeSentinel, added: out added);

		TValue P_GetOrAdd(ArgumentUtilitiesHandle<TKey> key, Func<TKey, TValue> valueFactory, Action<TKey, TValue> valueCleanup, bool keyAbsenceRequired, out bool added, bool keyDisposeTolerant = default, TValue keyDisposeSentinel = default) {
			key.EnsureNotNull();
			if (valueFactory is null)
				throw new ArgumentNullException(paramName: nameof(valueFactory));
			//
			valueCleanup = valueCleanup ?? __NopValueCleanup;
			var isAdded = false;
			P_ValueHolder newValueHolder = default;
			P_ValueHolder existingValueHolder = default;
			Exception caughtException = default;
			try {
				itrlck
					.Update(
						location: ref _innerTable,
						transform:
							(ImmutableDictionary<TKey, P_ValueHolder> locCurrent) => {
								ImmutableDictionary<TKey, P_ValueHolder> locChanged;
								if (keyAbsenceRequired)
									try {
										locChanged = locCurrent.Add(key: key.Value, value: newValueHolder ?? (newValueHolder = new P_ValueHolder()));
									}
									catch (ArgumentException locException) {
										throw
											new ArgumentException(
												paramName: key.Name,
												message: $"Specified key is already exists in this table.{Environment.NewLine}\tKey:{key.Value.FmtStr().GNLI2()}",
												innerException: locException);
									}
								else {
									locChanged = locCurrent;
									var locValueHolder = ImmutableInterlocked.GetOrAdd(location: ref locChanged, key: key.Value, valueFactory: locKey => newValueHolder ?? (newValueHolder = new P_ValueHolder()));
									if (ReferenceEquals(locCurrent, locChanged))
										// Словарь не был изменён (а значит, в словаре уже присутствовало значение для указанного ключа).
										//
										existingValueHolder = locValueHolder;
								}
								if (!ReferenceEquals(locCurrent, locChanged) && newValueHolder.ValueCleanup is null) {
									newValueHolder.Value = valueFactory(arg: key.Value);
									newValueHolder.ValueCleanup = valueCleanup;
									if (keyDisposeTolerant) {
										if (key.Value.IsDisposeRequested)
											return locCurrent;
										else
											try {
												key.Value.AfterDisposed += (locSender, locEventArgs) => P_EH_Key_AfterDisposed(sender: locSender, eventArgs: locEventArgs, valueHolder: newValueHolder);
											}
											catch (ObjectDisposedException) {
												return locCurrent;
											}
									}
									else
										key.Value.AfterDisposed += (locSender, locEventArgs) => P_EH_Key_AfterDisposed(sender: locSender, eventArgs: locEventArgs, valueHolder: newValueHolder);
								}
								return locChanged;
							},
						isUpdated: out isAdded);
				if (isAdded) {
					added = true;
					return newValueHolder.Value;
				}
				else if (keyDisposeTolerant) {
					added = false;
					return keyDisposeSentinel;
				}
				else {
					added = false;
					return existingValueHolder.Value;
				}
			}
			catch (Exception exception) {
				caughtException = exception;
				throw;
			}
			finally {
				if (!(isAdded || newValueHolder is null))
					try {
						itrlck.SetNull(location: ref newValueHolder.ValueCleanup)?.Invoke(arg1: key.Value, arg2: newValueHolder.Value);
					}
					catch (Exception exception) {
						if (caughtException is null)
							throw;
						else
							throw new AggregateException(caughtException, exception);
					}
			}
		}

		void P_EH_Key_AfterDisposed(object sender, DisposeEventArgs eventArgs, P_ValueHolder valueHolder) {
			var key = sender.EnsureNotNull(nameof(sender)).EnsureOfType<TKey>().Value;
			eventArgs.EnsureNotNull(nameof(eventArgs));
			valueHolder.EnsureNotNull(nameof(valueHolder));
			//
			itrlck
				.Update(
					location: ref _innerTable,
					transform:
						(ImmutableDictionary<TKey, P_ValueHolder> locCurrent) => {
							var locChanged = locCurrent;
							if (ImmutableInterlocked.TryRemove(location: ref locChanged, key: key, value: out var locRemoving) && ReferenceEquals(valueHolder, locRemoving))
								return locChanged;
							else
								return locCurrent;
						},
					isUpdated: out var isRemoved);
			if (isRemoved)
				itrlck.SetNull(location: ref valueHolder.ValueCleanup)?.Invoke(arg1: key, arg2: valueHolder.Value);
		}

		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory, Action<TKey, TValue> valueCleanup = default, bool keyDisposeTolerant = default, TValue keyDisposeSentinel = default)
			=> P_GetOrAdd(key: key, valueFactory: valueFactory, valueCleanup: valueCleanup, keyAbsenceRequired: false, added: out _, keyDisposeTolerant: keyDisposeTolerant, keyDisposeSentinel: keyDisposeSentinel);

		public TValue GetOrAdd(TKey key, TValue value, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: locKey => value, valueCleanup: valueCleanup, keyAbsenceRequired: false, added: out _);

		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory, out bool added, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: valueFactory, valueCleanup: valueCleanup, keyAbsenceRequired: false, added: out added);

		public TValue GetOrAdd(TKey key, TValue value, out bool added, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: locKey => value, valueCleanup: valueCleanup, keyAbsenceRequired: false, added: out added);

		public TValue Add(TKey key, TValue value, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: locKey => value, valueCleanup: valueCleanup, keyAbsenceRequired: true, added: out _);

		public TValue Add(TKey key, Func<TKey, TValue> valueFactory, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: valueFactory, valueCleanup: valueCleanup, keyAbsenceRequired: true, out _);

		public TValue GetOrAdd(ArgumentUtilitiesHandle<TKey> key, Func<TKey, TValue> valueFactory, Action<TKey, TValue> valueCleanup = default, bool keyDisposeTolerant = default, TValue keyDisposeSentinel = default)
			=> P_GetOrAdd(key: key, valueFactory: valueFactory, valueCleanup: valueCleanup, keyAbsenceRequired: false, added: out _, keyDisposeTolerant: keyDisposeTolerant, keyDisposeSentinel: keyDisposeSentinel);

		public TValue GetOrAdd(ArgumentUtilitiesHandle<TKey> key, TValue value, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: locKey => value, valueCleanup: valueCleanup, keyAbsenceRequired: false, added: out var added);

		public TValue GetOrAdd(ArgumentUtilitiesHandle<TKey> key, Func<TKey, TValue> valueFactory, out bool added, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: valueFactory, valueCleanup: valueCleanup, keyAbsenceRequired: false, added: out added);

		public TValue GetOrAdd(ArgumentUtilitiesHandle<TKey> key, TValue value, out bool added, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: locKey => value, valueCleanup: valueCleanup, keyAbsenceRequired: false, added: out added);

		public TValue Add(ArgumentUtilitiesHandle<TKey> key, TValue value, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: locKey => value, valueCleanup: valueCleanup, keyAbsenceRequired: true, out var added);

		public TValue Add(ArgumentUtilitiesHandle<TKey> key, Func<TKey, TValue> valueFactory, Action<TKey, TValue> valueCleanup = default)
			=> P_GetOrAdd(key: key, valueFactory: valueFactory, valueCleanup: valueCleanup, keyAbsenceRequired: true, out _);

		public bool ContainsKey(TKey key)
			=> itrlck.Get(ref _innerTable).ContainsKey(key);

		public bool TryGetValue(TKey key, out TValue value) {
			if (itrlck.Get(ref _innerTable).TryGetValue(key: key, value: out var locValueHolder)) {
				value = locValueHolder.Value;
				return true;
			}
			else {
				value = default;
				return false;
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			=> itrlck.Get(ref _innerTable).Select(locItem => new KeyValuePair<TKey, TValue>(key: locItem.Key, value: locItem.Value.Value)).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

	}

}