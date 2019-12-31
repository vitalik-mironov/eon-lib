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
	public class StringEquals
		:StringCriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(Equals)).GetLocalizableText("UI/DisplayText");

		public StringEquals()
			: base(comparison: default, isReadOnly: false) { }

		[JsonConstructor]
		protected StringEquals(SerializationContext ctx)
			: base(ctx: ctx) { }

		public StringEquals(StringComparison? comparison, bool isReadOnly = false)
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
					Expression.Constant(true, typeof(bool)));
		}

		protected sealed override void CreateReadOnlyCopy(out StringCriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out StringEquals readOnlyCopy)
			=> readOnlyCopy = new StringEquals(comparison: Comparison, isReadOnly: true);

	}

}