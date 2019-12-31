using System;

namespace Eon.Context {

	/// <summary>
	/// Context (<see cref="IContext"/>) property.
	/// </summary>
	/// <typeparam name="T">Type of property value.</typeparam>
	public class ContextProperty<T>
		:ContextProperty {

		readonly ArgumentPlaceholder<T> _fallbackValue;

		readonly Action<IContext, T> _cleanup;

		readonly DisposableTable<IContext, PropertyValueHolder<IContext, T>> _values;

		readonly Action<ArgumentUtilitiesHandle<T>> _validator;

		public ContextProperty(string name, Action<IContext, T> cleanup = default, ArgumentPlaceholder<T> fallbackValue = default, Action<ArgumentUtilitiesHandle<T>> validator = default)
			: base(name: name, type: typeof(T)) {
			_fallbackValue = fallbackValue;
			_cleanup = cleanup;
			_values = new DisposableTable<IContext, PropertyValueHolder<IContext, T>>();
			_validator = validator;
		}

		public ArgumentPlaceholder<T> FallbackValue
			=> _fallbackValue;

		PropertyValueHolder<IContext, T> P_GetValueHolder(IContext ctx, bool disposeTolerant) {
			if (_cleanup is null) {
				if (disposeTolerant)
					return
						_values
						.GetOrAdd(
							key: ctx.Arg(nameof(ctx)),
							valueFactory: valueHolderEvaluator,
							valueCleanup: null,
							keyDisposeTolerant: true,
							keyDisposeSentinel: new PropertyValueHolder<IContext, T>(instance: ctx, exception: ContextDisposeSentinel));
				else
					return _values.GetOrAdd(key: ctx.Arg(nameof(ctx)), valueFactory: valueHolderEvaluator, valueCleanup: null);
			}
			else if (disposeTolerant)
				return
					_values
					.GetOrAdd(
						key: ctx.Arg(nameof(ctx)),
						valueFactory: valueHolderEvaluator,
						valueCleanup:
							(locCtx, locValueHolder) => {
								if (ReferenceEquals(locCtx, locValueHolder.Instance) && locValueHolder.Exception is null)
									_cleanup(arg1: locCtx, locValueHolder.Value);
							},
						keyDisposeTolerant: true,
						keyDisposeSentinel: new PropertyValueHolder<IContext, T>(instance: ctx, exception: ContextDisposeSentinel));
			else
				return
					_values
					.GetOrAdd(
						key: ctx.Arg(nameof(ctx)),
						valueFactory: valueHolderEvaluator,
						valueCleanup:
							(locCtx, locValueHolder) => {
								if (ReferenceEquals(locCtx, locValueHolder.Instance) && locValueHolder.Exception is null)
									_cleanup(arg1: locCtx, locValueHolder.Value);
							});
			//
			PropertyValueHolder<IContext, T> valueHolderEvaluator(IContext locCtx) {
				try {
					if (EvaluateLocalValue(ctx: locCtx, value: out var locValue))
						return new PropertyValueHolder<IContext, T>(instance: locCtx, value: locValue);
					else {
						var locOuterCtx = locCtx;
						for (; locOuterCtx.HasOuterContext;) {
							locOuterCtx = locOuterCtx.OuterContext;
							if (_values.TryGetValue(key: locOuterCtx, value: out var locValueHolder))
								return locValueHolder;
						}
						if (_fallbackValue.HasExplicitValue)
							return new PropertyValueHolder<IContext, T>(instance: locCtx, value: _fallbackValue.ExplicitValue);
						else
							return new PropertyValueHolder<IContext, T>(instance: locCtx, exception: MissingValueSentinel);
					}
				}
				catch (Exception locException) {
					return new PropertyValueHolder<IContext, T>(instance: locCtx, exception: locException);
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		bool P_GetValue(IContext ctx, out T value, bool disposeTolerant = default, bool missingTolerant = default) {
			var holder = P_GetValueHolder(ctx: ctx, disposeTolerant: disposeTolerant);
			var holderException = holder.Exception?.SourceException;
			if (holderException is null) {
				var locValue = holder.Value;
				if (!disposeTolerant) {
					var valueHolderInstance = holder.Instance;
					ctx.EnsureNotDisposeState();
					if (!ReferenceEquals(objA: ctx, objB: valueHolderInstance))
						valueHolderInstance.EnsureNotDisposeState();
				}
				value = locValue;
				return true;
			}
			else if (ReferenceEquals(objA: MissingValueSentinel, objB: holderException)) {
				if (missingTolerant) {
					value = default;
					return false;
				}
				else
					throw new EonException(message: $"Property not exists on this flow context.{Environment.NewLine}\tProperty:{this.FmtStr().GNLI2()}{Environment.NewLine}\tContext:{ctx.FmtStr().GNLI2()}");
			}
			else if (ReferenceEquals(objA: ContextDisposeSentinel, objB: holderException)) {
				if (disposeTolerant) {
					value = default;
					return false;
				}
				else
					throw DisposableUtilities.NewObjectDisposedException(disposable: ctx, disposeRequestedException: false);
			}
			else if (disposeTolerant) {
				value = holder.Value; // Here ValueGetterException will be thrown.
				return true;
			}
			else {
				var valueHolderInstance = holder.Instance;
				ctx.EnsureNotDisposeState();
				if (!ReferenceEquals(objA: ctx, objB: valueHolderInstance))
					valueHolderInstance.EnsureNotDisposeState();
				value = holder.Value; // Here ValueGetterException will be thrown.
				return true;
			}
		}

		public T GetValue(IContext ctx, bool disposeTolerant = default) {
			P_GetValue(ctx: ctx, value: out var value, disposeTolerant: disposeTolerant, missingTolerant: false);
			return value;
		}

		public bool TryGetValue(IContext ctx, out T value, bool disposeTolerant = default)
			=> P_GetValue(ctx: ctx, value: out value, disposeTolerant: disposeTolerant, missingTolerant: true);

		/// <summary>
		/// Sets value of this property for the specified context locally.
		/// <para>Local value can be set once.</para>
		/// </summary>
		/// <param name="ctx">
		/// Context for which value to be set.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="value">
		/// Value to be set,
		/// <para>Must be valid (<see cref="Validate(T)"/>).</para>
		/// </param>
		public void SetLocalValue(IContext ctx, T value) {
			Validate(value: value);
			if (_cleanup is null)
				_values.Add(key: ctx.Arg(nameof(ctx)), value: new PropertyValueHolder<IContext, T>(instance: ctx, value: value), valueCleanup: null);
			else
				_values.Add(key: ctx.Arg(nameof(ctx)), value: new PropertyValueHolder<IContext, T>(instance: ctx, value: value), valueCleanup: (locCtx, locValueHolder) => _cleanup(arg1: locCtx, arg2: locValueHolder.Value));
		}

		public void SetLocalValue(IContext ctx, ArgumentUtilitiesHandle<T> value) {
			Validate(value: value);
			if (_cleanup is null)
				_values.Add(key: ctx.Arg(nameof(ctx)), value: new PropertyValueHolder<IContext, T>(instance: ctx, value: value.Value), valueCleanup: null);
			else
				_values.Add(key: ctx.Arg(nameof(ctx)), value: new PropertyValueHolder<IContext, T>(instance: ctx, value: value.Value), valueCleanup: (locCtx, locValueHolder) => _cleanup(arg1: locCtx, arg2: locValueHolder.Value));
		}

		protected virtual bool EvaluateLocalValue(IContext ctx, out T value) {
			if (ctx is null)
				throw new ArgumentNullException(paramName: nameof(ctx));
			//
			value = default;
			return false;
		}

		public void Validate(T value) {
			var valueArg = value.ArgProp(name: Name);
			DoValidate(value: valueArg);
		}

		public void Validate(ArgumentUtilitiesHandle<T> value)
			=> DoValidate(value: value);

		protected virtual void DoValidate(ArgumentUtilitiesHandle<T> value) {
			if (!(_validator is null))
				_validator(obj: value);
		}

	}

}