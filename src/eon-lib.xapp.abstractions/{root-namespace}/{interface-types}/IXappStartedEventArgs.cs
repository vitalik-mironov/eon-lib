using Eon.Description;

namespace Eon {
	using IXApp = IXApp<IXAppDescription>;

	public interface IXAppStartedEventArgs {

		IXApp App { get; }

	}

}