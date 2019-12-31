using System;
using System.Linq.Expressions;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TExpression> EnsureMethodCallBody<TExpression>(this ArgumentUtilitiesHandle<TExpression> hnd, out MethodCallExpression methodCall)
			where TExpression : LambdaExpression {
			var body = hnd.Value?.Body;
			if (body == null)
				methodCall = null;
			else {
				var locMethodCall = body as MethodCallExpression;
				if (locMethodCall == null)
					throw new ArgumentException($"Specified expression is not a method call expression.{Environment.NewLine}\tExpression:{hnd.Value.FmtStr().GNLI2()}.", hnd.Name);
				else
					methodCall = locMethodCall;
			}
			return hnd;
		}

		public static ArgumentUtilitiesHandle<TExpression> EnsureMethodCallBody<TExpression>(this ArgumentUtilitiesHandle<TExpression> hnd)
			where TExpression : LambdaExpression => EnsureMethodCallBody(hnd, out _);

	}

}