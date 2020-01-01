using System.Threading;
using System.Threading.Tasks;

namespace Eon.IO {

	public interface ITextWriterPublisher
		:IEonDisposable {

		Task WriteAsync(string value, CancellationToken ct = default);

		Task WriteLineAsync(string value, CancellationToken ct = default);

	}

}