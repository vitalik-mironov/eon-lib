using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

using Eon.Collections;
using Eon.Resources.Internal;
using Eon.Threading;

using StringResourcesRepoKey = System.Tuple<System.Type, System.Globalization.CultureInfo>;

namespace Eon.Resources {

	public static class ResourceUtilities {

		/// <summary>
		/// Значение: <see cref="StringComparer.Ordinal"/> (<see cref="ResourceUtilitiesCoreL0.ResourceNameComparer"/>).
		/// </summary>
		public static readonly StringComparer ResourceNameComparer = ResourceUtilitiesCoreL0.ResourceNameComparer;

		/// <summary>
		/// Значение: <see cref="StringComparison.Ordinal"/> (<see cref="ResourceUtilitiesCoreL0.ResourceNameComparison"/>).
		/// </summary>
		public static readonly StringComparison ResourceNameComparison = ResourceUtilitiesCoreL0.ResourceNameComparison;

		static readonly IDictionary<StringResourcesRepoKey, DisposableLazy<string>> __StringResourcesRepo;

		static readonly PrimitiveSpinLock __StringResourcesRepoSpinLock;

		static ResourceUtilities() {
			__StringResourcesRepoSpinLock = new PrimitiveSpinLock();
			__StringResourcesRepo = new Dictionary<StringResourcesRepoKey, DisposableLazy<string>>();
		}

		public static string RequireStringResource(Type resourceSource, CultureInfo culture) {
			resourceSource.EnsureNotNull(nameof(resourceSource));
			culture.EnsureNotNull(nameof(culture));
			//
			return
				__StringResourcesRepo
				.GetOrAdd(
					spinLock: __StringResourcesRepoSpinLock,
					key: new StringResourcesRepoKey(resourceSource, culture),
					factory: key => new DisposableLazy<string>(factory: () => P_RequireStringResource(key)))
				.Value;
		}

		static string P_RequireStringResource(StringResourcesRepoKey key) {
			key.EnsureNotNull(nameof(key));
			//
			var resourceManager = new ResourceManager(resourceSource: key.Item1);
			string result;
			try {
				result = resourceManager.GetString(key.Item1.Name, key.Item2);
			}
			catch (Exception exception) when (exception is MissingManifestResourceException) {
				throw
					new MissingManifestResourceException(
						message: $"Ошибка при получении ресурса (тип-источник '{key.Item1.AssemblyQualifiedName}', культура '{key.Item2.Name}').",
						inner: exception);
			}
			return result;
		}

		public static Uri ConstructManifestResourceUri(AssemblyName assemblyName, string name) {
			assemblyName.EnsureNotNull(nameof(assemblyName));
			name
				.EnsureNotNull(nameof(name))
				.EnsureNotEmpty();
			//
			return
				new Uri(
					uriString: $"{UriUtilities.UriSchemeAsemblyManifestResource}:///{Uri.EscapeDataString(assemblyName.FullName)}/{Uri.EscapeDataString(name)}",
					uriKind: UriKind.Absolute);
		}

		// TODO: Put strings into the resources.
		//
		static bool P_TryDeconstructManifestResourceUri(ArgumentUtilitiesHandle<Uri> uri, bool partial, out AssemblyName assemblyName, out string name, out Exception exception) {
			uri.EnsureNotNull();
			//
			try {
				uri
					.EnsureAbsolute()
					.EnsureScheme(scheme: UriUtilities.UriSchemeAsemblyManifestResource)
					.EnsureComponentsOnly(components: UriComponents.Scheme | UriComponents.Path)
					.EnsureFixedSegmentCount(count: partial ? 2 : 3);
				try {
					var locAssemblyNameString = Uri.UnescapeDataString(stringToUnescape: uri.Value.Segments[ 1 ].TrimEndSingle('/'));
					var locAssemblyName = new AssemblyName(assemblyName: locAssemblyNameString);
					string locName;
					if (partial)
						locName = null;
					else {
						locName = Uri.UnescapeDataString(uri.Value.Segments[ 2 ]);
						if (string.IsNullOrEmpty(locName))
							throw new EonException(message: "В указанном URI отсутствует имя ресурса.");
					}
					assemblyName = locAssemblyName;
					name = locName;
					exception = null;
					return true;
				}
				catch (Exception locException) {
					throw
						new ArgumentException(paramName: uri.Name, message: "Ошибка обработки компонентов URI.", innerException: locException);
				}
			}
			catch (Exception locException) {
				exception = locException;
				assemblyName = default;
				name = default;
				return false;
			}
		}

