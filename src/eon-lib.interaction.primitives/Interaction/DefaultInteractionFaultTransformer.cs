using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Diagnostics;
using Eon.Linq;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Interaction {

	public class DefaultInteractionFaultTransformer
		:IInteractionFaultTransformer {

		#region Static members

		static readonly DefaultInteractionFaultTransformer __Instance = new DefaultInteractionFaultTransformer();

		public static DefaultInteractionFaultTransformer Instance
			=> __Instance;

		#endregion

		public DefaultInteractionFaultTransformer() { }

		public virtual Exception ToException(IInteractionFault fault) {
			fault.EnsureNotNull(nameof(fault));
			//
			return
				fault
				.TreeNode()
				.Transform<IInteractionFault, Exception>(
					childrenSelector: locFault => locFault.InnerFaults,
					transform: CreateException);
		}

		// TODO: Put strings into the resources.
		//
		protected virtual Exception CreateException(IInteractionFault fault, IEnumerable<Exception> innerExceptions) {
			fault.EnsureNotNull(nameof(fault));
			//
			var faultCode = fault.Code;
			var exceptionMessage = string.IsNullOrEmpty(fault.Message) ? null : fault.Message;
			exceptionMessage =
				$"{Environment.NewLine}--- Начало сведений об ошибке удалённой стороны ---"
				+ $"{Environment.NewLine}\tКод:{faultCode.FmtStr().GNLI2()}"
				+ (exceptionMessage == null ? string.Empty : $"{Environment.NewLine}\tСообщение:{exceptionMessage.FmtStr().GNLI2()}")
				+ (fault.TypeName == null ? string.Empty : $"{Environment.NewLine}\tИмя типа:{fault.TypeName.FmtStr().GNLI2()}")
				+ (fault.StackTrace == null ? string.Empty : $"{Environment.NewLine}\tТрассировка стека:{fault.StackTrace.FmtStr().GNLI2()}")
				+ $"{Environment.NewLine}--- Конец сведений об ошибке удалённой стороны ---";
			var exception =
				new InteractionFaultException(
					message: exceptionMessage,
					fault: fault,
					innerExceptions: innerExceptions.EmptyIfNull().SkipNull());
			if (faultCode != null)
				exception
					.SetErrorCode(
						code:
							new ErrorCode(
								identifier: faultCode.Identifier,
								description: faultCode.Description,
								severityLevel: faultCode.SeverityLevel));
			return exception;
		}

		public virtual IInteractionFault FromException(Exception exception, bool includeDetails) {
			exception.EnsureNotNull(nameof(exception));
			//
			if (includeDetails)
				return
					exception
					.TreeNode()
					.Transform<Exception, IInteractionFault>(
						childrenSelector: locException => locException.SelectInner(flatAggregateException: true),
						transform: (locException, locInnerFaults) => CreateHttpFault(locException, locInnerFaults, true));
			else {
				var innerFaults =
					exception
					.FlattenBase(locException => locException.HasErrorCode())
					.InnerExceptions
					.Select(locException => CreateHttpFault(locException, null, false))
					.ToArray();
				if (innerFaults.Length == 1)
					return innerFaults[ 0 ];
				else
					return CreateHttpFault(exception, innerFaults, false);
			}
		}

		// TODO: Put strings into the resources.
		//
		protected virtual IInteractionFault CreateHttpFault(
			Exception exception,
			IEnumerable<IInteractionFault> innerFaults,
			bool includeDetails) {
			//
			exception.EnsureNotNull(nameof(exception));
			//
			var exceptionErrorCode = exception.GetErrorCode();
			if (includeDetails)
				return
					new InteractionFault() {
						Code = exceptionErrorCode == null ? null : new InteractionFaultCode(exceptionErrorCode),
						Message = exception.Message,
						StackTrace = exception.StackTrace,
						TypeName = exception.GetType().FullName,
						InnerFaults = innerFaults?.Select(i => new InteractionInnerFault(i)).ToArray() ?? new InteractionInnerFault[ ] { }
					};
			else {
				if (exceptionErrorCode == null)
					return
						new InteractionFault() {
							Message = FormatXResource(typeof(InteractionException), "DefaultMessage"),
							InnerFaults = innerFaults?.Select(i => new InteractionInnerFault(i)).ToArray() ?? new InteractionInnerFault[ ] { }
						};
				else
					return
						new InteractionFault() {
							Code = new InteractionFaultCode(exceptionErrorCode),
							InnerFaults = innerFaults?.Select(i => new InteractionInnerFault(i)).ToArray() ?? new InteractionInnerFault[ ] { }
						};
			}
		}

	}

}