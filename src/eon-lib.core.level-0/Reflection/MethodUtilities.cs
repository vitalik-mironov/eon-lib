using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eon.Reflection {

	public static class MethodUtilities {

		public static bool IsMatchSignature(this MethodBase method, IEnumerable<Type> argTypes) {
			if (method is null)
				throw new ArgumentNullException(paramName: nameof(method));
			else if (argTypes is null)
				throw new ArgumentNullException(paramName: nameof(argTypes));
			var argTypesBuffer = argTypes.Select(selector: (locItem, locIndex) => locItem ?? throw new ArgumentNullException(paramName: $"{nameof(argTypes)}[{locIndex:d}]")).ToArray();
			//
			var methodParameters = method.GetParameters();
			if (methodParameters.Length == argTypesBuffer.Length) {
				for (var i = 0; i < methodParameters.Length; i++)
					if (!methodParameters[ i ].ParameterType.IsAssignableFrom(argTypesBuffer[ i ]))
						return false;
				return true;
			}
			else
				return false;
		}

	}

}