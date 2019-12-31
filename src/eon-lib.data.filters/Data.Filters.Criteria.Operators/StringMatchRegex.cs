using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using Eon.Globalization;
using Eon.Reflection;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

using regex = System.Text.RegularExpressions;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class StringMatchRegex
		:StringCriterionOperatorBase, IStringRegexCriterionOperator {

		#region Static & constant members

		/// <summary>
		/// Значение: <see cref="RegexOptions.None"/>.
		/// </summary>
		public static readonly RegexOptions DefaultRegexOptions = regex.RegexOptions.None;

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(StringMatchRegex)).GetLocalizableText("DisplayText");

		#endregion

		RegexOptions? _regexOptions;
		[DataMember(Order = 0, IsRequired = true, Name = nameof(RegexOptions))]
		string P_RegexOptions_DataMember {
			get {
				var options = _regexOptions;
				if (options.HasValue)
					return options.Value.ToString();
				else
					return null;
			}
			set {
				try {
					_regexOptions = EnumUtilities.Parse<RegexOptions>(value);
				}
				catch (Exception firstException) {
					throw new MemberDeserializationException(GetType(), nameof(RegexOptions), firstException);
				}
			}
		}

		public StringMatchRegex()
			: this(regexOptions: default, isReadOnly: false) { }

		public StringMatchRegex(RegexOptions? regexOptions, bool isReadOnly = false)
			: this(comparison: default, regexOptions: regexOptions, isReadOnly: isReadOnly) { }

		public StringMatchRegex(StringComparison? comparison, RegexOptions? regexOptions, bool isReadOnly = false)
			: base(comparison: comparison, isReadOnly: isReadOnly) {
			_regexOptions = regexOptions;
		}

		[JsonConstructor]
		protected StringMatchRegex(SerializationContext context)
			: base(ctx: context) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		public RegexOptions? RegexOptions {
			get => _regexOptions;
			set {
				EnsureNotReadOnly();
				_regexOptions = value;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			base.OnValidate();
			//
			var comparison = Comparison;
			var regexOptions = RegexOptions;
			if (comparison.HasValue) {
				if (!regexOptions.HasValue)
					throw
						new EonException(message: $"Не задано значение свойства '{nameof(RegexOptions)}'. Значение данного свойства обязательно должно быть установлено, когда устанавлено значение для свойства '{nameof(Comparison)}'.");
				var isCultureInvarianRegexFlag = (regexOptions.Value & regex.RegexOptions.CultureInvariant) == regex.RegexOptions.CultureInvariant;
				var isIgnoreCaseRegexFlag = (regexOptions.Value & regex.RegexOptions.IgnoreCase) == regex.RegexOptions.IgnoreCase;
				var isCultureInvariantRegexFlagExpected = comparison.Value == StringComparison.Ordinal || comparison.Value == StringComparison.OrdinalIgnoreCase;
				var isIgnoreCaseRegexFlagExpected = comparison.Value == StringComparison.OrdinalIgnoreCase || comparison.Value == StringComparison.CurrentCultureIgnoreCase;
				if (isCultureInvariantRegexFlagExpected != isCultureInvarianRegexFlag || isIgnoreCaseRegexFlagExpected != isIgnoreCaseRegexFlag)
					throw
						new EonException(message: $"Значение '{comparison.Value.ToString()}' свойства '{nameof(Comparison)}' не согласуется со значением '{regexOptions.Value.ToString()}' свойства '{nameof(RegexOptions)}'.");
			}
		}

		public override BinaryExpression BuildExpression(Expression left, Expression right)
			=> Expression.Equal(BuildRegexMatchTestExpression(left, right, RegexOptions ?? DefaultRegexOptions), Expression.Constant(true));

		protected virtual Expression BuildRegexMatchTestExpression(Expression left, Expression right, RegexOptions regexOptions)
			=>
			Expression
			.Call(
				arg0: left.EnsureNotNull(nameof(left)).Value,
				arg1: right.EnsureNotNull(nameof(right)).Value,
				arg2: Expression.Constant(value: regexOptions, type: typeof(RegexOptions)),
				method: typeof(Regex).GetMethod(nameof(Regex.IsMatch), new Type[ ] { typeof(string), typeof(string), typeof(RegexOptions) }));

		protected sealed override void CreateReadOnlyCopy(out StringCriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out StringMatchRegex readOnlyCopy)
			=> readOnlyCopy = new StringMatchRegex(comparison: Comparison, regexOptions: RegexOptions, isReadOnly: true);

		public new StringMatchRegex AsReadOnly()
			=> (StringMatchRegex)base.AsReadOnly();

		IStringRegexCriterionOperator IStringRegexCriterionOperator.AsReadOnly()
			=> AsReadOnly();

	}

}