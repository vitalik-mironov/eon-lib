using System;
using System.Runtime.Serialization;
using System.Security;

using Eon.Runtime.Serialization;

namespace Eon {

	[DataContract]
	public abstract class ReadOnlyScopeBase
		:DisposeNotifying, IReadOnlyScope {

		ReadOnlyStateTag _readOnlyState;

		protected ReadOnlyScopeBase(ReadOnlyStateTag readOnlyState = default) {
			_readOnlyState = readOnlyState ?? new ReadOnlyStateTag();
		}

		protected ReadOnlyScopeBase(SerializationContext ctx)
			: base(context: ctx) { }

		public bool IsReadOnly
			=> ReadDA(ref _readOnlyState).IsReadOnly || (OuterReadOnlyScope?.IsReadOnly ?? false);

		public bool IsPermanentReadOnly
			=> ReadDA(ref _readOnlyState).IsPermanentReadOnly || (OuterReadOnlyScope?.IsPermanentReadOnly ?? false);

		public ReadOnlyStateTag ReadOnlyState
			=> ReadDA(ref _readOnlyState);

		protected abstract IReadOnlyScope OuterReadOnlyScope { get; }

		IReadOnlyScope IReadOnlyScope.OuterReadOnlyScope
			=> OuterReadOnlyScope;

		// TODO: Put strings into the resources.
		//
		public void SetReadOnly(bool isReadOnly, bool isPermanent) {
			var previousReadOnly = ReadDA(ref _readOnlyState);
			if (previousReadOnly.IsPermanent != isPermanent || previousReadOnly.IsReadOnly != isReadOnly) {
				if (previousReadOnly.IsPermanent) {
					throw new EonException(message: $"Невозможно изменить состояние доступности редактирования объекта. Для данного объекта это состояние является неизменным.{Environment.NewLine}\tОбъект:{this.FmtStr().GNLI2()}");
				}
				//
				var newReadOnly = new ReadOnlyStateTag(isReadOnly, isPermanent);
				ReadOnlyStateTag readOnlyStateOriginal;
				if (ReferenceEquals(previousReadOnly, readOnlyStateOriginal = WriteDA(ref _readOnlyState, newReadOnly, previousReadOnly)))
					OnSetReadOnly(previousReadOnly, newReadOnly);
				else if (!(readOnlyStateOriginal.IsPermanent == newReadOnly.IsPermanent && readOnlyStateOriginal.IsReadOnly == newReadOnly.IsReadOnly))
					throw new EonException(message: $"Состояние доступности редактирования объекта не может быть изменено в текущем вызове, так как состояние уже было изменено другим вызовом.{Environment.NewLine}\tОбъект:{this.FmtStr().GNLI2()}");
			}
		}

		void IReadOnlyScope.SetReadOnly(bool readOnly)
			=> SetReadOnly(isReadOnly: readOnly, isPermanent: false);

		protected virtual void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			previousState.EnsureNotNull(nameof(previousState));
			newState.EnsureNotNull(nameof(newState));
			//
		}

		[SecuritySafeCritical]
		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			WriteDA(ref _readOnlyState, ReadDA(ref _readOnlyState) ?? new ReadOnlyStateTag());
		}

		[SecuritySafeCritical]
		protected override void Dispose(bool explicitDispose) {
			_readOnlyState = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}