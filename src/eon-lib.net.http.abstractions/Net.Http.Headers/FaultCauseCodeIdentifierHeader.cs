using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Net.Http.Headers {

	public static class FaultCauseCodeIdentifierHeader {

		static string __Name;

		/// <summary>
		/// Value: ' '.
		/// </summary>
		public static readonly string DefaultDelimiter = " ";

		/// <summary>
		/// Value: 'x-{suffix}-fault-cause'. Where '{suffix}' is <see cref="XHeader.DefaultSuffix"/>.
		/// </summary>
		public static string Name {
			get {
				var name = itrlck.Get(location: ref __Name);
				if (name is null) {
					name = $"x-{XHeader.DefaultSuffix}-fault-cause";
					name = string.Intern(str: name);
					name = itrlck.UpdateIfNull(location: ref __Name, value: name);
				}
				return name;
			}
		}

		public static KeyValuePair<string, string> CreateFrom(Exception exception, string delimiter = default) {
			exception.EnsureNotNull(nameof(exception));
			//
			delimiter = delimiter ?? " ";
			using (var acquiredBuffer = EonStringBuilderUtilities.AcquireBuffer()) {
				var stringBuilder = acquiredBuffer.StringBuilder;
				//
				var notFirstCode = false;
				foreach (var errorCode in exception.FlattenBase(selector: locException => locException.HasErrorCode()).InnerExceptions.Select(e => e.GetErrorCode())) {
					if (notFirstCode)
						stringBuilder.Append(delimiter);
					else
						notFirstCode = true;
					//
					stringBuilder.Append(errorCode.Identifier);
				}
				//
				return new KeyValuePair<string, string>(Name, stringBuilder.ToString());
			}
		}

		public static void SetFor(HttpResponseMessage msg, Exception exception, string delimiter = default) {
			msg.EnsureNotNull(nameof(msg));
			//
			var headerValue = CreateFrom(exception: exception, delimiter: delimiter).Value;
			msg.Headers.Remove(Name);
			msg.Headers.Add(Name, headerValue);
		}

	}

}