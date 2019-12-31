using System;
using System.Reflection;

namespace Eon.ComponentModel.Properties {

	public interface IPropertyValueCopierConfiguration
		:IDisposable {

		bool IsCopyableFrom(Type type);

		bool IsCopyableTo(Type type);

		bool IsCopyableFrom(PropertyInfo property, MethodInfo getAccessor, MethodInfo setAccessor);

		bool IsCopyableTo(PropertyInfo property, MethodInfo getAccessor, MethodInfo setAccessor);

	}

}