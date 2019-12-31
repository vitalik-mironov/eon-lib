using System;

namespace Eon.MessageFlow.Local {

	public sealed class LocalMessage<TPayload>
		:Disposable, ILocalMessage<TPayload> {

		readonly DateTimeOffset _localCreationTimestamp;

		IVh<TPayload> _payload;

		public LocalMessage(TPayload payload)
			: this(payload: payload, ownsPayload: false) { }

		public LocalMessage(TPayload payload, bool ownsPayload) {
			payload.EnsureNotNull(nameof(payload));
			//
			_localCreationTimestamp = DateTimeOffset.Now;
			_payload = payload.ToValueHolder(ownsValue: ownsPayload);
		}

		public DateTimeOffset LocalCreationTimestamp
			=> _localCreationTimestamp;

		public TPayload Payload
			=> ReadDA(ref _payload).Value;

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_payload?.Dispose();
			_payload = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}