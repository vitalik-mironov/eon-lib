using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

using static Eon.DisposableUtilities;

namespace Eon {

	[DataContract]
	public abstract class CloneableDisposeNotifying
		:DisposeNotifying, IOxyCloneableOrigin {

		CloneableDisposeNotifying _cloneOrigin;

		protected CloneableDisposeNotifying() { }

		[JsonConstructor]
		protected CloneableDisposeNotifying(SerializationContext context)
			: base(context) { }

		object IOxyCloneable.Clone()
			=> Clone(cloneContext: null);

		object IOxyCloneable.Clone(ICloneContext cloneContext)
			=> Clone(cloneContext: cloneContext);

		public CloneableDisposeNotifying Clone(ICloneContext cloneContext) {
			EnsureNotDisposeState(considerDisposeRequest: true);
			//
			var clone = (CloneableDisposeNotifying)MemberwiseClone();
			clone.RecreateDisposableImplementation();
			try {
				PopulateClone(cloneContext: cloneContext, clone: clone);
				if (cloneContext?.SetCloneOrigin == true)
					clone.WriteDA(location: ref _cloneOrigin, value: this);
				else
					EnsureNotDisposeState(considerDisposeRequest: true);
				return clone;
			}
			catch (Exception exception) {
				DisposeMany(exception: exception, disposables: clone);
				throw;
			}
		}

		public CloneableDisposeNotifying Clone()
			=> Clone(cloneContext: null);

		protected abstract void PopulateClone(ICloneContext cloneContext, CloneableDisposeNotifying clone);

		public bool HasCloneOrigin
			=> ReadDA(ref _cloneOrigin) != null;

		// TODO: Put exception messages into the resources.
		//
		public CloneableDisposeNotifying CloneOrigin {
			get {
				var cloneOrigin = ReadDA(ref _cloneOrigin);
				if (cloneOrigin is null)
					throw new EonException(message: $"Объект-источник данного объекта-клона отсутствует.{Environment.NewLine}\tОбъект-клон:{this.FmtStr().GNLI2()}");
				else
					return cloneOrigin;
			}
		}

		object IOxyCloneableOrigin.CloneOrigin
			=> CloneOrigin;

		protected override void Dispose(bool explicitDispose) {
			_cloneOrigin = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}