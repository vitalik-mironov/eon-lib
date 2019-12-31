using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Eon.Data.EfCore.Internal {

	public sealed class NopConcurrencyDetector
		:Disposable, IConcurrencyDetector {

		public NopConcurrencyDetector() { }

		public IDisposable EnterCriticalSection()
			=> NopDisposable.Instance;

		public async Task<IDisposable> EnterCriticalSectionAsync(CancellationToken ct) {
			await Task.CompletedTask;
			return NopDisposable.Instance;
		}

	}

}