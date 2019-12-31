using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Eon.Linq;

using static Eon.Reflection.MemberText;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Reflection {

	public static class ActivationUtilities {

		public static readonly BindingFlags FindConstructorDefaultBindingFlags =
			BindingFlags.DeclaredOnly
			| BindingFlags.Instance
			| BindingFlags.NonPublic
			| BindingFlags.Public;

		public static ConstructorInfo FindConstructor(this Type type, params Type[ ] constructorParametersTypes)
			=> FindConstructors(type, FindConstructorDefaultBindingFlags, constructorParametersTypes).FirstOrDefault();

		[Obsolete(message: "Do not use this method.", error: true)]
		public static ConstructorInfo FindConstructor(this Type type, BindingFlags bindingFlags, params Type[ ] constructorParametersTypes)
			=> FindConstructors(type, bindingFlags, constructorParametersTypes).FirstOrDefault();

		public static IEnumerable<ConstructorInfo> FindConstructors(this Type type, BindingFlags bindingFlags, params Type[ ] argsTypes) {
			type.EnsureNotNull(nameof(type));
			//
			argsTypes = argsTypes.IsNullOrEmpty() ? null : argsTypes;
			var typeIntrospectionApi = type.GetTypeInfo();
			// TODO: Сортировка в порядке: первый конструктор тот, каждый тип параметра которого наиболее соответствует соотвествующем типу из constructorParametersTypes. В идеальном варианте должен быть найден только один конструктор.
			//
			return from ctor in typeIntrospectionApi.GetConstructors(bindingFlags)
						 let ctorParameters = ctor.GetParameters()
						 //
						 where argsTypes == null ? ctorParameters.Length == 0 : ctorParameters.Length == argsTypes.Length
						 //
						 where
						 ctorParameters
						 .TakeWhile((ctorParameterType, i) => ctorParameters[ i ].ParameterType.GetTypeInfo().IsAssignableFrom(argsTypes[ i ]))
						 .Count() == ctorParameters.Length
						 //
						 select ctor;
		}

		public static T CreateInstance<T>(Type[ ] constructorParametersTypes, params object[ ] constructorParameters)
			=> CreateInstance<T>(typeof(T), constructorParametersTypes, constructorParameters);

		public static T CreateInstance<T>(this Type type, params object[ ] args) {
			if (args == null || args.Length < 1)
				return CreateInstance<T>(type, (Type[ ])null, (object[ ])null);
			var constructorParametersTypes = new Type[ args.Length ];
			object currentParameter;
			for (var i = 0; i < constructorParametersTypes.Length; i++) {
				currentParameter = args[ i ];
				if (currentParameter == null)
					throw new ArgumentException(FormatXResource(typeof(Array), "CanNotContainNull/NullAt", i), "constructorArgs");
				constructorParametersTypes[ i ] = currentParameter.GetType();
			}
			return CreateInstance<T>(type, constructorParametersTypes, args);
		}

		public static T CreateInstance<T>(this Type type, Type[ ] argsTypes, params object[ ] args) {
			type.EnsureNotNull(nameof(type));
			//
			var typeIntrospectionApi = type.GetTypeInfo();
			if (!typeof(T).GetTypeInfo().IsAssignableFrom(type))
				throw
					new ArgumentException(
						FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/InvalidDerivation", type.FmtStr().G(), typeof(T).FmtStr().G()),
						nameof(type));
			//
			if (typeIntrospectionApi.IsValueType() && typeIntrospectionApi.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return CreateInstance<T>(typeIntrospectionApi.GetGenericArguments()[ 0 ], argsTypes, args);
			else {
				ConstructorInfo foundConstructor;
				P_EnsureInstantiationAbility(type, out foundConstructor, argsTypes);
				if (foundConstructor == null)
					throw new EonException(message: FormatXResource(typeof(Type), "InstantiationFaulted/NoCtor/CtorSignature", type.FmtStr().G(), GetConstructorSignatureText(constructorParameterTypes: argsTypes)));
				T result;
				try {
					result = (T)foundConstructor.Invoke(args);
				}
				catch (Exception exception) {
					throw new EonException(message: FormatXResource(typeof(Type), "InstantiationFaulted/CtorInvocationFail", type.FmtStr().G(), foundConstructor.FmtStr().G()), innerException: exception is TargetInvocationException ? exception.InnerException : exception);
				}
				return result;
			}
		}

		// TODO: Exceptions (InstantiationException type).
		//
		static void P_EnsureInstantiationAbility(Type type, BindingFlags bindingFlags, out ConstructorInfo foundConstructor, params Type[ ] argsConcreteTypes) {
			type.EnsureNotNull(nameof(type));
			//
			var typeIntrospectionApi = type.GetTypeInfo();
			var ctorArgsArray = argsConcreteTypes.IsNullOrEmpty() ? null : argsConcreteTypes;
			if (type.IsArray || typeIntrospectionApi.IsEnum || typeIntrospectionApi.IsPrimitive()) {
				if (ctorArgsArray != null)
					throw
						new EonException(FormatXResource(typeof(Type), "InstantiationFaulted/NoCtor/CtorSignature", type.FmtStr().G(), GetConstructorSignatureText(constructorParameterTypes: ctorArgsArray)));
			}
			else if (typeIntrospectionApi.IsInterface())
				throw new EonException(FormatXResource(typeof(Type), "InstantiationFaulted/TypeIsInterface", type.FmtStr().G()));
			else if (typeIntrospectionApi.IsGenericType()) {
				if (typeIntrospectionApi.IsGenericTypeDefinition())
					throw new EonException(FormatXResource(typeof(Type), "InstantiationFaulted/TypeIsGenericDefinition", type.FmtStr().G()));
				if (typeIntrospectionApi.ContainsGenericParameters)
					throw new EonException(FormatXResource(typeof(Type), "InstantiationFaulted/TypeContainsGenericParameters", type.FmtStr().G()));
			}
			else if (typeIntrospectionApi.IsAbstract())
				throw new EonException(FormatXResource(typeof(Type), "InstantiationFaulted/TypeIsAbstract", type.FmtStr().G()));
			else if (type.IsPointer)
				throw new EonException(FormatXResource(typeof(Type), "InstantiationFaulted/TypeIsPointer", type.FmtStr().G()));
			var foundCtors = FindConstructors(type, bindingFlags, argsConcreteTypes).Take(2).ToArray();
			if (foundCtors.Length < 1)
				throw new EonException(message: FormatXResource(typeof(Type), "InstantiationFaulted/NoCtor/CtorSignature", type.FmtStr().G(), GetConstructorSignatureText(constructorParameterTypes: argsConcreteTypes)));
			else if (foundCtors.Length > 1)
				throw
					new EonException(
						message:
							FormatXResource(
								locator: typeof(Type),
								subpath: "InstantiationFaulted/CtorAmbiguity",
								args: new[ ] {
									type.FmtStr().G(),
									GetConstructorSignatureText(constructorParameterTypes: argsConcreteTypes),
									GetConstructorSignatureText(constructorParameterTypes: foundCtors[ 0 ].GetParameters().Select(i => i.ParameterType).ToArray()),
									GetConstructorSignatureText(constructorParameterTypes: foundCtors[ 1 ].GetParameters().Select(i => i.ParameterType).ToArray()) }));
			else
				foundConstructor = foundCtors[ 0 ];
		}

		static void P_EnsureInstantiationAbility(Type type, out ConstructorInfo foundConstructor, params Type[ ] argsTypes)
			=> P_EnsureInstantiationAbility(type, FindConstructorDefaultBindingFlags, out foundConstructor, argsTypes);

		/// <summary>
		/// Выполняет проверку на возможность создания экземпляра данного типа.
		/// <para>Проверка предполагает наличие у данного типа конструктора, сигнатура параметров которого соответствует указанным типам.</para>
		/// </summary>
		/// <param name="type">Тип экземпляра.</param>
		/// <param name="argsTypes">Типы значений параметров конструктора.</param>
		public static void EnsureInstantiation(this Type type, params Type[ ] argsTypes) {
			ConstructorInfo ctor;
			P_EnsureInstantiationAbility(type, out ctor, argsTypes);
		}

		#region RequireConstructor
		// TODO: Реализовать кэширование делегатов конструкторов (если имеет смысл в плане производительности).
		//

		static TDelegate P_CreateConstructorDelegate<TDelegate>(
			Type baseType,
			Type concreteType,
			Type[ ] argsBaseTypes,
			Type[ ] argsConcreteTypes,
			BindingFlags? bindingFlags = default) {
			//
			ConstructorInfo constructor;
			P_EnsureInstantiationAbility(type: concreteType, bindingFlags: bindingFlags ?? FindConstructorDefaultBindingFlags, foundConstructor: out constructor, argsConcreteTypes: argsConcreteTypes);
			var constructorDeclaredParameters =
				constructor
				.GetParameters();
			var argsBaseTypeExprs =
				argsBaseTypes.EmptyIfNull()
				.Select((locArgType, locIndex) => Expression.Parameter(locArgType, string.Format(CultureInfo.InvariantCulture, "arg{0}", locIndex)))
				.ToArray();
			var argsConcreteTypeExprs =
				argsBaseTypeExprs
				.Select(
					selector: (locArgBaseTypeExpr, locIndex) => locArgBaseTypeExpr.Type == argsConcreteTypes[ locIndex ] ? (Expression)locArgBaseTypeExpr : Expression.Convert(locArgBaseTypeExpr, argsConcreteTypes[ locIndex ]))
				.Select(
					selector: (locArgConcreteTypeExpr, locIndex) => locArgConcreteTypeExpr.Type == constructorDeclaredParameters[ locIndex ].ParameterType ? locArgConcreteTypeExpr : Expression.Convert(locArgConcreteTypeExpr, constructorDeclaredParameters[ locIndex ].ParameterType))
				.ToArray();
			return
				(baseType == concreteType
					? Expression.Lambda<TDelegate>(
						body: Expression.New(constructor, argsConcreteTypeExprs),
						parameters: argsBaseTypeExprs)
					: Expression.Lambda<TDelegate>(
						body: Expression.Convert(Expression.New(constructor, argsConcreteTypeExprs), baseType),
						parameters: argsBaseTypeExprs))
				.Compile();
		}

		public static Func<T> RequireConstructor<T>(
			Type concreteType = null,
			BindingFlags? bindingFlags = default) {
			//
			var typeOfT = typeof(T);
			concreteType =
				concreteType
				.Arg(nameof(concreteType))
				.EnsureCompatible(typeOfT)
				.Value
				?? typeOfT;
			//
			ConstructorInfo constructor;
			P_EnsureInstantiationAbility(
				concreteType,
				bindingFlags ?? FindConstructorDefaultBindingFlags,
				out constructor);
			//
			return P_CreateConstructorDelegate<Func<T>>(baseType: typeOfT, concreteType: concreteType, argsBaseTypes: null, argsConcreteTypes: null, bindingFlags: bindingFlags);
		}

		public static Func<TArg1, T> RequireConstructor<TArg1, T>(Type concreteType = null, BindingFlags? bindingFlags = default, TArg1 arg1 = default) {
			var typeOfT = typeof(T);
			concreteType = concreteType.Arg(nameof(concreteType)).EnsureCompatible(typeOfT).Value ?? typeOfT;
			//
			var argsTypes =
				new Type[ ] {
					typeof(TArg1)
				};
			var constructorArgsTypes =
				argsTypes
				.Select(
					selector:
					(locItem, locIndex) => {
						switch (locIndex) {
							case 0:
								return arg1?.GetType() ?? locItem;
							default:
								return locItem;
						}
					})
				.ToArray();
			//
			return
				P_CreateConstructorDelegate<Func<TArg1, T>>(
					baseType: typeOfT,
					concreteType: concreteType,
					argsBaseTypes: argsTypes,
					argsConcreteTypes: constructorArgsTypes,
					bindingFlags: bindingFlags);
		}

		public static Func<TArg1, TArg2, T> RequireConstructor<TArg1, TArg2, T>(Type concreteType = null, BindingFlags? bindingFlags = default, Type arg1ConcreteType = default, Type arg2ConcreteType = default) {
			var typeOfT = typeof(T);
			concreteType = concreteType.Arg(nameof(concreteType)).EnsureCompatible(typeOfT).Value ?? typeOfT;
			//
			var argsTypes =
				new Type[ ] {
					typeof(TArg1),
					typeof(TArg2)
			};
			var constructorArgsConcreteTypes =
				argsTypes
				.Select(
					(locType, locIndex) => {
						switch (locIndex) {
							case 0:
								return arg1ConcreteType.Arg(nameof(arg1ConcreteType)).EnsureCompatible(locType).Value ?? locType;
							case 1:
								return arg2ConcreteType.Arg(nameof(arg2ConcreteType)).EnsureCompatible(locType).Value ?? locType;
							default:
								throw new EonException();
						}
					})
				.ToArray();
			//
			return
				P_CreateConstructorDelegate<Func<TArg1, TArg2, T>>(
					baseType: typeOfT,
					concreteType: concreteType,
					argsBaseTypes: argsTypes,
					argsConcreteTypes: constructorArgsConcreteTypes,
					bindingFlags: bindingFlags);
		}

		public static Func<TArg1, TArg2, TArg3, T> RequireConstructor<TArg1, TArg2, TArg3, T>(Type concreteType = null, BindingFlags? bindingFlags = default, Type arg1ConcreteType = default, Type arg2ConcreteType = default, Type arg3ConcreteType = default) {
			var typeOfT = typeof(T);
			concreteType = concreteType.Arg(nameof(concreteType)).EnsureCompatible(typeOfT).Value ?? typeOfT;
			//
			var argsTypes =
				new Type[ ] {
					typeof(TArg1),
					typeof(TArg2),
					typeof(TArg3)
			};
			var constructorArgsConcreteTypes =
				argsTypes
				.Select(
					(locType, locIndex) => {
						switch (locIndex) {
							case 0:
								return arg1ConcreteType.Arg(nameof(arg1ConcreteType)).EnsureCompatible(locType).Value ?? locType;
							case 1:
								return arg2ConcreteType.Arg(nameof(arg2ConcreteType)).EnsureCompatible(locType).Value ?? locType;
							case 2:
								return arg3ConcreteType.Arg(nameof(arg3ConcreteType)).EnsureCompatible(locType).Value ?? locType;
							default:
								throw new EonException();
						}
					})
				.ToArray();
			//
			return
				P_CreateConstructorDelegate<Func<TArg1, TArg2, TArg3, T>>(
					baseType: typeOfT,
					concreteType: concreteType,
					argsBaseTypes: argsTypes,
					argsConcreteTypes: constructorArgsConcreteTypes,
					bindingFlags: bindingFlags);
		}

		public static Func<TArg1, TArg2, TArg3, T> RequireConstructor<TArg1, TArg2, TArg3, T>(Type concreteType = null, BindingFlags? bindingFlags = default, TArg1 arg1 = default, TArg2 arg2 = default, TArg3 arg3 = default) {
			var typeOfT = typeof(T);
			concreteType = concreteType.Arg(nameof(concreteType)).EnsureCompatible(typeOfT).Value ?? typeOfT;
			//
			var argsTypes = new Type[ ] {
				typeof(TArg1),
				typeof(TArg2),
				typeof(TArg3),
			};
			var constructorArgsTypes = argsTypes
				.Select((t, i) => {
					switch (i) {
						case 0:
							return arg1?.GetType() ?? t;
						case 1:
							return arg2?.GetType() ?? t;
						case 2:
							return arg3?.GetType() ?? t;
						default:
							return t;
					}
				})
				.ToArray();
			//
			return P_CreateConstructorDelegate<Func<TArg1, TArg2, TArg3, T>>(baseType: typeOfT, concreteType: concreteType, argsBaseTypes: argsTypes, argsConcreteTypes: constructorArgsTypes, bindingFlags: bindingFlags);
		}

		public static Func<TArg1, TArg2, TArg3, TArg4, T> RequireConstructor<TArg1, TArg2, TArg3, TArg4, T>(Type concreteType = null, BindingFlags? bindingFlags = default, TArg1 arg1 = default, TArg2 arg2 = default, TArg3 arg3 = default, TArg4 arg4 = default) {
			var typeOfT = typeof(T);
			concreteType = concreteType.Arg(nameof(concreteType)).EnsureCompatible(typeOfT).Value ?? typeOfT;
			//
			var argsTypes = new Type[ ] {
				typeof(TArg1),
				typeof(TArg2),
				typeof(TArg3),
				typeof(TArg4),
			};
			var constructorArgsTypes = argsTypes
				.Select((t, i) => {
					switch (i) {
						case 0:
							return arg1?.GetType() ?? t;
						case 1:
							return arg2?.GetType() ?? t;
						case 2:
							return arg3?.GetType() ?? t;
						case 3:
							return arg3?.GetType() ?? t;
						default:
							return t;
					}
				})
				.ToArray();
			//
			return P_CreateConstructorDelegate<Func<TArg1, TArg2, TArg3, TArg4, T>>(baseType: typeOfT, concreteType: concreteType, argsBaseTypes: argsTypes, argsConcreteTypes: constructorArgsTypes, bindingFlags: bindingFlags);
		}

		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, T> RequireConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, T>(Type concreteType = null, BindingFlags? bindingFlags = default, TArg1 arg1 = default, TArg2 arg2 = default, TArg3 arg3 = default, TArg4 arg4 = default, TArg5 arg5 = default) {
			var typeOfT = typeof(T);
			concreteType = concreteType.Arg(nameof(concreteType)).EnsureCompatible(typeOfT).Value ?? typeOfT;
			//
			var argsTypes = new Type[ ] {
				typeof(TArg1),
				typeof(TArg2),
				typeof(TArg3),
				typeof(TArg4),
				typeof(TArg5),
			};
			var constructorArgsTypes = argsTypes
				.Select((t, i) => {
					switch (i) {
						case 0:
							return arg1?.GetType() ?? t;
						case 1:
							return arg2?.GetType() ?? t;
						case 2:
							return arg3?.GetType() ?? t;
						case 3:
							return arg3?.GetType() ?? t;
						case 4:
							return arg4?.GetType() ?? t;
						default:
							return t;
					}
				})
				.ToArray();
			//
			return P_CreateConstructorDelegate<Func<TArg1, TArg2, TArg3, TArg4, TArg5, T>>(baseType: typeOfT, concreteType: concreteType, argsBaseTypes: argsTypes, argsConcreteTypes: constructorArgsTypes, bindingFlags: bindingFlags);
		}

		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T> RequireConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T>(Type concreteType = default, BindingFlags? bindingFlags = default, TArg1 arg1 = default, TArg2 arg2 = default, TArg3 arg3 = default, TArg4 arg4 = default, TArg5 arg5 = default, TArg6 arg6 = default) {
			var typeOfT = typeof(T);
			concreteType = concreteType.Arg(nameof(concreteType)).EnsureCompatible(typeOfT).Value ?? typeOfT;
			//
			var argsTypes = new Type[ ] {
				typeof(TArg1),
				typeof(TArg2),
				typeof(TArg3),
				typeof(TArg4),
				typeof(TArg5),
				typeof(TArg6),
			};
			var constructorArgsTypes = argsTypes
				.Select((t, i) => {
					switch (i) {
						case 0:
							return arg1?.GetType() ?? t;
						case 1:
							return arg2?.GetType() ?? t;
						case 2:
							return arg3?.GetType() ?? t;
						case 3:
							return arg3?.GetType() ?? t;
						case 4:
							return arg4?.GetType() ?? t;
						case 5:
							return arg5?.GetType() ?? t;
						default:
							return t;
					}
				})
				.ToArray();
			//
			return P_CreateConstructorDelegate<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T>>(baseType: typeOfT, concreteType: concreteType, argsBaseTypes: argsTypes, argsConcreteTypes: constructorArgsTypes, bindingFlags: bindingFlags);
		}

		#endregion

	}

}