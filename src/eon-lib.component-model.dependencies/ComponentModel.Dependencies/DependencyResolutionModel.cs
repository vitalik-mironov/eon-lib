using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Eon.Collections;
using Eon.Resources.XResource;

namespace Eon.ComponentModel.Dependencies {
	using static XResourceUtilities;

	public class DependencyResolutionModel
		:Disposable, IDependencyResolutionModel {

		IVh<IDependencyHandler2>[ ] _handlerChain;

		IReadOnlySet<IDependencyHandler2> _handlerUniqueSet;

		public DependencyResolutionModel(IEnumerable<IVh<IDependencyHandler2>> handlerChainSource = default) {
			if (handlerChainSource is null) {
				_handlerChain = new IVh<IDependencyHandler2>[ 0 ];
				_handlerUniqueSet = SetReadOnlyWrap<IDependencyHandler2>.Empty;
			}
			else {
				var effectiveHandlerChain = new List<IVh<IDependencyHandler2>>();
				var handlerUniqueSet = new HashSet<IDependencyHandler2>(comparer: ReferenceEqualityComparer<IDependencyHandler2>.Instance);
				var indexCounter = -1;
				foreach (var handlerVh in handlerChainSource) {
					indexCounter++;
					var handler = handlerVh.Arg($"{nameof(handlerChainSource)}[{indexCounter:d}]").EnsureSelfAndValueNotNull().Value.Value;
					if (handlerUniqueSet.Add(item: handler))
						effectiveHandlerChain.Add(handlerVh);
				}
				_handlerChain = effectiveHandlerChain.ToArray();
				_handlerUniqueSet = new SetReadOnlyWrap<IDependencyHandler2>(set: handlerUniqueSet);
			}
		}

		public DependencyResolutionModel(IDependencyHandler2 handler = default, bool ownsHandler = default)
			: this(handlerChainSource: handler is null ? null : new[ ] { handler.ToValueHolder(ownsHandler) }) { }

		public DependencyResolutionModel(IEnumerable<IDependencyHandler2> handlerChainSource = default, bool ownsHandlers = default)
			: this(handlerChainSource: handlerChainSource?.Select(locItem => locItem.ToValueHolder(ownsHandlers))) { }

		public bool IsReadOnly
			=> true;

		public int Count
			=> ReadDA(ref _handlerChain).Length;

		public IDependencyHandler2 this[ int index ]
			=> ReadDA(ref _handlerChain[ index ])?.Value;

		public void Add(IDependencyHandler2 handler) {
			handler.EnsureNotNull(nameof(handler));
			//
			throw new EonException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));
		}

		public void Clear()
			=> throw new EonException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));

		public bool Contains(IDependencyHandler2 handler)
			=> handler is null ? false : ReadDA(ref _handlerUniqueSet).Contains(handler);

		public void CopyTo(IDependencyHandler2[ ] array, int arrayIndex)
			=> ReadDA(ref _handlerUniqueSet).CopyTo(array, arrayIndex);

		public bool Remove(IDependencyHandler2 handler)
			=> throw new EonException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));

		public IEnumerator<IDependencyHandler2> GetEnumerator()
			=> EnumerateDA(_handlerChain).Select(i => i.Value).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_handlerChain?.DisposeMany();
				_handlerChain?.ClearArray();
			}
			_handlerChain = null;
			_handlerUniqueSet = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}