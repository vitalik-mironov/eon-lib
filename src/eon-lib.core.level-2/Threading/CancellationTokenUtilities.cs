using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Eon.Threading {

	public static class CancellationTokenUtilities {

		static readonly CancellationTokenSource __Canceled;

		static CancellationTokenUtilities() {
			__Canceled = new CancellationTokenSource();
			__Canceled.Cancel();
		}

		public static CancellationToken Canceled
			=> __Canceled.Token;

		public static bool IsEmpty(this CancellationToken cancellationToken)
			=> cancellationToken == CancellationToken.None;

		public static bool IsCancellationRequested(this CancellationToken cancellationToken)
			=> !cancellationToken.IsEmpty() && cancellationToken.IsCancellationRequested;

		public static CancellationToken SingleOrLinked(this CancellationToken ct1, CancellationToken ct2, out CancellationTokenSource linkedCts)
			=> P_Link(ctCollection: new[ ] { ct1, ct2 }, ctsRequired: false, cts: out linkedCts);

		public static CancellationToken SingleOrLinked(this CancellationToken ct1, CancellationToken ct2, CancellationToken ct3, out CancellationTokenSource linkedCts)
			=> P_Link(ctCollection: new[ ] { ct1, ct2, ct3 }, ctsRequired: false, cts: out linkedCts);

		public static CancellationToken SingleOrLinked(this CancellationToken ct1, CancellationToken ct2, CancellationToken ct3, CancellationToken ct4, out CancellationTokenSource linkedCts)
			=> P_Link(ctCollection: new[ ] { ct1, ct2, ct3, ct4 }, ctsRequired: false, cts: out linkedCts);

		public static CancellationToken LinkedOrNew(this CancellationToken ct1, out CancellationTokenSource cts)
			=> P_Link(ctCollection: new[ ] { ct1 }, ctsRequired: true, cts: out cts);

		public static CancellationToken LinkedOrNew(this CancellationToken ct1, CancellationToken ct2, out CancellationTokenSource cts)
			=> P_Link(ctCollection: new[ ] { ct1, ct2 }, ctsRequired: true, cts: out cts);

		public static CancellationToken LinkedOrNew(this CancellationToken ct1, CancellationToken ct2, CancellationToken ct3, out CancellationTokenSource cts)
			=> P_Link(ctCollection: new[ ] { ct1, ct2, ct3 }, ctsRequired: true, cts: out cts);

		public static CancellationToken LinkedOrNew(this CancellationToken ct1, CancellationToken ct2, CancellationToken ct3, CancellationToken ct4, out CancellationTokenSource cts)
			=> P_Link(ctCollection: new[ ] { ct1, ct2, ct3, ct4 }, ctsRequired: true, cts: out cts);

		static CancellationToken P_Link(CancellationToken[ ] ctCollection, bool ctsRequired, out CancellationTokenSource cts) {
			CancellationTokenSource locCts;
			var cancelableTokens = ctCollection.Where(locCt => locCt.CanBeCanceled).ToArray();
			if (cancelableTokens.IsNullOrEmpty()) {
				if (ctsRequired) {
					cts = locCts = new CancellationTokenSource();
					return locCts.Token;
				}
				else {
					cts = null;
					return CancellationToken.None;
				}
			}
			else if (cancelableTokens.Length == 1 && !ctsRequired) {
				cts = null;
				return cancelableTokens[ 0 ];
			}
			else {
				try {
					locCts = CancellationTokenSource.CreateLinkedTokenSource(tokens: cancelableTokens);
				}
				catch (ArgumentException exception) {
					var operationCanceledException = exception.GetBaseException() as OperationCanceledException;
					if (operationCanceledException is null) {
						for (var i = 0; i < cancelableTokens.Length; i++)
							if (cancelableTokens[ i ].IsCancellationRequested)
								throw new OperationCanceledException(message: null, innerException: exception, token: cancelableTokens[ i ]);
						throw;
					}
					else
						throw new OperationCanceledException(message: null, innerException: exception, token: operationCanceledException.CancellationToken);
				}
				cts = locCts;
				return locCts.Token;
			}
		}

		/// <summary>
		/// Вызывает исключение <see cref="OperationCanceledException"/>, если для указанного токена отмены поступил запрос отмены операции (см. <see cref="CancellationToken.IsCancellationRequested"/>).
		/// </summary>
		/// <param name="ct">Токен отмены.</param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static void ThrowExceptionIfCancellationRequested(this CancellationToken ct) {
			if (ct.IsCancellationRequested)
				throw new OperationCanceledException(token: ct);
		}

	}

}