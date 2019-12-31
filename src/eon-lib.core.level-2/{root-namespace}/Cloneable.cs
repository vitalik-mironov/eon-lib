using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

namespace Eon {

	[DataContract]
	public abstract class Cloneable
		:IOxyCloneable {

		protected Cloneable() { }

		protected Cloneable(SerializationContext ctx) { }

		object IOxyCloneable.Clone()
			=> Clone();

		public Cloneable Clone()
			=> Clone(cloneCtx: null);

		// TODO: Put strings into the resources.
		//
		public Cloneable Clone(ICloneContext cloneCtx) {
			var result = (Cloneable)MemberwiseClone();
			if (cloneCtx != null && cloneCtx.SetCloneOrigin)
				throw new NotSupportedException($"Setting clone origin is not supported for this object.{Environment.NewLine}\tObject:{this.FmtStr().GNLI2()}").SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
			PopulateClone(cloneCtx, result);
			return result;
		}

		protected abstract void PopulateClone(ICloneContext cloneCtx, Cloneable clone);

		object IOxyCloneable.Clone(ICloneContext cloneCtx)
			=> Clone(cloneCtx: cloneCtx);

	}

}