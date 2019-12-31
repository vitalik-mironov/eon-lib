using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

namespace Eon {

	[DataContract]
	public abstract class CopyableDisposableBase
		:DisposeNotifying {

		protected CopyableDisposableBase() { }

		protected CopyableDisposableBase(SerializationContext context)
			: base(context: context) { }

		public CopyableDisposableBase CreateCopy(object copyArgs = null) {
			EnsureNotDisposeState(considerDisposeRequest: true);
			//
			var copy = (CopyableDisposableBase)MemberwiseClone();
			copy.RecreateDisposableImplementation();
			try {
				PopulateCopy(copyingArgs: copyArgs, copy: copy);
				EnsureNotDisposeState(considerDisposeRequest: true);
				return copy;
			}
			catch (Exception exception) {
				copy?.Dispose(exception: exception);
				throw;
			}
		}

		protected virtual void PopulateCopy(object copyingArgs, CopyableDisposableBase copy) { }

	}

}