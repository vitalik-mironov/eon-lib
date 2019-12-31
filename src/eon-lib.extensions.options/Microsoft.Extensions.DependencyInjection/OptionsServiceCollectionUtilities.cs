using System;

using Eon;
using Eon.Description;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection {

	public static class OptionsServiceCollectionUtilities {

		#region Nested types

		enum P_DelegatedOptionsFactoryMode {

			Default = 0,

			UseServiceProvider

		}

		sealed class P_DelegatedOptionsFactory<TOptions>
			:IOptionsFactory<TOptions>
			where TOptions : class, new() {

			readonly P_DelegatedOptionsFactoryMode _mode;

			readonly Func<TOptions> _factory1;

			readonly IServiceProvider _serviceProvider;

			readonly Func<IServiceProvider, TOptions> _factory2;

			internal P_DelegatedOptionsFactory(Func<TOptions> factory) {
				factory.EnsureNotNull(nameof(factory));
				//
				_mode = P_DelegatedOptionsFactoryMode.Default;
				_factory1 = factory;
				_serviceProvider = null;
			}

			internal P_DelegatedOptionsFactory(IServiceProvider serviceProvider, Func<IServiceProvider, TOptions> factory) {
				serviceProvider.EnsureNotNull(nameof(serviceProvider));
				factory.EnsureNotNull(nameof(factory));
				//
				_mode = P_DelegatedOptionsFactoryMode.UseServiceProvider;
				_serviceProvider = serviceProvider;
				_factory2 = factory;
			}

			public TOptions Create(string name) {
				switch (_mode) {
					case P_DelegatedOptionsFactoryMode.Default:
						return _factory1();
					case P_DelegatedOptionsFactoryMode.UseServiceProvider:
						return _factory2(_serviceProvider);
					default:
						throw new NotSupportedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
				}
			}

		}

		#endregion

		/// <summary>
		/// Replaces implementation of service <see cref="IOptionsFactory{TOptions}"/> (where TOptions — <typeparamref name="TSettings"/>) with delegated implementation <paramref name="factory"/>.
		/// </summary>
		/// <typeparam name="TSettings">Type of settings.</typeparam>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="factory">
		/// Method, implementing settings creation.
		/// </param>
		public static IServiceCollection ConfigureSettingsFactory<TSettings>(this IServiceCollection svcs, Func<TSettings> factory)
			where TSettings : class, ISettings, new() {
			svcs.EnsureNotNull(nameof(svcs));
			factory.EnsureNotNull(nameof(factory));
			//
			svcs.Configure<SettingsOptionsAccessor<TSettings>>(configureOptions: configure);
			svcs
				.Replace(
					descriptor:
						new ServiceDescriptor(
							serviceType: typeof(IOptionsFactory<TSettings>),
							factory:
								locSp => {
									var locAccessor = locSp.GetRequiredService<IOptions<SettingsOptionsAccessor<TSettings>>>();
									return new P_DelegatedOptionsFactory<TSettings>(factory: locAccessor.Value.Get);
								},
							lifetime: ServiceLifetime.Transient));
			return svcs;
			//
			void configure(SettingsOptionsAccessor<TSettings> locOptions) {
				locOptions.Update(transform: locCurrent => factory());
			}
		}

		/// <summary>
		/// Replaces implementation of service <see cref="IOptionsFactory{TOptions}"/> (where TOptions — <typeparamref name="TSettings"/>) with delegated implementation <paramref name="factory"/>.
		/// </summary>
		/// <typeparam name="TSettings">Type of settings.</typeparam>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="factory">
		/// Method, implementing settings creation.
		/// </param>
		public static IServiceCollection ConfigureSettingsFactory<TSettings>(this IServiceCollection svcs, Func<IServiceProvider, TSettings> factory)
			where TSettings : class, ISettings, new() {
			svcs.EnsureNotNull(nameof(svcs));
			factory.EnsureNotNull(nameof(factory));
			//
			svcs.AddTransient<IConfigureOptions<SettingsOptionsAccessor<TSettings>>>(implementationFactory: locSp => new ConfigureNamedOptions<SettingsOptionsAccessor<TSettings>, IServiceProvider>(name: Options.Options.DefaultName, dependency: locSp, action: configure));
			svcs
				.Replace(
					descriptor:
						new ServiceDescriptor(
							serviceType: typeof(IOptionsFactory<TSettings>),
							factory:
								locSp => {
									var locAccessor = locSp.GetRequiredService<IOptions<SettingsOptionsAccessor<TSettings>>>();
									return new P_DelegatedOptionsFactory<TSettings>(factory: locAccessor.Value.Get);
								},
							lifetime: ServiceLifetime.Transient));
			return svcs;
			//
			void configure(SettingsOptionsAccessor<TSettings> locOptions, IServiceProvider locSp) {
				locOptions.Update(transform: locCurrent => factory(locSp));
			}
		}

		/// <summary>
		/// Replaces implementation of service <see cref="IOptionsFactory{TOptions}"/> (where TOptions — <typeparamref name="TSettings"/>) with getting the settings from named configuration section.
		/// </summary>
		/// <typeparam name="TSettings">Type of settings.</typeparam>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="name">
		/// Configuration section name.
		/// <para>Can't be <see langword="null"/>.</para>
		/// <para>Can't be <see cref="string.Empty"/>.</para>
		/// </param>
		public static IServiceCollection ConfigureSettingsFactory<TSettings>(this IServiceCollection svcs, string name)
			where TSettings : class, ISettings, new() {
			svcs.EnsureNotNull(nameof(svcs));
			name.EnsureNotNull(nameof(name)).EnsureNotEmpty();
			//
			svcs.AddTransient<IConfigureOptions<SettingsOptionsAccessor<TSettings>>>(implementationFactory: locSp => new ConfigureNamedOptions<SettingsOptionsAccessor<TSettings>, IServiceProvider>(name: Options.Options.DefaultName, dependency: locSp, action: configure));
			svcs
				.Replace(
					descriptor:
						new ServiceDescriptor(
							serviceType: typeof(IOptionsFactory<TSettings>),
							factory:
								locSp => {
									var locAccessor = locSp.GetRequiredService<IOptions<SettingsOptionsAccessor<TSettings>>>();
									return new P_DelegatedOptionsFactory<TSettings>(factory: locAccessor.Value.Get);
								},
							lifetime: ServiceLifetime.Transient));
			return svcs;
			//
			void configure(SettingsOptionsAccessor<TSettings> locOptions, IServiceProvider locSp) {
				var locConfig = locSp.GetRequiredService<IConfiguration>();
				var locConfigSection = locConfig.RequireSection(name: name);
				var locSettings = new TSettings();
				locConfigSection.Bind(instance: locSettings);
				locOptions.Update(transform: locCurrent => locSettings);
			}
		}

		/// <summary>
		/// Replaces implementation of service <see cref="IOptionsFactory{TOptions}"/> (where TOptions — <typeparamref name="TSettings"/>) with getting the settings by deserializing <typeparamref name="TSettings"/> object from the named configuration section (using <see cref="ConfigurationUtilities.Deserialize{T}(IConfiguration,IServiceProvider)"/>).
		/// </summary>
		/// <typeparam name="TSettings">Type of settings.</typeparam>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="name">
		/// Configuration section name.
		/// <para>Can't be <see langword="null"/>.</para>
		/// <para>Can't be <see cref="string.Empty"/>.</para>
		/// </param>
		public static IServiceCollection ConfigureSettingsFactoryAsDeserialized<TSettings>(this IServiceCollection svcs, string name)
			where TSettings : class, ISettings, new() {
			svcs.EnsureNotNull(nameof(svcs));
			name.EnsureNotNull(nameof(name)).EnsureNotEmpty();
			//
			svcs.AddTransient<IConfigureOptions<SettingsOptionsAccessor<TSettings>>>(implementationFactory: locSp => new ConfigureNamedOptions<SettingsOptionsAccessor<TSettings>, IServiceProvider>(name: Options.Options.DefaultName, dependency: locSp, action: configure));
			svcs
				.Replace(
					descriptor:
						new ServiceDescriptor(
							serviceType: typeof(IOptionsFactory<TSettings>),
							factory:
								locSp => {
									var locAccessor = locSp.GetRequiredService<IOptions<SettingsOptionsAccessor<TSettings>>>();
									return new P_DelegatedOptionsFactory<TSettings>(factory: locAccessor.Value.Get);
								},
							lifetime: ServiceLifetime.Transient));
			return svcs;
			//
			void configure(SettingsOptionsAccessor<TSettings> locOptions, IServiceProvider locSp) {
				var locConfigSection = locSp.GetRequiredService<IConfiguration>().RequireSection(name: name);
				var locSettings = locConfigSection.Deserialize<TSettings>(serviceProvider: locSp);
				locOptions.Update(transform: locCurrent => locSettings);
			}
		}

		/// <summary>
		/// Replaces implementation of service <see cref="IOptionsFactory{TOptions}"/> (where TOptions — <typeparamref name="TSettings"/>) with getting the settings by deserializing <typeparamref name="TSettings"/> object from the named configuration section (using <see cref="ConfigurationUtilities.Deserialize{T}(IConfiguration,IServiceProvider)"/>).
		/// </summary>
		/// <typeparam name="TSettings">Type of settings.</typeparam>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="name">
		/// Configuration section name.
		/// <para>Can't be <see langword="null"/>.</para>
		/// <para>Can't be <see cref="string.Empty"/>.</para>
		/// </param>
		/// <param name="configSettings">
		/// Creates/edits <see cref="JsonSerializerSettings"/> that will be used by <see cref="JsonSerializer"/>.
		/// </param>
		public static IServiceCollection ConfigureSettingsFactoryAsDeserializedJson<TSettings>(this IServiceCollection svcs, string name, Func<JsonSerializerSettings, JsonSerializerSettings> configSettings = default)
			where TSettings : class, ISettings, new() {
			svcs.EnsureNotNull(nameof(svcs));
			name.EnsureNotNull(nameof(name)).EnsureNotEmpty();
			//
			svcs.AddTransient<IConfigureOptions<SettingsOptionsAccessor<TSettings>>>(implementationFactory: locSp => new ConfigureNamedOptions<SettingsOptionsAccessor<TSettings>, IServiceProvider>(name: Options.Options.DefaultName, dependency: locSp, action: configure));
			svcs
				.Replace(
					descriptor:
						new ServiceDescriptor(
							serviceType: typeof(IOptionsFactory<TSettings>),
							factory:
								locSp => {
									var locAccessor = locSp.GetRequiredService<IOptions<SettingsOptionsAccessor<TSettings>>>();
									return new P_DelegatedOptionsFactory<TSettings>(factory: locAccessor.Value.Get);
								},
							lifetime: ServiceLifetime.Transient));
			return svcs;
			//
			void configure(SettingsOptionsAccessor<TSettings> locOptions, IServiceProvider locSp) {
				var locConfigSection = locSp.GetRequiredService<IConfiguration>().RequireSection(name: name);
				var locSettings = locConfigSection.DeserializeJson<TSettings>(configSettings: configSettings, serviceProvider: locSp);
				locOptions.Update(transform: locCurrent => locSettings);
			}
		}

		/// <summary>
		/// Replaces implementation of service <see cref="IOptionsFactory{TOptions}"/> with delegated implementation <paramref name="factory"/>.
		/// </summary>
		/// <typeparam name="TOptions">Type of options.</typeparam>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="factory">
		/// Method, implementing options creation.
		/// </param>
		public static IServiceCollection UseOptionsFactory<TOptions>(this IServiceCollection svcs, Func<TOptions> factory)
			where TOptions : class, new() {
			svcs.EnsureNotNull(nameof(svcs));
			factory.EnsureNotNull(nameof(factory));
			//
			svcs.Replace(descriptor: new ServiceDescriptor(serviceType: typeof(IOptionsFactory<TOptions>), factory: locSp => new P_DelegatedOptionsFactory<TOptions>(factory: factory), lifetime: ServiceLifetime.Transient));
			return svcs;
		}

		/// <summary>
		/// Replaces implementation of service <see cref="IOptionsFactory{TOptions}"/> with delegated implementation <paramref name="factory"/>.
		/// </summary>
		/// <typeparam name="TOptions">Type of options.</typeparam>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="factory">
		/// Method, implementing options creation.
		/// </param>
		public static IServiceCollection UseOptionsFactory<TOptions>(this IServiceCollection svcs, Func<IServiceProvider, TOptions> factory)
			where TOptions : class, new() {
			svcs.EnsureNotNull(nameof(svcs));
			factory.EnsureNotNull(nameof(factory));
			//
			svcs.Replace(descriptor: new ServiceDescriptor(serviceType: typeof(IOptionsFactory<TOptions>), factory: locSp => new P_DelegatedOptionsFactory<TOptions>(serviceProvider: locSp, factory: factory), lifetime: ServiceLifetime.Transient));
			return svcs;
		}

	}

}