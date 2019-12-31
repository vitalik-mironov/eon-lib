using System;
using System.Linq.Expressions;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		#region Nested types

		static class P_CastFunc<TFrom, TTo> {

			#region Static members

			public static readonly Func<TFrom, TTo> Implementation;

			public static readonly Type FromType;

			public static readonly Type ToType;

			#endregion

			static P_CastFunc() {
				FromType = typeof(TFrom);
				ToType = typeof(TTo);
				var fromParameter = Expression.Parameter(FromType, "from");
				Implementation = Expression.Lambda<Func<TFrom, TTo>>(ToType.IsPrimitive ? Expression.ConvertChecked(fromParameter, ToType) : Expression.Convert(fromParameter, ToType), fromParameter).Compile();
			}

		}

		#endregion

		// TODO: Put strings into the resources.
		//
		public static TTo Cast<TFrom, TTo>(this TFrom value) {
			try {
				return P_CastFunc<TFrom, TTo>.Implementation(value);
			} catch (TypeInitializationException exception) {
				throw
					new InvalidCastException(
						message: $"Ошибка при явном приведении значения типа '{typeof(TFrom)}' к типу '{typeof(TTo)}'.",
						innerException: exception);
			}
		}

		public static TTo Cast<TFrom, TTo>(this Func<TFrom> valueGetter)
			=> Cast<TFrom, TTo>(value: valueGetter.EnsureNotNull(nameof(valueGetter)).Value());

		public static ArgumentUtilitiesHandle<TTo> Cast<TFrom, TTo>(this ArgumentUtilitiesHandle<TFrom> hnd) {
			try {
				return hnd.AsChanged(value: hnd.Value.Cast<TFrom, TTo>());
			} catch (Exception exception) {
				throw
					new ArgumentException(
						message: $"{FormatXResource(typeof(ArgumentException), "InvalidType/IncompatibleWithT", hnd.Value?.GetType() ?? typeof(TFrom), typeof(TTo))}{Environment.NewLine}Имя параметра: '{hnd.Name}'.",
						innerException: exception);
			}
		}

	}

}