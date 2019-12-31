using System;
using System.Diagnostics;

namespace Eon {

	/// <summary>
	/// Аргументы события какого-либо сбоя, описываемого объектом исключения (см. <see cref="Fault"/>).
	/// </summary>
	//[DebuggerStepThrough]
	//[DebuggerNonUserCode]
	public class FaultEventArgs
		:BubbleEventArgs {

		readonly Exception _fault;

		public FaultEventArgs(Exception fault) {
			fault.EnsureNotNull(nameof(fault));
			//
			_fault = fault;
		}

		/// <summary>
		/// Объект исключения, описывающий сбой.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		public Exception Fault 
			=> _fault;

	}

}