		public static bool TryDeconstructManifestResourceUri(ArgumentUtilitiesHandle<Uri> uri, out AssemblyName assemblyName, out string name, out Exception exception)
			=> P_TryDeconstructManifestResourceUri(uri: uri, partial: false, assemblyName: out assemblyName, name: out name, exception: out exception);

		public static bool TryDeconstructManifestResourceUri(ArgumentUtilitiesHandle<Uri> uri, out AssemblyName assemblyName, out Exception exception)
			=> P_TryDeconstructManifestResourceUri(uri: uri, partial: true, assemblyName: out assemblyName, name: out var name, exception: out exception);

		public static bool TryDeconstructManifestResourceUri(Uri uri, out AssemblyName assemblyName, out string name, out Exception exception)
			=> TryDeconstructManifestResourceUri(uri: uri.Arg(nameof(uri)), assemblyName: out assemblyName, name: out name, exception: out exception);

		public static bool TryDeconstructManifestResourceUri(Uri uri, out AssemblyName assemblyName, out Exception exception)
			=> TryDeconstructManifestResourceUri(uri: uri.Arg(nameof(uri)), assemblyName: out assemblyName, exception: out exception);

		// TODO: Put strings into the resources.
		//
		public static void DeconstructManifestResourceUri(ArgumentUtilitiesHandle<Uri> uri, out AssemblyName assemblyName, out string name) {
			if (!TryDeconstructManifestResourceUri(uri: uri, assemblyName: out assemblyName, name: out name, exception: out var exception))
				throw
					new ArgumentException(
						paramName: uri.Name,
						message: $"Ошибка разбора указанного URI ресурса сборки.{Environment.NewLine}\tURI:{uri.Value.FmtStr().GNLI2()}",
						innerException: exception);
		}

		// TODO: Put strings into the resources.
		//
		public static void DeconstructManifestResourceUri(ArgumentUtilitiesHandle<Uri> uri, out AssemblyName assemblyName) {
			if (!TryDeconstructManifestResourceUri(uri: uri, assemblyName: out assemblyName, exception: out var exception))
				throw
					new ArgumentException(
						paramName: uri.Name,
						message: $"Ошибка разбора указанного URI ресурса сборки.{Environment.NewLine}\tURI:{uri.Value.FmtStr().GNLI2()}",
						innerException: exception);
		}

		public static void DeconstructManifestResourceUri(Uri uri, out AssemblyName assemblyName, out string name)
			=> DeconstructManifestResourceUri(uri: uri.Arg(nameof(uri)), assemblyName: out assemblyName, name: out name);

		public static void DeconstructManifestResourceUri(Uri uri, out AssemblyName assemblyName)
			=> DeconstructManifestResourceUri(uri: uri.Arg(nameof(uri)), assemblyName: out assemblyName);

