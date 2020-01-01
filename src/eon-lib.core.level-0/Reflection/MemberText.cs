using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Eon.Reflection {

	public static class MemberText {

		public static readonly string MethodSignatureParameterDelimiterText = ", ";

		public const char TypeNameDelimiter = EonTypeUtilities.TypeNameDelimiter;

		// TODO: Put strings into the resources.
		//
		static string P_GetMethodSignatureText(string name, Type declaringType = default, params Type[ ] parameterTypes) {
			if (name is null)
				throw new ArgumentNullException(paramName: nameof(name));
			else if (string.Empty == name)
				throw new ArgumentException(paramName: nameof(name), message: "Cannot be empty string.");
			//
			if (parameterTypes is null || parameterTypes.Length == 0)
				return $"{(declaringType is null ? string.Empty : (declaringType.FullName + TypeNameDelimiter))}{name}()";
			else {
				using (var buffer = EonStringBuilderUtilities.AcquireBuffer()) {
					var sb = buffer.StringBuilder;
					if (declaringType != null) {
						sb.Append(value: declaringType.FullName);
						sb.Append(value: TypeNameDelimiter);
					}
					sb.Append(name);
					sb.Append('(');
					for (var i = 0; i < parameterTypes.Length; i++) {
						if (i > 0)
							sb.Append(value: MethodSignatureParameterDelimiterText);
						sb.AppendFormat(CultureInfo.InvariantCulture, "{0} arg{1:d}", parameterTypes[ i ], i);
					}
					sb.Append(')');
					return sb.ToString();
				}
			}
		}

		public static string GetMethodSignatureText<TResult>(Func<TResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			var method = func.Method;
			return P_GetMethodSignatureText(name: method.Name, declaringType: method.DeclaringType, parameterTypes: method.GetParameters().Select(locItem => locItem.ParameterType).ToArray());
		}

		public static string GetMethodSignatureText<TArg, TResult>(Func<TArg, TResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			var method = func.Method;
			return P_GetMethodSignatureText(name: method.Name, declaringType: method.DeclaringType, parameterTypes: method.GetParameters().Select(locItem => locItem.ParameterType).ToArray());
		}

		public static string GetMethodSignatureText<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			var method = func.Method;
			return P_GetMethodSignatureText(name: method.Name, declaringType: method.DeclaringType, parameterTypes: method.GetParameters().Select(locItem => locItem.ParameterType).ToArray());
		}

		public static string GetMethodSignatureText<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			var method = func.Method;
			return P_GetMethodSignatureText(name: method.Name, declaringType: method.DeclaringType, parameterTypes: method.GetParameters().Select(locItem => locItem.ParameterType).ToArray());
		}

		public static string GetMethodSignatureText<TArg1, TArg2, TArg3, TArg4, TResult>(Func<TArg1, TArg2, TArg3, TArg4, TResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			var method = func.Method;
			return P_GetMethodSignatureText(name: method.Name, declaringType: method.DeclaringType, parameterTypes: method.GetParameters().Select(locItem => locItem.ParameterType).ToArray());
		}

		public static string GetMethodSignatureText(Type declaringType, string name, params Type[ ] parameterTypes) {
			declaringType.EnsureNotNull(nameof(declaringType));
			return P_GetMethodSignatureText(name: name, declaringType: declaringType, parameterTypes: parameterTypes);
		}

		public static string GetMethodSignatureText(string name, params Type[ ] parameterTypes)
			=> P_GetMethodSignatureText(name, parameterTypes: parameterTypes);

		// TODO: Put strings into the resources.
		//
		public static string GetMethodSignatureText<TArg1, TResult>(Expression<Func<TArg1, TResult>> expr) {
			if (expr is null)
				throw new ArgumentNullException(paramName: nameof(expr));
			else if (expr.Body is MethodCallExpression callExpression) {
				var method = callExpression.Method;
				return P_GetMethodSignatureText(name: method.Name, declaringType: method.DeclaringType, parameterTypes: method.GetParameters().Select(locItem => locItem.ParameterType).ToArray());
			}
			else
				throw new ArgumentException(paramName: nameof(expr), message: "Specified expression is not a method call expression.");
		}

		// TODO: Put strings into the resources.
		//
		public static string GetMethodSignatureText<TArg1, TArg2, TResult>(Expression<Func<TArg1, TArg2, TResult>> expression) {
			if (expression is null)
				throw new ArgumentNullException(paramName: nameof(expression));
			else if (expression.Body is MethodCallExpression callExpression) {
				var method = callExpression.Method;
				return
					P_GetMethodSignatureText(name: method.Name, declaringType: method.DeclaringType, parameterTypes: method.GetParameters().Select(locItem => locItem.ParameterType).ToArray());
			}
			else
				throw new ArgumentException(paramName: nameof(expression), message: "Specified expression is not a method call expression.");
		}

		public static string GetConstructorSignatureText(ConstructorInfo constructor)
			=>
			P_GetMethodSignatureText(
				name: (constructor ?? throw new ArgumentNullException(paramName: nameof(constructor))).IsStatic ? ConstructorInfo.TypeConstructorName : ConstructorInfo.ConstructorName,
				parameterTypes: constructor.GetParameters().Select(locItem => locItem.ParameterType).ToArray());

		public static string GetConstructorSignatureText(params Type[ ] constructorParameterTypes)
			=> P_GetMethodSignatureText(name: ConstructorInfo.ConstructorName, parameterTypes: constructorParameterTypes);

	}

}