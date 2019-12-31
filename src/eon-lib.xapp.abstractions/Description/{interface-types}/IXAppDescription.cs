#region Compilation conditional symbols

#define DO_NOT_USE_OXY_LOGGING_API

#endregion

using System;

using Eon.ComponentModel.Dependencies.Description;
using Eon.MessageFlow.Local.Description;

namespace Eon.Description {

	/// <summary>
	/// Base description (configuration) of any Oxy-app.
	/// </summary>
	public interface IXAppDescription
		:IDependencySupportDescription {

		IXAppLocalPublisherDescription AppMessageFlowPublisher { get; set; }

#if !DO_NOT_USE_OXY_LOGGING_API

		ILoggingAutoSubscriptionDescription LoggingAutoSubscription { get; set; }

#endif

		IXAppInitializationListDescription AppInitializationList { get; set; }

		IActivationListDescription AppStartActivationList { get; set; }

		string AppTitle { get; set; }

		Version AppVersion { get; set; }

		string AppInstanceIdTemplate { get; set; }

	}

}