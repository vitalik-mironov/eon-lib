namespace Eon {

	public sealed class NopDisposable
		:Disposable {

		#region Static members

		public static NopDisposable Instance = new NopDisposable();

		#endregion

		NopDisposable() 
			: base(nopDispose: true) { }

	}

}