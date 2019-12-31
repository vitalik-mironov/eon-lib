using Eon;
using Eon.Description;

namespace Microsoft.Extensions.Hosting {

	public static class DefaultGenericHostBuilderUtilities {

		public static IHostBuilder UseDefaults(this IHostBuilder builder, string[ ] commandLineArgs) {
			builder.EnsureNotNull(name: nameof(builder));
			//
			DefaultHostConfigureService.Default.UseDefaults(builder: builder, commandLineArgs: commandLineArgs);
			return builder;
		}

		public static IHostBuilder UseXApp(this IHostBuilder builder, DescriptionLocator locator = default) {
			builder.EnsureNotNull(nameof(builder));
			//
			builder.ConfigureServices(configureDelegate: (locHostBuilder, locSvcs) => DefaultHostConfigureService.Default.UseXApp(svcs: locSvcs, locator: locator));
			return builder;
		}

	}

}