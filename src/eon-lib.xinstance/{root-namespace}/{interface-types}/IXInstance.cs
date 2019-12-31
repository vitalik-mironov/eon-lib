using System;
using System.Collections.Generic;

using Eon.ComponentModel;
using Eon.ComponentModel.Dependencies;
using Eon.Description;
using Eon.Description.Annotations;

namespace Eon {

	// TODO_HIGH: Удалить наследование IDependencyHandler.
	//
	/// <summary>
	/// Defines base component created from the description (configuration) <see cref="IDescription"/>.
	/// </summary>
	public interface IXInstance
		:IDisposeNotifying, IDisposableDependencySupport, IInitializable, ITextViewSupport {

		/// <summary>
		/// Initializes this component.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Регистрирует обработчик инициализации данного компонента.
		/// <para>Регистрация обработчика возможна только до начала инициализации данного компонента, т.е. до перехода состояния <seealso cref="State"/> в <seealso cref="XInstanceStates.InitializeSent"/>. В случае, когда инициализация компонента уже была начата, выполнение метода вызовет исключение <see cref="InvalidOperationException"/>.</para>
		/// </summary>
		/// <param name="initialization">
		/// Регистрируемый обработчик инициализации.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		void RegisterInitialization(XInstanceInitialization initialization);

		/// <summary>
		/// Gets current state flags.
		/// <para>Dispose tolerant.</para>
		/// </summary>
		XInstanceStates State { get; }

		/// <summary>
		/// Формирует сводку состояния <see cref="IXInstanceStateSummary"/> данного экземпляра.
		/// <para>Может возвращает <see langword="null"/>.</para>
		/// <para>Обращение к методу не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		/// <returns>Объект <see cref="IXInstanceStateSummary"/>.</returns>
		IXInstanceStateSummary TryGetStateSummary();

		/// <summary>
		/// Indicates settings presence for this component. See <see cref="Description"/>.
		/// <para>Dispose tolerant.</para>
		/// </summary>
		bool HasDescription { get; }

		/// <summary>
		/// Gets component settings.
		/// <para>Depending on implementation some component no needs for settings. Depending on implementation, some components no needs for settings. Thus, if component have not a settings, then this property throws an exception <see cref="EonException"/>. See <see cref="HasDescription"/>.</para>
		/// </summary>
		IDescription Description { get; }

		/// <summary>
		/// Gets component settings.
		/// <para>Depending on implementation some component no needs for settings. Depending on implementation, some components no needs for settings. Thus, if component have not a settings, then this property throws an exception <see cref="EonException"/>. See <see cref="HasDescription"/>.</para>
		/// <para>Dispose tolerant.</para>
		/// </summary>
		IDescription DescriptionDisposeTolerant { get; }

		/// <summary>
		/// Ensures that this component has been initialized.
		/// <para>If component not initialized yet, then method throws <see cref="EonException"/>.</para>
		/// </summary>
		void EnsureInitialized();

		/// <summary>
		/// Gets component in scope of which this component was created.
		/// <para>Can be <see langword="null"/>.</para>
		/// </summary>
		IXInstance Scope { get; }

		/// <summary>
		/// Gets component in scope of which this component was created.
		/// <para>Can be <see langword="null"/>.</para>
		/// <para>Dispose tolerant.</para>
		/// </summary>
		IXInstance ScopeDisposeTolerant { get; }

		/// <summary>
		/// Возвращает список компонентов, принадлежащих данному компоненту.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		IReadOnlyList<IXInstance> ScopedInstances { get; }

		/// <summary>
		/// Возвращает список компонентов, принадлежащих данному компоненту.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки (в случае выгрузки возвращается пустой список).</para>
		/// </summary>
		IReadOnlyList<IXInstance> ScopedInstancesDisposeTolerant { get; }

		/// <summary>
		/// Creates scoped XInstance-component from specified description (configuration) <paramref name="description"/>.
		/// <para>In general case, type of XInstance-component to be created is the type defined by <seealso cref="XInstanceContractAttribute"/> for <paramref name="description"/>.</para>
		/// <para>Created component places into <see cref="ScopedInstances"/>.</para>
		/// </summary>
		/// <typeparam name="TInstance">Type constraint of XInstance-component to be created.</typeparam>
		/// <param name="description">
		/// XInstance-component description (configuration).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="constraint">
		/// Strict type constraint of XInstance-component to be created.
		/// <para>Specified type must be compatible with <typeparamref name="TInstance"/>.</para>
		/// </param>
		/// <param name="ignoreDisabilityOption">
		/// Ignore functional disability of <paramref name="description"/> (see <see cref="IAbilityOption.IsDisabled"/>).
		/// </param>
		TInstance CreateScopedInstance<TInstance>(IDescription description, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance;

		/// <summary>
		/// Creates scoped XInstance-component from specified description (configuration) <paramref name="description"/>.
		/// <para>In general case, type of XInstance-component to be created is the type defined by <seealso cref="XInstanceContractAttribute"/> for <paramref name="description"/>.</para>
		/// <para>Created component places into <see cref="ScopedInstances"/>.</para>
		/// </summary>
		/// <typeparam name="TArg1">Type of <paramref name="arg1"/>.</typeparam>
		/// <typeparam name="TInstance">Type constraint of XInstance-component to be created.</typeparam>
		/// <param name="description">
		/// XInstance-component description (configuration).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="arg1">
		/// XInstance-component constructor arg1 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="constraint">
		/// Strict type constraint of XInstance-component to be created.
		/// <para>Specified type must be compatible with <typeparamref name="TInstance"/>.</para>
		/// </param>
		/// <param name="ignoreDisabilityOption">
		/// Ignore functional disability of <paramref name="description"/> (see <see cref="IAbilityOption.IsDisabled"/>).
		/// </param>
		TInstance CreateScopedInstance<TArg1, TInstance>(IDescription description, TArg1 arg1, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance;

		/// <summary>
		/// Creates scoped XInstance-component from specified description (configuration) <paramref name="description"/>.
		/// <para>In general case, type of XInstance-component to be created is the type defined by <seealso cref="XInstanceContractAttribute"/> for <paramref name="description"/>.</para>
		/// <para>Created component places into <see cref="ScopedInstances"/>.</para>
		/// </summary>
		/// <typeparam name="TArg1">Type of <paramref name="arg1"/>.</typeparam>
		/// <typeparam name="TArg2">Type of <paramref name="arg2"/>.</typeparam>
		/// <typeparam name="TInstance">Type constraint of XInstance-component to be created.</typeparam>
		/// <param name="description">
		/// XInstance-component description (configuration).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="arg1">
		/// XInstance-component constructor arg1 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="arg2">
		/// XInstance-component constructor arg2 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="constraint">
		/// Strict type constraint of XInstance-component to be created.
		/// <para>Specified type must be compatible with <typeparamref name="TInstance"/>.</para>
		/// </param>
		/// <param name="ignoreDisabilityOption">
		/// Ignore functional disability of <paramref name="description"/> (see <see cref="IAbilityOption.IsDisabled"/>).
		/// </param>
		TInstance CreateScopedInstance<TArg1, TArg2, TInstance>(IDescription description, TArg1 arg1, TArg2 arg2, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance;

		/// <summary>
		/// Creates scoped XInstance-component from specified description (configuration) <paramref name="description"/>.
		/// <para>In general case, type of XInstance-component to be created is the type defined by <seealso cref="XInstanceContractAttribute"/> for <paramref name="description"/>.</para>
		/// <para>Created component places into <see cref="ScopedInstances"/>.</para>
		/// </summary>
		/// <typeparam name="TArg1">Type of <paramref name="arg1"/>.</typeparam>
		/// <typeparam name="TArg2">Type of <paramref name="arg2"/>.</typeparam>
		/// <typeparam name="TArg3">Type of <paramref name="arg3"/>.</typeparam>
		/// <typeparam name="TInstance">Type constraint of XInstance-component to be created.</typeparam>
		/// <param name="description">
		/// XInstance-component description (configuration).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="arg1">
		/// XInstance-component constructor arg1 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="arg2">
		/// XInstance-component constructor arg2 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="arg3">
		/// XInstance-component constructor arg3 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="constraint">
		/// Strict type constraint of XInstance-component to be created.
		/// <para>Specified type must be compatible with <typeparamref name="TInstance"/>.</para>
		/// </param>
		/// <param name="ignoreDisabilityOption">
		/// Ignore functional disability of <paramref name="description"/> (see <see cref="IAbilityOption.IsDisabled"/>).
		/// </param>
		TInstance CreateScopedInstance<TArg1, TArg2, TArg3, TInstance>(IDescription description, TArg1 arg1, TArg2 arg2, TArg3 arg3, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance;

		/// <summary>
		/// Creates scoped XInstance-component from specified description (configuration) <paramref name="description"/>.
		/// <para>In general case, type of XInstance-component to be created is the type defined by <seealso cref="XInstanceContractAttribute"/> for <paramref name="description"/>.</para>
		/// <para>Created component places into <see cref="ScopedInstances"/>.</para>
		/// </summary>
		/// <typeparam name="TArg1">Type of <paramref name="arg1"/>.</typeparam>
		/// <typeparam name="TArg2">Type of <paramref name="arg2"/>.</typeparam>
		/// <typeparam name="TArg3">Type of <paramref name="arg3"/>.</typeparam>
		/// <typeparam name="TArg4">Type of <paramref name="arg4"/>.</typeparam>
		/// <typeparam name="TInstance">Type constraint of XInstance-component to be created.</typeparam>
		/// <param name="description">
		/// XInstance-component description (configuration).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="arg1">
		/// XInstance-component constructor arg1 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="arg2">
		/// XInstance-component constructor arg2 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="arg3">
		/// XInstance-component constructor arg3 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="arg4">
		/// XInstance-component constructor arg4 (not regarding the first two args according to XInstance-contract: '<see cref="IXInstance"/> scope' and '<see cref="IDescription"/> description').
		/// </param>
		/// <param name="constraint">
		/// Strict type constraint of XInstance-component to be created.
		/// <para>Specified type must be compatible with <typeparamref name="TInstance"/>.</para>
		/// </param>
		/// <param name="ignoreDisabilityOption">
		/// Ignore functional disability of <paramref name="description"/> (see <see cref="IAbilityOption.IsDisabled"/>).
		/// </param>
		TInstance CreateScopedInstance<TArg1, TArg2, TArg3, TArg4, TInstance>(IDescription description, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance;

	}

}