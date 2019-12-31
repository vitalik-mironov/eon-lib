namespace Eon {

	public sealed class WrappedStruct<T>
		where T : struct {

		public readonly T Value;

		public WrappedStruct(T value) {
			Value = value;
		}

		public static implicit operator WrappedStruct<T>(T value)
			=> new WrappedStruct<T>(value);

		public static implicit operator T? (WrappedStruct<T> wrappedStruct)
			=> wrappedStruct == null ? (T?)null : wrappedStruct.Value;

		public static implicit operator T(WrappedStruct<T> wrappedStruct)
			=> wrappedStruct is null ? default : wrappedStruct.Value;

	}

}