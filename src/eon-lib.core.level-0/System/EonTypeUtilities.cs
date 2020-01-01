using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Eon;
using Eon.Reflection;

namespace System {

	public static class EonTypeUtilities {

		public const char TypeAssemblyQualifiedNamePartDelimiter = ',';

		public const string TypeAssemblyQualifiedNamePartDelimiterString = ",";

		public const char TypeNameDelimiter = '.';

		public const char TypeNameGenericParametersMarker = '`';

		static readonly Type __NullableTypeDefinition = typeof(Nullable<>);

		static readonly IDictionary<string, Type> __TypeByNameRepository = new Dictionary<string, Type>(StringComparer.Ordinal);

		public static IEnumerable<Type> GetHierarchyOfGenericDefinitions(this Type type, bool includeSelf = true) {
			type.EnsureNotNull(nameof(type));
			//
			if (includeSelf && type.IsGenericType())
				yield return type.IsGenericTypeDefinition() ? type : type.GetGenericTypeDefinition();
			// Return as for non-interface type.
			//
			if (!type.IsInterface()) {
				var currentType = type.BaseType();
				for (; currentType != null;) {
					if (currentType.IsGenericType())
						yield return currentType.GetGenericTypeDefinition();
					currentType = currentType.BaseType();
				}
			}
			// Return as for interface type.
			//
			foreach (var interfaceType in type.GetInterfaces()) {
				if (interfaceType.IsGenericType())
					yield return interfaceType.GetGenericTypeDefinition();
			}
		}

		public static bool IsNullable(this Type type) {
			type.EnsureNotNull(nameof(type));
			//
			return type.IsGenericType() && (type.IsGenericTypeDefinition() ? type : type.GetGenericTypeDefinition()) == __NullableTypeDefinition;
		}

		public static bool IsNullableEnum(this Type type) => type.IsNullable() && type.GetGenericArguments()[ 0 ].IsEnum();

		public static bool IsGenericType(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsGenericType;

		public static Assembly GetAssembly(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.Assembly;

		public static bool IsGenericTypeDefinition(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsGenericTypeDefinition;

		public static bool IsEnum(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsEnum;

		public static bool IsInterface(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsInterface;

		public static bool IsClass(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsClass;

		public static bool IsAbstract(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsAbstract;

		public static bool IsValueType(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsValueType;

		public static bool IsPrimitive(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsPrimitive;

		public static bool IsSealed(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.IsSealed;

		public static Type BaseType(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.BaseType;

		public static bool ContainsGenericParameters(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value
#if TRG_NETSTANDARD1_5 && !TRG_NETFRAMEWORK
			.GetTypeInfo()
#endif
			.ContainsGenericParameters;

#if TRG_NETSTANDARD1_5

#if !TRG_NETFRAMEWORK

		public static Type GetGenericTypeDefinition(this Type type)
			=>
			type.EnsureNotNull(nameof(type)).Value.GetTypeInfo().GetGenericTypeDefinition();

		public static bool IsSubclassOf(this Type type, Type ofType)
			=> type.EnsureNotNull(nameof(type)).Value.GetTypeInfo().IsSubclassOf(ofType.EnsureNotNull(nameof(ofType)));

		public static bool IsDefined(this Type type, Type attributeType, bool inherit)
			=> type.EnsureNotNull(nameof(type)).Value.GetTypeInfo().IsDefined(attributeType.EnsureNotNull(nameof(attributeType)), inherit);

		public static Attribute[ ] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
			=> type.EnsureNotNull(nameof(type)).Value.GetTypeInfo().GetCustomAttributes(attributeType.EnsureNotNull(nameof(attributeType)), inherit).Cast<Attribute>().ToArray();

#endif

		public static bool IsInterface(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.IsInterface;

		public static bool IsClass(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.IsClass;

		public static bool IsEnum(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.IsEnum;

		public static bool IsGenericType(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.IsGenericType;

		public static bool IsGenericTypeDefinition(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.IsGenericTypeDefinition;

		public static bool ContainsGenericParameters(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.ContainsGenericParameters;

		public static bool IsAbstract(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.IsAbstract;

		public static bool IsValueType(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.IsValueType;

		public static bool IsPrimitive(this TypeInfo type)
			=> type.EnsureNotNull(nameof(type)).Value.IsPrimitive;

#endif

		// TODO: Put strings into the resources.
		//
		public static MethodInfo GetMethod(this Type type, string name, Type[ ] parameterTypes = default, bool throwIfNotFound = default) {
			if (type is null)
				throw new ArgumentNullException(paramName: nameof(type));
			else if (name is null)
				throw new ArgumentNullException(paramName: nameof(name));
			else if (string.Empty == name)
				throw new ArgumentException(paramName: nameof(name), message: "Cannot be an empty string.");
			//
			var result = type.GetMethod(name: name, types: parameterTypes ?? Type.EmptyTypes);
			if (result is null && throwIfNotFound)
				throw new MissingMemberException(string.Format($"Method not found.{Environment.NewLine}\tMethod:{Environment.NewLine}\t\t{MemberText.GetMethodSignatureText(declaringType: type, name: name, parameterTypes: parameterTypes)}"));
			else
				return result;
		}

	}

}