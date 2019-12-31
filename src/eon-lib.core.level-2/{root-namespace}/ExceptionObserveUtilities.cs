using System;

using Eon.Context;
using Eon.Diagnostics;

namespace Eon {

	public static class ExceptionObserveUtilities {

		sealed class P_DelegatedUnhandledExceptionObserver
			:IUnhandledExceptionObserver {

			readonly Func<Exception, object, string, IContext, bool> _observe;

			readonly ArgumentPlaceholder<object> _component;

			readonly ArgumentPlaceholder<string> _message;

			readonly ArgumentPlaceholder<IContext> _exceptionCtx;

			internal P_DelegatedUnhandledExceptionObserver(
				Func<Exception, object, string, IContext, bool> observe,
				ArgumentPlaceholder<object> component = default,
				ArgumentPlaceholder<string> message = default,
				ArgumentPlaceholder<IContext> exceptionCtx = default) {
				//
				_observe = observe;
				_component = component;
				_message = message;
				_exceptionCtx = exceptionCtx;
			}

			public bool ObserveException(Exception exception, object component = default, string message = default, IContext exceptionCtx = default)
				=>
				_observe
				?.Invoke(
					arg1: exception,
					arg2: _component.Substitute(value: component),
					arg3: _message.Substitute(value: message),
					arg4: _exceptionCtx.Substitute(value: exceptionCtx))
				?? false;

		}

		public static IUnhandledExceptionObserver CreateObserver(Func<Exception, bool> observe) {
			observe.EnsureNotNull(nameof(observe));
			//
			return CreateObserver(observe: (locException, locComponent, locMessage, locExceptionCtx) => observe(arg: locException));
		}

		public static IUnhandledExceptionObserver CreateObserver(Func<Exception, object, bool> observe) {
			observe.EnsureNotNull(nameof(observe));
			//
			return CreateObserver(observe: (locException, locComponent, locMessage, locExceptionCtx) => observe(arg1: locException, arg2: locComponent));
		}

		public static IUnhandledExceptionObserver CreateObserver(Func<Exception, object, string, bool> observe) {
			observe.EnsureNotNull(nameof(observe));
			//
			return CreateObserver(observe: (locException, locComponent, locMessage, locExceptionCtx) => observe(arg1: locException, arg2: locComponent, arg3: locMessage));
		}

		public static IUnhandledExceptionObserver CreateObserver(Func<Exception, object, string, IContext, bool> observe) {
			observe.EnsureNotNull(nameof(observe));
			//
			return new P_DelegatedUnhandledExceptionObserver(observe: observe);
		}

		public static IUnhandledExceptionObserver CreateObserver(Func<Exception, string, bool> observe) {
			observe.EnsureNotNull(nameof(observe));
			//
			return CreateObserver(observe: (locException, locComponent, locMessage, locExceptionCtx) => observe(arg1: locException, arg2: locMessage));
		}

		public static IUnhandledExceptionObserver CreateObserver(
			Func<Exception, object, string, IContext, bool> observe,
			ArgumentPlaceholder<object> component = default,
			ArgumentPlaceholder<string> message = default,
			ArgumentPlaceholder<IContext> exceptionCtx = default) {
			//
			observe.EnsureNotNull(nameof(observe));
			//
			return new P_DelegatedUnhandledExceptionObserver(observe: observe, component: component, message: message, exceptionCtx: exceptionCtx);
		}

		public static IUnhandledExceptionObserver DeriveObserver(
			this IUnhandledExceptionObserver baseObserver,
			ArgumentPlaceholder<object> component = default,
			ArgumentPlaceholder<string> message = default,
			ArgumentPlaceholder<IContext> exceptionCtx = default) {
			//
			if (baseObserver is null)
				return null;
			else 
				return new P_DelegatedUnhandledExceptionObserver(observe: baseObserver.ObserveException, component: component, message: message, exceptionCtx: exceptionCtx);
		}

	}

}