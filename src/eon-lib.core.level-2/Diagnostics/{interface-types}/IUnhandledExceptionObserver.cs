using System;
using Eon.Context;

namespace Eon.Diagnostics {

	/// <summary>
	/// Special component responsible for exception handling.
	/// </summary>
	public interface IUnhandledExceptionObserver {

		/// <summary>
		/// Observes exception.
		/// <para>On success returns <see langword="true"/>.</para>
		/// <para>On fault returns <see langword="false"/>.</para>
		/// <para>This method must not throw any exception excepting the case when the method args is not valid.</para>
		/// </summary>
		/// <param name="exception">
		/// Exception to be observed.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="component">
		/// Component associated with the specified <paramref name="exception"/>.
		/// </param>
		/// <param name="message">
		/// Special message for the specified exception case.
		/// </param>
		/// <param name="exceptionCtx">
		/// Operation context associated with the specified <paramref name="exception"/>.
		/// </param>
		bool ObserveException(Exception exception, object component = default, string message = default, IContext exceptionCtx = default);

	}

}