using System;

namespace Eon.Description {

	public static class DescriptionPackageUtilities {

		/// <summary>
		/// Возвращает расширение файла, содержащего описание в формате JSON.
		/// <para>Значение: <see cref="DescriptionPackageConstants.DescriptionJsonFormatFileExtension"/>.</para>
		/// </summary>
		public static readonly string DescriptionJsonFormatFileExtension = DescriptionPackageConstants.DescriptionJsonFormatFileExtension;

		/// <summary>
		/// Возвращает расширение файла, содержащего описание в формате XML.
		/// <para>Значение: <see cref="DescriptionPackageConstants.DescriptionXmlFormatFileExtension"/>.</para>
		/// </summary>
		public static readonly string DescriptionXmlFormatFileExtension = DescriptionPackageConstants.DescriptionXmlFormatFileExtension;

		/// <summary>
		/// Возвращает расширение файла, содержащего пакет описаний в формате XML: <see cref="DescriptionPackageConstants.PackageXmlFormatFileExtension"/>.
		/// </summary>
		public static readonly string PackageXmlFormatFileExtension = DescriptionPackageConstants.PackageXmlFormatFileExtension;

		public static readonly string PackageDefinitionXmlFormatFileExtension = DescriptionPackageConstants.PackageDefinitionXmlFormatFileExtension;

		/// <summary>
		/// Возвращает расширение файла, содержащего пакет описаний в формате JSON: <see cref="DescriptionPackageConstants.PackageJsonFormatFileExtension"/>.
		/// </summary>
		public static readonly string PackageJsonFormatFileExtension = DescriptionPackageConstants.PackageJsonFormatFileExtension;

		public static readonly string PackageDefinitionJsonFormatFileExtension = DescriptionPackageConstants.PackageDefinitionJsonFormatFileExtension;

		/// <summary>
		/// Возвращает расширение файла, содержащего пакет описаний в формате ZIP-архива: <see cref="DescriptionPackageConstants.PackageZipFormatFileExtension"/>.
		/// </summary>
		public static readonly string PackageZipFormatFileExtension = DescriptionPackageConstants.PackageZipFormatFileExtension;

		/// <summary>
		/// Возвращает имя файла пакета описаний по умолчанию (без расширения): <see cref="DescriptionPackageConstants.PackageFileNameWithoutExtension"/>.
		/// </summary>
		public static readonly string PackageFileNameWithoutExtension = DescriptionPackageConstants.PackageFileNameWithoutExtension;

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