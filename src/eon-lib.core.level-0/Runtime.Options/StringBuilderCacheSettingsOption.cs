using System;

using Eon.Text;

namespace Eon.Runtime.Options {

	public sealed class StringBuilderCacheSettingsOption
		:RuntimeOptionBase {

		#region Nested types

		sealed class P_Settings
			:IStringBuilderBufferSetSettings {

			#region Static members

			public static readonly P_Settings Fallback = new P_Settings(bufferMaxCapacity: 16777216, bufferMinCapacity: 32768, bufferSetMaxSize: 32);

			#endregion

			readonly int _bufferMaxCapacity;

			readonly int _bufferMinCapacity;

			readonly int _bufferSetMaxSize;

			internal P_Settings(int bufferMaxCapacity, int bufferMinCapacity, int bufferSetMaxSize) {
				_bufferSetMaxSize = bufferSetMaxSize;
				_bufferMinCapacity = bufferMinCapacity;
				_bufferMaxCapacity = bufferMaxCapacity;
			}

			public int? BufferMaxCapacity {
				get => _bufferMaxCapacity;
				set => throw new InvalidOperationException(message: $"This object is read-only.{Environment.NewLine}\tObject:{Environment.NewLine}\t\t{this}");
			}

			public int? BufferMinCapacity {
				get => _bufferMinCapacity;
				set => throw new InvalidOperationException(message: $"This object is read-only.{Environment.NewLine}\tObject:{Environment.NewLine}\t\t{this}");
			}

			public int? BufferSetMaxSize {
				get => _bufferSetMaxSize;
				set => throw new InvalidOperationException(message: $"This object is read-only.{Environment.NewLine}\tObject:{Environment.NewLine}\t\t{this}");
			}

			public bool IsReadOnly
				=> true;

			public IStringBuilderBufferSetSettings AsReadOnly()
				=> this;

			public void Validate() { }

		}

		#endregion

		#region Static & constant members

		/// <summary>
		/// Value:
		/// <para><see cref="IStringBuilderBufferSetSettings.BufferMaxCapacity"/>: <see langword="16777216"/></para>
		/// <para><see cref="IStringBuilderBufferSetSettings.BufferMinCapacity"/>: <see langword="32768"/></para>
		/// <para><see cref="IStringBuilderBufferSetSettings.BufferSetMaxSize"/>: <see langword="32"/></para>
		/// </summary>
		public static readonly StringBuilderCacheSettingsOption Fallback;

		static StringBuilderCacheSettingsOption() {
			Fallback = new StringBuilderCacheSettingsOption(settings: P_Settings.Fallback);
			RuntimeOptions.Option<StringBuilderCacheSettingsOption>.SetFallback(option: Fallback);
		}

		public static IStringBuilderBufferSetSettings Require()
			=> RuntimeOptions.Option<StringBuilderCacheSettingsOption>.Require().Settings;

		#endregion

		public StringBuilderCacheSettingsOption(IStringBuilderBufferSetSettings settings) {
			if (settings is null)
				throw new ArgumentNullException(paramName: nameof(settings));
			else if (!settings.IsReadOnly)
				throw new ArgumentException(paramName: nameof(settings), message: $"Specified settings is muttable (see '{nameof(settings)}.{nameof(settings.IsReadOnly)}'). Immutable settings expected.");
			else
				try {
					settings.Validate();
				}
				catch (Exception exception) {
					throw new ArgumentException(paramName: nameof(settings), message: "Specified settings are not valid.", innerException: exception);
				}
			//
			if (settings.BufferMaxCapacity is null || settings.BufferMinCapacity is null || settings.BufferSetMaxSize is null) {
				var maxCapacity = settings.BufferMaxCapacity ?? P_Settings.Fallback.BufferMaxCapacity.Value;
				var minCapacity = settings.BufferMinCapacity ?? P_Settings.Fallback.BufferMinCapacity.Value;
				if (minCapacity > maxCapacity)
					maxCapacity = minCapacity;
				Settings = new P_Settings(bufferMinCapacity: minCapacity, bufferMaxCapacity: maxCapacity, bufferSetMaxSize: settings.BufferSetMaxSize ?? P_Settings.Fallback.BufferSetMaxSize.Value);
			}
			else
				Settings = settings;
		}

		public IStringBuilderBufferSetSettings Settings { get; }

	}

}