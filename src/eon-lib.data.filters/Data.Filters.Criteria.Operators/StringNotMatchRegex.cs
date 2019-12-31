using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Eon.Globalization;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class StringNotMatchRegex
		:StringMatchRegex {

		#region Static members

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(StringNotMatchRegex)).GetLocalizableText("DisplayText");

		#endregion

		public StringNotMatchRegex()
			: this(regexOptions: default, isReadOnly: false) { }

		public StringNotMatchRegex(RegexOptions? regexOptions, bool isReadOnly = false)
			: this(comparison: default, regexOptions: regexOptions, isReadOnly: isReadOnly) { }

		public StringNotMatchRegex(StringComparison? comparison, RegexOptions? regexOptions, bool isReadOnly = false)
			: base(comparison: comparison, regexOptions: regexOptions, isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected StringNotMatchRegex(SerializationContext ctx)
			: base(context: ctx) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		public override BinaryExpression BuildExpression(Expression left, Expression right)
			=> Expression.Equal(BuildRegexMatchTestExpression(left, right, RegexOptions ?? DefaultRegexOptions), Expression.Constant(false));

		protected sealed override void CreateReadOnlyCopy(out StringMatchRegex readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out StringNotMatchRegex readOnlyCopy)
			=> readOnlyCopy = new StringNotMatchRegex(comparison: Comparison, regexOptions: RegexOptions, isReadOnly: true);

		public new StringNotMatchRegex AsReadOnly()
			=> (StringNotMatchRegex)base.AsReadOnly();

	}

}