using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Eon.Collections;
using Eon.Reflection;

namespace Eon {

	public static class FluentApiUtilities {

		public static FluentApiUtilitiesHandle<T> Fluent<T>(this T value)
			=> new FluentApiUtilitiesHandle<T>(value: value);

		public static FluentApiUtilitiesHandle<TResult> Select<TSource, TResult>(this FluentApiUtilitiesHandle<TSource> hnd, Func<TSource, TResult> selector) {
			selector.EnsureNotNull(nameof(selector));
			//
			return new FluentApiUtilitiesHandle<TResult>(value: selector(arg: hnd.Value));
		}

		public static void NullCond<TArg>(this FluentApiUtilitiesHandle<TArg> hnd, Action<TArg> action) {
			if (action is null)
				throw new ArgumentNullException(paramName: nameof(action));
			//
			if (hnd.Value != null)
				action(obj: hnd.Value);
		}

		public static TResult NullCond<TArg, TResult>(this FluentApiUtilitiesHandle<TArg> hnd, Func<TArg, TResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			if (hnd.Value == null)
				return default;
			else
				return func(hnd.Value);
		}

		/// <summary>
		/// Конвертирует указанное значение (объект) <paramref name="hnd"/>, используя функцию-конвертер <paramref name="converter"/>.
		/// <para>Если указанное значение есть <see langword="null"/>, то метод не вызывает функцию-конвертер и возвращает <see langword="null"/>.</para>
		/// <para>Если указанное значение может быть приведено к типу <typeparamref name="TResult"/>, то метод не вызывает функцию-конвертер и возвращает приведенное к типу <typeparamref name="TResult"/> указанное значение.</para>
		/// <para>Для других случаев метод возвращает результат функции-конвертера <paramref name="converter"/>.</para>
		/// </summary>
		/// <typeparam name="TSource">Тип исходного значения.</typeparam>
		/// <typeparam name="TResult">Тип конвертации исходного значения.</typeparam>
		/// <param name="hnd">Конвертируемое значение.</param>
		/// <param name="converter">
		/// Функция-конвертер.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <typeparamref name="TResult"/>.</returns>
		public static TResult ConvertNullCond<TSource, TResult>(this FluentApiUtilitiesHandle<TSource> hnd, Func<TSource, TResult> converter)
			where TResult : class {
			//
			if (converter is null)
				throw new ArgumentNullException(paramName: nameof(converter));
			//
			return hnd.Value == null ? null : (hnd as TResult ?? converter(hnd.Value));
		}

		/// <summary>
		/// Конвертирует указанное значение (объект) <paramref name="hnd"/>, используя функцию-конвертер <paramref name="converter"/>.
		/// <para>Если указанное значение может быть приведено к типу <typeparamref name="TResult"/>, то метод не вызывает функцию-конвертер и возвращает приведенное к типу <typeparamref name="TResult"/> значение.</para>
		/// </summary>
		/// <typeparam name="TSource">Тип исходного значения.</typeparam>
		/// <typeparam name="TResult">Тип конвертации исходного значения.</typeparam>
		/// <param name="hnd">
		/// Конвертируемое значение.
		/// <para>Не может указывать на <see langword="null"/> (<see cref="FluentApiUtilitiesHandle{T}.Value"/>).</para>
		/// </param>
		/// <param name="converter">
		/// Функция-конвертер.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <typeparamref name="TResult"/>.</returns>
		public static TResult ConvertCond<TSource, TResult>(this FluentApiUtilitiesHandle<TSource> hnd, Func<TSource, TResult> converter)
			where TResult : class {
			//
			if (hnd.Value == null)
				throw new ArgumentNullException(paramName: $"{nameof(hnd)}.{nameof(hnd.Value)}");
			else if (converter is null)
				throw new ArgumentNullException(paramName: nameof(converter));
			//
			return (hnd.Value as TResult ?? converter(arg: hnd.Value));
		}

		public static TResult IfTrue<TResult>(this FluentApiUtilitiesHandle<TResult> hnd, bool condition, Func<TResult, TResult> trueBody)
			=> condition ? trueBody.EnsureNotNull(nameof(trueBody)).Value(hnd.Value) : hnd.Value;

		public static TResult IfTrue<TResult>(this FluentApiUtilitiesHandle<TResult> hnd, Func<TResult, bool> condition, TResult trueValue)
			=> condition.EnsureNotNull(nameof(condition)).Value(hnd.Value) ? trueValue : hnd.Value;

		public static TResult Convert<TSource, TResult>(this TSource value, Func<TResult> nullConverter, Func<TSource, TResult> converter)
			where TResult : class
			=> value == null ? nullConverter.EnsureNotNull(nameof(nullConverter)).Value() : (value as TResult ?? converter.EnsureNotNull(nameof(converter)).Value(value));

