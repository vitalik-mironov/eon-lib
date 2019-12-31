using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Linq;

namespace Eon.Interaction {

	public class InteractionFaultException
		:InteractionException, IInnerExceptionsGetter {

		readonly IInteractionFault _fault;

		readonly ICollection<Exception> _innerExceptions;

		public InteractionFaultException()
			: this(message: null, fault: null, innerException: null) { }

		public InteractionFaultException(string message)
			: this(message: message, fault: null, innerException: null) { }

		public InteractionFaultException(string message, Exception innerException)
			: this(message: message, fault: null, innerException: innerException) { }

		public InteractionFaultException(string message, IInteractionFault fault, Exception innerException)
			: this(message, fault, innerException.Sequence()) { }

		public InteractionFaultException(string message, IInteractionFault fault, IEnumerable<Exception> innerExceptions)
			: base(message, innerExceptions.SkipNull().FirstOrDefault()) {
			_fault = fault;
			_innerExceptions = innerExceptions.SkipNull().ToArray().AsReadOnlyCollection();
		}

		public IInteractionFault Fault
			=> _fault;

		public ICollection<Exception> InnerExceptions
			=> _innerExceptions;

		IEnumerable<Exception> IInnerExceptionsGetter.InnerExceptions
			=> InnerExceptions;

	}

}