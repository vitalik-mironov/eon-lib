using System;
using System.Reflection;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon {

	public sealed class DelegatedDependencyScopedComponentFactory<TComponent>
		:IDependencyScopedComponentFactory<TComponent>
		where TComponent : class {

		readonly DependencyScopedComponentFactory<TComponent> _factory;

		public DelegatedDependencyScopedComponentFactory(DependencyScopedComponentFactory<TComponent> factory) {
			factory.EnsureNotNull(nameof(factory));
			//
			_factory = factory;
		}

		public bool CanCreate(IServiceProvider serviceProvider = default, IDependencySupport dependencies = default)
			=> true;

		// TODO: Put strings into the resources.
		//
		public TComponent Create(IServiceProvider serviceProvider = default, IDependencySupport dependencies = default, IContext ctx = default) {
			try {
				var result = _factory(serviceProvider: serviceProvider, dependencies: dependencies);
				if (result is null)
					throw new EonException($"Factory method '{_factory.GetMethodInfo().FmtStr().GForMemberInfo()}' has returned invalid value '{result.FmtStr().GNLI2()}'.");
				else
					return result;
			}
			catch (Exception exception) {
				throw new EonException($"An exception occurred while factory the component '{typeof(TComponent)}'.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}", exception);
			}
		}

		public TaskWrap<TComponent> CreateAsync(IServiceProvider serviceProvider, IDependencySupport dependencies, IContext ctx = default)
			=> TaskWrap<TComponent>.Wrap(func: () => Create(serviceProvider: serviceProvider, dependencies: dependencies, ctx: ctx));

		ITaskWrap<TComponent> IOptionalFactory<IServiceProvider, IDependencySupport, TComponent>.CreateAsync(IServiceProvider arg1, IDependencySupport arg2, IContext ctx)
			=> CreateAsync(serviceProvider: arg1, dependencies: arg2, ctx: ctx);
	}

}