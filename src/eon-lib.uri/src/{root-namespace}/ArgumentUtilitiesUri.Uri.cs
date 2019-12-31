using System;
using System.Globalization;

using static System.String;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesUri {

		[Obsolete("Use EnsureAbsolute(ArgumentUtilitiesHandle<Uri>) instead.", true)]
		public static Uri EnsureAbsolute(this Uri value, string name) {
			if (value is null || value.IsAbsoluteUri)
				return value;
			throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(value) : name, FormatXResource(typeof(Uri), "NotAbsoluteUri", value));
		}

		public static ArgumentUtilitiesHandle<Uri> EnsureAbsolute(this ArgumentUtilitiesHandle<Uri> hnd) {
			if (hnd.Value != null && !hnd.Value.IsAbsoluteUri) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "NotAbsoluteUri", hnd.Value)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			return hnd;
		}

		public static ArgumentUtilitiesHandle<Uri> EnsureRelative(this ArgumentUtilitiesHandle<Uri> hnd) {
			if (hnd.Value == null || !hnd.Value.IsAbsoluteUri)
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "NotRelativeUri", hnd.Value)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		public static ArgumentUtilitiesHandle<Uri> EnsureScheme(this ArgumentUtilitiesHandle<Uri> hnd, string scheme, string altScheme) {
			scheme.Arg(nameof(scheme)).EnsureNotNullOrWhiteSpace();
			altScheme.Arg(nameof(altScheme)).EnsureNotNullOrWhiteSpace();
			//
			if (!(hnd.Value is null) && hnd.Value.IsAbsoluteUri && !(hnd.Value.Scheme.EqualsOrdinalCI(scheme) || hnd.Value.Scheme.EqualsOrdinalCI(altScheme))) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "NotValidScheme/Expected", hnd.Value, scheme.FmtStr().G())}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<Uri> EnsureScheme(this ArgumentUtilitiesHandle<Uri> hnd, string scheme) {
			scheme.Arg(nameof(scheme)).EnsureNotNullOrWhiteSpace();
			//
			if (hnd.Value?.IsAbsoluteUri == true && !hnd.Value.Scheme.EqualsOrdinalCI(scheme)) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "NotValidScheme/Expected", hnd.Value, scheme.FmtStr().G())}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<Uri> EnsureFileScheme(this ArgumentUtilitiesHandle<Uri> hnd)
			=> EnsureScheme(hnd, UriUtilities.UriSchemeFile);

		public static ArgumentUtilitiesHandle<Uri> EnsureHttpScheme(this ArgumentUtilitiesHandle<Uri> hnd) {
			if (hnd.Value?.IsAbsoluteUri == true && !(hnd.Value.Scheme.EqualsOrdinalCI(UriUtilities.UriSchemeHttp) || hnd.Value.Scheme.EqualsOrdinalCI(UriUtilities.UriSchemeHttps))) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "NotValidScheme/Expected", hnd.Value, UriUtilities.UriSchemeHttp)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<Uri> EnsureNoFragmentNoUserInfo(this ArgumentUtilitiesHandle<Uri> hnd) {
			if (hnd.Value?.IsAbsoluteUri == true && (!string.IsNullOrEmpty(hnd.Value.Fragment) || !string.IsNullOrEmpty(hnd.Value.UserInfo))) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "FragmentOrUserInfoNotAllowed", hnd.Value)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<Uri> EnsureLoopback(this ArgumentUtilitiesHandle<Uri> hnd) {
			if (hnd.Value?.IsAbsoluteUri == true && !hnd.Value.IsLoopback) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "NotLoopback", hnd.Value)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<Uri> EnsureLoopbackOrUnc(this ArgumentUtilitiesHandle<Uri> hnd) {
			if (hnd.Value?.IsAbsoluteUri == true && !(hnd.Value.IsLoopback || hnd.Value.IsUnc)) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "NorLoopbackNorUnc", hnd.Value)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<Uri> EnsurePortSpecified(this ArgumentUtilitiesHandle<Uri> hnd) {
			if (hnd.Value?.IsAbsoluteUri == true && IsNullOrEmpty(hnd.Value.GetComponents(components: UriComponents.Port, format: UriFormat.Unescaped))) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Uri), "PortNotSpecified", hnd.Value)}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<Uri> EnsureSchemeHostPortOnly(this ArgumentUtilitiesHandle<Uri> hnd)
			=> EnsureComponentsOnly(hnd: hnd, components: UriComponents.Scheme | UriComponents.Host | UriComponents.Port);

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<Uri> EnsureComponentsOnly(this ArgumentUtilitiesHandle<Uri> hnd, UriComponents components) {
			const UriComponents supportedComponents = UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.Port | UriComponents.Path | UriComponents.Query | UriComponents.Fragment;
			//
			if ((components | supportedComponents) != supportedComponents)
				throw
					new ArgumentOutOfRangeException(
						paramName: nameof(components),
						message: $"Указанные компоненты URI не поддерживаются.{Environment.NewLine}\tКомпоненты:{(components & ~supportedComponents).FmtStr().GNLI2()}");
			//
			if (!(hnd.Value is null) && hnd.Value.IsAbsoluteUri) {
				UriComponents specifiedComponents = default;
				string componentValue;
				var uri = hnd.Value;
				if (!IsNullOrEmpty(componentValue = uri.GetComponents(UriComponents.Scheme, UriFormat.Unescaped)))
					addAndValidateComponent(UriComponents.Scheme, componentValue, ref specifiedComponents);
				if (!IsNullOrEmpty(componentValue = uri.GetComponents(UriComponents.Host, UriFormat.Unescaped)))
					addAndValidateComponent(UriComponents.Host, componentValue, ref specifiedComponents);
				if (uri.Port > -1 && !uri.IsDefaultPort)
					addAndValidateComponent(UriComponents.Port, uri.Port.ToString("d", CultureInfo.InvariantCulture), ref specifiedComponents);
				if (!IsNullOrEmpty(componentValue = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped)))
					addAndValidateComponent(UriComponents.Path, componentValue, ref specifiedComponents);
				if (!IsNullOrEmpty(componentValue = uri.GetComponents(UriComponents.Query, UriFormat.Unescaped)))
					addAndValidateComponent(UriComponents.Query, componentValue, ref specifiedComponents);
				if (!IsNullOrEmpty(componentValue = uri.GetComponents(UriComponents.UserInfo, UriFormat.Unescaped)))
					addAndValidateComponent(UriComponents.UserInfo, componentValue, ref specifiedComponents);
				if (!IsNullOrEmpty(componentValue = uri.GetComponents(UriComponents.Fragment, UriFormat.Unescaped)))
					addAndValidateComponent(UriComponents.Fragment, componentValue, ref specifiedComponents);
				//
				var missingComponents = components ^ specifiedComponents;
				if (missingComponents != default) {
					var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}В URI отсутствуют требуемые компоненты.{Environment.NewLine}\tURI:{hnd.Value.FmtStr().GNLI2()}{Environment.NewLine}\tТребуемые компоненты:{getSupportedComponentsDisplayName(missingComponents).FmtStr().GNLI2()}";
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
						?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
				}
			}
			return hnd;
			//
			void addAndValidateComponent(UriComponents locComponent, string locComponentValue, ref UriComponents locAllComponents) {
				if ((components & locComponent) != locComponent) {
					var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}URI содержит недопустимый в данном контексте компонент.{Environment.NewLine}\tURI:{hnd.Value.FmtStr().GNLI2()}{Environment.NewLine}\tНедопустимый компонент:{($"{getSupportedComponentsDisplayName(locComponent)}:{locComponentValue.FmtStr().GNLI()}").FmtStr().GNLI2()}";
					throw
						hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
						?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
				}
				else
					locAllComponents |= locComponent;
			}
			string getSupportedComponentsDisplayName(UriComponents locComponents) {
				var locResult = string.Empty;
				if ((locComponents & UriComponents.Scheme) == UriComponents.Scheme)
					locResult = "Схема";
				if ((locComponents & UriComponents.UserInfo) == UriComponents.UserInfo)
					locResult = StringUtilities.ConcatNotEmptyStrings(", ", locResult, "Сведения о пользователе");
				if ((locComponents & UriComponents.Host) == UriComponents.Host)
					locResult = StringUtilities.ConcatNotEmptyStrings(", ", locResult, "Имя хоста");
				if ((locComponents & UriComponents.Port) == UriComponents.Port)
					locResult = StringUtilities.ConcatNotEmptyStrings(", ", locResult, "Номер порта");
				if ((locComponents & UriComponents.Path) == UriComponents.Path)
					locResult = StringUtilities.ConcatNotEmptyStrings(", ", locResult, "Путь");
				if ((locComponents & UriComponents.Query) == UriComponents.Query)
					locResult = StringUtilities.ConcatNotEmptyStrings(", ", locResult, "Запрос");
				if ((locComponents & UriComponents.Fragment) == UriComponents.Fragment)
					locResult = StringUtilities.ConcatNotEmptyStrings(", ", locResult, "Фрагмент");
				return locResult;
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<Uri> EnsureFixedSegmentCount(this ArgumentUtilitiesHandle<Uri> hnd, int count) {
			count.Arg(nameof(count)).EnsureNotLessThan(1);
			//
			if (!(hnd.Value is null) && hnd.Value.IsAbsoluteUri && hnd.Value.Segments.Length != count) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}В указанном URI количество сегментов не соответствует требуемому.{Environment.NewLine}\tURI:{hnd.Value.FmtStr().GNLI2()}{Environment.NewLine}\tТребуемое кол-во:{count.ToString("d").FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			return hnd;
		}

	}

}