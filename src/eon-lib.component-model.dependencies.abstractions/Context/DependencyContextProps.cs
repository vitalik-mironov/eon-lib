using Eon.ComponentModel.Dependencies;

namespace Eon.Context {

	/// <summary>
	/// Defines dependency specific context properties.
	/// </summary>
	public static class DependencyContextProps {

		/// <summary>
		/// Defines dependencies property for context.
		/// </summary>
		public static readonly ContextProperty<IVh<IDependencySupport>> DependenciesProp =
			new ContextProperty<IVh<IDependencySupport>>(
				name: nameof(Dependencies),
				cleanup: (locCtx, locValue) => locValue?.Dispose(),
				validator: locValue => locValue.EnsureSelfAndValueNotNull());

		/// <summary>
		/// Gets dependencies for this context.
		/// </summary>
		/// <param name="ctx">
		/// Context.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="disposeTolerant">
		/// Dispose tolerancy flag.
		/// <para><see langword="true"/> — context dispose state ignored and default value returned.</para>
		/// </param>
		public static IDependencySupport Dependencies(this IContext ctx, bool disposeTolerant = default)
			=> DependenciesProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant)?.Value;

		/// <summary>
		/// Indicates that a dependencies available for this context.
		/// </summary>
		/// <param name="ctx">
		/// Context.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="disposeTolerant">
		/// Dispose tolerancy flag.
		/// <para><see langword="true"/> — context dispose state ignored and default value returned.</para>
		/// </param>
		/// <returns></returns>
		public static bool HasDependencies(this IContext ctx, bool disposeTolerant = default)
			=> DependenciesProp.TryGetValue(ctx: ctx, value: out _, disposeTolerant: disposeTolerant);

	}

}