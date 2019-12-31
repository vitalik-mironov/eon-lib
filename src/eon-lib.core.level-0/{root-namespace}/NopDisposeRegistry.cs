using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Eon {

	public sealed class NopDisposeRegistry
		:IDisposeRegistry {

		#region Static members

		public static NopDisposeRegistry Instance = new NopDisposeRegistry();

		#endregion

		NopDisposeRegistry() { }

		public void Register(IDisposable disposable) {
			// Замечание: Контракт метода должен соблюдаться вне зависимости от внутренней реализации типа.
			//
			if (disposable is null)
				throw new ArgumentNullException(paramName: nameof(disposable));
			//
		}

		public IReadOnlyList<IDisposable> GetAlive(bool disposeTolerant)
			=> ImmutableList<IDisposable>.Empty;

		public void Dispose() { }

	}

}