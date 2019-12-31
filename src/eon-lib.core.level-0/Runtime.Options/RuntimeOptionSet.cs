using System;
using System.Collections.Generic;

using Eon.Collections;

namespace Eon.Runtime.Options {

	public sealed class RuntimeOptionSet {

		#region Static members

		public static readonly RuntimeOptionSet Empty = new RuntimeOptionSet();

		#endregion

		readonly Dictionary<RuntimeOptions.Identity, RuntimeOptionBase> _options;

		RuntimeOptionSet() {
			_options = new Dictionary<RuntimeOptions.Identity, RuntimeOptionBase>(comparer: ReferenceEqualityComparer<RuntimeOptions.Identity>.Instance);
		}

		public RuntimeOptionSet(IEnumerable<RuntimeOptionBase> options) {
			if (options is null)
				throw new ArgumentNullException(paramName: nameof(options));
			//
			var locOptions = new Dictionary<RuntimeOptions.Identity, RuntimeOptionBase>(comparer: ReferenceEqualityComparer<RuntimeOptions.Identity>.Instance);
			var index = -1;
			foreach (var option in options) {
				index++;
				if (option is null)
					throw new ArgumentNullException(paramName: $"{nameof(options)}[{index:d}]");
				else
					try {
						locOptions.Add(key: option.Identity, value: option);
					}
					catch (Exception exception) {
						throw new ArgumentException(paramName: $"{nameof(options)}[{index:d}]", innerException: exception, message: $"The specified option is not unique within the specified set.{Environment.NewLine}\tOption identity:{Environment.NewLine}\t\t{option.Identity}");
					}
			}
			_options = locOptions;
		}

		public IEnumerable<RuntimeOptionBase> Options
			=> _options.Values;

		public RuntimeOptionBase this[ RuntimeOptions.Identity identity ] {
			get {
				identity.EnsureNotNull(nameof(identity));
				//
				try {
					return _options[ key: identity ];
				}
				catch (KeyNotFoundException exception) {
					throw new KeyNotFoundException(message: $"There is not an option with the specified identity.{Environment.NewLine}\tOption identity:{Environment.NewLine}\t\t{identity}", innerException: exception);
				}
			}
		}

		public bool TryGetOption(RuntimeOptions.Identity identity, out RuntimeOptionBase option)
			=> _options.TryGetValue(key: identity.EnsureNotNull(nameof(identity)).Value, value: out option);

	}

}