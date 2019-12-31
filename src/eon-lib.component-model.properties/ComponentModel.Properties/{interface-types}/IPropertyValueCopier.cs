using System;

using Eon.Context;

namespace Eon.ComponentModel.Properties {

	public interface IPropertyValueCopier
		:IDisposable {

		int CopyValuesOf<T>(T from, ref T to, IPropertyValueCopierConfiguration configuration = default, IContext ctx = default);

	}

}