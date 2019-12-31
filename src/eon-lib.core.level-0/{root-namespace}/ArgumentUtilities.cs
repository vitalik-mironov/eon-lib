using System;
using System.Runtime.CompilerServices;

namespace Eon {

	public static partial class ArgumentUtilities {

		/// <summary>
		/// Creates argument utilities context.
		/// </summary>
		/// <typeparam name="T">Argument type constraint.</typeparam>
		/// <param name="value">Argument.</param>
		/// <param name="name">
		/// Name of argument.
		/// </param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<T> Arg<T>(this T value, string name)
			=> new ArgumentUtilitiesHandle<T>(value: value, name: name);

		/// <summary>
		/// Creates argument utilities context.
		/// </summary>
		/// <typeparam name="T">Argument type constraint.</typeparam>
		/// <param name="value">Argument.</param>
		/// <param name="name">
		/// Name of argument.
		/// </param>
		/// <param name="exceptionFactory">
		/// Exception factory to be used when exception case occurs.
		/// </param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<T> Arg<T>(this T value, string name, ExceptionFactoryDelegate2 exceptionFactory)
			=> new ArgumentUtilitiesHandle<T>(value: value, name: name, isPropertyValue: false, exceptionFactory: exceptionFactory);

		/// <summary>
		/// Creates argument utilities context.
		/// <para>Argument expressed as a property value.</para>
		/// </summary>
		/// <typeparam name="TArg">Argument type constraint.</typeparam>
		/// <param name="value">Argument</param>
		/// <param name="name">Name of argument.</param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<TArg> ArgProp<TArg>(this TArg value, string name)
			=> new ArgumentUtilitiesHandle<TArg>(value: value, name: name, isPropertyValue: true);

		/// <summary>
		/// Creates argument utilities context.
		/// <para>Argument expressed as a property value.</para>
		/// </summary>
		/// <typeparam name="T">Argument type constraint.</typeparam>
		/// <param name="value">Argument</param>
		/// <param name="name">Name of argument.</param>
		/// <param name="exceptionFactory">
		/// Exception factory to be used when exception case occurs.
		/// </param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<T> ArgProp<T>(this T value, string name, ExceptionFactoryDelegate2 exceptionFactory)
			=> new ArgumentUtilitiesHandle<T>(value: value, name: name, isPropertyValue: true, exceptionFactory: exceptionFactory);

		/// <summary>
		/// Creates argument utilities context.
		/// <para>Argument expressed as a property value.</para>
		/// </summary>
		/// <typeparam name="T">Argument type constraint.</typeparam>
		/// <param name="value">Argument</param>
		/// <param name="name">Name of argument.</param>
		/// <param name="exceptionFactory">
		/// Exception factory to be used when exception case occurs.
		/// </param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<T> PropArg<T>(this T value, string name, ExceptionFactoryDelegate2 exceptionFactory)
			=> new ArgumentUtilitiesHandle<T>(value: value, name: name, isPropertyValue: true, exceptionFactory: exceptionFactory);

		/// <summary>
		/// Creates argument utilities context.
		/// <para>Argument expressed as a property value.</para>
		/// </summary>
		/// <typeparam name="T">Argument type constraint.</typeparam>
		/// <param name="value">Argument</param>
		/// <param name="name">Name of argument.</param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<T> PropArg<T>(this T value, string name)
			=> new ArgumentUtilitiesHandle<T>(value: value, name: name, isPropertyValue: true, exceptionFactory: null);

		// TODO: Put strings into the resources.
		//
		internal static Exception Internal_CreateNullValueException(bool isPropertyValue, string name, ExceptionFactoryDelegate2 exceptionFactory = default) {
			var exceptionMessage = isPropertyValue ? $"Не указано значение свойства '{name}'." : $"Значение не может быть '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.";
			return
				exceptionFactory?.Invoke(message: exceptionMessage)
				?? new ArgumentNullException(message: exceptionMessage, paramName: name);
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<T> EnsureNotNull<T>(this ArgumentUtilitiesHandle<T> hnd) {
			if (hnd.Value == null)
				throw Internal_CreateNullValueException(isPropertyValue: hnd.IsProp, name: hnd.Name, exceptionFactory: hnd.ExceptionFactory);
			else
				return hnd;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<T> EnsureNotNull<T>(this T value, string name) {
			if (value == null)
				throw Internal_CreateNullValueException(isPropertyValue: false, name: name);
			else
				return new ArgumentUtilitiesHandle<T>(value: value, name: name);
		}

		public static ArgumentUtilitiesHandle<TProp> Prop<T, TProp>(this ArgumentUtilitiesHandle<T> hnd, Func<T, (TProp value, string name)> getter) {
			getter.EnsureNotNull(nameof(getter));
			//
			var prop = getter(arg: hnd.Value);
			return hnd.AsChanged(value: prop.value, name: $"{hnd.Name}.{prop.name}", isProp: true);
		}

	}

}