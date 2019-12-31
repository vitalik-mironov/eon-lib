using System.IO;

namespace Eon.Context {

	/// <summary>
	/// Контекст загрузки какого-либо предмета (объекта) из файла.
	/// </summary>
	public interface ISubjectFileLoadContext
		:ISubjectLoadContext {

		/// <summary>
		/// Возвращает объект сведений о файле, из которого выполняется загрузка.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		FileInfo File { get; }

	}

}