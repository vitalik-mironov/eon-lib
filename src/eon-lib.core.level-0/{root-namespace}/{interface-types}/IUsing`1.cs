using System;

namespace Eon {

	public interface IUsing<out T>
		:IDisposable
		where T : class {

		T Value { get; }

	}

}