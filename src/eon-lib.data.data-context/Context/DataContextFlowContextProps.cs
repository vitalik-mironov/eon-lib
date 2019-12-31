using System.Runtime.CompilerServices;
using Eon.Data;

namespace Eon.Context {

	/// <summary>
	/// Defines the data context specific properties for the flow context (<see cref="IContext"/>).
	/// </summary>
	public static class DataContextFlowContextProps {

		/// <summary>
		/// Defines the data context (<see cref="IDataContext"/>) property for the flow context.
		/// </summary>
		public static readonly ContextProperty<IDataContext2> DataContextProp =
			new ContextProperty<IDataContext2>(name: nameof(DataContext), cleanup: (locCtx, locValue) => { }, fallbackValue: default, validator: locValue => locValue.EnsureNotNull());

		/// <summary>
		/// Gets the data context (<see cref="IDataContext2"/>) for this flow context.
		/// </summary>
		/// <param name="ctx">
		/// Flow context.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="disposeTolerant">
		/// Dispose tolerancy flag.
		/// <para><see langword="true"/> — the flow context dispose state will be ignored and the default value will be returned (in case of the flow context dispose).</para>
		/// </param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static IDataContext2 DataContext(this IContext ctx, bool disposeTolerant = default)
			=> DataContextProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant);

		/// <summary>
		/// Indicates that the data context (<see cref="IDataContext2"/>) is available for this flow context.
		/// </summary>
		/// <param name="ctx">
		/// Flow context.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="disposeTolerant">
		/// Dispose tolerancy flag.
		/// <para><see langword="true"/> — the flow context dispose state will be ignored and <see langword="true"/> will be returned if data context exists on the spcified flow context.</para>
		/// </param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool HasDataContext(this IContext ctx, bool disposeTolerant = default)
			=> DataContextProp.TryGetValue(ctx: ctx, value: out _, disposeTolerant: disposeTolerant);

		/// <summary>
		/// Indicates that the data context (<see cref="IDataContext2"/>) is available for this flow context and returns it into <paramref name="dataCtx"/>.
		/// </summary>
		/// <param name="ctx">
		/// Flow context.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="dataCtx">
		/// The location in which the data context will be placed.
		/// </param>
		/// <param name="disposeTolerant">
		/// Dispose tolerancy flag.
		/// <para><see langword="true"/> — the flow context dispose state will be ignored and <see langword="true"/> will be returned if data context exists on the spcified flow context.</para>
		/// </param>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool HasDataContext(this IContext ctx, out IDataContext2 dataCtx, bool disposeTolerant = default)
			=> DataContextProp.TryGetValue(ctx: ctx, value: out dataCtx, disposeTolerant: disposeTolerant);

	}

}