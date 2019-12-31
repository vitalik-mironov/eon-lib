using System;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	public class UriContext
		:IUriContext {

		readonly Uri _baseUri;

		readonly Uri _relativeUri;

		readonly Uri _absoluteUri;

		string _absolutePathUnescaped;

		public UriContext(Uri baseUri, Uri relativeUri) {
			baseUri
				.EnsureNotNull(nameof(baseUri))
				.EnsureAbsolute()
				.EnsureNoFragmentNoUserInfo();
			relativeUri
				.EnsureNotNull(nameof(relativeUri))
				.EnsureRelative();
			//
			_baseUri = baseUri;
			_relativeUri = relativeUri;
			_absoluteUri = new Uri(baseUri: baseUri, relativeUri: relativeUri);
		}

		public UriContext(Uri absoluteUri) {
			absoluteUri
				.EnsureNotNull(nameof(absoluteUri))
				.EnsureAbsolute();
			//
			_baseUri = new Uri(uriString: absoluteUri.GetComponents(components: UriComponents.SchemeAndServer | UriComponents.KeepDelimiter, format: UriFormat.UriEscaped), uriKind: UriKind.Absolute);
			_relativeUri = _baseUri.MakeRelativeUri(uri: absoluteUri);
			_absoluteUri = absoluteUri;
		}

		public Uri BaseUri
			=> _baseUri;

		public Uri RelativeUri
			=> _relativeUri;

		public Uri AbsoluteUri
			=> _absoluteUri;

		public string AbsolutePathUnescaped {
			get {
				var result = itrlck.Get(location: ref _absolutePathUnescaped);
				if (result is null)
					itrlck
						.UpdateIfNullBool(
							location: ref _absolutePathUnescaped,
							value: _absoluteUri.GetComponents(components: UriComponents.Path, format: UriFormat.Unescaped),
							current: out result);
				return result;
			}
		}

		public override string ToString()
			=> _absoluteUri.ToString();

	}

}