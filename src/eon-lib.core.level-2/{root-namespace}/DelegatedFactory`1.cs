using System;
using System.Reflection;

using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon {

	public sealed class DelegatedFactory<T>
		:IFactory<T> {

		readonly Func<T> _factory;

		public DelegatedFactory(Func<T> factory) {
			_factory = factory.EnsureNotNull(nameof(factory));
		}

		// TODO: Put strings into the resources.
		//
		public T Create(IContext ctx = default) {
			try {
				var result = _factory();
				if (result == null)
					throw new EonException($"Factory method '{_factory.GetMethodInfo().FmtStr().GForMemberInfo()}' has returned invalid value '{result.FmtStr().GNLI2()}'.");
				else
					return result;
			}
			catch (Exception exception) {
				throw new EonException($"An exception occurred while factory an object of type '{typeof(T)}'.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}", exception);
			}
		}

		public TaskWrap<T> CreateAsync(IContext ctx = default)
			=> TaskUtilities.Wrap(arg: ctx, func: Create);

		ITaskWrap<T> IFactory<T>.CreateAsync(IContext ctx)
			=> CreateAsync(ctx: ctx);

	}

}