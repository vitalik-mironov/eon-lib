using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon {

	[DataContract]
	public abstract class CloneableDisposable
		:Disposable, IEonCloneableOrigin {

		CloneableDisposable _cloneOrigin;

		private protected CloneableDisposable(bool nopDispose)
			: base(nopDispose: nopDispose) { }

		protected CloneableDisposable() { }

		[JsonConstructor]
		protected CloneableDisposable(SerializationContext ctx)
			: base(ctx: ctx) { }

		object IEonCloneable.Clone()
			=> Clone(null);

		object IEonCloneable.Clone(ICloneContext cloneCtx)
			=> Clone(cloneCtx: cloneCtx);

		public CloneableDisposable Clone()
			=> Clone(null);

		protected abstract void PopulateClone(ICloneContext cloneCtx, CloneableDisposable clone);

		public CloneableDisposable Clone(ICloneContext cloneCtx) {
			EnsureNotDisposeState(considerDisposeRequest: true);
			//
			var clone = (CloneableDisposable)MemberwiseClone();
			clone.RecreateDisposableImplementation();
			try {
				PopulateClone(cloneCtx: cloneCtx, clone: clone);
				if (cloneCtx?.SetCloneOrigin == true)
					clone.WriteDA(location: ref _cloneOrigin, value: this);
				EnsureNotDisposeState(considerDisposeRequest: true);
				return clone;
			}
			catch (Exception exception) {
				clone?.Dispose(exception: exception);
				throw;
			}
		}

		public bool HasCloneOrigin
			=> !(ReadDA(ref _cloneOrigin) is null);

		// TODO: Put exception messages into the resources.
		//
		public CloneableDisposable CloneOrigin {
			get {
				var cloneOrigin = ReadDA(ref _cloneOrigin);
				if (cloneOrigin is null)
					throw new EonException(message: $"Object have not a clone origin.{Environment.NewLine}\tObject:{this.FmtStr().GNLI2()}");
				else
					return cloneOrigin;
			}
		}

		object IEonCloneableOrigin.CloneOrigin
			=> CloneOrigin;

		protected override void Dispose(bool explicitDispose) {
			_cloneOrigin = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}