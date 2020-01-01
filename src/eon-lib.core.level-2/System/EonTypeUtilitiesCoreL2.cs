using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Eon;
using Eon.Threading;

namespace System {

	public static class EonTypeUtilitiesCoreL2 {

		static readonly IDictionary<string, Type> __TypeByNameRepository = new Dictionary<string, Type>(StringComparer.Ordinal);

		static readonly PrimitiveSpinLock __TypeByNameRepositorySpinLock = new PrimitiveSpinLock();

		// TODO: Put exception messages into the resources.
		//
		public static Type GetType(string typeName, bool throwOnError) {
			typeName.Arg(nameof(typeName)).EnsureNotNullOrWhiteSpace();
			//
			var result = default(Type);
			if (__TypeByNameRepositorySpinLock.Invoke(() => __TypeByNameRepository.TryGetValue(key: typeName, value: out result)))
				return result;
			else {
				var platformGetTypeException = default(Exception);
				try { result = Type.GetType(typeName: typeName, throwOnError: true); }
				catch (Exception exception) {
					result = null;
					platformGetTypeException = exception;
				}
				if (platformGetTypeException is null)
					return P_GetOrAddIntoTypeByNameRepository(typeName: typeName, type: result);
				//
				static Type returnResult(Exception locError, string locTypeName, Type locType, bool locThrowOnError) {
					if (locError is null)
						return locType;
					else if (locThrowOnError)
						throw new EonException(message: $"Ошибка при попытке получить тип по его имени '{locTypeName}'.", innerException: locError);
					else
						return null;
				}
				//
				var assemblyNamePartStartIndex = typeName.IndexOf(EonTypeUtilities.TypeAssemblyQualifiedNamePartDelimiter);
				string assemblyNamePart;
				if (assemblyNamePartStartIndex < 1 || assemblyNamePartStartIndex == typeName.Length - 1 || string.IsNullOrEmpty((assemblyNamePart = typeName.Substring(assemblyNamePartStartIndex + 1).Trim())))
					return returnResult(locError: platformGetTypeException, locTypeName: typeName, locType: null, locThrowOnError: throwOnError);
				var assemblyName = new AssemblyName(assemblyName: assemblyNamePart);
				AssemblyName loadedAssemblyName;
				var matchAssemblies =
					AppDomain
					.CurrentDomain
					.GetAssemblies()
					.Where(
						predicate:
						locItem
						=>
						string
						.Equals(
							a: (loadedAssemblyName = new AssemblyName(locItem.FullName)).Name,
							b: assemblyName.Name,
							comparisonType: StringComparison.OrdinalIgnoreCase)
						&& loadedAssemblyName.CultureName.EqualsOrdinalCS(assemblyName.CultureName)
						&& loadedAssemblyName.GetPublicKeyToken().IsEqual(array2: assemblyName.GetPublicKeyToken()))
					.Take(2)
					.ToArray();
				if (matchAssemblies.Length != 1)
					return returnResult(locError: platformGetTypeException, locTypeName: typeName, locType: null, locThrowOnError: throwOnError);
				else {
					try { result = matchAssemblies[ 0 ].GetType(name: typeName.Substring(startIndex: 0, length: assemblyNamePartStartIndex), throwOnError: true); }
					catch (Exception exception) { return returnResult(locError: exception, locTypeName: typeName, locType: null, locThrowOnError: throwOnError); }
					return P_GetOrAddIntoTypeByNameRepository(typeName: typeName, type: result);
				}
			}
		}

		static Type P_GetOrAddIntoTypeByNameRepository(string typeName, Type type) {
			typeName.EnsureNotNull(nameof(typeName));
			type.EnsureNotNull(nameof(type));
			var result = default(Type);
			return
				__TypeByNameRepositorySpinLock.Invoke(func: () => __TypeByNameRepository.TryGetValue(key: typeName, value: out result))
				? result
				: __TypeByNameRepositorySpinLock
					.Invoke(
						func:
						() => {
							if (__TypeByNameRepository.TryGetValue(key: typeName, value: out result))
								return result;
							else {
								__TypeByNameRepository.Add(key: typeName, value: type);
								return type;
							}
						});
		}

	}

}