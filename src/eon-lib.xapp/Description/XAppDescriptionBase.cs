#region Compilation conditional symbols

#define DO_NOT_USE_EON_LOGGING_API

#endregion

using System;
using System.Runtime.Serialization;

using Eon.ComponentModel.Dependencies.Description;
using Eon.MessageFlow.Local.Description;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Description {

	[DataContract]
	public abstract class XAppDescriptionBase
		:DependencySupportDescriptionBase, IXAppDescription {

		#region Static members

		/// <summary>
		/// Значение: '1.0.0'.
		/// </summary>
		static readonly Version __DefaultAppVersion = new Version("1.0.0");

		#endregion

		[DataMember(Order = 0, Name = nameof(AppMessageFlowPublisher), IsRequired = true)]
		MetadataReference<IXAppLocalPublisherDescription> _appMessageFlowPublisher;

#if !DO_NOT_USE_EON_LOGGING_API
		[DataMember(Order = 1, Name = nameof(LoggingAutoSubscription), IsRequired = true)]
		MetadataReference<ILoggingAutoSubscriptionDescription> _loggingAutoSubscription;
#endif

		[DataMember(Order = 2, Name = nameof(AppInitializationList), IsRequired = true)]
		MetadataReference<IXAppInitializationListDescription> _appInitializationList;

		[DataMember(Order = 3, Name = nameof(AppStartActivationList), IsRequired = true)]
		MetadataReference<IActivationListDescription> _appStartActivationListt;

		[DataMember(Order = 4, Name = nameof(AppTitle), IsRequired = true)]
		string _appTitle;

		Version _appVersion;
		[DataMember(Order = 5, Name = nameof(AppVersion), IsRequired = true)]
		string P_AppVersion_DataMember {
			get => _appVersion?.ToString();
			set {
				if (value is null)
					_appVersion = null;
				else
					try {
						_appVersion = Version.Parse(value);
					}
					catch (Exception exception) {
						throw new MemberDeserializationException(GetType(), nameof(AppVersion), exception);
					}
			}
		}

		[DataMember(Order = 6, Name = nameof(AppInstanceIdTemplate), IsRequired = true)]
		string _appInstanceIdTemplate;

		internal XAppDescriptionBase(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		internal XAppDescriptionBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public IXAppLocalPublisherDescription AppMessageFlowPublisher {
			get => ReadDA(ref _appMessageFlowPublisher).Resolve(this);
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
			}
		}

#if !DO_NOT_USE_EON_LOGGING_API
		public ILoggingAutoSubscriptionDescription LoggingAutoSubscription
			=> ReadDA(ref _loggingAutoSubscription).Resolve(this);
#endif

		public IXAppInitializationListDescription AppInitializationList {
			get => ReadDA(ref _appInitializationList).Resolve(this);
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
			}
		}

		public IActivationListDescription AppStartActivationList {
			get => ReadDA(ref _appStartActivationListt).Resolve(this);
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
			}
		}

		public string AppTitle {
			get {
				var appTitle = ReadDA(ref _appTitle);
				if (appTitle is null) {
					appTitle = FullName;
					appTitle = WriteDA(location: ref _appTitle, value: appTitle, comparand: null) ?? appTitle;
				}
				return appTitle;
			}
			set {
				EnsureNotReadOnly();
				WriteDA(location: ref _appTitle, value: value);
			}
		}

		public Version AppVersion {
			get {
				var appVersion = ReadDA(ref _appVersion);
				if (appVersion is null) {
					appVersion = __DefaultAppVersion;
					appVersion = WriteDA(location: ref _appVersion, value: appVersion, comparand: null) ?? appVersion;
				}
				return appVersion;
			}
			set {
				EnsureNotReadOnly();
				WriteDA(location: ref _appVersion, value: value);
			}
		}

		public string AppInstanceIdTemplate {
			get => ReadDA(ref _appInstanceIdTemplate);
			set {
				EnsureNotReadOnly();
				WriteDA(ref _appInstanceIdTemplate, value);
			}
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			this.ArgProp(AppMessageFlowPublisher, nameof(AppMessageFlowPublisher)).EnsureNotNull();
			this.ArgProp(AppInitializationList, nameof(AppInitializationList)).EnsureNotNull();
			this.ArgProp(AppStartActivationList, nameof(AppStartActivationList)).EnsureNotNull();
#if !DO_NOT_USE_EON_LOGGING_API
			this.PropertyArg(ReadDA(ref _loggingAutoSubscription), nameof(LoggingAutoSubscription)).EnsureReachable(@base: this);
#endif
			this.ArgProp(AppVersion, nameof(AppVersion)).EnsureNotNull();
			this.ArgProp(AppInstanceIdTemplate, nameof(AppInstanceIdTemplate)).EnsureNotNull();
		}

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			UpdDAIfNullBool(location: ref _appTitle, value: GetType().ToString());
			UpdDAIfNullBool(location: ref _appVersion, value: __DefaultAppVersion);
		}

		protected override void Dispose(bool explicitDispose) {
			_appMessageFlowPublisher = null;
#if !DO_NOT_USE_EON_LOGGING_API
			_loggingAutoSubscription = null;
#endif
			_appInitializationList = null;
			_appStartActivationListt = null;
			_appTitle = null;
			_appVersion = null;
			_appInstanceIdTemplate = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}