using System;
using System.Runtime.Serialization;
using System.Xml;

using Eon.Net.Mime;

using Newtonsoft.Json;

namespace Eon.Context {

	/// <summary>
	/// Определяет программный интерфейс контекста какого-либо предмета загрузки.
	/// </summary>
	public interface ISubjectLoadContext
		:IDisposable, IContext {

		/// <summary>
		/// Возвращает URI происхождения предмета загрузки.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Не может быть относительным (см. <seealso cref="Uri.IsAbsoluteUri"/>).</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		Uri SiteOrigin { get; }

		/// <summary>
		/// Возвращает базовый URI загрузки.
		/// <para>Используется как указатель на место (источник) загрузки.</para>
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Не может быть относительным (см. <seealso cref="Uri.IsAbsoluteUri"/>).</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		Uri BaseUri { get; }

		/// <summary>
		/// Возвращает медиа-тип, которому должен соответствовать медиа-тип содержимого источника загрузки.
		/// <para>По умолчанию медиа-тип, возвращаемый данным свойством, есть универсальный медиа-тип <seealso cref="MediaTypeNameUtilities.AppOctetStream"/>.</para>
		/// <para>Не может быть <see langword="null"/>, пустой строкой, строкой, содержащей только пробельные символы, или строкой, длина которой превышает <seealso cref="MediaTypeNameUtilities.MediaTypeNameDefaultMaxLength"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		string MediaType { get; }

		/// <summary>
		/// Создает настройки XMOxy-считывателя, характерные для данного контекста зугрузки.
		/// </summary>
		/// <returns>Объект <see cref="XmlReaderSettings"/>.</returns>
		XmlReaderSettings CreateXmlReaderSettings();

		/// <summary>
		/// Создает XMOxy-сериализатор, характерный для данного контекста зугрузки.
		/// </summary>
		/// <param name="type">Тип объекта.</param>
		/// <returns>Объект <see cref="XmlObjectSerializer"/>.</returns>
		XmlObjectSerializer CreateXmlObjectSerializer(Type type);

		/// <summary>
		/// Создает JSON-сериализатор, характерный для данного контекста зугрузки.
		/// </summary>
		/// <returns>Объект <see cref="JsonSerializer"/>.</returns>
		JsonSerializer CreateJsonSerializer();

	}

}