using System;
using System.Threading.Tasks;
using Eon.ComponentModel;

namespace Eon {
#pragma warning disable CS3027 // Type is not CLS-compliant because base interface is not CLS-compliant

	public interface IAsyncProgress<T>
		:IDisposable, IProgress<T> {

		IAsyncEventSubscribeManager<AsyncProgressReportedEventArgs<T>> ReportedEvent { get; }

		Task<bool> ReportAsync(T value);

	}

#pragma warning restore CS3027 // Type is not CLS-compliant because base interface is not CLS-compliant
}