using System;

namespace Eon.Description {

	public static class DescriptionPackageConstants {

		/// <summary>
		/// Возвращает расширение файла, содержащего описание в формате JSON.
		/// <para>Значение: '.limedj'.</para>
		/// </summary>
		public static readonly string DescriptionJsonFormatFileExtension = ".limedj";

		/// <summary>
		/// Возвращает расширение файла, содержащего описание в формате XML.
		/// <para>Значение: '.limedx'.</para>
		/// </summary>
		public static readonly string DescriptionXmlFormatFileExtension = ".limedx";

		/// <summary>
		/// Возвращает расширение файла, содержащего пакет описаний в формате XML: '.limedpx'.
		/// </summary>
		public static readonly string PackageXmlFormatFileExtension = ".limedpx";

		public static readonly string PackageDefinitionXmlFormatFileExtension = ".limedpdx";

		/// <summary>
		/// Возвращает расширение файла, содержащего пакет описаний в формате JSON: '.limedpj'.
		/// </summary>
		public static readonly string PackageJsonFormatFileExtension = ".limedpj";

		public static readonly string PackageDefinitionJsonFormatFileExtension = ".limedpdj";

		/// <summary>
		/// Возвращает расширение файла, содержащего пакет описаний в формате ZIP-архива: '.limedpz'.
		/// </summary>
		public static readonly string PackageZipFormatFileExtension = ".limedpz";

		/// <summary>
		/// Возвращает имя файла пакета описаний по умолчанию (без расширения): 'package'.
		/// </summary>
		public static readonly string PackageFileNameWithoutExtension = "package";

		public const string UndefinedSiteOriginString = UriUtilities.UndefinedUriString;

		public static readonly Uri UndefinedSiteOrigin = new Uri(UndefinedSiteOriginString, UriKind.Absolute);

		public const string InMemoryGeneratedPackageBaseUriString = UriUtilities.UndefinedUriString;

		public static readonly Uri InMemoryGeneratedPackageBaseUri = new Uri(InMemoryGeneratedPackageBaseUriString, UriKind.Absolute);

		/// <summary>
		/// Значение: '.'.
		/// </summary>
		public static readonly string SatellitePackageLocationQualifier = ".";

	}

}