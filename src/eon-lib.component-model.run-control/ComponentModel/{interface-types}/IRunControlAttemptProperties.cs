using Eon.Context;

namespace Eon.ComponentModel {

	/// <summary>
	/// Defines properties of start/stop attempt for <see cref="IRunControl"/>.
	/// </summary>
	public interface IRunControlAttemptProperties {

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
		/// Gets a tag (some user state object) associated with this attempt.
		/// <para>Can be <see langword="null"/>.</para>
		/// </summary>
		object Tag { get; }

	}

}