using System;
using System.Collections.Specialized;

namespace Eon.Collections {

	public static class NameValueCollectionUtilities {

		// TODO: Put strings into the resources.
		//
		public static string RequireSingleValue(this ArgumentUtilitiesHandle<NameValueCollection> collection, string name) {
			collection.EnsureNotNull();
			name.EnsureNotNull(nameof(name));
			//
			var values = collection.Value.GetValues(name: name);
			if (values is null) {
				var message = $"The required named value is missing.{Environment.NewLine}\tName:{name.FmtStr().GNLI2()}";
				throw collection.ExceptionFactory?.Invoke(message: message) ?? new ArgumentException(message: message, paramName: collection.Name);
			}
			else if (values.Length == 1)
				return values[ 0 ];
			else {
				var message = $"The specified name maps to more than one values.{Environment.NewLine}\tName:{name.FmtStr().GNLI2()}";
				throw collection.ExceptionFactory?.Invoke(message: message) ?? new ArgumentException(message: message, paramName: collection.Name);
			}
		}

		public static string RequireSingleValue(this NameValueCollection collection, string name)
			=> RequireSingleValue(collection: collection.Arg(nameof(collection)), name: name);

		public static T RequireSingleParameter<T>(this ArgumentUtilitiesHandle<NameValueCollection> collection, string name, Func<string, T> converter) {
			collection.EnsureNotNull();
			converter.EnsureNotNull(nameof(converter));
			//
			string valueString;
			try {
				valueString = collection.RequireSingleValue(name: name);
			}
			catch (Exception exception) {
				throw new OperationIllegalParameterException(message: $"Parameter reading error.{Environment.NewLine}Parameter name:{name.FmtStr().GNLI2()}", innerException: exception);
			}
			T value;
			try {
				value = converter(arg: valueString);
			}
			catch (Exception exception) {
				throw new OperationIllegalParameterException(message: $"An exception occurred while converting the string value of the specified parameter to the value of type '{typeof(T)}'.{Environment.NewLine}Parameter name:{name.FmtStr().GNLI2()}", innerException: exception);
			}
			return value;
		}

		public static T RequireSingleParameter<T>(this NameValueCollection collection, string name, Func<string, T> converter)
			=> RequireSingleParameter<T>(collection: collection.Arg(nameof(collection)), name: name, converter: converter);

	}

}