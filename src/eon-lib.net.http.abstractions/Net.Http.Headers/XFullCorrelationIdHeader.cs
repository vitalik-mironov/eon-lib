using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

using Eon.Context;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Net.Http.Headers {

	public static class XFullCorrelationIdHeader {

		static string __Name;

		/// <summary>
		/// Value: 'x-{suffix}-correlation-id'. Where '{suffix}' is <see cref="XHeader.DefaultSuffix"/>.
		/// </summary>
		public static string Name {
			get {
				var name = itrlck.Get(location: ref __Name);
				if (name is null) {
					name = $"x-{XHeader.DefaultSuffix}-correlation-id";
					name = string.Intern(str: name);
					name = itrlck.UpdateIfNull(location: ref __Name, value: name);
				}
				return name;
			}
		}

		// TODO: Put strings into the resources.
		//
		public static KeyValuePair<string, string> CreateFrom(XFullCorrelationId fullCorrelationId) {
			fullCorrelationId.EnsureNotNull(nameof(fullCorrelationId));
			if (fullCorrelationId.IsEmpty)
				throw new ArgumentException(paramName: nameof(fullCorrelationId), message: "Can't be empty.");
			//
			return new KeyValuePair<string, string>(key: Name, value: fullCorrelationId.Value);
		}

		public static void SetFor(HttpHeaders headers, XFullCorrelationId fullCorrelationId) {
			headers.EnsureNotNull(nameof(headers));
			//
			var header = CreateFrom(fullCorrelationId: fullCorrelationId);
			headers.Remove(header.Key);
			headers.Add(header.Key, header.Value);
		}

		// TODO: Put strings into the resources.
		//
		public static XFullCorrelationId GetFrom(HttpHeaders headers, bool requirePresence) {
			headers.EnsureNotNull(nameof(headers));
			//
			if (headers.TryGetValues(name: Name, values: out var headerValues)) {
				var errorMessagePrologue = $"HTTP header '{Name}' set incorrectly.{Environment.NewLine}";
				var headerValuesArray = headerValues.Take(2).ToArray();
				if (headerValuesArray.Length == 1 && headerValuesArray[ 0 ] != null) {
					var correlationIdPathString = headerValuesArray[ 0 ];
					XFullCorrelationId correlationIdPath;
					try {
						correlationIdPath = XFullCorrelationId.Parse(idString: correlationIdPathString.Arg(nameof(correlationIdPathString)));
					}
					catch (Exception exception) {
						throw new EonException(message: $"{errorMessagePrologue}Header value is not valid.{Environment.NewLine}\tHeader value:{correlationIdPathString.FmtStr().GNLI2()}", innerException: exception);
					}
					return correlationIdPath;
				}
				else if (headerValuesArray.Length > 1)
					throw new EonException(message: $"{errorMessagePrologue}Header have multiple values ({headerValuesArray.Length} values), whereas only one value expected.");
				else
					throw new EonException(message: $"{errorMessagePrologue}Header value is missing.");
			}
			else if (requirePresence)
				throw new EonException(message: $"HTTP header '{Name}' is missing.");
			else
				return null;
		}

		public static XFullCorrelationId GetFrom(HttpHeaders headers)
			=> GetFrom(headers: headers, requirePresence: false);

		public static XFullCorrelationId RequireFrom(HttpHeaders headers)
			=> GetFrom(headers: headers, requirePresence: true);

	}

}