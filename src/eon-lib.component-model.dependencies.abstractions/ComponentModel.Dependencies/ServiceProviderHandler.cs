using System;

using Microsoft.Extensions.DependencyInjection;

namespace Eon.ComponentModel.Dependencies {

	public class ServiceProviderHandler
		:Disposable, IServiceProviderHandler {

		readonly bool _hasServiceScope;

		IServiceProvider _serviceProvider;

		IVh<IServiceScope> _serviceScope;

		public ServiceProviderHandler(IServiceProvider serviceProvider)
			: this(serviceProvider: serviceProvider.Arg(nameof(serviceProvider))) { }

		public ServiceProviderHandler(ArgumentUtilitiesHandle<IServiceProvider> serviceProvider) {
			serviceProvider.EnsureNotNull();
			//
			_serviceProvider = serviceProvider.Value;
			_serviceScope = null;
			_hasServiceScope = false;
		}

		public ServiceProviderHandler(IServiceScope serviceScope, bool ownsServiceScope = default)
			: this(serviceScope: serviceScope.Arg(nameof(serviceScope)), ownsServiceScope: ownsServiceScope) { }

		public ServiceProviderHandler(ArgumentUtilitiesHandle<IServiceScope> serviceScope, bool ownsServiceScope = default) {
			var locServiceScope = serviceScope.EnsureNotNull().Value;
			var locServiceProvider = locServiceScope.ServiceProvider.ArgProp($"{nameof(serviceScope)}.{nameof(serviceScope.Value.ServiceProvider)}").EnsureNotNull().Value;
			//
			_serviceProvider = locServiceProvider;
			_serviceScope = locServiceScope.ToValueHolder(ownsValue: ownsServiceScope);
			_hasServiceScope = true;
		}

		public IServiceProvider ServiceProvider
			=> ReadDA(location: ref _serviceProvider);

		public bool HasServiceScope
			=> _hasServiceScope;

		public IServiceScope ServiceScope
			=> ReadDA(location: ref _serviceScope).Value;

		public bool CanShareDependency {
			get {
				EnsureNotDisposeState();
				return true;
			}
		}

		public virtual bool CanRedirect {
			get {
				EnsureNotDisposeState();
				return false;
			}
		}

		// TODO: Put strings into the resources.
		//
		public virtual DependencyResult ExecuteResolution(IDependencyResolutionContext resolutionCtx) {
			resolutionCtx.EnsureNotNull(nameof(resolutionCtx));
			//
			if (resolutionCtx.Specs.IsNewInstanceRequired)
				return DependencyResult.None;
			else {
				var scope = _hasServiceScope ? ServiceScope : null;
				var sp = scope is null ? ServiceProvider : scope.ServiceProvider;
				var instance = rethrowSpException(locOp: locSp => locSp.GetService(serviceType: resolutionCtx.Specs.DependencyType), locSp: sp, locScope: scope);
				if (instance is null)
					return DependencyResult.None;
				else
					return new DependencyResult(instance: instance, isNewInstance: false);
			}
			//
			object rethrowSpException(Func<IServiceProvider, object> locOp, IServiceProvider locSp, IServiceScope locScope) {
				try {
					return locOp(arg: locSp);
				}
				catch (Exception exception) {
					throw
						new EonException(
							message: $"The service provider has thrown an exception while handling the dependency resolution request.{Environment.NewLine}Service provider:{locSp.FmtStr().GNLI2()}{(locScope is null ? string.Empty : $"{Environment.NewLine}Service scope:{locScope.FmtStr().GNLI2()}")}{Environment.NewLine}Component:{this.FmtStr().GNLI2()}",
							innerException: exception)
						.SetErrorCode(code: DependencyErrorCodes.Fault);
				}
			}
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_serviceScope?.Dispose();
			_serviceProvider = null;
			_serviceScope = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}