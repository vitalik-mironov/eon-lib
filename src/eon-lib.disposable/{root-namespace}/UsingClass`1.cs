using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	public sealed class UsingClass<T>
		:Disposable, IUsing<T>
		where T : class {

		#region Nested types

		sealed class P_State {

			public readonly T Value;

			public readonly DisposeAction<T> Dispose;

			internal P_State(T value, DisposeAction<T> dispose) {
				Value = value;
				Dispose = dispose;
			}

			internal P_State(ref Using<T> @using) {
				@using.Internal_GetArgs(value: out Value, dispose: out Dispose);
			}

		}

		#endregion

		P_State _state;

		public UsingClass(T value, DisposeAction<T> dispose) {
			_state = new P_State(value: value, dispose: dispose);
		}

		public UsingClass(Using<T> @using) {
			_state = new P_State(@using: ref @using);
		}

#pragma warning disable CS3006 // Overloaded method differing only in ref or out, or in array rank, is not CLS-compliant
		public UsingClass(ref Using<T> @using) {
			_state = new P_State(@using: ref @using);
		}
#pragma warning restore CS3006 // Overloaded method differing only in ref or out, or in array rank, is not CLS-compliant

		public T Value
			=> ReadDA(ref _state, considerDisposeRequest: true).Value;

		public override string ToString()
			=> _state?.Value?.ToString();

		protected override void Dispose(bool explicitDispose) {
			var state = itrlck.SetNull(location: ref _state);
			if (!(state is null))
				state.Dispose?.Invoke(value: state.Value, explicitDispose: explicitDispose);
			//
			base.Dispose(explicitDispose);
		}

	}

}