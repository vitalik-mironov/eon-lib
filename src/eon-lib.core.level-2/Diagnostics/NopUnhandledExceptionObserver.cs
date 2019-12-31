using System;

using Eon.Context;

namespace Eon.Diagnostics {

	public sealed class NopUnhandledExceptionObserver
		:IUnhandledExceptionObserver {

		#region Static & constant members

		public static readonly NopUnhandledExceptionObserver Instance = new NopUnhandledExceptionObserver();

		#endregion

		NopUnhandledExceptionObserver() { }

		public bool ObserveException(Exception exception, object component = default, string message = default, IContext exceptionCtx = default)
			=> false;

	}

}