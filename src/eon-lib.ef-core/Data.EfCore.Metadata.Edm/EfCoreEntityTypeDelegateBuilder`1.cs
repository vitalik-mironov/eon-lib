using System;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Data.TypeSystem;

namespace Eon.Data.EfCore.Metadata.Edm {

	public class EfCoreEntityTypeDelegateBuilder<TEntity>
		:IDataTypeBuilder, IEfCoreEntityTypeBuilder
		where TEntity : class {

		#region Static members

		protected static readonly Type EntityType = typeof(TEntity);

		#endregion

		readonly EfCoreEntityTypeBuilder<TEntity> _builder;

		public EfCoreEntityTypeDelegateBuilder(EfCoreEntityTypeBuilder<TEntity> builder) {
			builder.EnsureNotNull(nameof(builder));
			//
			_builder = builder;
		}

		public void BuildEntity(EfCoreModelBuilderArgs model) {
			model.EnsureNotNull(nameof(model));
			//
			BuildEntity(args: new EfCoreEntityTypeBuilderArgs<TEntity>(modelBuilder: model, builder: model.Builder.Entity<TEntity>()));
		}

		protected virtual void BuildEntity(EfCoreEntityTypeBuilderArgs<TEntity> args)
			=> _builder(args: args);

		public async Task<Type> BuildTypeAsync(IContext ctx = default) {
			await Task.CompletedTask;
			return EntityType;
		}

	}

}