using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Description {

	public sealed class DescriptionPackageLoadException
		:EonException {

		readonly DescriptionPackageLocator _packageLocator;

		public DescriptionPackageLoadException(string message, DescriptionPackageLocator packageLocator)
			: this(message, packageLocator, null) { }

		public DescriptionPackageLoadException(DescriptionPackageLocator packageLocator, Exception innerException)
			: this(null, packageLocator, innerException) { }

		public DescriptionPackageLoadException(string message, DescriptionPackageLocator packageLocator, Exception innerException)
			: base(
					message: string.IsNullOrEmpty(message) ? (packageLocator is null ? FormatXResource(typeof(DescriptionPackageLoadException), "DefaultMessage/WithoutNameReference") : FormatXResource(typeof(DescriptionPackageLoadException), "DefaultMessage", packageLocator)) : (packageLocator is null ? FormatXResource(typeof(DescriptionPackageLoadException), "UserMessage/WithoutNameReference", message) : FormatXResource(typeof(DescriptionPackageLoadException), "UserMessage", packageLocator, message)),
					innerException: innerException) {
			//
			_packageLocator = packageLocator;
		}

		public DescriptionPackageLocator PackageLocator
			=> _packageLocator;

	}

}