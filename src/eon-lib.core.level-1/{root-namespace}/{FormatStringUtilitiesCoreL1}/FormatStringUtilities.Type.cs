using System;
using System.Reflection;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string G(this FormatStringUtilitiesHandle<Type> hnd)
			=> P_GeneralForMemberInfo(memberInfo: hnd.Object?.GetTypeInfo());

		public static string GNLI(this FormatStringUtilitiesHandle<Type> hnd)
			=> Environment.NewLine + hnd.G().IndentLines();

		public static string GNLI2(this FormatStringUtilitiesHandle<Type> hnd)
			=> Environment.NewLine + hnd.G().IndentLines2();

	}

}