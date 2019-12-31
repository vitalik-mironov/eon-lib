using System;

namespace Eon.Data.Querying {

	/// <summary>
	/// Базовый тип компонента, предоставляющего определения запросов данных к к.л. источнику данных, определяемому контекстом <seealso cref="Context"/> (см. <seealso cref="IDataQueryingContext"/>).
	/// </summary>
	public interface IDataQuerying
		:IDisposable {

		/// <summary>
		/// Возвращает контекст запросов данных, с которым ассоциирован данный компонент.
		/// <para>В общем случае определяет источник данных для определений запросов, предаставляемых данным компонентом.</para>
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		IDataQueryingContext Context { get; }

	}

}