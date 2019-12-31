using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

using Eon.Globalization;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class StringNotContains
		:StringCriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(StringNotContains)).GetLocalizableText("UI/DisplayText");

		public StringNotContains() { }

		[JsonConstructor]
		protected StringNotContains(SerializationContext ctx)
			: base(ctx: ctx) { }

		public StringNotContains(StringComparison? comparison, bool isReadOnly = false)
			: base(comparison: comparison, isReadOnly: isReadOnly) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		// TODO: Put strings into the resources.
		//
		public override BinaryExpression BuildExpression(Expression left, Expression right) {
			left.EnsureNotNull(nameof(left));
			right.EnsureNotNull(nameof(right));
			//
			var comparison = Comparison ?? StringComparison.Ordinal;
			if (comparison == StringComparison.Ordinal)
				return
					Expression.Equal(
						Expression.AndAlso(
							Expression.NotEqual(left, Expression.Constant(null, typeof(string))),
							Expression.Call(left, typeof(string).GetTypeInfo().GetMethod(nameof(string.Contains)), right)),
						Expression.Constant(false));
			else
				throw new NotSupportedException($"Для данного оператора режим сравнения строк '{comparison}' не поддерживается.{Environment.NewLine}\tОператор:{this.FmtStr().GNLI2()}");
		}

		protected sealed override void CreateReadOnlyCopy(out StringCriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out StringNotContains readOnlyCopy)
			=> readOnlyCopy = new StringNotContains(comparison: Comparison, isReadOnly: true);

	}

}