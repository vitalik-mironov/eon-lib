using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Eon.Linq;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		static IEnumerable<TItem> P_EnsureUnique<TItem, TValue>(ArgumentUtilitiesHandle<IEnumerable<TItem>> hnd, Func<TItem, TValue> keySelector, IEqualityComparer<TValue> comparer = default, bool skipNull = default, string keyName = default) {
			keySelector.EnsureNotNull(nameof(keySelector));
			//
			return P_EnsureUnique<TItem, TValue>(hnd: hnd, keySelector: (locItem, locIndex) => keySelector(locItem), comparer: comparer, skipNull: skipNull, keyName: keyName);
		}

		static IEnumerable<TItem> P_EnsureUnique<TItem, TValue>(ArgumentUtilitiesHandle<IEnumerable<TItem>> hnd, Func<TItem, int, TValue> keySelector, IEqualityComparer<TValue> comparer = default, bool skipNull = default, string keyName = default) {
			if (hnd.Value is null)
				throw new ArgumentNullException(paramName: $"{nameof(hnd)}.{nameof(hnd.Value)}");
			else if (keySelector is null)
				throw new ArgumentNullException(paramName: nameof(keySelector));
			//
			var elementIndexCounter = -1;
			var buffer = new HashSet<TValue>(comparer: comparer);
			foreach (var element in (skipNull ? hnd.Value.SkipNull() : hnd.Value)) {
				elementIndexCounter++;
				var key = keySelector(element, elementIndexCounter);
				if (!buffer.Add(key)) {
					var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Array), "CanNotContainNonUnique/NonUniqueValueAt", key?.ToString(), elementIndexCounter.ToString("d"))}{(keyName is null ? string.Empty : $"{Environment.NewLine}\tKey name:{keyName.FmtStr().GNLI2()}")}";
					throw hnd.ExceptionFactory?.Invoke(message: exceptionMessage) ?? new ArgumentException(paramName: $"{hnd.Name}[{elementIndexCounter:d}]", message: exceptionMessage);
				}
				yield return element;
			}
		}

		static IEnumerable<T> P_EnsureNoNullElements<T>(ArgumentUtilitiesHandle<IEnumerable<T>> hnd) {
			if (hnd.Value == null)
				throw new ArgumentNullException(paramName: $"{nameof(hnd)}.{nameof(hnd.Value)}");
			//
			var elementIndexCounter = -1L;
			foreach (var element in hnd.Value) {
				elementIndexCounter++;
				if (element == null) {
					var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Array), "CanNotContainNull/NullAt", elementIndexCounter.ToString("d"))}";
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
						?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
				}
				else
					yield return element;
			}
		}

		static IEnumerable<T> P_EnsureHasMaxLength<T>(ArgumentUtilitiesHandle<IEnumerable<T>> hnd, int maxLength) {
			if (hnd.Value is null)
				throw new ArgumentNullException(paramName: $"{nameof(hnd)}.{nameof(hnd.Value)}");
			//
			var elementCounter = 0;
			foreach (var element in hnd.Value) {
				if (elementCounter >= maxLength) {
					var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Array), "TooBig/ExpectedMaxLength", maxLength.ToString("d"))}";
					throw hnd.ExceptionFactory?.Invoke(message: exceptionMessage) ?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
				}
				elementCounter++;
				yield return element;
			}
		}

		static IEnumerable<IVh<T>> P_EnsureNoNullElements<T>(ArgumentUtilitiesHandle<IEnumerable<IVh<T>>> hnd) {
			if (hnd.Value == null)
				throw new ArgumentNullException($"{nameof(hnd)}.{nameof(hnd.Value)}");
			//
			var elementIndexCounter = -1L;
			foreach (var element in hnd.Value) {
				elementIndexCounter++;
				T elementValue;
				try {
					elementValue = element == null ? default : element.Value;
				}
				catch (Exception firstException) {
					throw
						new EonException(
							message: $"An exception occurred while fetching the next item from sequence{(hnd.IsProp ? $" of property '{hnd.Name}'" : string.Empty)}.{Environment.NewLine}\tItem position:{elementIndexCounter.ToString("d").FmtStr().GNLI2()}{Environment.NewLine}\tItem:{element.FmtStr().GNLI2()}",
							innerException: firstException);
				}
				if (elementValue == null) {
					var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Array), "CanNotContainNull/NullAt", elementIndexCounter.ToString("d"))}";
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
						?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
				}
				else
					yield return element;
			}
		}

		public static ArgumentUtilitiesHandle<IEnumerable<TItem>> EnsureUnique<TItem>(this ArgumentUtilitiesHandle<IEnumerable<TItem>> hnd, IEqualityComparer<TItem> comparer = default, bool skipNull = default) {
			if (hnd.Value is null)
				return hnd;
			else
				return hnd.AsChanged(value: P_EnsureUnique(hnd: hnd, keySelector: locItem => locItem, comparer: comparer, skipNull: skipNull));
		}

		public static ArgumentUtilitiesHandle<IEnumerable<TItem>> EnsureUnique<TItem, TValue>(this ArgumentUtilitiesHandle<IEnumerable<TItem>> hnd, Func<TItem, TValue> keySelector, string keyName, IEqualityComparer<TValue> comparer = default, bool skipNull = default) {
			keySelector.EnsureNotNull(nameof(keySelector));
			keyName.EnsureNotNull(nameof(keyName)).EnsureNotEmpty();
			//
			if (hnd.Value is null)
				return hnd;
			else
				return hnd.AsChanged(value: P_EnsureUnique(hnd: hnd, keySelector: keySelector, comparer: comparer, skipNull: skipNull, keyName: keyName));
		}

		public static ArgumentUtilitiesHandle<IEnumerable<TItem>> EnsureUnique<TItem, TValue>(this ArgumentUtilitiesHandle<IEnumerable<TItem>> hnd, Func<TItem, int, TValue> keySelector, string keyName, IEqualityComparer<TValue> comparer = default, bool skipNull = default) {
			keySelector.EnsureNotNull(nameof(keySelector));
			keyName.EnsureNotNull(nameof(keyName)).EnsureNotEmpty();
			//
			if (hnd.Value is null)
				return hnd;
			else
				return hnd.AsChanged(value: P_EnsureUnique(hnd: hnd, keySelector: keySelector, comparer: comparer, skipNull: skipNull, keyName: keyName));
		}

		public static ArgumentUtilitiesHandle<IEnumerable<T>> EnsureNotEmpty<T>(this ArgumentUtilitiesHandle<IEnumerable<T>> hnd) {
			if (hnd.Value == null)
				return hnd;
			else
				return new ArgumentUtilitiesHandle<IEnumerable<T>>(P_EnsureNotEmpty(hnd), hnd.Name);
		}

		public static ArgumentUtilitiesHandle<IEnumerable<T>> EnsureNoNullElements<T>(this ArgumentUtilitiesHandle<IEnumerable<T>> hnd) {
			if (hnd.Value is null)
				return hnd;
			else
				return hnd.AsChanged(value: P_EnsureNoNullElements(hnd));
		}

		public static ArgumentUtilitiesHandle<IEnumerable<T>> EnsureHasMaxLength<T>(this in ArgumentUtilitiesHandle<IEnumerable<T>> hnd, int maxLength) {
			maxLength.Arg(nameof(maxLength)).EnsureNotLessThan(operand: 0);
			//
			if (hnd.Value is null)
				return hnd;
			else
				return hnd.AsChanged(value: P_EnsureHasMaxLength(hnd, maxLength: maxLength));
		}

		public static ArgumentUtilitiesHandle<IEnumerable<IVh<T>>> EnsureNoNullElements<T>(this ArgumentUtilitiesHandle<IEnumerable<IVh<T>>> hnd) {
			if (hnd.Value == null)
				return hnd;
			else
				return hnd.AsChanged(value: P_EnsureNoNullElements(hnd));
		}

		static IEnumerable<T> P_EnsureNotEmpty<T>(ArgumentUtilitiesHandle<IEnumerable<T>> hnd) {
			if (hnd.Value == null)
				throw new ArgumentNullException(paramName: $"{nameof(hnd)}.{nameof(hnd.Value)}");
			//
			var hasElement = false;
			foreach (var element in hnd.Value) {
				hasElement = true;
				yield return element;
			}
			if (!hasElement) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Array), "CanNotEmpty")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<IEnumerable<TResult>> EnsureOfType<TSource, TResult>(this ArgumentUtilitiesHandle<IEnumerable<TSource>> hnd)
			where TSource : class
			where TResult : class, TSource {
			//
			if (hnd.Value is null)
				return new ArgumentUtilitiesHandle<IEnumerable<TResult>>(null, hnd.Name);
			else
				return
					new ArgumentUtilitiesHandle<IEnumerable<TResult>>(
						value:
							hnd
							.Value
							.Select(
								(locItem, locIndex) => {
									if (locItem is null)
										return null;
									else if (!(locItem is TResult locItemAsTResult)) {
										var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{(FormatXResource(typeof(Array), "InvalidValueAt", locIndex.ToString("d")))}{Environment.NewLine}{(FormatXResource(typeof(ArgumentException), "InvalidType/IncompatibleWithT", locItem.GetType(), typeof(TResult)))}";
										throw
											hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
											?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
									}
									else
										return locItemAsTResult;
								}),
						name: hnd.Name);
		}

		public static ArgumentUtilitiesHandle<IEnumerable<T>> Execute<T>(this ArgumentUtilitiesHandle<IEnumerable<T>> hnd) {
			if (hnd.Value != null)
				foreach (var item in hnd.Value) { }
			return hnd;
		}

		public static ArgumentUtilitiesHandle<IEnumerable<T>> EnsureValid<T>(this ArgumentUtilitiesHandle<IEnumerable<T>> hnd)
			where T : IValidatable {
			if (hnd.Value is null)
				return hnd;
			else
				return hnd.AsChanged(value: hnd.Value.Select(selector: validateItem));
			//
			T validateItem(T locItem, int locItemIndex) {
				locItem.ArgProp(name: $"{hnd.Name}[{locItemIndex.ToString("d", CultureInfo.InvariantCulture)}]", exceptionFactory: hnd.ExceptionFactory).EnsureValid();
				return locItem;
			}
		}

		public static ArgumentUtilitiesHandle<IEnumerable<T>> EnsureValid<T>(this ArgumentUtilitiesHandle<IEnumerable<T>> hnd, Action<ArgumentUtilitiesHandle<T>> validator)
			where T : IValidatable {
			if (validator is null)
				throw new ArgumentNullException(paramName: nameof(validator));
			//
			if (hnd.Value is null)
				return hnd;
			else
				return hnd.AsChanged(value: hnd.Value.Select(selector: validateItem));
			//
			T validateItem(T locItem, int locItemIndex) {
				locItem.ArgProp(name: $"{hnd.Name}[{locItemIndex.ToString("d", CultureInfo.InvariantCulture)}]", exceptionFactory: hnd.ExceptionFactory).EnsureValid(validator: validator);
				return locItem;
			}
		}

		public static ArgumentUtilitiesHandle<IEnumerable<TResult>> Select<TSource, TResult>(this ArgumentUtilitiesHandle<IEnumerable<TSource>> hnd, Func<TSource, TResult> selector) {
			selector.EnsureNotNull(nameof(selector));
			//
			return hnd.Value is null ? hnd.AsChanged(value: default(IEnumerable<TResult>)) : hnd.AsChanged(value: hnd.Value.Select(selector: selector));
		}

		public static ArgumentUtilitiesHandle<IEnumerable<TResult>> Select<TSource, TResult>(this ArgumentUtilitiesHandle<IEnumerable<TSource>> hnd, Func<TSource, int, TResult> selector) {
			selector.EnsureNotNull(nameof(selector));
			//
			return hnd.Value is null ? hnd.AsChanged(value: default(IEnumerable<TResult>)) : hnd.AsChanged(value: hnd.Value.Select(selector: selector));
		}

		public static ArgumentUtilitiesHandle<IEnumerable<TSource>> Observe<TSource>(this ArgumentUtilitiesHandle<IEnumerable<TSource>> hnd, Action<TSource> observe) {
			observe.EnsureNotNull(nameof(observe));
			//
			return hnd.Value is null ? hnd.AsChanged(value: default(IEnumerable<TSource>)) : hnd.AsChanged(value: hnd.Value.Select(selector: locItem => { observe(locItem); return locItem; }));
		}

		public static ArgumentUtilitiesHandle<IEnumerable<TSource>> Observe<TSource>(this ArgumentUtilitiesHandle<IEnumerable<TSource>> hnd, Action<TSource, int> observe) {
			observe.EnsureNotNull(nameof(observe));
			//
			return hnd.Value is null ? hnd.AsChanged(value: default(IEnumerable<TSource>)) : hnd.AsChanged(value: hnd.Value.Select(selector: (locItem, locIndex) => { observe(locItem, locIndex); return locItem; }));
		}

	}

}