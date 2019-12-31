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
	public class StringEndsWith
		:StringCriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(StringEndsWith)).GetLocalizableText("UI/DisplayText");

		public StringEndsWith() { }

		[JsonConstructor]
		protected StringEndsWith(SerializationContext ctx)
			: base(ctx: ctx) { }

		public StringEndsWith(StringComparison? comparison, bool isReadOnly = false)
			: base(comparison: comparison, isReadOnly: isReadOnly) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		public override BinaryExpression BuildExpression(Expression left, Expression right) {
			left.EnsureNotNull(nameof(left));
			right.EnsureNotNull(nameof(right));
			//
			var comparison = Comparison ?? StringComparison.CurrentCulture;
			var conditionalExpression =
				Expression.AndAlso(
					Expression.NotEqual(left, Expression.Constant(null, typeof(string))),
					comparison == StringComparison.CurrentCulture
						? Expression.Call(left, typeof(string).GetMethod(nameof(string.EndsWith), new Type[ ] { typeof(string) }), right)
						: Expression.Call(left, typeof(string).GetMethod(nameof(string.EndsWith), new Type[ ] { typeof(string), typeof(StringComparison) }), right, Expression.Constant(comparison)));
			return 
				Expression.Equal(
					conditionalExpression, 
					Expression.Constant(true, typeof(bool)));
		}

		protected sealed override void CreateReadOnlyCopy(out StringCriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out StringEndsWith readOnlyCopy)
			=> readOnlyCopy = new StringEndsWith(comparison: Comparison, isReadOnly: true);

	}

}