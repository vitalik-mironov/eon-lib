using System.Collections.Generic;

using Eon.Collections.Trees;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Определяет программный интерфейс контейнера компонентов структуры дерева метаданных <seealso cref="IMetadataTree"/>.
	/// </summary>
	/// <typeparam name="TOwner">Тип компонента-владельца контейнера.</typeparam>
	/// <typeparam name="TComponent">Тип компонентов контейнера.</typeparam>
	public interface IMetadataTreeStructureContainer<TOwner, TComponent>
		:ICollection<TComponent>
		where TOwner : class, IMetadataTreeStructureComponent
		where TComponent : class, IMetadataTreeStructureComponent {

		/// <summary>
		/// Возвращает компонент, предшествующий указанному (в порядке следования компонентов в контейнере).
		/// </summary>
		/// <param name="component">Компонент.</param>
		/// <returns>Объект <typeparamref name="TComponent"/>.</returns>
		TComponent GetBefore(TComponent component);

		/// <summary>
		/// Возвращает компонент, следующий за указанным (в порядке следования компонентов в контейнере).
		/// </summary>
		/// <param name="component">Компонент.</param>
		/// <returns>Объект <typeparamref name="TComponent"/>.</returns>
		TComponent GetAfter(TComponent component);

		/// <summary>
		/// Добавляет в контейнер компонент и возвращает объект, представляющий связь добавленного компонента и компонента-владельца данного контейнера.
		/// </summary>
		/// <param name="component">Компонент.</param>
		/// <returns>Объект <see cref="ILink{TSource, TTarget}"/>.</returns>
		ILink<TOwner, TComponent> AddComponent(TComponent component);

		/// <summary>
		/// Добавляет компонент в контейнер.
		/// <para>В указанном линке (<paramref name="link"/>) свойство <see cref="ILink{TSource, TTarget}.Source"/> должно соответствовать компоненту-владельцу данного контейнера <see cref="OwnerComponent"/>.</para>
		/// </summary>
		/// <param name="link">Линк, представляющий связь между компонентом-владельцем <see cref="OwnerComponent"/> и указанным в линке (<paramref name="link"/>), в свойстве <see cref="ILink{TSource, TTarget}.Target"/> компонентом.</param>
		void AddLink(ILink<TOwner, TComponent> link);

		/// <summary>
		/// Удаляет компонент из контейнера.
		/// <para>В указанном линке (<paramref name="link"/>) свойство <see cref="ILink{TSource, TTarget}.Source"/> должно соответствовать компоненту-владельцу данного контейнера <see cref="OwnerComponent"/>.</para>
		/// </summary>
		/// <param name="link">Линк, представляющий связь между компонентом-владельцем <see cref="OwnerComponent"/> и удаляемым компонентом (<see cref="ILink{TSource, TTarget}.Target"/>).</param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool Remove(ILink<TOwner, TComponent> link);

		/// <summary>
		/// Возвращает признак, указывающий, есть ли у данного контейнера компонент-владелец.
		/// </summary>
		bool HasOwnerComponent { get; }

		/// <summary>
		/// Возвращает компонент-владелец данного контейнера.
		/// <para>Если компонент-владелец отсутствует (см. <see cref="HasOwnerComponent"/>), то обращение к свойству вызовет исключение <see cref="System.InvalidOperationException"/>.</para>
		/// </summary>
		TOwner OwnerComponent { get; }

	}

}