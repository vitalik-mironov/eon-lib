using System;
using System.Linq.Expressions;
using System.Reflection;

using Eon.Reflection;

namespace Eon.Linq.Expressions {

	public static class ExpressionUtilities {

		public static string FullNameOfNonStaticProperty<T>(Expression<Func<T>> propExpr) {
			var propInfo = ExtractNonStaticProperty(propExpr: propExpr);
			return $"{propInfo.DeclaringType.FullName}.{propInfo.Name}";
		}

		// TODO: Put exception messages into the resources.
		//
		public static PropertyInfo ExtractNonStaticProperty(LambdaExpression propExpr, string altPropExprArgName = default) {
			var prop = ExtractMember(propExpr, altExprArgName: string.IsNullOrEmpty(altPropExprArgName) ? nameof(propExpr) : altPropExprArgName) as PropertyInfo;
			if (prop is null) {
				throw
					new ArgumentException(
						message: $"Specified member access expression does not access a property.{Environment.NewLine}\tExpression:{propExpr.FmtStr().GNLI2()}",
						paramName: string.IsNullOrEmpty(altPropExprArgName) ? nameof(propExpr) : altPropExprArgName);
			}
			else {
				prop.Arg(string.IsNullOrEmpty(altPropExprArgName) ? nameof(propExpr) : altPropExprArgName).EnsureNonStatic();
				return prop;
			}
		}

		// TODO: Put strings into the resources.
		//
		public static MemberInfo ExtractMember(LambdaExpression expr, string altExprArgName = default) {
			var argName = string.IsNullOrEmpty(altExprArgName) ? nameof(expr) : altExprArgName.FmtStr().G();
			expr.EnsureNotNull(argName);
			//
			var exprBody = expr.Body;
			if (exprBody is MemberExpression bodyAsMemberExpr)
				return bodyAsMemberExpr.Member;
			else if (exprBody is MethodCallExpression bodyAsMethodCallExpr)
				return bodyAsMethodCallExpr.Method;
			else
				throw new ArgumentException(message: $"Specified expression does not access a member and doesn't access a method.{Environment.NewLine}\tExpression:{expr.FmtStr().GNLI2()}", paramName: argName);
		}

		public static MemberInfo ExtractMember<T>(Expression<Func<T>> expr)
			=> ExtractMember(expr: (LambdaExpression)expr);

		public static bool IsPropertyReadExpression<TInstance, TValue>(Expression<Func<TInstance>> instance, Expression<Func<TValue>> prop, string altInstanceArgName = default, string altPropArgName = default)
			=> IsPropertyReadExpression(instance: (LambdaExpression)instance, prop: prop, altInstanceArgName: altInstanceArgName, altPropArgName: altPropArgName);

