namespace Eon {

	public readonly struct FluentApiUtilitiesHandle<T> {

		#region Static & constant members

		public static implicit operator T(FluentApiUtilitiesHandle<T> hnd) => hnd.Value;

		#endregion

		public readonly T Value;

		public FluentApiUtilitiesHandle(T value) {
			Value = value;
		}

	}

}