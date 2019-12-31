using System;
using System.Reflection;

using static Eon.Reflection.MemberText;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string G(this FormatStringUtilitiesHandle<ConstructorInfo> hnd, IFormatProvider formatProvider)
			=> hnd.Object is null ? GetNullValueText(formatProvider: formatProvider) : P_GeneralForMemberInfo(memberInfo: hnd.Object, overrideMemberInfoText: GetConstructorSignatureText(constructor: hnd.Object), formatProvider: formatProvider);

		public static string G(this FormatStringUtilitiesHandle<ConstructorInfo> hnd)
			=> hnd.Object is null ? GetNullValueText(formatProvider: null) : P_GeneralForMemberInfo(memberInfo: hnd.Object, overrideMemberInfoText: GetConstructorSignatureText(constructor: hnd.Object), formatProvider: null);

		public static string GNLI(this FormatStringUtilitiesHandle<ConstructorInfo> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: formatProvider).IndentLines();

		public static string GNLI2(this FormatStringUtilitiesHandle<ConstructorInfo> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: formatProvider).IndentLines2();


	}

}