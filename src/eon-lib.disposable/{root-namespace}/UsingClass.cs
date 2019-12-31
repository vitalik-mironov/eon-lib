namespace Eon {

	public static class UsingClass {

		public static UsingClass<T> ToClass<T>(this Using<T> @using)
			where T : class
			=> new UsingClass<T>(@using: ref @using);

	}

}