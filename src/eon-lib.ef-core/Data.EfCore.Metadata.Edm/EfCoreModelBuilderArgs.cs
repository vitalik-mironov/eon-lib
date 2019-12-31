using System;

using Microsoft.EntityFrameworkCore;

namespace Eon.Data.EfCore.Metadata.Edm {

	public class EfCoreModelBuilderArgs {

		readonly ModelBuilder _builder;

		readonly Type _contextType;

		readonly IDbProviderInfoProps _storeProviderInfo;

		public EfCoreModelBuilderArgs(ModelBuilder builder, Type contextType, IDbProviderInfoProps storeProviderInfo) {
			builder.EnsureNotNull(nameof(builder));
			contextType
				.EnsureNotNull(nameof(contextType))
				.EnsureCompatible(type: typeof(DbContext));
			storeProviderInfo =
				storeProviderInfo
				.EnsureNotNull(nameof(storeProviderInfo))
				.AsReadOnly()
				.EnsureValid()
				.Value;
			//
			_builder = builder;
			_contextType = contextType;
			_storeProviderInfo = storeProviderInfo;
		}

		public ModelBuilder Builder
			=> _builder;

		public Type ContextType
			=> _contextType;

		public IDbProviderInfoProps StoreProviderInfo
			=> _storeProviderInfo;

		public override string ToString()
			=>
			$"{GetType()}:"
			+ $"{Environment.NewLine}\t{nameof(Builder)}:{_builder.FmtStr().GNLI2()}"
			+ $"{Environment.NewLine}\t{nameof(ContextType)}:{_contextType.FmtStr().GNLI2()}"
			+ $"{Environment.NewLine}\t{nameof(StoreProviderInfo)}:{_storeProviderInfo.FmtStr().GNLI2()}";

	}

}