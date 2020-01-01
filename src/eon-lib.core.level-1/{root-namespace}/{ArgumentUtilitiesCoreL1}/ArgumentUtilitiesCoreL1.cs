using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureNotEq<T>(this ArgumentUtilitiesHandle<T> hnd, T operand) {
			if (EqualityComparer<T>.Default.Equals(x: hnd.Value, y: operand)) {
				var exceptionMessage = (hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty) + $"Value cannot be '{operand.FmtStr().G()}'.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(message: exceptionMessage, paramName: hnd.Name);
			}
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureEq<T>(this ArgumentUtilitiesHandle<T> hnd, T operand) {
			if (EqualityComparer<T>.Default.Equals(x: hnd.Value, y: operand))
				return hnd;
			else {
				var exceptionMessage = hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}\tExpected value:{operand.FmtStr().GNLI2()}" : $"Expected value:{operand.FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(message: exceptionMessage, paramName: hnd.Name);
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureEq<T>(this ArgumentUtilitiesHandle<T> hnd, T operand, bool skipNull) {
			if ((skipNull && hnd.Value == null) || EqualityComparer<T>.Default.Equals(x: hnd.Value, y: operand))
				return hnd;
			else {
				var exceptionMessage = hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}\tExpected value:{operand.FmtStr().GNLI2()}" : $"Expected value:{operand.FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(message: exceptionMessage, paramName: hnd.Name);
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureEqNull<T>(this ArgumentUtilitiesHandle<T> hnd) {
			if (hnd.Value == null)
				return hnd;
			else {
				var exceptionMessage = (hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty) + $"Expected value '{FormatStringUtilities.GetNullValueText(formatProvider: null)}'.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(message: exceptionMessage, paramName: hnd.Name);
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureIsNull<T>(this ArgumentUtilitiesHandle<T> hnd)
			where T : class {
			if (!(hnd.Value is null)) {
				var exceptionMessage = (hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty) + $"Expected value '{FormatStringUtilities.GetNullValueText(formatProvider: null)}'.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(message: exceptionMessage, paramName: hnd.Name);
			}
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureIs<T>(this ArgumentUtilitiesHandle<T> hnd, T operand)
			where T : class {
			if (!ReferenceEquals(hnd.Value, operand)) {
				var exceptionMessage = (hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty) + $"Expected value:{operand.FmtStr().GNLI()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(message: exceptionMessage, paramName: hnd.Name);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<T> EnsureNotDisposeState<T>(this ArgumentUtilitiesHandle<T> hnd)
			where T : IEonDisposable {
			//
			if (hnd.Value == null)
				return hnd;
			else {
				try {
					hnd.Value.EnsureNotDisposeState();
				}
				catch (ObjectDisposedException exception) {
					throw new ArgumentException(message: FormatXResource(locator: typeof(ObjectDisposedException), subpath: null), innerException: exception, paramName: hnd.Name);
				}
				return hnd;
			}
		}

		/// <summary>
		/// Выполняет проверку, является ли аргумент значением указанного типа.
		/// <para>Если аргумент не является значением указанного типа, то вызывается исключение <see cref="ArgumentOutOfRangeException"/>.</para>
		/// <para>Для null-аргумента проверка не производится.</para>
		/// </summary>
		/// <param name="hnd">Аргумент.</param>
		/// <param name="type">Тип аргумента.</param>
		/// <returns>Значение <paramref name="hnd"/>.</returns>
		public static ArgumentUtilitiesHandle<object> EnsureOfType(this ArgumentUtilitiesHandle<object> hnd, Type type) {
			type.EnsureNotNull(nameof(type));
			//
			if (hnd.Value is null || type.IsAssignableFrom(hnd.Value.GetType()))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentException), "InvalidType/ExpectedOfT", hnd.Value.GetType(), type)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TArg> EnsureOfType<TArg>(this ArgumentUtilitiesHandle<TArg> hnd, Type type) {
			type.EnsureNotNull(nameof(type)).EnsureCompatible(type: typeof(TArg));
			//
			if (hnd.Value == null || type.IsAssignableFrom(hnd.Value.GetType()))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentException), "InvalidType/ExpectedOfT", hnd.Value.GetType(), type)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureOfType<TArg, T>(this ArgumentUtilitiesHandle<TArg> hnd)
			where TArg : class
			where T : class {
			T newArgValue;
			if (hnd.Value is null)
				return hnd.AsChanged(value: default(T));
			else if ((newArgValue = hnd.Value as T) is null) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentException), "InvalidType/ExpectedOfT", hnd.Value.GetType(), typeof(T))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd.AsChanged(value: newArgValue);
		}

		public static ArgumentUtilitiesHandle<T> EnsureOfType<T>(this ArgumentUtilitiesHandle<object> hnd)
			where T : class
			=> EnsureOfType<object, T>(hnd: hnd);

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<CancellationToken> EnsureNotEmpty(this ArgumentUtilitiesHandle<CancellationToken> hnd) {
			if (hnd.Value == CancellationToken.None)
				throw new ArgumentOutOfRangeException(hnd.Name, $"Указанный токен отмены является пустым (т.е. '{nameof(CancellationToken)}.{nameof(CancellationToken.None)}').");
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<CancellationToken> EnsureNotEmpty(this CancellationToken argument, string argumentName)
			=> argument.Arg(argumentName).EnsureNotEmpty();

		public static ArgumentUtilitiesHandle<object> AsObject<T>(this ArgumentUtilitiesHandle<T> hnd)
			=> new ArgumentUtilitiesHandle<object>(value: hnd.Value, name: hnd.Name, isPropertyValue: hnd.IsProp, exceptionFactory: hnd.ExceptionFactory);

		public static ArgumentUtilitiesHandle<TResult> AsDerived<TSource, TResult>(this ArgumentUtilitiesHandle<TSource> hnd)
			where TSource : class
			where TResult : class, TSource
			=> new ArgumentUtilitiesHandle<TResult>(value: hnd.Value as TResult, name: hnd.Name, isPropertyValue: hnd.IsProp, exceptionFactory: hnd.ExceptionFactory);

		public static ArgumentUtilitiesHandle<TResult> AsBase<TSource, TResult>(this ArgumentUtilitiesHandle<TSource> hnd)
			where TSource : class, TResult
			where TResult : class
			=> new ArgumentUtilitiesHandle<TResult>(value: hnd.Value, name: hnd.Name, isPropertyValue: hnd.IsProp, exceptionFactory: hnd.ExceptionFactory);

		public static ArgumentUtilitiesHandle<TResult> AsNonDerived<TSource, TResult>(this ArgumentUtilitiesHandle<TSource> hnd)
			where TSource : class
			where TResult : class
			=> new ArgumentUtilitiesHandle<TResult>(value: hnd.Value as TResult, name: hnd.Name, isPropertyValue: hnd.IsProp, exceptionFactory: hnd.ExceptionFactory);

		[Obsolete(message: "Do not use. Use AsNonDerived or AsDerived.", error: true)]
		public static ArgumentUtilitiesHandle<TResult> As<TSource, TResult>(this ArgumentUtilitiesHandle<TSource> hnd)
			where TSource : class
			where TResult : class, TSource
			=> new ArgumentUtilitiesHandle<TResult>(value: hnd.Value as TResult, name: hnd.Name, isPropertyValue: hnd.IsProp, exceptionFactory: hnd.ExceptionFactory);

		public static ArgumentUtilitiesHandle<T> AsReadOnly<T>(this ArgumentUtilitiesHandle<T> argument)
			where T : IAsReadOnly<T> {
			if (argument.Value == null)
				return argument;
			else
				return new ArgumentUtilitiesHandle<T>(value: argument.Value.AsReadOnly(), name: argument.Name, isPropertyValue: argument.IsProp, exceptionFactory: argument.ExceptionFactory);
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureValid<T>(this ArgumentUtilitiesHandle<T> hnd)
			where T : IValidatable {
			if (hnd.Value != null) {
				try {
					hnd.Value.Validate();
				}
				catch (Exception validationException) {
					Exception rethrowException;
					try {
						var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : "Invalid value.")}";
						rethrowException =
							hnd.ExceptionFactory?.Invoke(message: exceptionMessage, innerException: validationException)
							??
							new ArgumentException(paramName: hnd.Name, message: exceptionMessage, innerException: validationException);
					}
					catch (Exception secondException) {
						throw new AggregateException(validationException, secondException);
					}
					throw rethrowException;
				}
			}
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureValid<T>(this ArgumentUtilitiesHandle<T> hnd, Action<ArgumentUtilitiesHandle<T>> validator) {
			if (validator is null)
				throw new ArgumentNullException(paramName: nameof(validator));
			//
			if (hnd.Value != null) {
				try {
					validator(obj: hnd);
				}
				catch (Exception validationException) {
					Exception rethrowException;
					try {
						var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : "Invalid value.")}";
						rethrowException =
							hnd.ExceptionFactory?.Invoke(message: exceptionMessage, innerException: validationException)
							??
							new ArgumentException(paramName: hnd.Name, message: exceptionMessage, innerException: validationException);
					}
					catch (Exception secondException) {
						throw new AggregateException(validationException, secondException);
					}
					throw rethrowException;
				}
			}
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<IVh<T>> EnsureSelfAndValueNotNull<T>(this ArgumentUtilitiesHandle<IVh<T>> hnd) {
			if (hnd.Value is null) {
				var exceptionMessage = hnd.IsProp ? $"Не указано значение свойства '{hnd.Name}'." : $"Значение не может быть '{FormatStringUtilities.GetNullValueText()}'.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentNullException(message: exceptionMessage, paramName: hnd.Name);
			}
			else if (hnd.Value.HasException)
				return hnd;
			else {
				string exceptionMessage;
				T value;
				try {
					value = hnd.Value.ValueDisposeTolerant;
				}
				catch (Exception exception) {
					exceptionMessage = $"Ошибка при доступе к значению в хранилище значения (свойство '{hnd.Value.Value}'){(hnd.IsProp ? $", представленного свойством '{hnd.Name}'" : string.Empty)}.";
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage, innerException: exception)
						?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage, innerException: exception);
				}
				if (value == null && !hnd.Value.IsDisposeRequested) {
					exceptionMessage = $"В хранилище значения{(hnd.IsProp ? $", представленного свойством '{hnd.Name}'," : string.Empty)} значение отсутствует (т.е. = '{FormatStringUtilities.GetNullValueText()}').";
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
						?? new ArgumentNullException(message: exceptionMessage, paramName: hnd.Name);
				}
				return hnd;
			}
		}

		// TODO: Put strings into the resources.
		//
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static NullReferenceException NewNullReferenceException(string varName, object component)
			=> new NullReferenceException(message: $"Variable '{varName}' doesn't point to an object.{(component is null ? string.Empty : $"{Environment.NewLine}\tComponent:{component.FmtStr().GNLI2()}")}");

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TEnum> EnsureDefinedEnum<TEnum>(this ArgumentUtilitiesHandle<TEnum> hnd)
			where TEnum : struct {
			if (typeof(TEnum).IsEnum) {
				if (Enum.IsDefined(enumType: typeof(TEnum), value: hnd.Value))
					return hnd;
				else {
					var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'." : "Invalid value.")}{Environment.NewLine}Value is not defined on enum type.{Environment.NewLine}\tValue:{hnd.Value.FmtStr().GNLI2()}{Environment.NewLine}\tType:{typeof(TEnum).FmtStr().GNLI2()}";
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
						?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
				}
			}
			else
				throw new ArgumentOutOfRangeException(paramName: nameof(TEnum), message: $"The type '{typeof(TEnum)}' is not enum type.");
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TEnum?> EnsureDefinedEnum<TEnum>(this ArgumentUtilitiesHandle<TEnum?> hnd)
			where TEnum : struct {
			if (typeof(TEnum).IsEnum) {
				if (hnd.Value.HasValue) {
					if (Enum.IsDefined(enumType: typeof(TEnum), value: hnd.Value.Value))
						return hnd;
					else {
						var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'." : "Invalid value.")}{Environment.NewLine}Value is not defined on enum type.{Environment.NewLine}\tValue:{hnd.Value.Value.FmtStr().GNLI2()}{Environment.NewLine}\tType:{typeof(TEnum).FmtStr().GNLI2()}";
						throw
							hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
							?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
					}
				}
				else
					return hnd;
			}
			else
				throw new ArgumentOutOfRangeException(paramName: nameof(TEnum), message: $"The type '{typeof(TEnum)}' is not enum type.");
		}

	}

}