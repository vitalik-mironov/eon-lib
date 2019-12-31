using System;
using System.Globalization;
using System.IO;

using Eon.Resources.Internal;

namespace Eon.Resources.XResource {

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
	public sealed class AssemblyManifestXResourceSourcePointerAttribute
		:Attribute, IXResourceSourcePointer, IEquatable<AssemblyManifestXResourceSourcePointerAttribute> {

		readonly Type _type;

		readonly string _resourceName;

		// TODO: Put strings into the resources.
		//
		public AssemblyManifestXResourceSourcePointerAttribute(Type type, string resourceName = default) {
			if (type is null)
				throw new ArgumentNullException(paramName: nameof(type));
			else if (resourceName == string.Empty)
				throw new ArgumentException(paramName: nameof(resourceName), message: "Cannot be an empty string.");
			//
			_type = type;
			_resourceName = resourceName;
		}

		public Type Type
			=> _type;

		public string ResourceName
			=> _resourceName;

		// TODO: Put strings into the resources.
		//
		public Stream GetStream(CultureInfo culture, bool throwIfMissing = default) {
			ResourceUtilitiesCoreL0.GetManifestResourceStream(mainAssembly: _type.Assembly, culture: culture, resourceName: _resourceName ?? (_type.IsGenericType ? _type.GetGenericTypeDefinition() : _type).FullName, throwIfMissing: throwIfMissing, stream: out var resourceStream, info: out _);
			return resourceStream;
		}

		public bool Equals(AssemblyManifestXResourceSourcePointerAttribute other) {
			if (other is null)
				return false;
			else
				return _type.Equals(o: other._type) && string.Equals(a: _resourceName, b: other._resourceName, comparisonType: ResourceUtilitiesCoreL0.ResourceNameComparison);
		}

		public bool Equals(IXResourceSourcePointer other)
			=> Equals(other: other as AssemblyManifestXResourceSourcePointerAttribute);

		public override bool Equals(object other)
			=> Equals(other: other as AssemblyManifestXResourceSourcePointerAttribute);

		public override int GetHashCode()
			=> _type.GetHashCode() ^ (_resourceName?.GetHashCode() ?? 0);

		public override string ToString()
			=> $"{nameof(AssemblyManifestXResourceSourcePointerAttribute)}: {_type}{(_resourceName is null ? string.Empty : $", {_resourceName}")}";

	}

}