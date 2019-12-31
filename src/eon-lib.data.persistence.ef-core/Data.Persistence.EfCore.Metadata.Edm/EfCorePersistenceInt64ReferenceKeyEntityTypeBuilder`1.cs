using System;

using Eon.Data.EfCore;
using Eon.Data.EfCore.Metadata.Edm;

using Microsoft.EntityFrameworkCore;

namespace Eon.Data.Persistence.Ef6.Metadata.Edm {

	public class EfCorePersistenceInt64ReferenceKeyEntityTypeBuilder<TEntity>
		:EfCoreEntityTypeDelegateBuilder<TEntity>
		where TEntity : PersistenceEntityBase<long> {

		public EfCorePersistenceInt64ReferenceKeyEntityTypeBuilder(EfCoreEntityTypeBuilder<TEntity> builder)
			: base(builder: builder) { }

		// TODO: Put strings into the resources.
		//
		protected override void BuildEntity(EfCoreEntityTypeBuilderArgs<TEntity> args) {
			args.EnsureNotNull(nameof(args));
			//
			var builder = args.Builder;
			builder.ToTable(name: EntityType.Name, schema: args.ModelBuilder.StoreProviderInfo.DefaultSchemaName);
			builder.HasKey(keyExpression: e => e.ReferenceKey);
			if (args.ModelBuilder.StoreProviderInfo.IsSqlProvider()) {
				builder
					.Property(e => e.ReferenceKey)
					.HasColumnName("Ref")
					.HasColumnType("BIGINT")
					.ValueGeneratedNever()
					.UsePropertyAccessMode(PropertyAccessMode.Property)
					.IsRequired();
				builder
					.Property(e => e.Etag)
					.HasColumnName("TIMESTAMP")
					.ValueGeneratedOnAddOrUpdate()
					.UsePropertyAccessMode(PropertyAccessMode.Property)
					.IsRowVersion();
				builder
					.Property(e => e.EtagInt64)
					.HasColumnType("BIGINT")
					.HasColumnName("TimestampInt64")
					.UsePropertyAccessMode(PropertyAccessMode.Property)
					.ValueGeneratedOnAddOrUpdate();
			}
			else if (args.ModelBuilder.StoreProviderInfo.IsPgsqlProvider()) {
				builder
					.Property(e => e.ReferenceKey)
					.HasColumnName("Ref")
					.HasColumnType("Int8")
					.ValueGeneratedNever()
					.UsePropertyAccessMode(PropertyAccessMode.Property)
					.IsRequired();
				builder.Ignore(e => e.Etag);
				builder
					.Property(e => e.EtagInt64)
					.HasColumnName("xmin")
					.HasColumnType("xid")
					.UsePropertyAccessMode(PropertyAccessMode.Property)
					.IsRowVersion();
			}
			else
				throw
					new NotSupportedException(
						message: $"The storage provider (see '{nameof(args)}.{nameof(args.ModelBuilder)}.{nameof(args.ModelBuilder.StoreProviderInfo)}') is not supported.{Environment.NewLine}\tProvider:{args.ModelBuilder.StoreProviderInfo.FmtStr().GNLI2()}");
			//
			base.BuildEntity(args: args);
		}

	}

}