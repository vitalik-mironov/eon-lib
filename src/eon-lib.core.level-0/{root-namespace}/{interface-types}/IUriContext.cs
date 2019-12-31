using System;

namespace Eon {

	public interface IUriContext {

		Uri BaseUri { get; }

		Uri RelativeUri { get; }

		Uri AbsoluteUri { get; }

		string AbsolutePathUnescaped { get; }

	}

}