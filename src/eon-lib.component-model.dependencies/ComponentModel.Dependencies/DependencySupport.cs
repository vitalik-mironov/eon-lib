using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using static Eon.DisposableUtilities;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Defines a component having own dependency scope (see <see cref="IDependencyScope"/>, <see cref="GetDependencyScope"/>).
	/// </summary>
	[DataContract]
	public abstract class DependencySupport
		:DisposeNotifying, IDependencySupport {

		#region Nested types

		sealed class P_OuterDependencyScopeGetter
			:IOuterDependencyScopeGetter {

			readonly Func<IDependencyScope> _getter;

			internal P_OuterDependencyScopeGetter(Func<IDependencyScope> getter) {
				getter.EnsureNotNull(nameof(getter));
				//
				_getter = getter;
			}

			public IDependencyScope GetOuterScope()
				=> _getter();
		}

		sealed class P_DelegatedDependencyExporter
			:Disposable, IDependencyExporter {

			Func<IEnumerable<IVh<IDependencyHandler2>>> _export;

			internal P_DelegatedDependencyExporter(Func<IEnumerable<IVh<IDependencyHandler2>>> export) {
				export.EnsureNotNull(nameof(export));
				//
				_export = export;
			}

			public IEnumerable<IVh<IDependencyHandler2>> ExportDependencies()
				=> ReadDA(ref _export)();

			protected override void Dispose(bool explicitDispose) {
				_export = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		IDependencySupport _outerDependencies;

		DisposableLazy<IDependencyScope> _dependencyScopeLazy;

		ServiceProviderHandler _outerServiceProviderHandler;

		protected DependencySupport(IServiceProvider outerServiceProvider = default, IDependencySupport outerDependencies = default) {
			_outerDependencies = outerDependencies;
			_dependencyScopeLazy = new DisposableLazy<IDependencyScope>(factory: P_DependencyScopeLazyInitializer);
			_outerServiceProviderHandler = outerServiceProvider is null ? null : new ServiceProviderHandler(serviceProvider: outerServiceProvider);
		}

		protected IDependencySupport OuterDependencies
			=> ReadDA(location: ref _outerDependencies);

		protected IServiceProvider OuterServiceProvider
			=> ReadDA(location: ref _outerServiceProviderHandler)?.ServiceProvider;

		IDependencyScope P_GetOuterDependencyScope()
			=> OuterDependencies?.GetDependencyScope();

		// TODO: Put strings into the resources.
		//
		IDependencyScope P_DependencyScopeLazyInitializer() {
			var exporter = default(IVh<IDependencyExporter>);
			var scope = default(IDependencyScope);
			try {
				GetOuterDependencyScopeGetter(getter: out var outerScopeGetter);
				if (outerScopeGetter is null)
					throw new EonException(message: $"Поставщик внешней (окружающей) области функциональной зависимсоти не создан. Операция создания возвратила '{outerScopeGetter.FmtStr().GI2()}'.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
				//
				BuildDependencyExporter(outerScopeGetter: outerScopeGetter, exporter: out exporter);
				if (exporter is null)
					throw new EonException(message: $"Экспортер функциональных зависимостей для области зависимости не был построен. Операция построения возвратила '{exporter.FmtStr().GI2()}'.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
				//
				BuildDependencyScope(outerScopeGetter: outerScopeGetter, exporter: exporter.Value, ownsExporter: exporter.OwnsValue, scope: out scope);
				if (scope is null)
					throw new EonException(message: $"Область функциональной зависимости не создана. Операция создания возвратила '{scope.FmtStr().G()}'.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
				//
				exporter.RemoveValue();
				exporter.Dispose();
				//
				return scope;
			}
			catch (Exception exception) {
				DisposeMany(exception: exception, disposables: new IDisposable[ ] { scope, exporter });
				throw;
			}
		}

		protected virtual void BuildDependencyScope(IOuterDependencyScopeGetter outerScopeGetter, IDependencyExporter exporter, bool ownsExporter, out IDependencyScope scope)
			=> scope = new DependencyScope(outerScopeGetter: outerScopeGetter, exporter: exporter, ownsExporter: ownsExporter, owner: this);

		protected virtual void GetOuterDependencyScopeGetter(out IOuterDependencyScopeGetter getter)
			=> getter = new P_OuterDependencyScopeGetter(getter: P_GetOuterDependencyScope);

		protected virtual void BuildDependencyExporter(IOuterDependencyScopeGetter outerScopeGetter, out IVh<IDependencyExporter> exporter)
			=> exporter = new P_DelegatedDependencyExporter(export: LocalDependencies).ToValueHolder(ownsValue: true);

		public virtual IEnumerable<IVh<IDependencyHandler2>> LocalDependencies() {
			var spHandler = ReadDA(ref _outerServiceProviderHandler);
			if (!(spHandler is null))
				yield return spHandler.ToValueHolder(ownsValue: false);
		}

		public IDependencyScope GetDependencyScope()
			=> ReadDA(ref _dependencyScopeLazy).Value;

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			_dependencyScopeLazy = new DisposableLazy<IDependencyScope>(factory: P_DependencyScopeLazyInitializer);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_dependencyScopeLazy?.Dispose();
				_outerServiceProviderHandler?.Dispose();
			}
			_dependencyScopeLazy = null;
			_outerServiceProviderHandler = null;
			_outerDependencies = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}