		public static TResult If<T, TResult>(this FluentApiUtilitiesHandle<T> hnd, Func<T, bool> condition, Func<T, TResult> trueBody, Func<T, TResult> falseBody) {
			condition.EnsureNotNull(nameof(condition));
			trueBody.EnsureNotNull(nameof(trueBody));
			falseBody.EnsureNotNull(nameof(falseBody));
			//
			return condition(hnd.Value) ? trueBody(hnd.Value) : falseBody(hnd.Value);
		}

		public static TResult If<T, TResult>(this FluentApiUtilitiesHandle<T> hnd, bool condition, Func<T, TResult> trueBody, Func<T, TResult> falseBody) {
			trueBody.EnsureNotNull(nameof(trueBody));
			falseBody.EnsureNotNull(nameof(falseBody));
			//
			return condition ? trueBody(hnd.Value) : falseBody(hnd.Value);
		}

		public static TResult Convert<TSource, TResult>(this FluentApiUtilitiesHandle<TSource> hnd, Func<TSource, TResult> converter)
			=> converter.EnsureNotNull(nameof(converter)).Value(hnd.Value);

		/// <summary>
		/// Конвертирует указанную строку (<paramref name="hnd"/>) посредством функции-конвертера <paramref name="converter"/>.
		/// <para>Если указанная строка есть <see langword="null"/> или <see cref="string.Empty"/>, то функция-конвертер не вызывается и возвращается <see langword="null"/>.</para>
		/// </summary>
		/// <typeparam name="TResult">Тип конвертации.</typeparam>
		/// <param name="hnd">Конвертируемая строка.</param>
		/// <param name="converter">
		/// Функция-конвертер.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <typeparamref name="TResult"/>.</returns>
		public static TResult ConvertNullOrEmptyCond<TResult>(this FluentApiUtilitiesHandle<string> hnd, Func<string, TResult> converter) {
			converter.EnsureNotNull(nameof(converter));
			//
			return string.IsNullOrEmpty(hnd.Value) ? default : converter(hnd.Value);
		}

		public static Task NullCondInvoke(this FluentApiUtilitiesHandle<Func<Task>> hnd) {
			if (hnd.Value is null)
				return Task.CompletedTask;
			else
				try {
					return hnd.Value();
				}
				catch (Exception exception) {
					return Task.FromException(exception: exception);
				}
		}

		public static Task NullCondInvoke<TArg1>(this FluentApiUtilitiesHandle<Func<TArg1, Task>> hnd, TArg1 arg) {
			if (hnd.Value is null)
				return Task.CompletedTask;
			else
				try {
					return hnd.Value(arg: arg);
				}
				catch (Exception exception) {
					return Task.FromException(exception: exception);
				}
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static Task NullCondInvoke<TArg1, TArg2>(this FluentApiUtilitiesHandle<Func<TArg1, TArg2, Task>> hnd, TArg1 arg1, TArg2 arg2) {
			if (hnd.Value is null)
				return Task.CompletedTask;
			else
				try {
					return hnd.Value(arg1: arg1, arg2: arg2);
				}
				catch (Exception exception) {
					return Task.FromException(exception: exception);
				}
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static Task NullCondInvoke<TArg1, TArg2, TArg3>(this FluentApiUtilitiesHandle<Func<TArg1, TArg2, TArg3, Task>> hnd, TArg1 arg1, TArg2 arg2, TArg3 arg3) {
			if (hnd.Value is null)
				return Task.CompletedTask;
			else
				try {
					return hnd.Value(arg1: arg1, arg2: arg2, arg3: arg3);
				}
				catch (Exception exception) {
					return Task.FromException(exception: exception);
				}
		}

		public static T Default<T>(this FluentApiUtilitiesHandle<T> hnd)
			=> default;

		public static TResult ConvertNullCond<T, TResult>(this FluentApiUtilitiesHandle<T> hnd, bool isReadOnly)
			where T : class, IAsReadOnly<T>
			where TResult : class, T
			=> AsReadOnlyUtilities.AsReadOnlyIf<T, TResult>(value: hnd.Value, condition: isReadOnly);

		public static void ConvertNullCond<T, TResult>(this FluentApiUtilitiesHandle<T> hnd, bool isReadOnly, out TResult result)
			where T : class, IAsReadOnly<T>
			where TResult : class, T
			=> result = AsReadOnlyUtilities.AsReadOnlyIf<T, TResult>(value: hnd.Value, condition: isReadOnly);

		public static void ConvertNullCond<T, TResult>(this FluentApiUtilitiesHandle<IEnumerable<T>> hnd, bool isReadOnly, bool emptyIfNull, out IList<TResult> result)
			where T : class, IAsReadOnly<T>
			where TResult : class, T
			=> result = AsReadOnlyUtilities.AsReadOnlyIf<T, TResult>(source: hnd.Value, condition: isReadOnly, emptyIfNull: emptyIfNull);

	}

}