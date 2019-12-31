using System;
using System.Linq;
using System.Reflection;

namespace Eon.Reflection {

	public static class CustomAttributeDataUtilities {

		public static TValue GetNamedArgumentValueOrDefault<TValue>(this CustomAttributeData attributeData, string argumentName) {
			if (attributeData is null)
				throw new ArgumentNullException(paramName: nameof(attributeData));
			//
			var arg = attributeData.NamedArguments.Where(locItem => locItem.MemberName == argumentName).SingleOrDefault();
			if (arg == default)
				return default;
			else {
				var typedValue = arg.TypedValue.Value;
				return typedValue == null ? default : (TValue)typedValue;
			}
		}

	}

}