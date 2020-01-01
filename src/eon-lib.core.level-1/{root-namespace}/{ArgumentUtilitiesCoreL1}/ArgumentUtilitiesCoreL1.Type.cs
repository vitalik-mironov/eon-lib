using System;

using Eon.Reflection;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		/// <summary>
		/// Выполняет проверку, является ли тип типом перечисления.
		/// <para>Если тип таковым не является, то вызывается исключение <see cref="ArgumentOutOfRangeException"/>.</para>
		/// </summary>
		/// <param name="hnd">Аргумент, представляющий проверяемый тип.</param>
		/// <returns>Значение <paramref name="hnd"/>.</returns>
		public static ArgumentUtilitiesHandle<Type> EnsureEnum(this ArgumentUtilitiesHandle<Type> hnd) {
			if (hnd.Value == null || hnd.Value.IsEnum())
				return hnd;
			else
				throw
					new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: FormatXResource(typeof(Type), "NotEnum", hnd.Value.ToString()));
		}

		/// <summary>
		/// Выполняет проверку, является ли тип классом.
		/// <para>Если тип не является классом, то вызывается исключение <see cref="ArgumentOutOfRangeException"/>.</para>
		/// </summary>
		/// <param name="hnd">Аргумент, представляющий проверяемый тип.</param>
		/// <returns>Значение <paramref name="hnd"/>.</returns>
		public static ArgumentUtilitiesHandle<Type> EnsureClass(this ArgumentUtilitiesHandle<Type> hnd) {
			if (hnd.Value == null || hnd.Value.IsClass())
				return hnd;
			else
				throw
					new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: FormatXResource(typeof(Type), "NotClass", hnd.Value));
		}

		public static ArgumentUtilitiesHandle<Type> EnsureClassOrInterface(this ArgumentUtilitiesHandle<Type> hnd) {
			if (hnd.Value == null || hnd.Value.IsClass() || hnd.Value.IsInterface())
				return hnd;
			throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(Type), "NotClass", hnd.Value));
		}

		/// <summary>
		/// Выполняет проверку, является ли тип значимым типом (типом структуры).
		/// <para>Если тип не является значимым типом, то вызывается исключение <see cref="ArgumentOutOfRangeException"/>.</para>
		/// </summary>
		/// <param name="hnd">Аргумент, представляющий проверяемый тип.</param>
		/// <returns>Значение <paramref name="hnd"/>.</returns>
		public static ArgumentUtilitiesHandle<Type> EnsureValueType(this ArgumentUtilitiesHandle<Type> hnd) {
			if (hnd.Value is null || hnd.Value.IsValueType())
				return hnd;
			else
				throw
					new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: FormatXResource(typeof(Type), "NotValueType", hnd.Value));
		}

		public static ArgumentUtilitiesHandle<Type> EnsureNotContainsGenericParameters(this ArgumentUtilitiesHandle<Type> hnd) {
			if (hnd.Value == null || !hnd.Value.ContainsGenericParameters())
				return hnd;
			else
				throw new ArgumentOutOfRangeException(paramName: hnd.Name, message: FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/ContainsGenericParameters", hnd.Value));
		}

		/// <summary>
		/// Выполняет проверку, совместим ли тип, указанный свойством <seealso cref="ArgumentUtilitiesHandle{TArg}.Value"/> в <paramref name="hnd"/>, с типом, указаным <paramref name="type"/>.
		/// <para>Условием успешной проверки является возможность переменной типа, указанного в <paramref name="type"/>, принимать значения типа, указанного свойством <seealso cref="ArgumentUtilitiesHandle{TArg}.Value"/> в <paramref name="hnd"/>.</para>
		/// </summary>
		/// <param name="hnd">Аргумент, представляющий проверяемый на совместимость тип.</param>
		/// <param name="type">
		/// Тип, совместимый с типом, указанным в <paramref name="hnd"/>.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Значение <paramref name="hnd"/>.</returns>
		public static ArgumentUtilitiesHandle<Type> EnsureCompatible(this ArgumentUtilitiesHandle<Type> hnd, Type type) {
			type.EnsureNotNull(nameof(type));
			//
			if (hnd.Value == null || type.IsAssignableFrom(hnd.Value))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "Type/Incompatible", hnd.Value.FmtStr().G(), type.FmtStr().G())}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		public static ArgumentUtilitiesHandle<Type> EnsureIs(this ArgumentUtilitiesHandle<Type> hnd, Type operand) {
			operand.EnsureNotNull(nameof(operand));
			//
			if (hnd.Value == null || hnd.Value == operand)
				return hnd;
			else
				throw
					new ArgumentOutOfRangeException(
						hnd.Name,
						FormatXResource(typeof(ArgumentOutOfRangeException), "Type/NotExpected", hnd.Value, operand));
		}

		public static ArgumentUtilitiesHandle<Type> EnsureIs(this ArgumentUtilitiesHandle<Type> hnd, Type operand1, Type operand2) {
			operand1.EnsureNotNull(nameof(operand1));
			operand2.EnsureNotNull(nameof(operand2));
			//
			if (hnd.Value == null || hnd.Value == operand1 || hnd.Value == operand2)
				return hnd;
			else
				throw
					new ArgumentOutOfRangeException(
						hnd.Name,
						FormatXResource(typeof(ArgumentOutOfRangeException), "Type/NotExpected", hnd.Value, operand1));
		}

	}

}