		public static bool IsPropertyReadExpression(LambdaExpression instance, LambdaExpression prop, string altInstanceArgName = default, string altPropArgName = default) {
			instance.EnsureNotNull(nameof(instance));
			prop.EnsureNotNull(string.IsNullOrEmpty(altPropArgName) ? nameof(prop) : altPropArgName);
			//
			if (prop.Body is MemberExpression memberExpr) {
				var propertyMember = memberExpr.Member as PropertyInfo;
				if (propertyMember == null || !propertyMember.IsNonStaticRead())
					return false;
				Type propertyAccessInstanceType;
				if (memberExpr.Expression is ConstantExpression accessConstantExpr)
					propertyAccessInstanceType = accessConstantExpr.Type;
				else if (memberExpr.Expression is MemberExpression accessMemberExpr) {
					var accessMemberField = accessMemberExpr.Member as FieldInfo;
					if (accessMemberField == null)
						return false;
					propertyAccessInstanceType = accessMemberField.FieldType;
				}
				else if (memberExpr.Expression is ParameterExpression accessParameterExpr)
					propertyAccessInstanceType = accessParameterExpr.Type;
				else
					return false;
				if (instance.ReturnType.IsAssignableFrom(propertyAccessInstanceType) && propertyMember.DeclaringType.IsAssignableFrom(instance.ReturnType))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Определяет имеет ли указанный тип нестатическое свойство, определенное <paramref name="propExpr"/>.
		/// </summary>
		/// <param name="type">Тип, принадлежность свойства которому выполняется проверка.</param>
		/// <param name="propExpr">Выражение, задающее свойство.</param>
		/// <param name="altTypeArgName">Алтернативное имя аргумента <paramref name="type"/>.</param>
		/// <param name="altPropExprArgName">Альтернативное имя аргумента <paramref name="propExpr"/></param>
		/// <returns>True — указанный тип имеет заданное нестатическое свойство; False — иначе.</returns>
		public static bool IsDefinedProperty(this Type type, LambdaExpression propExpr, string altTypeArgName = default, string altPropExprArgName = default)
			=> P_IsDefinedProperty(type: type, propExpr: propExpr, extractedPropInfo: out _, altTypeArgName: altTypeArgName, altPropExprArgName: altPropExprArgName);

		static bool P_IsDefinedProperty(this Type type, LambdaExpression propExpr, out PropertyInfo extractedPropInfo, string altTypeArgName = default, string altPropExprArgName = default) {
			if (type is null)
				throw new ArgumentNullException(nameof(type));
			else if (propExpr == null)
				throw new ArgumentNullException(nameof(propExpr));
			var propertyInfoLoc = ExtractNonStaticProperty(propExpr);
			extractedPropInfo = propertyInfoLoc;
			return propertyInfoLoc.DeclaringType.IsAssignableFrom(type);
		}

		/// <summary>
		/// Проверяет имеет ли указанный тип нестатическое свойство, определенное <paramref name="propExpr"/>.
		/// <para>Если указанный тип не имеет указанного свойства, тогда генерируется исключение <see cref="ArgumentException"/>.</para>
		/// </summary>
		/// <param name="type">Тип, принадлежность свойства которому выполняется проверка.</param>
		/// <param name="propExpr">Выражение, задающее свойство.</param>
		/// <param name="altTypeArgName">Алтернативное имя аргумента <paramref name="type"/>.</param>
		/// <param name="altPropExprArgName">Альтернативное имя аргумента <paramref name="propExpr"/></param>
		public static void EnsureDefinedProperty(this Type type, LambdaExpression propExpr, string altTypeArgName = default, string altPropExprArgName = default)
			=> P_EnsureDefinedProperty(type, propExpr, out _, altTypeArgName: altTypeArgName, altPropExprArgName: altPropExprArgName);

		static void P_EnsureDefinedProperty(Type type, LambdaExpression propExpr, out PropertyInfo extractedPropInfo, string altTypeArgName = null, string altPropExprArgName = null) {
			if (!P_IsDefinedProperty(type, propExpr, out extractedPropInfo, altTypeArgName: altTypeArgName, altPropExprArgName: altPropExprArgName))
				throw new ArgumentException(message: $"Specified type doesn't have a property specified by expression.{Environment.NewLine}\tType:{type.FmtStr().GNLI2()}{Environment.NewLine}\tExpression:{propExpr.FmtStr().GNLI2()}", paramName: string.IsNullOrEmpty(altPropExprArgName) ? "propertyExpression" : altPropExprArgName);
		}

		/// <summary>
		/// Возвращает имя нестатического свойства, определенного выражением <paramref name="propExpr"/>.
		/// </summary>
		/// <typeparam name="TInstance">Тип экземпляря, для которого определено заданное свойство.</typeparam>
		/// <typeparam name="TProperty">Тип значения свойства <paramref name="propExpr"/>.</typeparam>
		/// <param name="propExpr">Выражение свойства.</param>
		/// <returns>Имя заданного свойства.</returns>
		public static string NameOfProperty<TInstance, TProperty>(Expression<Func<TInstance, TProperty>> propExpr) {
			P_EnsureDefinedProperty(typeof(TInstance), propExpr, out var propertyInfo);
			return propertyInfo.Name;
		}

		/// <summary>
		/// Возвращает имя нестатического свойства, определенного выражением <paramref name="propertyExpr"/>.
		/// </summary>
		/// <typeparam name="TProperty">Тип значения свойства <paramref name="propertyExpr"/>.</typeparam>
		/// <param name="propertyExpr">Выражение свойства.</param>
		/// <returns>Имя заданного свойства.</returns>
		public static string NameOfProperty<TProperty>(Expression<Func<TProperty>> propertyExpr)
			=> ExtractNonStaticProperty(propertyExpr).Name;

		// TODO: Put strings into the resources.
		//
		public static void EnsurePropertyReadExpression(LambdaExpression instance, LambdaExpression prop, string altInstanceArgName = default, string altPropArgName = default) {
			if (!IsPropertyReadExpression(instance: instance, prop: prop, altInstanceArgName: altInstanceArgName, altPropArgName: altPropArgName))
				throw
					new ArgumentException(
						message: $"Specified expression is not an expression reading a property of the specified type.{Environment.NewLine}\tExpression:{prop.FmtStr().GNLI2()}{Environment.NewLine}\tType:{instance.ReturnType.FmtStr().GNLI2()}",
						paramName: string.IsNullOrEmpty(altPropArgName) ? nameof(prop) : altPropArgName);
		}

		public static void EnsurePropertyReadExpression<TInstance, TValue>(Expression<Func<TInstance>> instance, Expression<Func<TValue>> prop, string altInstanceArgName = default, string altPropArgName = default)
			=> EnsurePropertyReadExpression(instance: (LambdaExpression)instance, prop: prop, altInstanceArgName: altInstanceArgName, altPropArgName: altPropArgName);

		public static void EnsurePropertyReadExpression<TInstance, TValue>(Expression<Func<TInstance, TValue>> expr, string altExprArgName = default) {
			var instance = default(TInstance);
			EnsurePropertyReadExpression(instance: () => instance, prop: expr, altPropArgName: altExprArgName);
		}

		public static void EnsurePropertyReadExpression<TInstance, TValue>(Expression<Func<TInstance>> instance, Expression<Func<TInstance, TValue>> prop, string altInstanceArgName = null, string altPropArgName = null)
			=> EnsurePropertyReadExpression(instance: (LambdaExpression)instance, prop: prop, altInstanceArgName: altInstanceArgName, altPropArgName: altPropArgName);

		public static string SignatureOfMethod<TArg0, TResult>(Expression<Func<TArg0, TResult>> methodExpr)
			=> MemberText.GetMethodSignatureText(expr: methodExpr);

		public static string NameOf<T>(Expression<Func<T>> expr, string altExprArgName = default) {
			expr.EnsureNotNull(nameof(expr));
			//
			return ExtractMember(expr: expr, altExprArgName: string.IsNullOrEmpty(altExprArgName) ? nameof(expr) : altExprArgName).Name;
		}



	}

}