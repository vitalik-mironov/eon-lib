using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalFlare.Resources.XResource.Tests {

	[TestClass]
	public class DefaultXResourceServiceProviderTests {

		#region Static & constant members

		static readonly IXResourceServiceProvider __Provider;

		static DefaultXResourceServiceProviderTests() {
			__Provider = new DefaultXResourceServiceProvider();
		}

		#endregion

		[TestMethod]
		public void Supply() {
			XResourceUtilities.SupplyProvider(provider: __Provider);
			Assert.AreSame(expected: __Provider, actual: XResourceUtilities.Provider);
		}

		[TestMethod]
		public void BasicTextResourceReading() {
			const string expectedMessageA_ru = "–азмер указанной строки слишком велик.";
			const string expectedMessageA_en = "The specified string is too long.";
			const string subpathA = "TooLong";
			const int messageBFormatArg0 = 10;
			const string expectedMessageB_en = "The specified string is too long. The max. allowed length is '10'.";
			const string subpathB = subpathA + "/MaxLength";
			//
			var enCulture = CultureInfo.GetCultureInfo(name: "en");
			var enCultureText = __Provider.Format(locator: typeof(string), culture: enCulture, subpath: subpathA, throwIfMissing: false, args: null);
			Assert.AreEqual(expected: expectedMessageA_en, actual: enCultureText);
			//
			var ruCulture = CultureInfo.GetCultureInfo(name: "ru");
			var ruCultureText = __Provider.Format(locator: typeof(string), culture: ruCulture, subpath: subpathA, throwIfMissing: false, args: null);
			Assert.AreEqual(expected: expectedMessageA_ru, actual: ruCultureText);
			//
			enCultureText = __Provider.Format(locator: typeof(string), culture: enCulture, subpath: subpathB, throwIfMissing: false, args: new object[ ] { messageBFormatArg0.ToString() });
			Assert.AreEqual(expected: expectedMessageB_en, actual: enCultureText);
		}

	}
}
