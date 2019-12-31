using System;

namespace Eon {

	/// <summary>
	/// Defined read-only property <see cref="Timestamp"/>.
	/// </summary>
	public interface ITimestampProperty {

		/// <summary>
		/// Gets the timestamp associated with this object.
		/// </summary>
		DateTimeOffset Timestamp { get; }

	}

}