using System;

namespace Eon {

	public interface ICloneContext
		:IDisposable {

		bool SetCloneOrigin { get; }

		object CloneOrigin { get; }

	}

}