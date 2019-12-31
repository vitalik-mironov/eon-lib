using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;

using Eon.Globalization;

namespace Eon.Resources.Internal {

	internal static class ResourceUtilitiesCoreL0 {

		/// <summary>
		/// Значение: <see cref="StringComparer.Ordinal"/>.
		/// </summary>
		public static readonly StringComparer ResourceNameComparer = StringComparer.Ordinal;

		/// <summary>
		/// Значение: <see cref="StringComparison.Ordinal"/>.
		/// </summary>
		public static readonly StringComparison ResourceNameComparison = StringComparison.Ordinal;

		public static Stream RequireManifestResourceStream(Assembly mainAssembly, string resourceName, out ManifestResourceInfo info, CultureInfo culture = default) {
			GetManifestResourceStream(mainAssembly: mainAssembly, culture: culture, resourceName: resourceName, throwIfMissing: true, info: out info, stream: out var stream);
			return stream;
		}

		// TODO: Put strings into the resources.
		//
		public static bool GetManifestResourceStream(Assembly mainAssembly, string resourceName, bool throwIfMissing, out ManifestResourceInfo info, out Stream stream, CultureInfo culture = default) {
			//
			if (mainAssembly is null)
				throw new ArgumentNullException(paramName: nameof(mainAssembly));
			else if (resourceName is null)
				throw new ArgumentNullException(paramName: nameof(resourceName));
			else if (resourceName == string.Empty)
				throw new ArgumentOutOfRangeException(paramName: nameof(resourceName), message: "Cannot be an empty string.");
			//
			culture = culture ?? Thread.CurrentThread.CurrentCulture;
			var neutralLanguage = mainAssembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>();
			Assembly selectedAssembly;
			if (string.Equals(a: culture.Name, b: neutralLanguage?.CultureName, comparisonType: CultureNameUtilities.Comparison))
				selectedAssembly = neutralLanguage?.Location == UltimateResourceFallbackLocation.MainAssembly ? mainAssembly : getSatelliteAssembly(locMainAssembly: mainAssembly, locCulture: culture, locThrowIfMissing: throwIfMissing);
			else {
				try {
					selectedAssembly = getSatelliteAssembly(locMainAssembly: mainAssembly, locCulture: culture, locThrowIfMissing: true);
				}
				catch (Exception exception) {
					if (neutralLanguage is null) {
						if (throwIfMissing)
							throw;
						else
							selectedAssembly = null;
					}
					else
						try {
							if (neutralLanguage.Location == UltimateResourceFallbackLocation.Satellite)
								selectedAssembly = getSatelliteAssembly(locMainAssembly: mainAssembly, locCulture: CultureInfo.GetCultureInfo(name: neutralLanguage.CultureName), locThrowIfMissing: throwIfMissing);
							else
								selectedAssembly = mainAssembly;
						}
						catch (Exception secondException) {
							throw new AggregateException(exception, secondException);
						}
				}
			}
			return getResource(locAssembly: selectedAssembly, locResourceName: resourceName, locThrowIfMissing: throwIfMissing, locInfo: out info, locStream: out stream);
			//
			Assembly getSatelliteAssembly(Assembly locMainAssembly, CultureInfo locCulture, bool locThrowIfMissing) {
				try {
					return locMainAssembly.GetSatelliteAssembly(culture: locCulture);
				}
				catch (Exception locException) {
					var locParentCulture = locCulture.Parent;
					if (locParentCulture is null || ReferenceEquals(locParentCulture, locParentCulture.Parent)) {
						if (locThrowIfMissing)
							throw;
						else
							return null;
					}
					else
						try {
							return locMainAssembly.GetSatelliteAssembly(culture: locParentCulture);
						}
						catch (Exception locSecondException) {
							if (locThrowIfMissing)
								throw new AggregateException(locException, locSecondException);
							else
								return null;
						}
				}
			}
			bool getResource(Assembly locAssembly, string locResourceName, bool locThrowIfMissing, out Stream locStream, out ManifestResourceInfo locInfo) {
				var locResourceNames = locAssembly?.GetManifestResourceNames();
				if (locResourceNames?.Any(locItem => string.Equals(locResourceName, locItem, comparisonType: ResourceNameComparison)) ?? false) {
					try {
						locInfo = locAssembly.GetManifestResourceInfo(resourceName: locResourceName);
						locStream = locAssembly.GetManifestResourceStream(name: locResourceName);
						return true;
					}
					catch (Exception locException) {
						throw new InvalidOperationException(message: $"Error while loading required resource from assembly.{Environment.NewLine}\tAssembly:{Environment.NewLine}\t\t{locAssembly}{Environment.NewLine}\tResource name:{Environment.NewLine}\t\t{locResourceName}", innerException: locException);
					}
				}
				else if (locThrowIfMissing)
					throw
						new MissingManifestResourceException(
							message: $"Required resource is missing.{Environment.NewLine}\tAssembly:{Environment.NewLine}\t\t{(locAssembly is null ? FormatStringUtilitiesCoreL0.GetNullValueText() : locAssembly.ToString())}{Environment.NewLine}\tResource name:{Environment.NewLine}\t\t{locResourceName}");
				else {
					locStream = null;
					locInfo = null;
					return false;
				}
			}
		}

	}

}