using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Eon.Collections {

	public sealed class ReferenceEqualityComparer<T>
		:IEqualityComparer<T> where T: class {

		#region Static members

		public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

		#endregion

		ReferenceEqualityComparer() { }

		public bool Equals(T a, T b)
			=> ReferenceEquals(objA: a, objB: b);

		public int GetHashCode(T obj) {
			if (obj is null)
				throw new ArgumentNullException(paramName: nameof(obj));
			else
				return RuntimeHelpers.GetHashCode(o: obj);
		}
	}

}