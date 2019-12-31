using System;

namespace Eon.Resources.XResource {

	public interface IXResourcePointer {

		Type Locator { get; }

		string Subpath { get; }

	}

}