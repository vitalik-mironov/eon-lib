namespace Microsoft.Extensions.Logging {

	public static class RunControlEventIds {

		/// <summary>
		/// 50100, run_control_start.
		/// </summary>
		public static readonly EventId Start = new EventId(id: 50100, name: "run_control_start");

		/// <summary>
		/// 50101, run_control_stop.
		/// </summary>
		public static readonly EventId Stop = new EventId(id: 50101, name: "run_control_stop");

	}

}