		// TODO: Put strings into the resources.
		//
		static Stream P_RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri, CultureInfo culture, bool ignoreCulture, out ManifestResourceInfo info, out AssemblyName assemblyName, out string resourceName) {
			DeconstructManifestResourceUri(uri: uri, assemblyName: out var locAssemblyName, name: out var locName);
			var mainAssembly = Assembly.Load(assemblyRef: locAssemblyName);
			if (ignoreCulture) {
				if (mainAssembly.GetManifestResourceNames().Any(locItem => locItem.EqualsOrdinalCS(locName))) {
					var locInfo = mainAssembly.GetManifestResourceInfo(resourceName: locName);
					var stream = mainAssembly.GetManifestResourceStream(name: locName);
					assemblyName = locAssemblyName;
					resourceName = locName;
					info = locInfo;
					return stream;
				}
				else
					throw new MissingManifestResourceException(message: $"Required manifest resource is missing.{Environment.NewLine}\tResource URI:{uri.FmtStr().GNLI2()}{Environment.NewLine}\tAssembly:{mainAssembly.FullName.FmtStr().GNLI2()}{Environment.NewLine}\tResource name:{locName.FmtStr().GNLI2()}");
			}
			else {
				Stream stream;
				ManifestResourceInfo locInfo;
				try {
					ResourceUtilitiesCoreL0.GetManifestResourceStream(mainAssembly: mainAssembly, resourceName: locName, throwIfMissing: true, info: out locInfo, stream: out stream, culture: culture);
				}
				catch (MissingManifestResourceException exception) {
					throw new MissingManifestResourceException(message: $"Required manifest resource is missing.{Environment.NewLine}\tResource URI:{uri.FmtStr().GNLI2()}{Environment.NewLine}\tAssembly:{mainAssembly.FullName.FmtStr().GNLI2()}{Environment.NewLine}\tResource name:{locName.FmtStr().GNLI2()}", inner: exception);
				}
				assemblyName = locAssemblyName;
				resourceName = locName;
				info = locInfo;
				return stream;
			}
		}

		public static Stream RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri, CultureInfo culture, out ManifestResourceInfo info, out AssemblyName assemblyName, out string resourceName)
			=> P_RequireManifestResourceStream(uri: uri, info: out info, assemblyName: out assemblyName, resourceName: out resourceName, ignoreCulture: false, culture: culture);

		public static Stream RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri, out ManifestResourceInfo info, out AssemblyName assemblyName, out string resourceName)
			=> P_RequireManifestResourceStream(uri: uri, info: out info, assemblyName: out assemblyName, resourceName: out resourceName, ignoreCulture: true, culture: null);

		public static Stream RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri, out string resourceName)
			=> RequireManifestResourceStream(uri: uri, info: out _, assemblyName: out _, resourceName: out resourceName);

		public static Stream RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri, out AssemblyName assemblyName, out string resourceName)
			=> RequireManifestResourceStream(uri: uri, info: out _, assemblyName: out assemblyName, resourceName: out resourceName);

		public static Stream RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri)
			=> RequireManifestResourceStream(uri: uri, info: out _, assemblyName: out _, resourceName: out _);

		public static Stream RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri, CultureInfo culture, out string resourceName)
			=> RequireManifestResourceStream(uri: uri, info: out _, assemblyName: out _, resourceName: out resourceName, culture: culture);

		public static Stream RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri, CultureInfo culture, out AssemblyName assemblyName, out string resourceName)
			=> RequireManifestResourceStream(uri: uri, info: out _, assemblyName: out assemblyName, resourceName: out resourceName, culture: culture);

		public static Stream RequireManifestResourceStream(ArgumentUtilitiesHandle<Uri> uri, CultureInfo culture)
			=> RequireManifestResourceStream(uri: uri, info: out _, assemblyName: out _, resourceName: out _, culture: culture);

		public static Stream RequireManifestResourceStream(Uri uri)
			=> RequireManifestResourceStream(uri: uri.Arg(nameof(uri)));

		public static Stream RequireManifestResourceStream(Uri uri, CultureInfo culture)
			=> RequireManifestResourceStream(uri: uri.Arg(nameof(uri)), culture: culture);

		public static bool GetManifestResourceStream(Assembly mainAssembly, string resourceName, bool throwIfMissing, out Stream stream, CultureInfo culture = default)
			=> ResourceUtilitiesCoreL0.GetManifestResourceStream(mainAssembly: mainAssembly, culture: culture, resourceName: resourceName, throwIfMissing: throwIfMissing, stream: out stream, info: out _);

		public static Stream RequireManifestResourceStream(Assembly mainAssembly, string resourceName, CultureInfo culture = default)
			=> ResourceUtilitiesCoreL0.RequireManifestResourceStream(mainAssembly: mainAssembly, culture: culture, resourceName: resourceName, info: out _);

	}

}