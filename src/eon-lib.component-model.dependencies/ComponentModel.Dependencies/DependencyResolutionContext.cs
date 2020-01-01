#define DO_NOT_USE_EON_LOGGING_API

using System;
using System.Collections.Generic;
using System.Text;

using Eon.Collections;

#if !DO_NOT_USE_EON_LOGGING_API

using Eon.Diagnostics.Logging;

#endif
using Eon.Threading;

namespace Eon.ComponentModel.Dependencies {

	public class DependencyResolutionContext
		:Disposable, IDependencyResolutionContext {

		readonly long _sequentialId;

		IDependencyScope _scope;

		IVh<IDependencyResolutionSpecs> _specs;

		IDictionary<IDependencyHandler2, int> _involvedHandlerCounters;

		List<IDependencyHandler2> _involvedHandlerChain;

		PrimitiveSpinLock _involvedHandlerSpinLock;

		readonly int _usageCountLimitPerHandler;

		readonly bool _isAdvancedLoggingEnabled;

		public DependencyResolutionContext(IDependencyScope scope, IDependencyResolutionSpecs specs, bool ownsSpecs = default, bool? isAdvancedLoggingEnabled = default) {
			scope.EnsureNotNull(nameof(scope));
			specs.EnsureNotNull(nameof(specs));
			//
			_sequentialId = DependencyUtilities.NextResolutionContextSequentialId();
			_scope = scope;
			_specs = specs.ToValueHolder(ownsValue: ownsSpecs);
			_involvedHandlerCounters = new Dictionary<IDependencyHandler2, int>(comparer: ReferenceEqualityComparer<IDependencyHandler2>.Instance);
			_involvedHandlerChain = new List<IDependencyHandler2>();
			_involvedHandlerSpinLock = new PrimitiveSpinLock();
			//
			var resolutionSettings = DependencyUtilities.DefaultResolutionSettings;
			_usageCountLimitPerHandler = resolutionSettings.UsageCountLimitPerHandler;
			_isAdvancedLoggingEnabled = isAdvancedLoggingEnabled ?? resolutionSettings.IsAdvancedLoggingEnabled;
		}

		public long SequentialId {
			get {
				var result = _sequentialId;
				EnsureNotDisposeState();
				return result;
			}
		}

		public IDependencyScope Scope
			=> ReadDA(ref _scope);

		public IDependencyResolutionSpecs Specs
			=> ReadDA(ref _specs).Value;

		public virtual bool HasHandlerInvolved(IDependencyHandler2 handler) {
			handler.EnsureNotNull(nameof(handler));
			//
			return ReadDA(ref _involvedHandlerSpinLock).Invoke(() => ReadDA(ref _involvedHandlerCounters).ContainsKey(handler));
		}

		public virtual bool TryInvolveHandler(IDependencyHandler2 handler, IDependencyHandler2 referrer = null) {
			handler.EnsureNotNull(nameof(handler));
			//
			var involvedExecutorsCounters = ReadDA(ref _involvedHandlerCounters);
			var involvedExecutorsChain = ReadDA(ref _involvedHandlerChain);
			var spinLock = ReadDA(ref _involvedHandlerSpinLock);
			//
			var result = default(bool);
			var error = default(Exception);
			try {
				result =
					spinLock.Invoke(
						() => {
							EnsureNotDisposeState(considerDisposeRequest: true);
							//
							if (involvedExecutorsCounters.ContainsKey(handler)) {
								if (involvedExecutorsCounters[ handler ] >= ExecutionsCountLimitPerExecutor)
									return false;
								else
									involvedExecutorsCounters[ handler ] += 1;
							}
							else
								involvedExecutorsCounters.Add(handler, 1);
							involvedExecutorsChain.Add(handler);
							return true;
						});
			}
			catch (Exception exception) {
				error = exception;
				throw;
			}
			finally {
				if (_isAdvancedLoggingEnabled) {
					string contextDetailsText;
					using (var acquiredBuffer = EonStringBuilderUtilities.AcquireBuffer()) {
						var sb = acquiredBuffer.StringBuilder;
						sb.AppendLine("Параметры разрешения функциональной зависимости:");
						sb.AppendLine(Specs.ToString().IndentLines());
						sb.AppendLine("Область функциональной зависимости:");
						sb.AppendLine(Scope.ToString().IndentLines());
						contextDetailsText = sb.ToString();
					}
#if !DO_NOT_USE_EON_LOGGING_API
					if (error is null)
						this.IssueInformation(
							$"Разрешение функциональной зависимости (ИД контекста '{_sequentialId:d}').",
							$"Операция '{nameof(TryInvolveExecutor)}'.{Environment.NewLine}\tПараметры операции:{Environment.NewLine}\t\t{nameof(executor)}:{Environment.NewLine}{executor.ToString().IndentLines(indentSize: 3)}{Environment.NewLine}\t\t{nameof(referrer)}:{Environment.NewLine}{referrer.TextView().IndentLines(indentSize: 3)}{Environment.NewLine}\tКонтекст:{Environment.NewLine}{contextDetailsText.IndentLines(indentSize: 2)}{Environment.NewLine}\tРезультат: {result}");
					else
						try {
							this.IssueError(
								$"Разрешение функциональной зависимости (ИД контекста '{_sequentialId:d}').",
								$"Операция '{nameof(TryInvolveExecutor)}'. Ошибка выполнения операции.{Environment.NewLine}\tПараметры операции:{Environment.NewLine}\t\t{nameof(executor)}:{Environment.NewLine}{executor.ToString().IndentLines(indentSize: 3)}{Environment.NewLine}\t\t{nameof(referrer)}:{Environment.NewLine}{referrer.TextView().IndentLines(indentSize: 3)}{Environment.NewLine}\tКонтекст:{Environment.NewLine}{contextDetailsText.IndentLines(indentSize: 2)}",
								error,
								severityLevel: SeverityLevel.High);
						}
						catch (Exception firstException) {
							throw new AggregateException(error, firstException);
						}
#endif
				}
			}
			//
			return result;
		}

		public virtual bool IsMatchSelectCriterion(object instance)
			=> Specs.SelectCriterion?.IsMatch(instance) ?? true;

		public virtual IDependencyHandler2[ ] GetInvolvedHandlerChain() {
			return
				ReadDA(ref _involvedHandlerSpinLock)
				.Invoke(
					() => {
						var locInvolvedHandlerChain = ReadDA(ref _involvedHandlerChain);
						var locResult = new IDependencyHandler2[ locInvolvedHandlerChain.Count ];
						if (locResult.Length > 0)
							locInvolvedHandlerChain.CopyTo(array: locResult, arrayIndex: 0);
						return locResult;
					});
		}

		public int ExecutionsCountLimitPerExecutor {
			get {
				EnsureNotDisposeState();
				return _usageCountLimitPerHandler;
			}
		}

		public bool IsAdvancedLoggingEnabled {
			get {
				var result = _isAdvancedLoggingEnabled;
				EnsureNotDisposeState();
				return result;
			}
		}

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=> $"{GetType()} ({nameof(SequentialId)} = {_sequentialId:d})";

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_specs?.Dispose();
				_involvedHandlerSpinLock?.EnterAndExitLock();
				_involvedHandlerChain?.Clear();
				_involvedHandlerCounters?.Clear();
			}
			_involvedHandlerCounters = null;
			_involvedHandlerChain = null;
			_involvedHandlerSpinLock = null;
			_scope = null;
			_specs = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}