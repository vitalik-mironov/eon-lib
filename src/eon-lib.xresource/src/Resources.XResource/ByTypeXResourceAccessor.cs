using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Resources.XResource {

	public class ByTypeXResourceAccessor {

		readonly Type _type;

		readonly IXResourceService _provider;

		ValueHolderClass<ReadOnlyCollection<Type>> _locators;

		public ByTypeXResourceAccessor(Type type)
			: this(provider: null, type: type) { }

		protected internal ByTypeXResourceAccessor(Type type, IXResourceService provider) {
			if (type is null)
				throw new ArgumentNullException(paramName: nameof(type));
			//
			_type = type;
			_provider = provider;
		}

		public Type Type
			=> _type;

		protected internal IXResourceService GetProvider()
			=> _provider ?? XResourceUtilities.Service;

		public IReadOnlyList<Type> GetLocatorOrder() {
			var result = itrlck.Get(ref _locators);
			if (result is null) {
				var provider = XResourceUtilities.Service;
				try {
					var buffer = new List<Type>();
					var currentType = _type;
					do {
						provider.TryGetLocator(type: currentType, path: out var currentLocator);
						if (!(currentLocator is null || buffer.Contains(currentLocator)))
							buffer.Add(item: currentLocator);
						currentType = currentType.BaseType;
					}
					while (!(currentType is null));
					result = new ValueHolderClass<ReadOnlyCollection<Type>>(value: new ReadOnlyCollection<Type>(list: buffer));
				}
				catch (Exception exception) {
					result = new ValueHolderClass<ReadOnlyCollection<Type>>(exception: exception);
				}
				result = Interlocked.CompareExchange(location1: ref _locators, value: result, comparand: null) ?? result;
			}
			return result.Value;
		}

	}

}