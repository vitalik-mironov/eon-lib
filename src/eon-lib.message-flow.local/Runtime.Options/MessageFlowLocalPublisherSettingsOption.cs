using System;

using Eon.MessageFlow.Local;

namespace Eon.Runtime.Options {

	public sealed class MessageFlowLocalPublisherSettingsOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value:
		/// <para><see cref="ILocalPublisherSettings.PostingDop"/>: 'Math.Max(Math.Min(Environment.ProcessorCount, 32), 1)' — [1; 32].</para>
		/// </summary>
		public static readonly MessageFlowLocalPublisherSettingsOption Fallback;

		static MessageFlowLocalPublisherSettingsOption() {
			Fallback = new MessageFlowLocalPublisherSettingsOption(settings: new LocalPublisherSettings(postingDop: Math.Max(Math.Min(Environment.ProcessorCount, 32), 1), isReadOnly: true));
			RuntimeOptions.Option<MessageFlowLocalPublisherSettingsOption>.SetFallback(option: Fallback);
		}

		public static ILocalPublisherSettings Require()
			=> RuntimeOptions.Option<MessageFlowLocalPublisherSettingsOption>.Require().Settings;

		#endregion

		ILocalPublisherSettings _settings;

		public MessageFlowLocalPublisherSettingsOption(ILocalPublisherSettings settings) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureNotDisabled().EnsureValid();
			//
			_settings = settings;
		}

		public ILocalPublisherSettings Settings
			=> _settings;

	}

}