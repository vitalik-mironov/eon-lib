using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Eon.Annotations;
using Eon.Collections;
using Eon.ComponentModel.Dependencies;
using Eon.Description;
using Eon.Description.Annotations;
using Eon.Linq;
using Eon.Reflection;
using Eon.Threading;
using static Eon.Reflection.ActivationUtilities;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static class XInstanceContractUtilities {

		static readonly IDictionary<Type, IList<IFactoryDependencyHandler>> __DescriptionTypeXInstanceContractsHandlers;

		static readonly PrimitiveSpinLock __DescriptionTypeXInstanceContractsHandlersSpinLock;

		static XInstanceContractUtilities() {
			__DescriptionTypeXInstanceContractsHandlersSpinLock = new PrimitiveSpinLock();
			__DescriptionTypeXInstanceContractsHandlers = new Dictionary<Type, IList<IFactoryDependencyHandler>>();
		}

		public static IList<IVh<IFactoryDependencyHandler>> GetOrderedXInstanceContractHandlers(IDescription description) {
			description.EnsureNotNull(nameof(description));
			//
			IList<IVh<IFactoryDependencyHandler>> listBuffer = new List<IVh<IFactoryDependencyHandler>>();
			try {
				var descriptionDefinedContractType = description.ContractType?.Resolve();
				if (descriptionDefinedContractType != null) {
					foreach (var handler in P_CreateXInstanceContractHandlers(orderedContractsTypes: descriptionDefinedContractType.Sequence()))
						listBuffer.Add(handler.ToValueHolder(ownsValue: true));
				}
				//
				foreach (var handler in GetOrderedXInstanceContractHandlers(descriptionType: description.GetType()))
					listBuffer.Add(handler.ToValueHolder(ownsValue: false));
				//
				return new ListReadOnlyWrap<IVh<IFactoryDependencyHandler>>(list: listBuffer);
			}
			catch (Exception firstException) {
				foreach (var item in listBuffer)
					item?.Dispose(firstException);
				throw;
			}
		}

		public static IList<IFactoryDependencyHandler> GetOrderedXInstanceContractHandlers(Type descriptionType) {
			descriptionType.EnsureNotNull(nameof(descriptionType));
			//
			var existingList = default(IList<IFactoryDependencyHandler>);
			if (__DescriptionTypeXInstanceContractsHandlersSpinLock.Invoke(() => __DescriptionTypeXInstanceContractsHandlers.TryGetValue(descriptionType, out existingList)))
				return existingList;
			else {
				var orderedContractsTypes =
					GetOrderedXInstanceContractTypes(descriptionType: descriptionType);
				IList<IFactoryDependencyHandler> listBuffer = new List<IFactoryDependencyHandler>();
				try {
					foreach (var handler in P_CreateXInstanceContractHandlers(orderedContractsTypes: orderedContractsTypes))
						listBuffer.Add(handler);
					listBuffer = new ListReadOnlyWrap<IFactoryDependencyHandler>(list: listBuffer);
					//
					__DescriptionTypeXInstanceContractsHandlersSpinLock
						.Invoke(
							() => {
								if (!__DescriptionTypeXInstanceContractsHandlers.TryGetValue(descriptionType, out existingList))
									__DescriptionTypeXInstanceContractsHandlers.Add(descriptionType, existingList = listBuffer);
							});
					//
					if (!ReferenceEquals(listBuffer, existingList))
						listBuffer.DisposeMany();
					//
					return existingList;
				}
				catch (Exception firstException) {
					foreach (var item in listBuffer)
						item?.Dispose(firstException);
					throw;
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		public static IVh<IFactoryDependencyHandler> RequireSingleContractHandler(IDescription description, bool canShareDependency, IArgsTuple newInstanceFactoryArgs) {
			description.EnsureNotNull(nameof(description));
			newInstanceFactoryArgs.EnsureNotNull(nameof(newInstanceFactoryArgs));
			//
			var orderedContractsTypes = GetOrderedXInstanceContractTypes(description: description);
			var firstHandler = default(IFactoryDependencyHandler);
			var secondHandler = default(IFactoryDependencyHandler);
			try {
				foreach (var handler in
					P_CreateXInstanceContractHandlers(
						orderedContractsTypes: orderedContractsTypes,
						newInstanceFactoryArgs: newInstanceFactoryArgs,
						canShareDependency: canShareDependency)) {
					//
					if (firstHandler == null)
						firstHandler = handler;
					else {
						secondHandler = handler;
						throw
							new InvalidOperationException(
								$"Указанным параметрам соответствует несколько обработчиков функциональной зависимости, созданных на основе XInstance-контракта(-ов) указанного описания.{Environment.NewLine}\tОписание:{description.FmtStr().GNLI2()}{Environment.NewLine}\tПараметры фабрики экземпляра XInstance-контракта:{newInstanceFactoryArgs.FmtStr().GNLI2()}");
					}
				}
				//
				if (firstHandler == null)
					throw
						new InvalidOperationException(
							$"Нет обработчика функциональной зависимости, определяемого XInstance-контрактом(-ами) указанного описания, который бы соответствовал указанным параметрам.{Environment.NewLine}\tОписание:{description.FmtStr().GNLI2()}{Environment.NewLine}\tПараметры фабрики экземпляра XInstance-контракта:{newInstanceFactoryArgs.FmtStr().GNLI2()}");
				//
				return firstHandler.ToValueHolder(ownsValue: true);
			}
			catch (Exception firstException) {
				secondHandler?.Dispose(firstException);
				firstHandler?.Dispose(firstException);
				throw;
			}
		}

		// TODO: Put strings into the resources.
		//
		static IEnumerable<IFactoryDependencyHandler> P_CreateXInstanceContractHandlers(IEnumerable<Type> orderedContractsTypes, IArgsTuple newInstanceFactoryArgs = null, bool canShareDependency = false) {
			orderedContractsTypes =
				orderedContractsTypes
				.EnsureNotNull(nameof(orderedContractsTypes))
				.EnsureNoNullElements()
				.Value;
			if (newInstanceFactoryArgs != null)
				newInstanceFactoryArgs
					.Arg(nameof(newInstanceFactoryArgs))
					.EnsureOfType(type: ArgsTuple.GetIArgsTupleType(newInstanceFactoryArgs.Arg(nameof(newInstanceFactoryArgs))));
			//
			Func<ConstructorInfo, bool> newInstanceCtorPredicate;
			if (newInstanceFactoryArgs == null)
				newInstanceCtorPredicate = _ => true;
			else
				newInstanceCtorPredicate = locCtor => locCtor.IsMatchSignature(argTypes: newInstanceFactoryArgs.ArgsTypes);
			var newInstanceCtors =
				orderedContractsTypes
				.SelectMany(i => i.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				.Where(newInstanceCtorPredicate);
			//
			foreach (var newInstanceCtor in newInstanceCtors) {
				var handlerNewInstanceFactoryArgsTypes =
					(newInstanceFactoryArgs == null
					? newInstanceCtor.GetParameters().Select(i => i.ParameterType)
					: newInstanceFactoryArgs.ArgsTypes)
				 .ToArray();
				var handlerTypeGenericArgs =
					handlerNewInstanceFactoryArgsTypes
				 // Тип результата. Последний аргумент типа CtorFactoryDependencyHandler — TDependencyInstance. Он указывает конкретный тип объекта (экземпляра функциональной зависимости).
				 //
				 .Concat(newInstanceCtor.DeclaringType.Sequence())
				 .ToArray();
				Type handlerNewInstanceFactoryArgsTupleType;
				Type handlerType;
				switch (handlerTypeGenericArgs.Length - 1) {
					case 0:
						handlerType = typeof(CtorFactoryDependencyHandler<>).MakeGenericType(typeArguments: handlerTypeGenericArgs);
						handlerNewInstanceFactoryArgsTupleType = typeof(IArgsTuple);
						break;
					case 1:
						handlerType = typeof(CtorFactoryDependencyHandler<,>).MakeGenericType(typeArguments: handlerTypeGenericArgs);
						handlerNewInstanceFactoryArgsTupleType = typeof(IArgsTuple<>).MakeGenericType(typeArguments: handlerNewInstanceFactoryArgsTypes);
						break;
					case 2:
						handlerType = typeof(CtorFactoryDependencyHandler<,,>).MakeGenericType(typeArguments: handlerTypeGenericArgs);
						handlerNewInstanceFactoryArgsTupleType = typeof(IArgsTuple<,>).MakeGenericType(typeArguments: handlerNewInstanceFactoryArgsTypes);
						break;
					case 3:
						handlerType = typeof(CtorFactoryDependencyHandler<,,,>).MakeGenericType(typeArguments: handlerTypeGenericArgs);
						handlerNewInstanceFactoryArgsTupleType = typeof(IArgsTuple<,,>).MakeGenericType(typeArguments: handlerNewInstanceFactoryArgsTypes);
						break;
					case 4:
						handlerType = typeof(CtorFactoryDependencyHandler<,,,,>).MakeGenericType(typeArguments: handlerTypeGenericArgs);
						handlerNewInstanceFactoryArgsTupleType = typeof(IArgsTuple<,,,>).MakeGenericType(typeArguments: handlerNewInstanceFactoryArgsTypes);
						break;
					case 5:
						handlerType = typeof(CtorFactoryDependencyHandler<,,,,,>).MakeGenericType(typeArguments: handlerTypeGenericArgs);
						handlerNewInstanceFactoryArgsTupleType = typeof(IArgsTuple<,,,,>).MakeGenericType(typeArguments: handlerNewInstanceFactoryArgsTypes);
						break;
					default:
						throw
							new InvalidOperationException(
								$"Для одного из конструкторов XInstance-контракта отсутствует подходящий тип обработчика функциональной зависимости.{Environment.NewLine}\tКонструктор:{newInstanceCtor.FmtStr().GNLI2()}");
				}
				var handler =
					RequireConstructor<bool, IArgsTuple, IFactoryDependencyHandler>(
						concreteType: handlerType,
						arg2ConcreteType: handlerNewInstanceFactoryArgsTupleType)
					(arg1: canShareDependency, arg2: newInstanceFactoryArgs);
				yield return handler;
			}
		}

		public static bool HasXInstanceContract(IDescription description, Type contractCompatibleType = null) {
			description.EnsureNotNull(nameof(description));
			//
			// TODO_HIGH: Доработать проверку, что указанное описание (dependencyTargetDescription) имеет как минимум один XInstance-контракт, с учетом возможности явного определения контракта самим описанием (в .limedx).
			//
			Func<Type, bool> anyPredicate;
			if (contractCompatibleType == null)
				anyPredicate = _ => true;
			else
				anyPredicate = locContractType => contractCompatibleType.IsAssignableFrom(locContractType);
			return
				GetOrderedXInstanceContractTypes(description: description)
				.Any(anyPredicate);
		}

		public static IEnumerable<Type> GetOrderedXInstanceContractTypes(IDescription description) {
			description.EnsureNotNull(nameof(description));
			//
			return
				(description.ContractType?.Resolve().Sequence())
				.EmptyIfNull()
				.Concat(second: GetOrderedXInstanceContractTypes(descriptionType: description.GetType()));
		}

		public static IEnumerable<Type> GetOrderedXInstanceContractTypes(Type descriptionType) {
			descriptionType
				.EnsureNotNull(nameof(descriptionType))
				.EnsureCompatible(typeof(IDescription));
			//
			return
				AnnotationUtilities
				.GetAnnotations(annotatedType: descriptionType)
				.Items
				.OfType<XInstanceContractAttribute>()
				.OrderByDescending(i => i.Target.TypeInheritanceLevel)
				.Select(i => i.ContractType);
		}

		public static bool IsValidXInstanceContractType(Type type, out string errorMessage)
			=>
			IsValidXInstanceContractType(
				type: type.Arg(nameof(type)),
				errorMessage: out errorMessage);

		public static bool IsValidXInstanceContractType(ArgumentUtilitiesHandle<Type> type, out string errorMessage) {
			type.EnsureNotNull();
			//
			if (!type.Value.IsClass()) {
				errorMessage = FormatXResource(typeof(Type), "NotClass", type.Value.FmtStr().G());
				return false;
			}
			else if (type.Value.IsAbstract()) {
				errorMessage = FormatXResource(typeof(Type), "AbstractNotAllowed", type.Value.FmtStr().G());
				return false;
			}
			else if (!XInstanceBase.Internal_IXInstanceType.IsAssignableFrom(type.Value)) {
				errorMessage = FormatXResource(typeof(Type), "NoImplementationOfT", type.Value.FmtStr().G(), XInstanceBase.Internal_IXInstanceType.FmtStr().G());
				return false;
			}
			else {
				errorMessage = null;
				return true;
			}
		}

		public static void EnsureXInstanceContractTypeValid(ArgumentUtilitiesHandle<Type> type) {
			if (!IsValidXInstanceContractType(type: type, errorMessage: out var errorMessage)) {
				var exceptionMessage = $"{(type.IsProp ? $"Указано недопустимое значение свойства '{type.Name}'.{Environment.NewLine}" : string.Empty)}{errorMessage}";
				throw
					type.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: type.Name, message: exceptionMessage);
			}
		}

	}

}