using System;

namespace Eon.ComponentModel {

	/// <summary>
	/// Defines a success of start/stop attempt for <see cref="IRunControl"/>.
	/// </summary>
	public interface IRunControlAttemptSuccess
		:IRunControlAttemptProperties {

		/// <summary>
		/// Gets attempt execution duration.
		/// </summary>
		TimeSpan Duration { get; }

		/// <summary>
		/// Indicates that the particular operation was performed by this attempt, while other attempts (if such were) were awaiting the result of this attempt.
		/// <para>Note, that stop (<see cref="IRunControl.StopAsync(Context.IContext, bool)"/>) performed without preceding start (see <see cref="IRunControl.StartAsync(Context.IContext)"/>) also leads to <see langword="false"/>-value of this property.</para>
		/// </summary>
		bool IsMaster { get; }

	}

}