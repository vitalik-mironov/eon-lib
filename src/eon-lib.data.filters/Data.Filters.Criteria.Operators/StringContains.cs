using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Eon.Globalization;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class StringContains
		:StringCriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(StringContains)).GetLocalizableText("UI/DisplayText");

		public StringContains() { }

		public StringContains(StringComparison? comparison, bool isReadOnly = false)
			: base(comparison: comparison, isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected StringContains(SerializationContext ctx)
			: base(ctx: ctx) { }

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
#if TRG_NETSTANDARD1_5
							Expression.Call(left, typeof(string).GetTypeInfo().GetMethod(nameof(string.Contains)), right)),
#endif
						Expression.Constant(true));
			else
				throw new NotSupportedException($"Для данного оператора режим сравнения строк '{comparison}' не поддерживается.{Environment.NewLine}\tОператор:{this.FmtStr().GNLI2()}");
		}

		protected sealed override void CreateReadOnlyCopy(out StringCriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out StringContains readOnlyCopy)
			=> readOnlyCopy = new StringContains(comparison: Comparison, isReadOnly: true);

	}

}