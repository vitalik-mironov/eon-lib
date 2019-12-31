using System.Collections.Generic;

using Eon.Description;

namespace Eon {

	public interface IActivationList<out TDescription>
		:IXAppScopeInstance<TDescription>, IActivatableXAppScopeInstance<IActivationList<TDescription>>
		where TDescription : class, IActivationListDescription {

		IEnumerable<IActivatableXAppScopeInstance> ActivatableItems { get; }

	}

}