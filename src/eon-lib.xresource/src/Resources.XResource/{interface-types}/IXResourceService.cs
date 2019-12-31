using System;
using System.Collections.Generic;
using System.Globalization;

namespace Eon.Resources.XResource {

	public interface IXResourceService {

		bool EnsureLocatorValid(Type locator, bool notThrow = default);

		Type GetLocator(Type type);

		bool TryGetLocator(Type type, out Type path);

		IXResourceSourcePointer GetSource(Type locator);

		bool Locate(Type locator, CultureInfo culture, out IXResourcePointer pointer, string subpath = default);

		string Format(Type locator, CultureInfo culture = default, string subpath = default, bool throwIfMissing = default, IEnumerable<object> args = default);

		string FormatUI(Type locator, string subpath = default, bool throwIfMissing = default, IEnumerable<object> args = default);

		string Format(IXResourcePointer pointer, IEnumerable<object> args = default);

	}

}