using System;
using System.Collections.Generic;
using System.Linq;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Reflection {

	public static class TypeUtilitiesCoreL1 {

		static readonly Type __NullableTypeDefinition = typeof(Nullable<>);

		public static IEnumerable<Type> GetHierarchyOfClass(this Type type)
			=> GetHierarchyOfClass(type: type, to: typeof(object));

		public static IEnumerable<Type> GetHierarchyOfClass(this Type type, Type to) {
			type.EnsureNotNull(nameof(type)).EnsureClass();
			to.EnsureNotNull(nameof(to)).EnsureClass();
			//
			if (type.IsSubclassOf(to)) {
				while (true) {
					yield return type;
					if (type == to)
						break;
					type = type.BaseType();
				}
			}
			else if (type == to)
				yield return to;
			else
				throw new ArgumentException(FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/InvalidDerivation", type, to), nameof(type));
		}

		public static IEnumerable<Type> GetHierarchyOfInterface(this Type type) {
			if (type is null)
				throw new ArgumentNullException(nameof(type));
			else if (!type.IsInterface())
				throw new ArgumentException(FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/NotInterface", type), nameof(type));
			//
			var resultStack = new Stack<Type>();
			var interfacesSet = new HashSet<Type>(type.GetInterfaces());
			var interfaceAncestors = default(Type[ ]);
			for (; interfacesSet.Count > 0;) {
				foreach (var @interface in interfacesSet) {
					if ((interfaceAncestors = @interface.GetInterfaces()).Length < 1 || interfaceAncestors.All(i => !interfacesSet.Contains(i))) {
						resultStack.Push(@interface);
						interfacesSet.Remove(@interface);
						break;
					}
				}
			}
			yield return type;
			for (; resultStack.Count > 0;)
				yield return resultStack.Pop();
		}

		public static Type SelectMostCompatibleType(this Type source, params Type[ ] candidates) {
			const int maxInheritanceMarkInclusive = 2048;
			source.EnsureNotNull(nameof(source));
			if (!source.IsClass())
				throw new ArgumentException(FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/NotClass", source), nameof(source));
			if (source.ContainsGenericParameters())
				throw new ArgumentException(FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/ContainsGenericParameters", source), nameof(source));
			if (candidates == null)
				throw new ArgumentNullException("candidateTypes");
			if (candidates.Length < 1)
				return null;
			if (candidates.Length == 1) {
				var candidateType = candidates[ 0 ];
				if (candidateType == null)
					throw new ArgumentException(FormatXResource(typeof(Array), "CanNotContainNull/NullAt", 0), "candidateTypes");
				return candidateType.IsAssignableFrom(source) || (candidateType.IsGenericTypeDefinition() && source.GetHierarchyOfGenericDefinitions().Any(y => y == candidateType)) ? candidateType : null;
			}
			var inheritanceMarks = new Dictionary<Type, int>();
			var inheritanceMark = -1;
			var sourceTypeBaseType = source;
			inheritanceMarks.Add(source, ++inheritanceMark);
			if (source.IsGenericType())
				inheritanceMarks.Add(source.GetGenericTypeDefinition(), inheritanceMark);
			for (; (sourceTypeBaseType = sourceTypeBaseType.BaseType()) != null;) {
				if (inheritanceMark == maxInheritanceMarkInclusive)
					throw new EonException(string.Format("It is impossible to select the most compatible type for the '{0}' type from the specified set of types.", source));
				inheritanceMarks.Add(sourceTypeBaseType, ++inheritanceMark);
				if (sourceTypeBaseType.IsGenericType())
					inheritanceMarks.Add(sourceTypeBaseType.GetGenericTypeDefinition(), inheritanceMark);
			}
			foreach (var sourceTypeInterface in source.GetInterfaces()) {
				if (inheritanceMarks.ContainsKey(sourceTypeInterface))
					continue;
				foreach (var sourceTypeInterfaceH in sourceTypeInterface.GetHierarchyOfInterface()) {
					if (inheritanceMark == maxInheritanceMarkInclusive)
						throw new EonException($"It is impossible to select the most compatible type for the '{source}' type from the specified set of types.");
					if (inheritanceMarks.ContainsKey(sourceTypeInterfaceH)) {
						inheritanceMarks[ sourceTypeInterfaceH ] = ++inheritanceMark;
					}
					else {
						inheritanceMarks.Add(sourceTypeInterfaceH, ++inheritanceMark);
						if (sourceTypeInterfaceH.IsGenericType() && !inheritanceMarks.ContainsKey(sourceTypeInterfaceH.GetGenericTypeDefinition()))
							inheritanceMarks.Add(sourceTypeInterfaceH.GetGenericTypeDefinition(), inheritanceMark);
					}
				}
			}
			return (from candidateType in candidates.AsEnumerable().Arg(nameof(candidates)).EnsureNoNullElements().Value
							where inheritanceMarks.ContainsKey(candidateType) && (candidateType.IsAssignableFrom(source) || (candidateType.IsGenericTypeDefinition() && source.GetHierarchyOfGenericDefinitions().Any(y => y == candidateType)))
							orderby inheritanceMarks[ candidateType ] ascending
							select candidateType).FirstOrDefault();
		}

		// TODO: Put exception messages into the resources.
		//
		public static int GetInheritanceLevel(this Type type, Type @base) {
			type.EnsureNotNull(nameof(type));
			@base.EnsureNotNull(nameof(@base));
			if (!(type.IsClass() || type.IsInterface()))
				throw new ArgumentException(message: FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/NotClass", type), paramName: nameof(type));
			else if (!(@base.IsClass() || @base.IsInterface()))
				throw new ArgumentException(message: FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/NotClass", @base), paramName: nameof(@base));
			else if (!@base.IsAssignableFrom(type))
				throw new ArgumentException(message: FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/InvalidDerivation", type, @base), paramName: nameof(type));
			//
			Func<Type, Type, int> computeInheritanceLevelFunc = (b, d) => {
				var c = 0;
				for (c = 0; b != d;) {
					c++;
					d = d.BaseType();
				}
				return c;
			};
			int level;
			if (@base.IsInterface()) {
				if (type.IsInterface())
					level = computeInheritanceLevelFunc(@base, type);
				else {
					level = 0;
#if TRG_NETFRAMEWORK
					InterfaceMapping currentInterfaceMapping;
					for (; derivedClassOrInterface != null; ) {
						currentInterfaceMapping = derivedClassOrInterface.GetInterfaceMap(baseClassOrInterface);
						if (currentInterfaceMapping.TargetMethods != null && currentInterfaceMapping.TargetMethods.Length > 0)
							break;
						level++;
						derivedClassOrInterface = derivedClassOrInterface.BaseType();
					}
#endif
				}
			}
			else if (type.IsInterface())
				throw new ArgumentException(message: FormatXResource(typeof(ArgumentException), "ValueIsInvalid"), paramName: nameof(type));
			else
				level = computeInheritanceLevelFunc(@base, type);
			return level;
		}

		// TODO: Implement method for closed type presented by interfaceGenericTypeDefinition.
		//
		public static bool IsInterfaceImplementedBy(this Type genericDefinition, Type implementation, out Type[ ] genericArgs) {
			if (genericDefinition is null)
				throw new ArgumentNullException(nameof(genericDefinition));
			else if (!genericDefinition.IsInterface())
				throw new ArgumentException(FormatXResource(typeof(Type), "NotInterface", genericDefinition), nameof(genericDefinition));
			else if (!genericDefinition.IsGenericTypeDefinition())
				throw new ArgumentException(FormatXResource(typeof(Type), "NotGenericDefinition", genericDefinition), nameof(genericDefinition));
			else if (implementation is null)
				throw new ArgumentNullException(nameof(implementation));
			else if (implementation.ContainsGenericParameters())
				throw new ArgumentException(FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/ContainsGenericParameters", implementation), nameof(implementation));
			else if (implementation.IsInterface())
				throw new ArgumentException(FormatXResource(typeof(Type), "InterfaceNotAllowed", implementation), nameof(implementation));
			var implementedInterfaces = implementation.GetInterfaces();
			var result = false;
			genericArgs = null;
			for (var i = 0; i < implementedInterfaces.Length; i++) {
				result = implementedInterfaces[ i ].IsGenericType() && implementedInterfaces[ i ].GetGenericTypeDefinition() == genericDefinition;
				if (result) {
					genericArgs = implementedInterfaces[ i ].GetGenericArguments();
					break;
				}
			}
			return result;
		}

		public static bool IsNullable(this Type type, out Type typeArgument) {
			type
				.Arg(nameof(type))
				.EnsureNotNull()
				.EnsureNotContainsGenericParameters();
			if (type.IsGenericType() && (type.IsGenericTypeDefinition() ? type : type.GetGenericTypeDefinition()) == __NullableTypeDefinition) {
				typeArgument = type.GetGenericArguments()[ 0 ];
				return true;
			}
			else {
				typeArgument = null;
				return false;
			}
		}

	}

}