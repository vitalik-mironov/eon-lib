using System;

using Eon.Metadata;

namespace Eon {

	public static class ArgUtilitiesMetadata {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TValue> ArgProp<TValue>(this IMetadata metadata, TValue value, string name)
			=>
			new ArgumentUtilitiesHandle<TValue>(
				value: value,
				name: name,
				isPropertyValue: true,
				exceptionFactory:
					(locMessage, locInnerException, locErrorCode) => {
						if (locErrorCode is null)
							return
								new MetadataValidationException(metadata: metadata, innerException: locInnerException, message: $"Property validation error. Property: '{name}'.{(string.IsNullOrEmpty(locMessage) ? string.Empty : $"{Environment.NewLine}{locMessage}")}");
						else
							return
								new MetadataValidationException(metadata: metadata, innerException: new EonException(message: locMessage, innerException: locInnerException).SetErrorCode(code: locErrorCode), message: $"Property validation error. Property: '{name}'.");
					});

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TArg> EnsureReachable<TArg>(this ArgumentUtilitiesHandle<TArg> hnd)
			where TArg : MetadataReferenceSetBase {
			if (hnd.Value != null) {
				var ownerMetadata = hnd.Value.OwnerMetadata.ArgProp($"{hnd.Name}.{nameof(hnd.Value.OwnerMetadata)}").EnsureNotNull().Value;
				try {
					var counter = -1;
					foreach (var reference in hnd.Value.Refs) {
						counter++;
						if (reference != null)
							try {
								reference.EnsureReachable(@base: ownerMetadata);
							}
							catch (Exception exception) {
								if (exception is MetadataReferenceNotResolvedException)
									throw new EonException(message: $"Элемент ссылки, указанный в позиции '{counter:d}' не указывает на объект метаданных.", innerException: exception);
								else
									throw new EonException(message: $"Элемент ссылки, указанный в позиции '{counter:d}', вызвал ошибку проверки доступности метаданных по этой ссылке.", innerException: exception);
							}
					}
				}
				catch (Exception firstException) {
					var exceptionMessage = hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'." : string.Empty;
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage, innerException: firstException)
#if TRG_SL5
						?? new ArgumentException(message: exceptionMessage+$"{Environment.NewLine}Имя параметра: '{hnd.Name}'.", innerException: firstException);
#else
						?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage, innerException: firstException);
#endif
				}
			}
			return hnd;
		}

		public static ArgumentUtilitiesHandle<TArg> EnsureReachable<TArg>(this ArgumentUtilitiesHandle<TArg> hnd, IMetadata @base)
			where TArg : class, IMetadataReference {
			@base.EnsureNotNull(nameof(@base));
			//
			if (hnd.Value != null) {
				try {
					hnd.Value.EnsureReachable(@base: @base);
				}
				catch (Exception firstException) {
					var exceptionMessage = hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'." : string.Empty;
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage, innerException: firstException)
#if TRG_SL5
						?? new ArgumentException(message: exceptionMessage+$"{Environment.NewLine}Имя параметра: '{hnd.Name}'.", innerException: firstException);
#else
						?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage, innerException: firstException);
#endif
				}
			}
			return hnd;
		}

	}

}