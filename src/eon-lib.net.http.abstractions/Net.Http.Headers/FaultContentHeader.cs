using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Net.Http.Headers {

	public static class FaultContentHeader {

		static string __Name;

		/// <summary>
		/// Value: 'yes'.
		/// </summary>
		public static readonly string YesValue = "yes";

		/// <summary>
		/// Value: 'no'.
		/// </summary>
		public static readonly string NoValue = "no";

		/// <summary>
		/// Value: 'x-{suffix}-fault-content'. Where '{suffix}' is <see cref="XHeader.DefaultSuffix"/>.
		/// </summary>
		public static string Name {
			get {
				var name = itrlck.Get(location: ref __Name);
				if (name is null) {
					name = $"x-{XHeader.DefaultSuffix}-fault-content";
					name = string.Intern(str: name);
					name = itrlck.UpdateIfNull(location: ref __Name, value: name);
				}
				return name;
			}
		}

		public static KeyValuePair<string, string> CreateFrom(bool value)
			=> new KeyValuePair<string, string>(Name, value ? YesValue : NoValue);

		public static void SetFor(HttpResponseMessage msg, bool value) {
			msg.EnsureNotNull(nameof(msg));
			//
			var header = CreateFrom(value);
			msg.Headers.Remove(header.Key);
			msg.Headers.Add(header.Key, header.Value);
		}

		public static bool? ReadFrom(HttpResponseMessage msg) {
			msg.EnsureNotNull(nameof(msg));
			//
			IEnumerable<string> headerValues;
			if (msg.Headers.TryGetValues(Name, out headerValues)) {
				var headerValuesArray = headerValues.Take(2).ToArray();
				var errorMessagePrologue = $"HTTP header '{Name}' set incorrectly.{Environment.NewLine}";
				if (headerValuesArray.Length == 1) {
					if (headerValuesArray[ 0 ].EqualsOrdinalCI(YesValue))
						return true;
					else if (headerValuesArray[ 0 ].EqualsOrdinalCI(NoValue))
						return false;
					else
						throw new EonException($"{errorMessagePrologue}Header value is not valid.{Environment.NewLine}\tHeader value:{headerValuesArray[ 0 ].FmtStr().GNLI2()}");
				}
				else if (headerValuesArray.Length > 1)
					throw new EonException($"{errorMessagePrologue}Header have multiple values ({headerValuesArray.Length} values), whereas only one value expected.");
				else
					throw new EonException($"{errorMessagePrologue}Header value is missing.");
			}
			else
				return null;
		}

	}

}