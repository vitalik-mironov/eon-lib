using System;
using System.Globalization;
using System.IO;

namespace Eon.Resources.XResource {

	public interface IXResourceSourcePointer 
		:IEquatable<IXResourceSourcePointer> {

		Stream GetStream(CultureInfo culture, bool throwIfMissing = default);

	}

}