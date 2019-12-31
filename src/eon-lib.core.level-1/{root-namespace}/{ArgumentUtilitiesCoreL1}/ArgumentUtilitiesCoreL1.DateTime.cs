using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<DateTime?> EnsureHasZeroTime(this ArgumentUtilitiesHandle<DateTime?> hnd) {
			if (hnd.Value.HasValue && hnd.Value.Value.TimeOfDay != TimeSpan.Zero)
				throw new ArgumentException(string.Format("Указанное значение '{0}' содержит ненулевой компонент времени. Ожидалось значение с нулевым компонентом времени.", hnd.Value.Value.ToString("O")), hnd.Name);
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<DateTime> EnsureHasZeroTime(this ArgumentUtilitiesHandle<DateTime> hnd) {
			if (hnd.Value.TimeOfDay != TimeSpan.Zero)
				throw new ArgumentException(string.Format("Указанное значение '{0}' содержит ненулевой компонент времени. Ожидалось значение с нулевым компонентом времени.", hnd.Value.ToString("O")), hnd.Name);
			return hnd;
		}

		public static ArgumentUtilitiesHandle<DateTime> EnsureSameKind(this ArgumentUtilitiesHandle<DateTime> hnd, ArgumentUtilitiesHandle<DateTime> other) {
			DateTimeUtilities.EnsureSameKind(hnd.Value, other.Value, altDateAArgName: hnd.Name, altDateBArgName: other.Name);
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<DateTime> EnsureNotLessThan(this ArgumentUtilitiesHandle<DateTime> hnd, ArgumentUtilitiesHandle<DateTime> other) {
			if (hnd.Value < other.Value)
				throw new ArgumentException(FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan/OtherArg", other.Value.ToString("O"), other.Name));
			return hnd;
		}

	}

}