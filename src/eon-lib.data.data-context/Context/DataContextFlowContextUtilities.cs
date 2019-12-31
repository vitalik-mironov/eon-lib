using System;
using System.Threading;

using Eon.Data;

namespace Eon.Context {

	public static class DataContextFlowContextUtilities {

		public static IContext New(
			this IContext outerCtx,
			IDataContext2 dataCtx,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			ArgumentPlaceholder<CancellationToken> ct = default,
			ArgumentPlaceholder<IVh<IDisposeRegistry>> disposeRegistry = default,
			ArgumentPlaceholder<IVh<IAsyncProgress<IFormattable>>> progress = default,
			ArgumentPlaceholder<IFormattable> displayName = default) {
			//
			var newCtx = default(IContext);
			try {
				newCtx = ContextUtilities.Create(outerCtx: outerCtx, fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, ct: ct, disposeRegistry: disposeRegistry, progress: progress, displayName: displayName);
				DataContextFlowContextProps.DataContextProp.SetLocalValue(ctx: newCtx, value: dataCtx);
				return newCtx;
			}
			catch (Exception exception) {
				newCtx?.Dispose(exception);
				throw;
			}
		}

	}

}