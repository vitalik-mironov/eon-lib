using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Eon.Reflection;
using Eon.Threading;

namespace Eon {

	public sealed class MuttableVh<TValue>
		:CloneableDisposable {

		#region Static members

		static readonly TValue __DefaultValue;

		static readonly Action<MuttableVh<TValue>, TValue> __Setter;

		static readonly Func<MuttableVh<TValue>, Transform<TValue>, bool> __Updater;

		static readonly Action<TValue> __ExplicitDispose;

		static readonly Func<TValue, ICloneContext, TValue> __CloneValue;

		static MuttableVh() {
			__DefaultValue = default;
			//
			var typeOfValue = typeof(TValue);
			var valueStoreInstanceParameter = Expression.Parameter(typeof(MuttableVh<TValue>));
			var transformParameter = Expression.Parameter(typeof(Transform<TValue>));
			if (typeOfValue.IsValueType()) {
				__Setter = (locValueStore, locValue) => VolatileUtilities.Write(ref locValueStore._value, locValue);
				//
				__Updater =
					Expression
					.Lambda<Func<MuttableVh<TValue>, Transform<TValue>, bool>>(
						body:
							Expression
							.Call(
								method:
									typeof(MuttableVh<TValue>)
									.GetMethod(nameof(P_ValT_ValueUpdater), BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic)
									.MakeGenericMethod(typeOfValue),
								arg0: valueStoreInstanceParameter,
								arg1: transformParameter),
						parameters: new[ ] { valueStoreInstanceParameter, transformParameter })
					.Compile();
				//
				if (typeof(IDisposable).IsAssignableFrom(typeOfValue)) {
					var valueParameter = Expression.Parameter(typeOfValue);
					__ExplicitDispose =
						Expression
						.Lambda<Action<TValue>>(
							body:
								Expression
								.Call(
									instance: valueParameter,
									method: typeOfValue.GetMethod(nameof(IDisposable.Dispose), throwIfNotFound: true)),
							parameters: new[ ] { valueParameter })
						.Compile();
				}
				else
					__ExplicitDispose = locValue => { };
				//
				if (typeof(IOxyCloneable).IsAssignableFrom(typeOfValue)) {
					var valueParameter = Expression.Parameter(typeOfValue);
					var cloneContextParameter = Expression.Parameter(typeof(ICloneContext));
					__CloneValue = Expression.Lambda<Func<TValue, ICloneContext, TValue>>(
						Expression.Call(
							valueParameter,
							typeOfValue.GetMethod(
								nameof(IOxyCloneable.Clone),
								parameterTypes: new[ ] { cloneContextParameter.Type },
								throwIfNotFound: true),
							cloneContextParameter),
						valueParameter,
						cloneContextParameter).Compile();
				}
				else
					__CloneValue = (v, c) => v;
			}
			else {
				var valueParameter = Expression.Parameter(typeOfValue);
				//
				__Setter =
					Expression
					.Lambda<Action<MuttableVh<TValue>, TValue>>(
						body:
							Expression
							.Call(
								method:
									typeof(MuttableVh<TValue>)
									.GetMethod(nameof(P_RefT_ValueSetter), BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic)
									.MakeGenericMethod(typeOfValue),
								arg0: valueStoreInstanceParameter,
								arg1: valueParameter),
						parameters: new[ ] { valueStoreInstanceParameter, valueParameter })
					.Compile();
				//
				__Updater =
					Expression
					.Lambda<Func<MuttableVh<TValue>, Transform<TValue>, bool>>(
						body:
							Expression
							.Call(
								method:
									typeof(MuttableVh<TValue>)
									.GetMethod(nameof(P_RefT_ValueUpdater), BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic)
									.MakeGenericMethod(typeOfValue),
								arg0: valueStoreInstanceParameter,
								arg1: transformParameter),
						parameters: new[ ] { valueStoreInstanceParameter, transformParameter })
					.Compile();
				//
				if (typeof(IDisposable).IsAssignableFrom(typeOfValue))
					__ExplicitDispose = Expression.Lambda<Action<TValue>>(Expression.Call(typeof(MuttableVh<TValue>).GetMethod(nameof(P_RefT_ExplicitDispose), BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic).MakeGenericMethod(typeOfValue), valueParameter), valueParameter).Compile();
				else
					__ExplicitDispose = v => { };
				//
				if (typeof(IOxyCloneable).IsAssignableFrom(typeOfValue)) {
					var cloneContextParameter = Expression.Parameter(typeof(ICloneContext));
					__CloneValue = Expression.Lambda<Func<TValue, ICloneContext, TValue>>(
						Expression.Condition(
							Expression.ReferenceEqual(valueParameter, Expression.Constant(null, valueParameter.Type)),
							valueParameter,
							Expression.Convert(
								Expression.Call(
									Expression.Convert(valueParameter, typeof(IOxyCloneable)),
									typeof(IOxyCloneable).GetMethod(
										nameof(IOxyCloneable.Clone),
										parameterTypes: new[ ] { cloneContextParameter.Type },
										throwIfNotFound: true),
									cloneContextParameter),
								valueParameter.Type)),
						valueParameter,
						cloneContextParameter).Compile();
				}
				else
					__CloneValue = (v, c) => v;
			}
		}

		static void P_RefT_ExplicitDispose<T>(T value)
			where T : class, IDisposable
			=> value?.Dispose();

		static T P_RefT_ValueSetter<T>(MuttableVh<T> valueStore, T value)
			where T : class
			=> valueStore.WriteDA(ref valueStore._value, value);

		static bool P_ValT_ValueUpdater<T>(MuttableVh<T> valueStore, Transform<T> transform)
			where T : struct {
			transform.EnsureNotNull(nameof(transform));
			//
			var currentVersion = valueStore.ReadDA(ref valueStore._value);
			var newValue = transform(currentVersion);
			if (EqualityComparer<T>.Default.Equals(x: currentVersion, y: newValue))
				return false;
			else {
				VolatileUtilities.Write(ref valueStore._value, newValue);
				valueStore.EnsureNotDisposeState();
				return true;
			}
		}

		static bool P_RefT_ValueUpdater<T>(MuttableVh<T> valueStore, Transform<T> transform)
			where T : class {
			//
			return valueStore.UpdDABool(location: ref valueStore._value, transform: transform);
		}

		#endregion

		TValue _value;

		Action<TValue> _valueExplicitDisposeImpl;

		public MuttableVh()
			: this(__DefaultValue, false) { }

		public MuttableVh(TValue value)
			: this(value, false) { }

		public MuttableVh(TValue value, bool ownsValue) {
			_value = value;
			if (ownsValue)
				_valueExplicitDisposeImpl = __ExplicitDispose;
			else
				_valueExplicitDisposeImpl = v => { };
		}

		public TValue TryGetValueDisposeAffectable(bool considerDisposeRequest = false)
			=> TryReadDA(ref _value, considerDisposeRequest: considerDisposeRequest);

		public TValue Value {
			get => ReadDA(ref _value, considerDisposeRequest: false);
			set => __Setter(this, value);
		}

		public bool UpdateValue(Transform<TValue> transform)
			=> __Updater(this, transform);

		public void ClearValue()
			=> VolatileUtilities.Write(ref _value, __DefaultValue);

		protected override void PopulateClone(ICloneContext cloneContext, CloneableDisposable clone) {
			clone.EnsureNotNull(nameof(clone));
			//
			var locClone = (MuttableVh<TValue>)clone;
			locClone._value = __CloneValue(VolatileUtilities.Read(ref _value), cloneContext);
		}

		public new MuttableVh<TValue> Clone()
			=> (MuttableVh<TValue>)base.Clone();

		public new MuttableVh<TValue> Clone(ICloneContext cloneContext)
			=> (MuttableVh<TValue>)base.Clone(cloneContext);

		public static implicit operator MuttableVh<TValue>(TValue value)
			=> new MuttableVh<TValue>(value);

		public override string ToString()
			=> _value?.FmtStr().G() ?? base.ToString();

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_valueExplicitDisposeImpl?.Invoke(_value);
			_value = __DefaultValue;
			_valueExplicitDisposeImpl = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}