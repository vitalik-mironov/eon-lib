using System;
using System.Diagnostics;
using System.Runtime.Serialization;

using Eon.Runtime.Options;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Diagnostics {

	/// <summary>
	/// Defines base specs of an error code.
	/// </summary>
	[DataContract]
	[DebuggerDisplay("{ToString(),nq}")]
	public sealed class ErrorCode
		:IErrorCode {

		#region Static & constant members

		/// <summary>
		/// Value: '64'.
		/// </summary>
		public static readonly int IdentifierMaxLength;

		/// <summary>
		/// Value: '384'.
		/// </summary>
		public static readonly int DescriptionMaxLength;

		/// <summary>
		/// Value: <see cref="StringComparer.OrdinalIgnoreCase"/>.
		/// </summary>
		public static readonly StringComparer IdentifierComparer;

		/// <summary>
		/// Value: <see cref="StringComparison.OrdinalIgnoreCase"/>.
		/// </summary>
		public static readonly StringComparison IdentifierComparison;

		static ErrorCode() {
			IdentifierMaxLength = 64;
			DescriptionMaxLength = 384;
			IdentifierComparer = StringComparer.OrdinalIgnoreCase;
			IdentifierComparison = StringComparison.OrdinalIgnoreCase;
		}

		/// <summary>
		/// Gets the default error code identifier prefix set by option <see cref="ErrorCodeIdentifierPrefixOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static string DefaultIdentifierPrefix
			=> ErrorCodeIdentifierPrefixOption.Require();

		public static bool IdentifierEquals(string a, string b)
			=> string.Equals(a: a, b: b, comparisonType: IdentifierComparison);

		#endregion

		[DataMember(Order = 0, Name = nameof(Identifier), IsRequired = true)]
		string _identifier;

		[DataMember(Order = 1, Name = nameof(SeverityLevel), IsRequired = false, EmitDefaultValue = false)]
		SeverityLevel? _severityLevel;

		[DataMember(Order = 2, Name = nameof(Description), IsRequired = false, EmitDefaultValue = false)]
		string _description;

		public ErrorCode(IErrorCode other, ArgumentPlaceholder<string> description = default, ArgumentPlaceholder<SeverityLevel?> severityLevel = default)
			: this(
					identifier: other.EnsureNotNull(nameof(other)).Value.Identifier,
					severityLevel: severityLevel.Substitute(value: other.SeverityLevel),
					description: description.Substitute(value: other.Description)) { }

		public ErrorCode(string identifier, SeverityLevel? severityLevel, string description = default) {
			identifier
				.Arg(nameof(identifier))
				.EnsureHasMaxLength(IdentifierMaxLength)
				.EnsureNotNullOrWhiteSpace();
			description
				.Arg(nameof(description))
				.EnsureHasMaxLength(DescriptionMaxLength);
			//
			_identifier = identifier;
			_severityLevel = severityLevel;
			_description = description;
		}

		public ErrorCode(string identifier, string description = default)
			: this(identifier, null, description: description) { }

		[JsonConstructor]
		ErrorCode() { }

		public string Identifier
			=> _identifier;

		public SeverityLevel? SeverityLevel
			=> _severityLevel;

		public string Description
			=> _description;

		[OnDeserialized]
		void P_OnDeserialized(StreamingContext context) {
			try {
				_identifier
					.Arg(nameof(_identifier))
					.EnsureHasMaxLength(IdentifierMaxLength)
					.EnsureNotNullOrWhiteSpace();
			}
			catch (Exception exception) {
				throw new MemberDeserializationException(GetType(), nameof(Identifier), exception);
			}
			try {
				_description.Arg(nameof(_description)).EnsureHasMaxLength(DescriptionMaxLength);
			}
			catch (Exception exception) {
				throw new MemberDeserializationException(GetType(), nameof(Description), exception);
			}
		}

		public ErrorCode ChangeSeverityLevel(SeverityLevel? severityLevel)
			=>
			SeverityLevel == severityLevel
			? this
			: new ErrorCode(
				other: this,
				severityLevel: severityLevel);

		IErrorCode IErrorCode.ChangeSeverityLevel(SeverityLevel? severityLevel)
			=> ChangeSeverityLevel(severityLevel: severityLevel);

		public ErrorCode ChangeDescription(string description)
			=>
			description.EqualsOrdinalCS(Description)
			? this
			: new ErrorCode(other: this, description: description);

		IErrorCode IErrorCode.ChangeDescription(string description)
			=> ChangeDescription(description: description);


		public override string ToString()
			=> _severityLevel.HasValue ? string.Format("[{0}, {1}: {2}]", _identifier, _severityLevel, _description) : string.Format("[{0}: {1}]", _identifier, _description);

	}

}