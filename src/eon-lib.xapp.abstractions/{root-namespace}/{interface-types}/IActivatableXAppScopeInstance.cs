using System;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;

namespace Eon {

	public interface IActivatableXAppScopeInstance
		:IXAppScopeInstance, IAutoActivationOption {

		/// <summary>
		/// Возвращает управляющий элемент активации/деактивации этого компонента.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если свойство еще не инициализировано (см. <see cref="IInitializable.InitializeAsync(System.Threading.CancellationToken)"/>).</para>
		/// </summary>
		IRunControl<IXAppScopeInstance> ActivateControl { get; }

		/// <summary>
		/// Возвращает признак, указывающий на состояние функциональной активности этого компонента.
		/// <para>Определяется условиями:</para>
		/// <para>• Инициализирован (см. <see cref="IXInstance.State"/>).</para>
		/// <para>• Активирован (см. <see cref="ActivateControl"/>, <see cref="IRunControl.IsStarted"/>).</para>
		/// <para>• Отсутствие запроса деактивации (см. <see cref="ActivateControl"/>, <see cref="IRunControl.HasStopRequested"/>).</para>
		/// <para>• Отсутствие запроса выгрузки (см. <see cref="IEonDisposable.IsDisposeRequested"/>).</para>
		/// </summary>
		bool IsActive { get; }

		bool HasDeactivationRequested { get; }

		Task<IRunControlAttemptSuccess> ActivateAsync(IContext ctx = default);

		Task<IRunControlAttemptSuccess> ActivateAsync(TaskCreationOptions options, IContext ctx = default);

		Task DeactivateAsync(IContext ctx = default);

	}

}