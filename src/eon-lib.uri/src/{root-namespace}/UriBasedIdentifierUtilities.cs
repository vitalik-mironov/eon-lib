namespace Eon {

	public static class UriBasedIdentifierUtilities {

		public static UriBasedIdentifier AsChanged(this UriBasedIdentifier original, ArgumentUtilitiesHandle<string> newUriString) {
			if (newUriString.Value == null)
				return null;
			else if (ReferenceEquals(original, null) || !original.StringValue.EqualsOrdinalCS(newUriString.Value))
				return new UriBasedIdentifier(uriString: newUriString);
			else
				return original;
		}

		public static UriBasedIdentifier AsChanged(this UriBasedIdentifier original, string newUriString)
			=> AsChanged(original: original, newUriString: newUriString.Arg(nameof(newUriString)));

		public static bool IsNullOrUndefined(this UriBasedIdentifier value)
			=> ReferenceEquals(value, null) || value == UriBasedIdentifier.Undefined;

		public static UriBasedIdentifier SubstituteNullOrUndefined(this UriBasedIdentifier value, UriBasedIdentifier substitution)
			=> value.IsNullOrUndefined() ? substitution : value;

	}

}