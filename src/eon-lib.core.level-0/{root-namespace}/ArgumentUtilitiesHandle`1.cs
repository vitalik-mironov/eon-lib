
using System.Runtime.CompilerServices;

namespace Eon {

	/// <summary>
	/// Argument utilities context.
	/// </summary>
	/// <typeparam name="T">Argument type.</typeparam>
	public readonly struct ArgumentUtilitiesHandle<T> {

		/// <summary>
		/// Gets argument value.
		/// </summary>
		public readonly T Value;

		/// <summary>
		/// Gets the name of argument.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// Indicates the argument expressed as a property value.
		/// </summary>
		public readonly bool IsProp;

		/// <summary>
		/// Gets the exception factory being used to throw an exception on failed assertion.
		/// </summary>
		public readonly ExceptionFactoryDelegate2 ExceptionFactory;

		internal ArgumentUtilitiesHandle(T value, string name) {
			Value = value;
			Name = string.IsNullOrEmpty(name) ? nameof(name) : name;
			IsProp = false;
			ExceptionFactory = null;
		}

		internal ArgumentUtilitiesHandle(T value, string name, bool isPropertyValue) {
			Value = value;
			Name = string.IsNullOrEmpty(name) ? nameof(name) : name;
			IsProp = isPropertyValue;
			ExceptionFactory = null;
		}

		internal ArgumentUtilitiesHandle(T value, string name, bool isPropertyValue, ExceptionFactoryDelegate2 exceptionFactory) {
			Value = value;
			Name = string.IsNullOrEmpty(name) ? nameof(name) : name;
			IsProp = isPropertyValue;
			ExceptionFactory = exceptionFactory;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public ArgumentUtilitiesHandle<T> AsChanged(T value)
			=> new ArgumentUtilitiesHandle<T>(value: value, name: Name, isPropertyValue: IsProp, exceptionFactory: ExceptionFactory);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public ArgumentUtilitiesHandle<TChange> AsChanged<TChange>(TChange value)
			=> new ArgumentUtilitiesHandle<TChange>(value: value, name: Name, isPropertyValue: IsProp, exceptionFactory: ExceptionFactory);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public ArgumentUtilitiesHandle<TChange> AsChanged<TChange>(TChange value, string name, bool isProp)
			=> new ArgumentUtilitiesHandle<TChange>(value: value, name: name, isPropertyValue: isProp, exceptionFactory: ExceptionFactory);

		public static implicit operator T(ArgumentUtilitiesHandle<T> value)
			=> value.Value;

		public override string ToString()
			=> Value?.ToString() ?? FormatStringUtilitiesCoreL0.GetNullValueText();

	}

}