namespace Eon {

	public static class VoidVh {

		public static readonly IVh<Nil> Instance = Nil.Value.ToValueHolder(ownsValue: false, nopDispose: true);

	}

}