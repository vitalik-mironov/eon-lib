using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Eon.Linq;
using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	[Obsolete(message: "Use '" + nameof(ArgumentUtilities) + "' classes instead. Type will be removed.", error: false)]
	public static class MetadataValidationUtilities {

		public delegate bool ValueValidator<in TValue>(TValue value, out string errorMessage);

		// TODO: Put strings into the resources.
		//
		static MetadataValidationException P_CreateMetadataValidationException<TMetadata, TProperty>(
			this MetadataPropertyHandle<TMetadata, TProperty> hnd,
			Exception innerException)
			where TMetadata : class, IMetadata
			=>
			new MetadataValidationException(
				message: $"Валидация свойства '{hnd.PropertyName}' выявила ошибку(-и).",
				metadata: hnd.Metadata,
				innerException: innerException);

		// TODO: Put strings into the resources.
		//
		public static MetadataPropertyHandle<TMetadata, TPropertyValue> EnsureNotNull<TMetadata, TPropertyValue>(
			this MetadataPropertyHandle<TMetadata, TPropertyValue> hnd)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			if (hnd.PropertyValue == null)
				throw new MetadataValidationException($"Не указано значение свойства '{hnd.PropertyName}'.", hnd.Metadata);
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static MetadataPropertyHandle<TMetadata, string> EnsureNotEmptyOrWhiteSpace<TMetadata>(
			this MetadataPropertyHandle<TMetadata, string> hnd)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			if (hnd.PropertyValue == null || !string.IsNullOrWhiteSpace(hnd.PropertyValue))
				return hnd;
			else
				throw
					new MetadataValidationException(
						$"Валидация значения свойства '{hnd.PropertyName}' выявила ошибку(-и).{Environment.NewLine}{FormatXResource(typeof(string), "CanNotEmptyOrWhiteSpace")}{Environment.NewLine}\tЗначение:{hnd.PropertyValue.FmtStr().GNLI2()}", hnd.Metadata);
		}

		// TODO: Put strings into the resources.
		//
		public static MetadataPropertyHandle<TMetadata, TPropertyValue> EnsureNotEmpty<TMetadata, TPropertyValue>(
			this MetadataPropertyHandle<TMetadata, TPropertyValue> hnd)
			where TMetadata : class, IMetadata
			where TPropertyValue : IEnumerable {
			//
			hnd.EnsureHandleValid();
			//
			if (hnd.PropertyValue == null || hnd.PropertyValue.OfType<object>().Take(1).Count() > 0)
				return hnd;
			else
				throw
					new MetadataValidationException(
						$"Валидация значения свойства '{hnd.PropertyName}' выявила ошибку(-и).{Environment.NewLine}{FormatXResource(typeof(Array), "CanNotEmpty")}{Environment.NewLine}\tЗначение:{hnd.PropertyValue.FmtStr().GNLI2()}",
						hnd.Metadata);
		}

		public static MetadataPropertyHandle<TMetadata, int?> EnsureBetween<TMetadata>(
			this MetadataPropertyHandle<TMetadata, int?> hnd,
			int bound1Inclusive,
			int bound2Inclusive)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.Arg(nameof(hnd.PropertyValue))
					.EnsureBetween(bound1Inclusive, bound2Inclusive);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, long> EnsureNotLessThan<TMetadata>(
			this MetadataPropertyHandle<TMetadata, long> hnd,
			long operand)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.Arg(nameof(hnd.PropertyValue))
					.EnsureNotLessThan(operand);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, long?> EnsureNotLessThan<TMetadata>(
			this MetadataPropertyHandle<TMetadata, long?> hnd,
			long operand)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.Arg(nameof(hnd.PropertyValue))
					.EnsureNotLessThan(operand);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, int?> EnsureNotLessThan<TMetadata>(
			this MetadataPropertyHandle<TMetadata, int?> hnd,
			int operand)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.Arg(nameof(hnd.PropertyValue))
					.EnsureNotLessThan(operand);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, int> EnsureNotLessThan<TMetadata>(
			this MetadataPropertyHandle<TMetadata, int> hnd,
			int operand)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.Arg(nameof(hnd.PropertyValue))
					.EnsureNotLessThan(operand);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, TimeoutDuration> EnsureNotInfinite<TMetadata>(
			this MetadataPropertyHandle<TMetadata, TimeoutDuration> hnd)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.ArgProp(nameof(hnd.PropertyValue))
					.EnsureNotInfinite();
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, TMetadataRef> EnsureReachable<TMetadata, TMetadataRef>(
			this MetadataPropertyHandle<TMetadata, TMetadataRef> hnd)
			where TMetadata : class, IMetadata
			where TMetadataRef : class, IMetadataReference {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.EnsureReachable(@base: hnd.Metadata);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, string> EnsureHasMaxLength<TMetadata>(
			this MetadataPropertyHandle<TMetadata, string> hnd,
			int maxLength)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.Arg(nameof(hnd.PropertyValue))
					.EnsureHasMaxLength(maxLength);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, TPropertyValueItem[ ]> EnsureNoNullElements<TMetadata, TPropertyValueItem>(
			this MetadataPropertyHandle<TMetadata, TPropertyValueItem[ ]> hnd)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.Arg(nameof(hnd.PropertyValue))
					.EnsureNoNullElements();
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static MetadataPropertyHandle<TMetadata, TPropertyValue> EnsureValid<TMetadata, TPropertyValue>(this MetadataPropertyHandle<TMetadata, TPropertyValue> hnd, ValueValidator<TPropertyValue> validator)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			validator.EnsureNotNull(nameof(validator));
			//
			if (hnd.PropertyValue != null) {
				bool validationResult;
				string validationErrorMessage;
				try {
					validationResult =
						validator(
							value: hnd.PropertyValue,
							errorMessage: out validationErrorMessage);
				}
				catch (Exception firstException) {
					throw
						new MetadataValidationException(
							message: $"Ошибка валидации свойства '{hnd.PropertyName}'.",
							innerException: firstException,
							metadata: hnd.Metadata);
				}
				if (!validationResult)
					throw
						new MetadataValidationException(
							message: $"Валидация свойства '{hnd.PropertyName}' выявила ошибку(-и).{Environment.NewLine}{validationErrorMessage}",
							metadata: hnd.Metadata);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, TPropertyValue> EnsureValid<TMetadata, TPropertyValue>(
			this MetadataPropertyHandle<TMetadata, TPropertyValue> hnd)
			where TMetadata : class, IMetadata
			where TPropertyValue : IValidatable {
			//
			hnd.EnsureHandleValid();
			//
			if (hnd.PropertyValue != null)
				try {
					hnd.PropertyValue.Validate();
				}
				catch (Exception firstException) {
					throw hnd.P_CreateMetadataValidationException(firstException);
				}
			//
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static MetadataPropertyHandle<TMetadata, TPropertyValue> EnsureAllValid<TMetadata, TPropertyValue>(
			this MetadataPropertyHandle<TMetadata, TPropertyValue> hnd)
			where TMetadata : class, IMetadata
			where TPropertyValue : IEnumerable<IValidatable> {
			//
			hnd.EnsureHandleValid();
			//
			if (hnd.PropertyValue != null) {
				try {
					var itemPositionCounter = -1;
					foreach (var item in hnd.PropertyValue.SkipNull()) {
						itemPositionCounter++;
						try {
							item.Validate();
						}
						catch (Exception firstException) {
							throw
								new MetadataValidationException(
									message: $"Валидация элемента набора свойства '{hnd.PropertyName}' выявила ошибку(-и).{Environment.NewLine}\tПозиция элемента:{Environment.NewLine}{itemPositionCounter.FmtStr().Decimal().IndentLines2()}",
									metadata: hnd.Metadata,
									innerException: firstException);
						}
					}
				}
				catch (Exception firstException) when (!(firstException is MetadataValidationException)) {
					throw
						new MetadataValidationException(
							message: $"Сбой валидации набора свойства '{hnd.PropertyName}'.",
							metadata: hnd.Metadata,
							innerException: firstException);
				}
			}
			//
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static MetadataPropertyHandle<TMetadata, TPropertyValue> EnsureAllNotNull<TMetadata, TPropertyValue>(
			this MetadataPropertyHandle<TMetadata, TPropertyValue> hnd)
			where TMetadata : class, IMetadata
			where TPropertyValue : IEnumerable {
			//
			hnd.EnsureHandleValid();
			//
			if (hnd.PropertyValue != null) {
				try {
					var itemPositionCounter = -1;
					foreach (var item in hnd.PropertyValue) {
						itemPositionCounter++;
						if (item == null)
							throw
								new MetadataValidationException(
									message: $"Указано недопустимое значение свойства '{hnd.PropertyName}'.{Environment.NewLine}{FormatXResource(typeof(Array), "CanNotContainNull/NullAt", itemPositionCounter.ToString("d"))}",
									metadata: hnd.Metadata);
					}
				}
				catch (Exception firstException) when (!(firstException is MetadataValidationException)) {
					throw
						new MetadataValidationException(
							message: $"Сбой валидации набора свойства '{hnd.PropertyName}'.",
							metadata: hnd.Metadata,
							innerException: firstException);
				}
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, string> EnsureNotContains<TMetadata>(
			this MetadataPropertyHandle<TMetadata, string> hnd,
			char value)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.ArgProp(nameof(hnd.PropertyValue))
					.EnsureNotContains(value);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, string> EnsureNotEquals<TMetadata>(
			this MetadataPropertyHandle<TMetadata, string> hnd,
			ArgumentUtilitiesHandle<string> operand,
			StringComparison comparison)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.ArgProp(nameof(hnd.PropertyValue))
					.EnsureNotEquals(operand, comparison);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, TimeSpan> EnsureNotLessThan<TMetadata>(
			this MetadataPropertyHandle<TMetadata, TimeSpan> hnd,
			TimeSpan operand)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.ArgProp(nameof(hnd.PropertyValue))
					.EnsureNotLessThan(operand);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, TimeSpan?> EnsureNotLessThan<TMetadata>(
			this MetadataPropertyHandle<TMetadata, TimeSpan?> hnd,
			TimeSpan operand)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			if (hnd.PropertyValue.HasValue)
				try {
					hnd
						.PropertyValue
						.Value
						.ArgProp(nameof(hnd.PropertyValue))
						.EnsureNotLessThan(operand);
				}
				catch (ArgumentException firstException) {
					throw hnd.P_CreateMetadataValidationException(firstException);
				}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, TimeSpan> EnsureGreaterThanZero<TMetadata>(
			this MetadataPropertyHandle<TMetadata, TimeSpan> hnd)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.ArgProp(nameof(hnd.PropertyValue))
					.EnsureGreaterThanZero();
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

		public static MetadataPropertyHandle<TMetadata, TimeSpan> EnsureGreaterThan<TMetadata>(
			this MetadataPropertyHandle<TMetadata, TimeSpan> hnd,
			TimeSpan operand)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			//
			try {
				hnd
					.PropertyValue
					.ArgProp(nameof(hnd.PropertyValue))
					.EnsureGreaterThan(operand);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
			//
			return hnd;
		}

#if TRG_NETSTANDARD1_5

		public static MetadataPropertyHandle<TMetadata, Regex> EnsureValidRegex<TMetadata>(
			this MetadataPropertyHandle<TMetadata, string> hnd,
			RegexOptions regexOptions)
			where TMetadata : class, IMetadata {
			//
			hnd.EnsureHandleValid();
			hnd.EnsureNotEmpty();
			//
			try {
				var regex =
					hnd
					.PropertyValue
					.ArgProp(nameof(hnd.PropertyValue))
					.ParseRegex(regexOptions: regexOptions)
					.Value;
				return
					new MetadataPropertyHandle<TMetadata, Regex>(
						metadata: hnd.Metadata,
						propertyName: hnd.PropertyName,
						propertyValue: regex);
			}
			catch (ArgumentException firstException) {
				throw hnd.P_CreateMetadataValidationException(firstException);
			}
		}

		public static MetadataPropertyHandle<TMetadata, Regex> EnsureValidRegex<TMetadata>(
			this MetadataPropertyHandle<TMetadata, string> hnd)
			where TMetadata : class, IMetadata
			=>
			hnd
			.EnsureValidRegex(
				regexOptions: RegexOptions.None);

#endif

	}

}