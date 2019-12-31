using System.Runtime.CompilerServices;

namespace Eon {

	public static class ArgumentPlaceholderUtilities {

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static TValue? SubstituteDefault<TValue>(this ArgumentPlaceholder<TValue> arg)
			where TValue : struct
			=> arg.HasExplicitValue ? arg.ExplicitValue : default(TValue?);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static TValue? Substitute<TValue>(this ArgumentPlaceholder<TValue> arg, TValue? value)
			where TValue : struct
			=> arg.HasExplicitValue ? arg.ExplicitValue : value;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentPlaceholder<TValue> ArgPlaceholder<TValue>(this TValue value)
			=> new ArgumentPlaceholder<TValue>(value: value);

	}

}