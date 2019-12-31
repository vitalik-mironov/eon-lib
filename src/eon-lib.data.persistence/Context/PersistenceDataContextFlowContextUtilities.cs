using System;
using Eon.Data;
using Eon.Data.Persistence;

namespace Eon.Context {

	/// <summary>
	/// Defined utilities of persistence data context for the flow context (<see cref="IContext"/>).
	/// </summary>
	public static class PersistenceDataContextFlowContextUtilities {

		/// <summary>
		/// Requires the persistence data context (<see cref="IPersistenceDataContext"/>) for this flow context.
		/// <para>If flow context (<paramref name="ctx"/>) is <see langword="null"/> OR flow context have not a data context (<see cref="DataContextFlowContextProps.DataContextProp"/>, <see cref="DataContextFlowContextProps.HasDataContext(IContext, bool)"/>), then exception describing the persistence data context abscence will be thrown.</para>
		/// <para>If flow context (<paramref name="ctx"/>) has data context, that incompatible with <see cref="IPersistenceDataContext"/>, then exception (see above) will be thrown.</para>
		/// </summary>
		/// <param name="ctx">
		/// Flow context.
		/// <para>Can be <see langword="null"/>.</para>
		/// </param>
		public static IPersistenceDataContext PersistenceDataContext(this IContext ctx) {
			IDataContext2 dataCtx = default;
			if (ctx is null || !ctx.HasDataContext(dataCtx: out dataCtx) || !(dataCtx is IPersistenceDataContext persistenceDataCtx))
				throw new EonException(message: $"The persistence data context is missing{(ctx is null ? "." : " in this flow context.")}{(ctx is null ? string.Empty : $"{Environment.NewLine}\tContext:{ctx.FmtStr().GNLI2()}")}{(dataCtx is null ? string.Empty : $"{Environment.NewLine}\tExisting data context (type):{dataCtx.GetType().FmtStr().GNLI2()}")}");
			else
				return persistenceDataCtx;
		}

	}

}