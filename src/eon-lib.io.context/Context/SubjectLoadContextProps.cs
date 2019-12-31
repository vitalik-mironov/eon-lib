using System;

using Eon.Net.Mime;

namespace Eon.Context {

	public static class SubjectLoadContextProps {

		public static readonly ContextProperty<Uri> SubjectLoadBaseUriProp =
			new ContextProperty<Uri>(
				name: nameof(SubjectLoadBaseUri),
				fallbackValue: UriUtilities.UndefinedUri,
				validator: locValue => locValue.EnsureNotNull().EnsureAbsolute());

		public static readonly ContextProperty<Uri> SubjectLoadSiteOriginProp =
			new ContextProperty<Uri>(
				name: nameof(SubjectLoadSiteOrigin),
				fallbackValue: UriUtilities.UndefinedUri,
				validator: locValue => locValue.EnsureNotNull().EnsureAbsolute());

		public static readonly ContextProperty<string> SubjectLoadMediaTypeProp =
			new ContextProperty<string>(
				name: nameof(SubjectLoadMediaType),
				fallbackValue: MediaTypeNameUtilities.AppOctetStream,
				validator: locValue => locValue.EnsureNotNull().EnsureNotEmptyOrWhiteSpace().EnsureHasMaxLength(MediaTypeNameUtilities.MediaTypeNameDefaultMaxLength));

		public static Uri SubjectLoadBaseUri(this IContext ctx, bool disposeTolerant = default)
			=> SubjectLoadBaseUriProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant);

		public static Uri SubjectLoadSiteOrigin(this IContext ctx, bool disposeTolerant = default)
			=> SubjectLoadSiteOriginProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant);

		public static string SubjectLoadMediaType(this IContext ctx, bool disposeTolerant = default)
			=> SubjectLoadMediaTypeProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant);

	}

}