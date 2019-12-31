using System.Threading;
using System.Threading.Tasks;

namespace Eon.IO {

	public interface ITextWriterPublisher
		:IOxyDisposable {

		Task WriteAsync(string value, CancellationToken ct = default);

		Task WriteLineAsync(string value, CancellationToken ct = default);

	}

}