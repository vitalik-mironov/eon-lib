using System;
using System.Runtime.Serialization;
using System.Security;

using Eon.Runtime.Serialization;
using Eon.Threading;

using Newtonsoft.Json;

namespace Eon {

	[DataContract]
	public class DisposeNotifying
		:Disposable, IDisposeNotifying {

		EventHandler<DisposeEventArgs> _eventHandler_AfterDisposed;

		EventHandler<DisposeEventArgs> _eventHandler_BeforeDispose;

		public DisposeNotifying() { }

		[JsonConstructor]
		protected DisposeNotifying(SerializationContext context)
			: base(ctx: context) { }

		/// <summary>
		/// Событие, возникающее после выполнения явной выгрузки.
		/// </summary>
		public event EventHandler<DisposeEventArgs> AfterDisposed {
			add { AddEventHandler(ref _eventHandler_AfterDisposed, value); }
			remove { RemoveEventHandler(ref _eventHandler_AfterDisposed, value); }
		}

		/// <summary>
		/// Событие, возникающее перед началом выполнения явной выгрузки.
		/// </summary>
		public event EventHandler<DisposeEventArgs> BeforeDispose {
			add { AddEventHandler(ref _eventHandler_BeforeDispose, value); }
			remove { RemoveEventHandler(ref _eventHandler_BeforeDispose, value); }
		}

		/// <summary>
		/// Метод, вызываемый перед явной или неявной выгрузкой.
		/// <para>Не предназначен для прямого вызова из кода.</para>
		/// <para>Метод не вызывается после того, как один из вызывов был выполнен успешно.</para>
		/// <para>На момент вызова метода <seealso cref="Disposable.IsDisposeRequested"/> == true.</para>
		/// <para>Перед вызовом базовой реализации <seealso cref="Disposable.FireBeforeDispose(bool)"/> вызывает обработчик события <seealso cref="BeforeDispose"/>.</para>
		/// </summary>
		/// <param name="explicitDispose">Указывает на явную/не явную выгрузку.</param>
		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				VolatileUtilities.Read(ref _eventHandler_BeforeDispose)?.Invoke(this, new DisposeEventArgs(explicitDispose));
			//
			base.FireBeforeDispose(explicitDispose);
		}

		/// <summary>
		/// Метод, вызываемый после выполнения явной или неявной выгрузки.
		/// <para>Метод не вызывается после успешного выполнения.</para>
		/// <para>После успешного вызова базовой реализации <seealso cref="Disposable.FireAfterDisposed(bool)"/> вызывает обработчик события <seealso cref="AfterDisposed"/>.</para>
		/// </summary>
		/// <param name="explicitDispose">Указывает на явную/не явную выгрузку.</param>
		[SecuritySafeCritical]
		protected override void FireAfterDisposed(bool explicitDispose) {
			base.FireAfterDisposed(explicitDispose);
			//
			var notExceptionThrown = true;
			try {
				if (explicitDispose)
					VolatileUtilities.Read(ref _eventHandler_AfterDisposed)?.Invoke(this, new DisposeEventArgs(explicitDispose));
			} catch {
				notExceptionThrown = false;
				throw;
			} finally {
				if (notExceptionThrown)
					VolatileUtilities.Write(ref _eventHandler_AfterDisposed, null);
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_eventHandler_BeforeDispose = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}