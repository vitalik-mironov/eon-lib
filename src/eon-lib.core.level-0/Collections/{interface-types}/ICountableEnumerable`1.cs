using System.Collections.Generic;

namespace Eon.Collections {

	public interface ICountableEnumerable<out T>
		:IReadOnlyList<T> {

		bool IsEmpty { get; }

		T Last { get; }

		T First { get; }

	}

}
