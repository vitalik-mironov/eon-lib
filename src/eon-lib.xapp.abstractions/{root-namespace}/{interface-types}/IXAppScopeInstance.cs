using System.Threading;
using Eon.Description;

namespace Eon {

	/// <summary>
	/// Определяет компонент <seealso cref="IXInstance"/>, принадлежащий Eon-приложению <seealso cref="IXApp{TDescription}"/>.
	/// </summary>
	public interface IXAppScopeInstance
		:IXInstance {

		/// <summary>
		/// Возвращает Eon-приложение, которому принадлежит данный компонент.
		/// <para>Свойство является немутабельным.</para>
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		IXApp<IXAppDescription> App { get; }

		/// <summary>
		/// Возвращает Eon-приложение, которому принадлежит данный компонент.
		/// <para>Обращение к свойству в отличие от <see cref="App"/> не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		IXApp<IXAppDescription> AppDisposeTolerant { get; }

		/// <summary>
		/// Возвращает признак, указывающий на наличие запроса остановки и выгрузки компонента.
		/// <para>Вне зависимости от конкретной реализации компонента возвращается <see langword="true"/>, если выполняется одно из условий:</para>
		/// <para>• <see cref="App"/>: <see cref="IXApp{TDescription}.HasAppShutdownRequested"/>  == <see langword="true"/>;</para>
		/// <para>• <see cref="App"/> == <see langword="null"/> и <see cref="IEonDisposable.IsDisposeRequested"/> == <see langword="true"/>.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		bool HasShutdownRequested { get; }

		CancellationToken ShutdownToken { get; }

	}

}