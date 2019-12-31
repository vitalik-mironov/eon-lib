using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

using Newtonsoft.Json;

using Eon.Runtime.Serialization;
using Eon.Threading;

namespace Eon.Text.RegularExpressions {

	[DataContract]
	public class RegexConstructParameters
		:AsReadOnlyValidatableBase {

		[DataMember(Order = 0, Name = nameof(Pattern), IsRequired = true)]
		string _pattern;

		RegexOptions _options;
		[DataMember(Order = 1, Name = nameof(Options), IsRequired = true)]
		string P_Options_DataMember {
			get => _options.ToString();
			set {
				if (value is null)
					_options = default;
				else {
					try {
						_options = value.Arg(nameof(value)).ParseEnum<RegexOptions>(ignoreCase: false).Value;
					}
					catch (Exception exception) {
						throw new MemberDeserializationException(memberName: nameof(Options), type: GetType(), innerException: exception);
					}
				}
			}
		}

		Regex _regex;

		public RegexConstructParameters()
			: this(pattern: null, isReadOnly: false) { }

		public RegexConstructParameters(string pattern = default, RegexOptions options = default, bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			//
			_pattern = pattern;
			_options = options;
			_regex = null;
		}

		public RegexConstructParameters(RegexConstructParameters other, bool isReadOnly = false)
			: this(pattern: other.EnsureNotNull(nameof(other)).Value._pattern, options: other._options, isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected RegexConstructParameters(SerializationContext context)
			: base(ctx: context) { }

		public string Pattern {
			get => _pattern;
			set {
				EnsureNotReadOnly();
				_pattern = value;
			}
		}

		public RegexOptions Options {
			get => _options;
			set {
				EnsureNotReadOnly();
				_options = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out RegexConstructParameters copy)
			=> copy = new RegexConstructParameters(other: this, isReadOnly: true);

		public new RegexConstructParameters AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var copy);
				return copy;
			}
		}

		public Regex GetRegex() {
			var regex = VolatileUtilities.Read(ref _regex);
			if (regex is null) {
				var pattern = Pattern.ArgProp(nameof(Pattern));
				var options = Options.ArgProp(nameof(Options));
				//
				ParseRegex(pattern: pattern, options: options, regex: out regex);
				if (IsReadOnly)
					Interlocked.Exchange(location1: ref _regex, value: regex);
			}
			return regex;
		}

		protected override void OnValidate()
			=> GetRegex();

		protected virtual void ParseRegex(ArgumentUtilitiesHandle<string> pattern, ArgumentUtilitiesHandle<RegexOptions> options, out Regex regex)
			=> regex = pattern.EnsureNotNull().ParseRegex(regexOptions: options.Value).Value;

	}

}