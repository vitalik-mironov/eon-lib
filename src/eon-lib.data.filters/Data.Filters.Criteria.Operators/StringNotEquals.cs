using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Eon.Globalization;
using Eon.Reflection;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class StringNotEquals
		:StringCriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(StringNotEquals)).GetLocalizableText("UI/DisplayText");

		public StringNotEquals()
			: base(comparison: default, isReadOnly: false) { }

		[JsonConstructor]
		protected StringNotEquals(SerializationContext ctx)
			: base(ctx: ctx) { }

		public StringNotEquals(StringComparison? comparison, bool isReadOnly = false)
			: base(comparison: comparison, isReadOnly: isReadOnly) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		public override BinaryExpression BuildExpression(Expression left, Expression right) {
			left.EnsureNotNull(nameof(left));
			right.EnsureNotNull(nameof(right));
			//
			var comparison = Comparison ?? StringComparison.Ordinal;
			return
				Expression.Equal(
					Expression.Call(typeof(string).GetMethod(nameof(string.Equals), new Type[ ] { typeof(string), typeof(string), typeof(StringComparison) }), left, right, Expression.Constant(comparison)),
					Expression.Constant(false, typeof(bool)));
		}

		protected sealed override void CreateReadOnlyCopy(out StringCriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out StringNotEquals readOnlyCopy)
			=> readOnlyCopy = new StringNotEquals(comparison: Comparison, isReadOnly: true);

	}

}