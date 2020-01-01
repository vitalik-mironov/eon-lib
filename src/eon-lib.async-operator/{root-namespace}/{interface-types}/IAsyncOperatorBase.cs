using System;

namespace Eon {

	/// <summary>
	/// Defines the base specs of lazy asynchronous operation.
	/// </summary>
	public interface IAsyncOperatorBase
		:IEonDisposable {

		/// <summary>
		/// Indicates whether the execution of this operator can be repeated.
		/// <para>Dipose tolerant.</para>
		/// </summary>
		bool IsReexecutable { get; }

		bool IsCompleted { get; }

		Type ResultTypeConstraint { get; }

		bool OwnsResult { get; }

		/// <summary>
		/// Tries to set execution prevention of this operator before first or successive execution begins.
		/// <para><see langword="true"/> — no anymore execution will be performed.</para>
		/// </summary>
		bool TrySetPrevention();

		/// <summary>
		/// Indicates whether execution prevention set for this operator (<seealso cref="TrySetPrevention"/>).
		/// </summary>
		bool HasPrevention { get; }

	}

}