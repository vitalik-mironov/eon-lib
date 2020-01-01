using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static class ReadOnlyScopeUtilities {

		public static void EnsureNotReadOnly(this IReadOnlyScope scope) {
			scope.EnsureNotNull(nameof(scope));
			//
			if (scope.IsReadOnly)
				throw new EonException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", scope));
		}

		// TODO: Put strings into the resources.
		//
		public static void EnsurePermanentReadOnly(this IReadOnlyScope scope) {
			scope.EnsureNotNull(nameof(scope));
			//
			if (!scope.IsPermanentReadOnly)
				throw
					new EonException(message: $"Область (компонент) не имеет устойчивой недоступности редактирования (т.е. состояние доступности редактирования этой области (компонента) потенциально может быть изменено).{Environment.NewLine}\tОбласть (компонент):{Environment.NewLine}{scope.FmtStr().GI2()}");
		}

		// TODO: Put strings into the resources.
		//
		public static void EnsureChangeToPermanentReadOnly(this IReadOnlyScope scope, ReadOnlyStateTag newState) {
			scope.EnsureNotNull(nameof(scope));
			newState.EnsureNotNull(nameof(newState));
			//
			if (newState.IsReadOnly && !newState.IsPermanent)
				throw new EonException(message: $"Изменение состояния доступности редактирования данного объекта согласно указанным параметрам не может быть выполнено. Возможно, объект не поддерживает такое изменение состояния доступности редактирования.{Environment.NewLine}\tОбъект:{scope.FmtStr().GNLI2()}{Environment.NewLine}\tПараметры:{newState.FmtStr().GNLI2()}");
		}

		public static T AsReadOnly<T>(this T scope, bool isPermanentReadOnly)
			where T : IEonCloneable, IReadOnlyScope {
			//
			scope.EnsureNotNull(nameof(scope));
			//
			var scopeClone = scope.CloneAs();
			scopeClone.SetReadOnly(isReadOnly: true, isPermanent: isPermanentReadOnly);
			return scopeClone;
		}

	}

}