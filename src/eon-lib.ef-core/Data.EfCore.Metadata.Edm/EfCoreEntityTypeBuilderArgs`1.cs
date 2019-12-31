using System;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eon.Data.EfCore.Metadata.Edm {

	public class EfCoreEntityTypeBuilderArgs<TEntity>
		:IEfCoreModelBuilderReadOnlyProperty
		where TEntity : class {

		readonly EfCoreModelBuilderArgs _modelBuilder;

		readonly EntityTypeBuilder<TEntity> _builder;

		public EfCoreEntityTypeBuilderArgs(EfCoreModelBuilderArgs modelBuilder, EntityTypeBuilder<TEntity> builder) {
			modelBuilder.EnsureNotNull(nameof(modelBuilder));
			builder.EnsureNotNull(nameof(builder));
			//
			_modelBuilder = modelBuilder;
			_builder = builder;
		}

		public EfCoreModelBuilderArgs ModelBuilder
			=> _modelBuilder;

		public EntityTypeBuilder<TEntity> Builder
			=> _builder;

		public override string ToString()
			=>
			$"{GetType()}:"
			+ $"{Environment.NewLine}\t{nameof(ModelBuilder)}:{_modelBuilder.FmtStr().GNLI2()}"
			+ $"{Environment.NewLine}\t{nameof(Builder)}:{_builder.FmtStr().GNLI2()}";

	}

}