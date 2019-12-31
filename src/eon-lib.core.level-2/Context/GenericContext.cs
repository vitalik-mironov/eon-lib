using System;
using System.Diagnostics;

namespace Eon.Context {

	[DebuggerDisplay("{ToString(),nq}")]
	public class GenericContext
		:DisposeNotifying, IContext {

		readonly XCorrelationId _correlationId;

		readonly XFullCorrelationId _fullCorrelationId;

		readonly bool _hasOuterCtx;

		IContext _outerCtx;

		object _localTag;

		// TODO: Put strings into the resources.
		//
		public GenericContext(XFullCorrelationId fullCorrelationId = default, ArgumentPlaceholder<XCorrelationId> correlationId = default, object localTag = default, IContext outerCtx = default) {
			if (!(outerCtx is null))
				fullCorrelationId.Arg(nameof(fullCorrelationId)).EnsureIsNull();
			else if (!(fullCorrelationId is null)) {
				// If full correlation ID was explicitly assed.
				//
				if (fullCorrelationId.IsEmpty)
					throw
						new ArgumentException(message: "Cannot be an empty value.", paramName: nameof(fullCorrelationId));
				else if (correlationId.HasExplicitValue)
					// Correlation ID cannot be specified.
					//
					correlationId.ExplicitValue.ArgProp($"{nameof(correlationId)}.{nameof(correlationId.ExplicitValue)}").EnsureIsNull();
			}
			//
			if (correlationId.HasExplicitValue) {
				if (fullCorrelationId is null)
					_correlationId = correlationId.ExplicitValue ?? ContextUtilities.UndefinedCorrelationId;
				else
					_correlationId = fullCorrelationId[ fullCorrelationId.ComponentCount - 1 ];
			}
			else if (fullCorrelationId is null)
				_correlationId = XCorrelationId.New();
			else
				_correlationId = fullCorrelationId[ fullCorrelationId.ComponentCount - 1 ];
			_fullCorrelationId =
				outerCtx is null
				? (fullCorrelationId is null ? XFullCorrelationId.AppDomain + _correlationId : fullCorrelationId)
				: outerCtx.FullCorrelationId + _correlationId;
			_hasOuterCtx = !(outerCtx is null);
			_outerCtx = outerCtx;
			_localTag = localTag;
		}

		public XCorrelationId CorrelationId
			=> _correlationId;

		public XFullCorrelationId FullCorrelationId
			=> _fullCorrelationId;

		public bool HasOuterContext
			=> _hasOuterCtx;

		// TODO: Put strings into the resources.
		//
		public IContext OuterContext {
			get {
				if (_hasOuterCtx)
					return ReadDA(ref _outerCtx);
				else {
					EnsureNotDisposeState();
					throw new EonException(message: $"This context have not an outer context.{Environment.NewLine}\tContext:{this.FmtStr().GNLI2()}");
				}
			}
		}

		public object LocalTag
			=> ReadDA(ref _localTag);

		// TODO: Put strings into the resources.
		//
		public override string ToString() {
			ContextUtilities.DisplayNameProp.TryGetValue(ctx: this, value: out var displayName, disposeTolerant: true);
			var displayNameString = displayName?.ToString();
			if (string.IsNullOrEmpty(displayNameString))
				return _fullCorrelationId.Value;
			else
				return $"{displayName}, {_fullCorrelationId.Value}";
		}

		protected override void Dispose(bool explicitDispose) {
			_outerCtx = null;
			_localTag = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}