using System;
using System.Runtime.ExceptionServices;

using Eon;
using Eon.Description;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Microsoft.Extensions.Options {

	/// <summary>
	/// Special type of options representing settings — <see cref="ISettings"/>.
	/// </summary>
	/// <typeparam name="TSettings">Settings type constraint.</typeparam>
	public sealed class SettingsOptionsAccessor<TSettings>
		where TSettings : class, ISettings {

		#region Nested types

		sealed class P_State {

			public readonly TSettings Settings;

			public readonly bool SetOnceMark;

			public readonly bool GetOnceMark;

			public readonly ExceptionDispatchInfo ValidationError;

			internal P_State(TSettings settings, bool setOnceMark, bool getOnceMark, ExceptionDispatchInfo validationError) {
				Settings = settings;
				SetOnceMark = setOnceMark;
				GetOnceMark = getOnceMark;
				ValidationError = validationError;
			}

		}

		#endregion

		P_State _state;

		public SettingsOptionsAccessor() {
			_state = new P_State(settings: null, setOnceMark: false, getOnceMark: false, validationError: null);
		}

		// TODO: Put strings into the resources.
		//
		public void Update(Transform<TSettings> transform) {
			transform.EnsureNotNull(nameof(transform));
			//
			for (; ; ) {
				var currentState = itrlck.Get(location: ref _state);
				if (currentState.GetOnceMark)
					throw new EonException(message: $"Settings has already been read and cannot be updated.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				var currentSettings = currentState.Settings;
				var newSettings = transform(current: currentSettings);
				if (ReferenceEquals(objA: newSettings, objB: currentSettings) && currentState.SetOnceMark)
					break;
				else {
					var newState = new P_State(settings: newSettings, getOnceMark: false, setOnceMark: true, validationError: null);
					if (itrlck.UpdateBool(location: ref _state, value: newState, comparand: currentState))
						break;
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		public TSettings Get() {
			P_State state;
			for (; ; ) {
				var locState = itrlck.Get(location: ref _state);
				if (locState.GetOnceMark) {
					state = locState;
					break;
				}
				else {
					var settings = locState.Settings;
					if (!(settings?.IsReadOnly ?? true)) {
						if (settings is IAsReadOnlyMethod<TSettings> asReadOnlyMethod)
							settings = asReadOnlyMethod.AsReadOnly();
						else
							throw new EonException(message: $"The supplied settings is not in read-only state, and doesn't implement '{typeof(IAsReadOnlyMethod<TSettings>)}' type.{Environment.NewLine}Mentioned type is required to create a read-only copy instance of the specified settings.{Environment.NewLine}\tSettings:{settings.FmtStr().GNLI2()}{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
					}
					var validationError = default(ExceptionDispatchInfo);
					try {
						settings.Validate();
					}
					catch (Exception exception) {
						validationError = ExceptionDispatchInfo.Capture(source: exception);
					}
					var newState = new P_State(settings: settings, getOnceMark: true, setOnceMark: locState.SetOnceMark, validationError: validationError);
					if (itrlck.UpdateBool(location: ref _state, value: newState, comparand: locState)) {
						state = newState;
						break;
					}
				}
			}
			if (state.SetOnceMark) {
				if (state.ValidationError is null)
					return state.Settings;
				else {
					state.ValidationError.Throw();
					return default;
				}
			}
			else
				throw new EonException(message: $"Settings has not been supplied.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
		}

	}

}