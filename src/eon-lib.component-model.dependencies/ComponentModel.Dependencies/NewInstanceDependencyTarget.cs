using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Eon.Collections;
using Eon.Linq;
using Eon.Reflection;
using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;
using static Eon.Text.StringBuilderUtilities;

namespace Eon.ComponentModel.Dependencies {

	using TypeTubple6 = Tuple<Type, Type, Type, Type, Type, Type>;

	[DataContract]
	public class NewInstanceDependencyTarget
		:DependencyTarget, INewInstanceDependencyTarget {

		#region Nested types

		//[DebuggerStepThrough]
		//[DebuggerNonUserCode]
		sealed class P_EmptyType { P_EmptyType() { } }

		#endregion

		#region Static members

		static readonly Type __EmptyType = typeof(P_EmptyType);

		static readonly IDictionary<TypeTubple6, DisposableLazy<IFactoryDependencyHandler>> __DefaultHandlers;

		static readonly PrimitiveSpinLock __DefaultHandlersSpinLock;

		static NewInstanceDependencyTarget() {
			__DefaultHandlersSpinLock = new PrimitiveSpinLock();
			__DefaultHandlers = new Dictionary<TypeTubple6, DisposableLazy<IFactoryDependencyHandler>>();
		}

		public static IFactoryDependencyHandler GetDefaultDependencyHandler<TArg1, TInstance>() {
			var target =
				new NewInstanceDependencyTarget(
					targetType: typeof(TInstance),
					constructorSignature: new TypeNameReference[ ] { typeof(TArg1) },
					isReadOnly: true);
			target.Validate();
			return GetDefaultDependencyHandler(target: target);
		}

		// TODO: Put strings into the resources.
		//
		public static IFactoryDependencyHandler GetDefaultDependencyHandler(INewInstanceDependencyTarget target) {
			target.EnsureNotNull(nameof(target));
			//
			var dependencyInstanceType = target.TargetType?.Resolve();
			if (dependencyInstanceType is null)
				throw
					new ArgumentException(
						message: $"Указанным аргументом не определен целевой тип зависимости (свойство '{nameof(target.TargetType)}').",
						paramName: nameof(target));
			else if (dependencyInstanceType.IsValueType())
				throw
					new ArgumentException(
						message: $"Целевой тип, определенный указанным аргументом, не является ссылочным типом.{Environment.NewLine}\tЦелевой тип:{Environment.NewLine}{dependencyInstanceType.FmtStr().G().IndentLines2()}",
						paramName: nameof(target));
			//
			const int maxCtorSignatureArgsCount = 5;
			var dependencyInstanceCtorArgsTypes =
				target
				.ConstructorSignature
				.EmptyIfNull()
				.Take(maxCtorSignatureArgsCount + 1)
				.Select(locTypeReference => locTypeReference?.Resolve())
				.ToArray();
			int nullElementIndex;
			if (dependencyInstanceCtorArgsTypes.Length > maxCtorSignatureArgsCount)
				throw
					new ArgumentException(
						message: $"Количество аргументов сигнатуры конструктора целевого типа зависимости (свойство '{nameof(INewInstanceDependencyTarget)}.{nameof(target.ConstructorSignature)}') превышает максимально допустимое '{maxCtorSignatureArgsCount:d}'.",
						paramName: nameof(target));
			else if ((nullElementIndex = dependencyInstanceCtorArgsTypes.IndexOf(i => i is null)) > -1)
				throw
					new ArgumentException(
						message: $"Набор аргументов сигнатуры конструктора целевого типа зависимости (свойство '{nameof(INewInstanceDependencyTarget)}.{nameof(target.ConstructorSignature)}') недопустим. {FormatXResource(typeof(Array), "CanNotContainNull/NullAt", nullElementIndex.ToString("d"))}",
						paramName: nameof(target));
			//
			TypeTubple6 repositoryKey;
			switch (dependencyInstanceCtorArgsTypes.Length) {
				case 0:
					repositoryKey = new TypeTubple6(__EmptyType, __EmptyType, __EmptyType, __EmptyType, __EmptyType, dependencyInstanceType);
					break;
				case 1:
					repositoryKey = new TypeTubple6(__EmptyType, __EmptyType, __EmptyType, __EmptyType, dependencyInstanceCtorArgsTypes[ 0 ], dependencyInstanceType);
					break;
				case 2:
					repositoryKey = new TypeTubple6(__EmptyType, __EmptyType, __EmptyType, dependencyInstanceCtorArgsTypes[ 0 ], dependencyInstanceCtorArgsTypes[ 1 ], dependencyInstanceType);
					break;
				case 3:
					repositoryKey = new TypeTubple6(__EmptyType, __EmptyType, dependencyInstanceCtorArgsTypes[ 0 ], dependencyInstanceCtorArgsTypes[ 1 ], dependencyInstanceCtorArgsTypes[ 2 ], dependencyInstanceType);
					break;
				case 4:
					repositoryKey = new TypeTubple6(__EmptyType, dependencyInstanceCtorArgsTypes[ 0 ], dependencyInstanceCtorArgsTypes[ 1 ], dependencyInstanceCtorArgsTypes[ 2 ], dependencyInstanceCtorArgsTypes[ 3 ], dependencyInstanceType);
					break;
				default:
					repositoryKey = new TypeTubple6(dependencyInstanceCtorArgsTypes[ 0 ], dependencyInstanceCtorArgsTypes[ 1 ], dependencyInstanceCtorArgsTypes[ 2 ], dependencyInstanceCtorArgsTypes[ 3 ], dependencyInstanceCtorArgsTypes[ 4 ], dependencyInstanceType);
					break;
			}
			return
				__DefaultHandlers
				.GetOrAdd(
					spinLock: __DefaultHandlersSpinLock,
					key: repositoryKey,
					factory: factory,
					unclaimedValue: (locKey, locValue) => locValue?.Dispose())
				.Value;
			//
			DisposableLazy<IFactoryDependencyHandler> factory(TypeTubple6 locKey)
			 =>
			 new DisposableLazy<IFactoryDependencyHandler>(
				 factory:
					() => {
						// Проверить существование конструктора экземпляра зависимости.
						//
						dependencyInstanceType.EnsureInstantiation(argsTypes: dependencyInstanceCtorArgsTypes);
						//
						switch (dependencyInstanceCtorArgsTypes.Length) {
							case 0:
								return
									ActivationUtilities
									.RequireConstructor<IFactoryDependencyHandler>(
										concreteType: typeof(CtorFactoryDependencyHandler<>).MakeGenericType(locKey.Item6))
									();
							case 1:
								return
									ActivationUtilities
									.RequireConstructor<IFactoryDependencyHandler>(
										concreteType: typeof(CtorFactoryDependencyHandler<,>).MakeGenericType(locKey.Item5, locKey.Item6))
									();
							case 2:
								return
									ActivationUtilities
									.RequireConstructor<IFactoryDependencyHandler>(
										concreteType: typeof(CtorFactoryDependencyHandler<,,>).MakeGenericType(locKey.Item4, locKey.Item5, locKey.Item6))
									();
							case 3:
								return
									ActivationUtilities
									.RequireConstructor<IFactoryDependencyHandler>(
										concreteType: typeof(CtorFactoryDependencyHandler<,,,>).MakeGenericType(locKey.Item3, locKey.Item4, locKey.Item5, locKey.Item6))
									();
							case 4:
								return
									ActivationUtilities
									.RequireConstructor<IFactoryDependencyHandler>(
										concreteType: typeof(CtorFactoryDependencyHandler<,,,,>).MakeGenericType(locKey.Item2, locKey.Item3, locKey.Item4, locKey.Item5, locKey.Item6))
									();
							default:
								return
									ActivationUtilities
									.RequireConstructor<IFactoryDependencyHandler>(
										concreteType: typeof(CtorFactoryDependencyHandler<,,,,,>).MakeGenericType(locKey.Item1, locKey.Item2, locKey.Item3, locKey.Item4, locKey.Item5, locKey.Item6))
									();
						}
					},
				 ownsValue: true);
		}

		#endregion

		TypeNameReference _targetType;

		ICollection<TypeNameReference> _constructorSignature;

		public NewInstanceDependencyTarget()
			: this(targetType: null) { }

		public NewInstanceDependencyTarget(INewInstanceDependencyTarget other, bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			_targetType = other.TargetType;
			_constructorSignature = other.ConstructorSignature?.ToArray().AsReadOnlyCollection();
		}

		public NewInstanceDependencyTarget(TypeNameReference targetType = default, IEnumerable<TypeNameReference> constructorSignature = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			_targetType = targetType;
			_constructorSignature =
				constructorSignature is null
				? (isReadOnly ? (new TypeNameReference[ ] { }).AsReadOnlyCollection() : (new TypeNameReference[ ] { }))
				: (isReadOnly ? (ICollection<TypeNameReference>)new ListReadOnlyWrap<TypeNameReference>(collection: constructorSignature) : new List<TypeNameReference>(collection: constructorSignature));
		}

		[DataMember(Order = 0, IsRequired = true)]
		public TypeNameReference TargetType {
			get { return _targetType; }
			set {
				EnsureNotReadOnly();
				_targetType = value;
			}
		}

		[DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public ICollection<TypeNameReference> ConstructorSignature {
			get { return _constructorSignature; }
			set {
				EnsureNotReadOnly();
				_constructorSignature = value?.ToArray().AsReadOnlyCollection();
			}
		}

		IEnumerable<TypeNameReference> INewInstanceDependencyTarget.ConstructorSignature
			=> ConstructorSignature.EmptyIfNull();

		public override IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeContext = null)
			=> GetDefaultDependencyHandler(target: this).ToValueHolder();

		protected sealed override void CreateReadOnlyCopy(out DependencyTarget readOnlyCopy) {
			NewInstanceDependencyTarget locReadOnlyCopy;
			CreateReadOnlyCopy(out locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out NewInstanceDependencyTarget readOnlyCopy)
			=> readOnlyCopy = new NewInstanceDependencyTarget(other: this, isReadOnly: true);

		public new NewInstanceDependencyTarget AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				NewInstanceDependencyTarget readOnlyCopy;
				CreateReadOnlyCopy(out readOnlyCopy);
				return readOnlyCopy;
			}
		}

