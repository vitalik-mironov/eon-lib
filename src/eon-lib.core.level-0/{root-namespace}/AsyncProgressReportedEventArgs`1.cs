using System;
using System.Diagnostics;

namespace Eon {

	public class AsyncProgressReportedEventArgs<T>
		:EventArgs {

		readonly T _value;

		public AsyncProgressReportedEventArgs(T value) {
			_value = value;
		}

		public T Value => _value;

	}

}