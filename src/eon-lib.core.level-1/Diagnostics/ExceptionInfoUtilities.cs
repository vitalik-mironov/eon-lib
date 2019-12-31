using System;
using System.Collections.Generic;

using Eon.Threading;

namespace Eon.Diagnostics {

	public static class ExceptionInfoUtilities {

		#region Static members

		/// <summary>
		/// Значение: '160'.
		/// </summary>
		public static readonly int MaxAllowedCountOfRegisteredTranslators = 160;

		static readonly Dictionary<Type, IExceptionInfoTranslator> __RegisteredTranslatorsRepository = new Dictionary<Type, IExceptionInfoTranslator>();

		static readonly PrimitiveSpinLock __SpinLock = new PrimitiveSpinLock();

		#endregion

		// TODO: Put exception messages into the resources.
		//
		public static void RegisterTranslator(Type exceptionType, IExceptionInfoTranslator parser) {
			exceptionType
				.EnsureNotNull(nameof(exceptionType))
				.EnsureCompatible(typeof(Exception));
			parser.EnsureNotNull(nameof(parser));
			//
			__SpinLock
				.Invoke(
					() => {
						if (__RegisteredTranslatorsRepository.ContainsKey(exceptionType))
							__RegisteredTranslatorsRepository[ exceptionType ] = parser;
						else if (__RegisteredTranslatorsRepository.Count >= MaxAllowedCountOfRegisteredTranslators)
							throw new InvalidOperationException("It is impossible to register an exception info parser. The count of registered parsers has reached the max. allowed limit.");
						else
							__RegisteredTranslatorsRepository.Add(exceptionType, parser);
					});
		}

		public static bool UnregisterTranslator(Type exceptionType, IExceptionInfoTranslator parser) {
			exceptionType
				.EnsureNotNull(nameof(exceptionType))
				.EnsureOfType(typeof(Exception));
			parser.EnsureNotNull(nameof(parser));
			//
			return
				__SpinLock
				.Invoke(() => __RegisteredTranslatorsRepository.Remove(exceptionType));
		}

		public static int CountOfRegisteredParsers
			=> __RegisteredTranslatorsRepository.Count;

		public static IExceptionInfoTranslator SelectTranslator(Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			return
				__SpinLock
				.Invoke(
					() => {
						if (__RegisteredTranslatorsRepository.TryGetValue(exception.GetType(), out var result))
							return result;
						else
							return null;
					});
		}

	}

}