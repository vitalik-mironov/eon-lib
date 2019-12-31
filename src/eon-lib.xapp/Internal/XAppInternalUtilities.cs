using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;

namespace Eon.Internal {
	using IXApp = IXApp<IXAppDescription>;

	internal static class XAppInternalUtilities {

		public static async Task FollowStartupContextHostHintsAsync(XAppStartupContextHostHints hints, IXApp app, IContext ctx = default) {
			app.EnsureNotNull(nameof(app));
			//
			if ((hints & XAppStartupContextHostHints.InitializeApp) == XAppStartupContextHostHints.InitializeApp) {
				// Инициализировать экземпляр приложения.
				//
				await app.InitializeAsync(ctx: ctx).ConfigureAwait(false);
				if ((hints & XAppStartupContextHostHints.RunApp) == XAppStartupContextHostHints.RunApp) {
					// Запустить приложение.
					//
					await app.RunControl.StartAsync(options: TaskCreationOptions.None, ctx: ctx).ConfigureAwait(false);
				}
			}
		}

	}

}