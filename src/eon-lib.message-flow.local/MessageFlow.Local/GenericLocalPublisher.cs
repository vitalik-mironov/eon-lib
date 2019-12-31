namespace Eon.MessageFlow.Local {

	public class GenericLocalPublisher
		:LocalPublisher {

		/// <summary>
		/// Value: <see cref="LocalPublisherSettings.Default"/>.
		/// </summary>
		public static ILocalPublisherSettings DefaultSettings
			=> LocalPublisherSettings.Default;

		public GenericLocalPublisher(ILocalPublisherSettings settings)
			: base(settings: settings) { }

		public GenericLocalPublisher()
			: this(settings: DefaultSettings) { }

	}

}