		INewInstanceDependencyTarget IAsReadOnlyMethod<INewInstanceDependencyTarget>.AsReadOnly()
			=> AsReadOnly();

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			if (TargetType == null)
				throw new ValidationException(message: $"Не указано значение свойства '{nameof(TargetType)}'.");
			int indexOfNullElement;
			if ((indexOfNullElement = ConstructorSignature.EmptyIfNull().IndexOf(i => i is null)) > -1)
				throw
					new ValidationException(message: $"Указано недопустимое значение свойства '{nameof(ConstructorSignature)}'. {FormatXResource(typeof(Array), "CanNotContainNull/NullAt", indexOfNullElement)}");
		}

		public override string ToString() {
			using (var stringBuilderBuffer = AcquireBuffer()) {
				var sb = stringBuilderBuffer.StringBuilder;
				sb.Append((TargetType?.Type).FmtStr());
				sb.Append(' ');
				sb.Append(ConstructorInfo.ConstructorName);
				sb.Append('(');
				var ctorSignatureArgIndex = -1;
				foreach (var ctorSignatureArg in ConstructorSignature.EmptyIfNull()) {
					if (++ctorSignatureArgIndex > 0)
						sb.Append(", ");
					sb.Append((ctorSignatureArg?.Type).FmtStr().G());
				}
				sb.Append(')');
				return sb.ToString();
			}
		}

	}

}