using System;

using Eon.ComponentModel.Dependencies;
using Eon.Description;

namespace Eon {

	public class XAppCtorArgs<TDescription>
		:IXAppCtorArgs<TDescription>
		where TDescription : class, IXAppDescription {

		readonly TDescription _description;

		readonly IXAppContainerControl _containerControl;

		readonly IServiceProvider _outerServiceProvider;

		readonly IDependencySupport _outerDependencies;

		public XAppCtorArgs(TDescription description, IXAppContainerControl containerControl = default, IServiceProvider outerServiceProvider = default, IDependencySupport outerDependencies = default) {
			description.EnsureNotNull(nameof(description));
			//
			_description = description;
			_containerControl = containerControl;
			_outerServiceProvider = outerServiceProvider;
			_outerDependencies = outerDependencies;
		}

		public TDescription Description
			=> _description;

		public IXAppContainerControl ContainerControl
			=> _containerControl;

		public IServiceProvider OuterServiceProvider
			=> _outerServiceProvider;

		public IDependencySupport OuterDependencies
			=> _outerDependencies;

	}

}