using System;
using System.Runtime.Serialization;

using Eon.Context;

namespace Eon {

	/// <summary>
	/// Defines a hints to the component, acting as XApp host, which behavior should be applied on app startup (se  <see cref="IXApp{TDescription}"/>, <see cref="IXAppStartupContext.HostHints"/>, <see cref="XAppStartupContextFactory{TDescription}"/>).
	/// </summary>
	[Flags]
	[DataContract]
	public enum XAppStartupContextHostHints {

		[EnumMember]
		None = 0,

		[EnumMember]
		InitializeApp = 1,

		[EnumMember]
		RunApp = 2 | InitializeApp,

	}

}