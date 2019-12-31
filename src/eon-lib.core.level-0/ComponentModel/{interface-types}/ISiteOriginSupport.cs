using System;

namespace Eon.ComponentModel {

	/// <summary>
	/// Представляет свойство, определяющее URI происхождения компонента (см. <seealso cref="SiteOrigin"/>).
	/// </summary>
	public interface ISiteOriginSupport {

		/// <summary>
		/// Возвращает признак, указывающий, имеет ли данный компонент URI происхождения (см. <seealso cref="SiteOrigin"/>, <seealso cref="SetSiteOrigin(Uri)"/>).
		/// </summary>
		bool HasSiteOrigin { get; }

		/// <summary>
		/// Возвращает URI происхождения данного компонента (см. <seealso cref="HasSiteOrigin"/>).
		/// <para>Обращение к данному свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если на момент обращения URI происхождения данного компонента отсутствует (см. <seealso cref="HasSiteOrigin"/>).</para>
		/// </summary>
		Uri SiteOrigin { get; }

		/// <summary>
		/// Устанавливает для данного компонента URI происхождения.
		/// <para>URI происхождения может быть установлен только единажды в жизненном цикле компонента. При попытке установить другой URI метод вызовет исключение <seealso cref="InvalidOperationException"/>.</para>
		/// </summary>
		/// <param name="siteOrigin">
		/// Устанавливаемый URI происхождения данного компонента.
		/// <para>Не может быть null.</para>
		/// <para>Должен быть абсолютным (см. <seealso cref="Uri.IsAbsoluteUri"/>).</para>
		/// </param>
		void SetSiteOrigin(Uri siteOrigin);

	}

}