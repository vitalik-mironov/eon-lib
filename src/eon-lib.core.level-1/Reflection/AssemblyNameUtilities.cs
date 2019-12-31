using System.Reflection;

namespace Eon.Reflection {

	public static class AssemblyNameUtilities {

		public static string GetCultureName(this AssemblyName assemblyName) {
			assemblyName.EnsureNotNull(nameof(assemblyName));
			return assemblyName.CultureName;
		}

	}

}	