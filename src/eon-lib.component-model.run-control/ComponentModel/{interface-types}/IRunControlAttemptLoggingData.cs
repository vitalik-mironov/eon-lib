using System;

using Eon.Context;
using Eon.Diagnostics;

namespace Eon.ComponentModel {

	/// <summary>
	/// Defines data for logging about start/stop attempt(s) of <see cref="IRunControl"/>.
	/// </summary>
	public interface IRunControlAttemptLoggingData {

		/// <summary>
		/// Gets attempt completion status code.
		/// </summary>
		OperationCompletionStatusCode Status { get; }

		/// <summary>
		/// Gets correlation ID of the attempt.
		/// <para>Can be <see langword="null"/>.</para>
		/// <para>Can't be <see cref="XFullCorrelationId.IsEmpty"/>.</para>
		/// </summary>
		XFullCorrelationId CorrelationId { get; }

		/// <summary>
		/// Gets the run control component with which attempt associated.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		IRunControl RunControl { get; }

		/// <summary>
		/// Indicates attempt of start operation.
		/// </summary>
		bool IsStart { get; }

		/// <summary>
		/// Indicates attempt of stop operation.
		/// </summary>
		bool IsStop { get; }

		/// <summary>
		/// Gets count of successfully completed attempt before this attempt (see <seealso cref="AttemptNumber"/>).
		/// </summary>
		int SucceededAttemptCountBefore { get; }

		/// <summary>
		/// Gets attempt number.
		/// <para>Numbering start from zero.</para>
		/// </summary>
		int AttemptNumber { get; }

		/// <summary>
		/// Gets attempt execution duration.
		/// </summary>
		TimeSpan Duration { get; }

		/// <summary>
		/// Gets attempt fault exception (if <see cref="Status"/> == <see cref="OperationCompletionStatusCode.Fault"/>).
		/// </summary>
		Exception Exception { get; }

	}

}