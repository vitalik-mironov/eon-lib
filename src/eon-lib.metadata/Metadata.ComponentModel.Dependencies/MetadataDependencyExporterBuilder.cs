using System;
using System.Collections.Generic;

using Eon.Collections;
using Eon.ComponentModel.Dependencies;
using Eon.Linq;
using Eon.Threading;

namespace Eon.Metadata.ComponentModel.Dependencies {

	public class MetadataDependencyExporterBuilder
		:Disposable, IMetadataDependencyExporterBuilder {

		#region Nested types

		sealed class P_MetadataDependencyExporter
			:Disposable, IDependencyExporter {

			MetadataDependencyExporterBuilder _owner;

			IMetadata _metadata;

			internal P_MetadataDependencyExporter(MetadataDependencyExporterBuilder owner, IMetadata metadata) {
				owner.EnsureNotNull(nameof(owner));
				metadata.EnsureNotNull(nameof(metadata));
				//
				_owner = owner;
				_metadata = metadata;
			}

			// TODO_HIGH: Реализовать кэширование.
			//
			public IEnumerable<IVh<IDependencyHandler2>> ExportDependencies()
				=> ReadDA(ref _owner).ExportDependencies(metadata: ReadDA(ref _metadata));

			protected override void Dispose(bool explicitDispose) {
				_owner = null;
				_metadata = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		Dictionary<IMetadata, P_MetadataDependencyExporter> _exporters;

		PrimitiveSpinLock _exportersSpinLock;

		public MetadataDependencyExporterBuilder() {
			_exporters = new Dictionary<IMetadata, P_MetadataDependencyExporter>();
			_exportersSpinLock = new PrimitiveSpinLock();
		}

		public IVh<IDependencyExporter> BuildFor(IMetadata metadata) {
			metadata.EnsureNotNull(nameof(metadata));
			//
			var createdExporter = default(P_MetadataDependencyExporter);
			var existingExporter =
				ReadDA(ref _exporters)
				.GetOrAdd(
					spinLock: ReadDA(ref _exportersSpinLock),
					dictionaryOwner: this,
					key: metadata,
					factory: locKey => createdExporter = new P_MetadataDependencyExporter(owner: this, metadata: locKey),
					unclaimedValue: (locKey, locValue) => locValue?.Dispose())
				.ToValueHolder(ownsValue: false);
			if (ReferenceEquals(existingExporter, createdExporter))
				try {
					metadata.AfterDisposed += P_EH_description_AfterDispose;
				}
				catch (Exception exception) {
					try {
						if (TryReadDA(ref _exportersSpinLock, out var spinLock) && TryReadDA(ref _exporters, out var exporters))
							spinLock.Invoke(() => exporters.Remove(key: metadata));
						createdExporter.Dispose();
					}
					catch (Exception secondException) {
						throw new AggregateException(exception, secondException);
					}
					throw;
				}
			//
			return existingExporter;
		}

		void P_EH_description_AfterDispose(object sender, DisposeEventArgs e) {
			var metadata =
				sender
				.EnsureNotNull(nameof(sender))
				.EnsureOfType<IMetadata>()
				.Value;
			//
			if (TryReadDA(ref _exportersSpinLock, out var spinLock) && TryReadDA(ref _exporters, out var exporters)) {
				spinLock.Invoke(() => exporters.GetOrDefault(metadata))?.Dispose();
				spinLock.Invoke(() => exporters.Remove(metadata));
			}
		}

		protected virtual IEnumerable<IVh<IDependencyHandler2>> ExportDependencies(IMetadata metadata) {
			metadata.EnsureNotNull(nameof(metadata));
			//
			foreach (var executor in metadata.LocalDependencies())
				yield return executor;
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_exportersSpinLock?.EnterAndExitLock();
				_exporters?.Observe(i => i.Value.Dispose());
				_exporters?.Clear();
			}
			_exporters = null;
			_exportersSpinLock = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}