using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Eon.Diagnostics;
using Eon.Threading;
using Eon.Threading.Tasks;

namespace Eon.IO {

	public class TextWriterPublisher
		:Disposable, ITextWriterPublisher {

		TextWriter _writer;

		readonly bool _ownsWriter;

		StatefulSynchronizationContext<Nil> _syncCtx;

		public TextWriterPublisher(TextWriter writer, bool ownsWriter = default, IUnhandledExceptionObserver unhandledExceptionObserver = default) {
			writer.EnsureNotNull(nameof(writer));
			//
			_writer = writer;
			_ownsWriter = ownsWriter;
			_syncCtx = new StatefulSynchronizationContext<Nil>(stateFactory: locCt => VoidVh.Instance, stopToken: default, correlationId: default, autoStart: true, unhandledExceptionObserver: unhandledExceptionObserver);
		}

		TextWriter P_Writer
			=> ReadDA(ref _writer);

		public async Task WriteAsync(string value, CancellationToken ct = default) {
			if (!string.IsNullOrEmpty(value))
				ReadDA(location: ref _syncCtx)
					.InvokeOneWay(
						action:
							locArgs => {
								var locWriter = TryReadDA(location: ref _writer, considerDisposeRequest: true);
								if (!(locWriter is null) && !locArgs.Ct.IsCancellationRequested)
									locWriter.Write(value: value);
							},
						ct: ct);
			await Task.CompletedTask;
		}

		public async Task WriteLineAsync(string value, CancellationToken ct = default) {
			if (!string.IsNullOrEmpty(value))
				ReadDA(location: ref _syncCtx)
					.InvokeOneWay(
						action:
							locArgs => {
								var locWriter = TryReadDA(location: ref _writer, considerDisposeRequest: true);
								if (!(locWriter is null) && !locArgs.Ct.IsCancellationRequested)
									locWriter.WriteLine(value: value);
							},
						ct: ct);
			await Task.CompletedTask;
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				TryReadDA(location: ref _syncCtx)?.RunControl.StopAsync(finiteStop: true).WaitWithTimeout();
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_syncCtx?.Dispose();
				if (_ownsWriter)
					_writer?.Dispose();
			}
			_syncCtx = null;
			_writer = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}
