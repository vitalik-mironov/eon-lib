using Eon.Data.Storage.PgsqlClient;
using Eon.Data.Storage.SqlClient;

namespace Eon.Data.EfCore {

	public static class EfCoreUtilities {

		public static readonly IDbProviderInfoProps PgsqlDbProvider =
			new DbProviderInfoProps(providerInvarianName: PgsqlClientProviderName.InvariantName, defaultSchemaName: "public", providerManifestToken: string.Empty, isReadOnly: true)
			.ArgProp($"{nameof(EfCoreUtilities)}.{nameof(EfCoreUtilities.PgsqlDbProvider)}")
			.AsReadOnly()
			.EnsureValid()
			.Value;

		public static readonly IDbProviderInfoProps SqlDbProvider =
			new DbProviderInfoProps(providerInvarianName: SqlClientProviderName.InvariantName, defaultSchemaName: "dbo", providerManifestToken: string.Empty, isReadOnly: true)
			.ArgProp($"{nameof(EfCoreUtilities)}.{nameof(EfCoreUtilities.SqlDbProvider)}")
			.AsReadOnly()
			.EnsureValid()
			.Value;

		public static bool IsSqlProvider(this IDbProviderInfoProps provider) {
			provider.EnsureNotNull(nameof(provider));
			//
			return provider.ProviderInvariantName.EqualsOrdinalCS(otherString: SqlClientProviderName.InvariantName);
		}

		public static bool IsPgsqlProvider(this IDbProviderInfoProps provider) {
			provider.EnsureNotNull(nameof(provider));
			//
			return provider.ProviderInvariantName.EqualsOrdinalCS(otherString: PgsqlClientProviderName.InvariantName);
		}

		#region Suspended.

		///// <summary>
		///// Регистрирует сопоставления функций и хранимых процедур хранилища данных к публичным статическим/экземплярным методам типов иерархии указанного типа контекста данных.
		///// </summary>
		///// <param name="builder">Построитель модели хранилища данных. Используется для сопоставления объектов хранилища данных объектам CLR.</param>
		///// <param name="contextType">Тип контекста данных.</param>
		///// <param name="storeDefaultSchema">Схема доступа по умолчанию к функциям/процедурам в хранилище данных.</param>
		//public static void RegisterStoreFunctions(this DbModelBuilder builder, Type contextType, string storeDefaultSchema) {
		//	builder.EnsureNotNull(nameof(builder));
		//	contextType
		//		.Argument(nameof(contextType))
		//		.EnsureNotNull()
		//		.EnsureClass();
		//	//
		//	var dbFunctionAttributeType = typeof(DbFunctionAttribute);
		//	foreach (var type in (from type in contextType.GetClassHierarchyTo(typeof(object))
		//												where type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly).Any(method => method.IsDefined(dbFunctionAttributeType))
		//												select type))
		//		builder.Conventions.Add(new FunctionsConvention(storeDefaultSchema, type));
		//}

		#endregion

	}

}