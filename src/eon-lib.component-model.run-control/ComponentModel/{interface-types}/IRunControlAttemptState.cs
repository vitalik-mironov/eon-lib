using System.Threading;
using System.Threading.Tasks;

using Eon.Context;

namespace Eon.ComponentModel {

	/// <summary>
	/// Defines an execution state of start/stop attempt for <see cref="IRunControl"/>.
	/// </summary>
	public interface IRunControlAttemptState
		:IRunControlAttemptProperties {

		/// <summary>
		/// Gets an attempt cancellation token.
		/// </summary>
		CancellationToken Ct { get; }

		/// <summary>
		/// Gets an attempt context (<see cref="IContext"/>).
		/// <para>Cann't be <see langword="null"/>.</para>
		/// </summary>
		IContext Context { get; }

		/// <summary>
		/// Gets an awaitable of successive completion of this attempt.
		/// <para>Cann't be <see langword="null"/>.</para>
		/// </summary>
		Task<IRunControlAttemptSuccess> SuccessCompletion { get; }

		/// <summary>
		/// Gets an awaitable of completion of this attempt.
		/// <para>Cann't be <see langword="null"/>.</para>
		/// </summary>
		Task<IRunControlAttemptSuccess> Completion { get; }

	}

}