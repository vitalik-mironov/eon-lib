using System;

namespace Eon.Resources.XResource {

	public class XResourcePointer
		:IXResourcePointer {

		readonly Type _locator;

		readonly string _subpath;

		public XResourcePointer(Type locator, string subpath = default) {
			if (locator is null)
				throw new ArgumentNullException(paramName: nameof(locator));
			//
			_locator = locator;
			_subpath = subpath;
		}

		public Type Locator
			=> _locator;

		public string Subpath
			=> _subpath;

	}

}