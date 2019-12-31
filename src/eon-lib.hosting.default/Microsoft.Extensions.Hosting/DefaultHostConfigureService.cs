using System.IO;
using System.Linq;

using Eon;
using Eon.Description;
using Eon.Hosting;
using Eon.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting {

	/// <summary>
	/// Default implementation of the special service configuring the host (generic or web).
	/// </summary>
	public class DefaultHostConfigureService {

		#region Static & constant members

		public static readonly DefaultHostConfigureService Default = new DefaultHostConfigureService();

		#endregion

		public DefaultHostConfigureService() { }

		protected virtual string EnvironmentVariablePrefix
			=> "ASPNETCORE_";

		protected virtual string HostSettingsJsonFilePath
			=> "host-settings.json";

		protected virtual string AppSettingsJsonFilePath
			=> "app-settings.json";

		protected virtual bool UseDebugLogging
			=> true;

		protected virtual bool UseConsoleLifetime
			=> true;

		public virtual IHostBuilder UseDefaults(IHostBuilder builder, string[ ] commandLineArgs = default) {
			builder.EnsureNotNull(nameof(builder));
			//
			builder.ConfigureHostConfiguration(configureDelegate: locConfigBuilder => ConfigureHostConfiguration(hostConfig: locConfigBuilder, commandLineArgs: commandLineArgs));
			builder.ConfigureAppConfiguration(configureDelegate: (locHostBuilder, locAppConfig) => ConfigureAppConfiguration(hostBuilder: locHostBuilder, appConfig: locAppConfig, commandLineArgs: commandLineArgs));
			builder.ConfigureServices(configureDelegate: ConfigureServices);
			builder.ConfigureLogging(configureLogging: ConfigureLogging);
			if (UseConsoleLifetime)
				builder.UseConsoleLifetime();
			//
			return builder;
		}

		protected virtual void ConfigureHostConfiguration(IConfigurationBuilder hostConfig, string[ ] commandLineArgs = default) {
			hostConfig.EnsureNotNull(nameof(hostConfig));
			//
			hostConfig.SetBasePath(basePath: Directory.GetCurrentDirectory());
			hostConfig.AddJsonFile(path: HostSettingsJsonFilePath, optional: true);
			hostConfig.AddEnvironmentVariables(prefix: EnvironmentVariablePrefix);
			if (!(commandLineArgs is null))
				hostConfig.AddCommandLine(args: commandLineArgs);
		}

		protected virtual void ConfigureAppConfiguration(HostBuilderContext hostBuilder, IConfigurationBuilder appConfig, string[ ] commandLineArgs = default) {
			hostBuilder.EnsureNotNull(nameof(hostBuilder));
			appConfig.EnsureNotNull(nameof(appConfig));
			//
			var appSettingsJsonFilePath = AppSettingsJsonFilePath;
			appConfig.AddJsonFile(path: appSettingsJsonFilePath, optional: true);
			var hostingEnvironment = hostBuilder.HostingEnvironment.EnvironmentName;
			if (!string.IsNullOrWhiteSpace(hostingEnvironment))
				appConfig.AddJsonFile(Path.ChangeExtension(path: appSettingsJsonFilePath, extension: $"{hostingEnvironment.ToLowerInvariant()}.json"), optional: true);
			appConfig.AddEnvironmentVariables(prefix: EnvironmentVariablePrefix);
			if (!(commandLineArgs is null))
				appConfig.AddCommandLine(args: commandLineArgs);
		}

		protected virtual void ConfigureServices(HostBuilderContext hostBuilder, IServiceCollection svcs) {
			hostBuilder.EnsureNotNull(nameof(hostBuilder));
			svcs.EnsureNotNull(nameof(svcs));
			//
			svcs.AddDefaultXAppRuntimeServiceFactory();
			svcs.AddDefaultDescriptionPackageService();
		}

		protected virtual void ConfigureLogging(HostBuilderContext hostBuilder, ILoggingBuilder logging) {
			hostBuilder.EnsureNotNull(nameof(hostBuilder));
			logging.EnsureNotNull(nameof(logging));
			//
			logging.AddConsole();
			if (UseDebugLogging)
				logging.AddDebug();
		}

		public virtual IServiceCollection UseXApp(IServiceCollection svcs, DescriptionLocator locator = default) {
			svcs.EnsureNotNull(nameof(svcs));
			//
			var svcType = typeof(IXAppHostedService);
			if (!svcs.Select(locItem => locItem?.ServiceType).SkipNull().Any(locItem => svcType.IsAssignableFrom(c: locItem))) {
				svcs.AddTransient<IHostedService, DefaultXAppHostedService>(implementationFactory: locOuterSp => new DefaultXAppHostedService(serviceProvider: locOuterSp, locator: locator));
				if (locator is null)
					svcs.ConfigureSettingsFactory(factory: () => new CustomXAppDescriptionLocatorSettings(locator: CustomXAppDescriptionLocatorSettings.GetDefault().GetLocator(), isReadOnly: false));
				else
					svcs.ConfigureSettingsFactory(factory: () => new CustomXAppDescriptionLocatorSettings(locator: locator, isReadOnly: false));
			}
			return svcs;
		}

	}

}