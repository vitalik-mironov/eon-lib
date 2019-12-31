using System.Collections.Generic;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.Triggers.Description;

namespace Eon.Triggers {
	using ITriggerXInstance = ITrigger<ITriggerDescription>;

	public static class TriggerFactoryUtilities {

		public static Task<IList<ITriggerXInstance>> CreateInitializeAsync(ArgumentUtilitiesHandle<IEnumerable<ITriggerDescription>> descriptions, IXAppScopeInstance scope, IContext ctx = default)
			=> XInstanceFactoryUtilities.CreateInitializeAppScopeInstanceAsync<ITriggerXInstance>(scope: scope, ctx: ctx, descriptions: descriptions.AsBase<IEnumerable<ITriggerDescription>, IEnumerable<IDescription>>());

	}

}