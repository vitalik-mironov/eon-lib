using System;
using System.Threading;

using Eon.Reflection;
using Eon.Threading;

namespace Eon.Context {

	public static class ContextUtilities {

		public static readonly ContextProperty<CancellationToken> CtProp = new ContextProperty<CancellationToken>(name: nameof(Ct), fallbackValue: CancellationToken.None);

		public static readonly ContextProperty<IVh<IDisposeRegistry>> DisposeRegistryProp =
			new ContextProperty<IVh<IDisposeRegistry>>(name: nameof(DisposeRegistry), cleanup: (locCtx, locValue) => locValue?.Dispose());

		public static readonly ContextProperty<IVh<IAsyncProgress<IFormattable>>> ProgressProp =
			new ContextProperty<IVh<IAsyncProgress<IFormattable>>>(
				name: nameof(Progress),
				cleanup: (locCtx, locValue) => locValue?.Dispose(),
				fallbackValue: ((IAsyncProgress<IFormattable>)NopAsyncProgress<IFormattable>.Instance).ToValueHolder(ownsValue: false).ArgPlaceholder());

		public static readonly ContextProperty<IFormattable> DisplayNameProp = new ContextProperty<IFormattable>(name: nameof(DisplayName), fallbackValue: default(IFormattable).ArgPlaceholder());

		/// <summary>
		/// Value: 'undefined'.
		/// </summary>
		public static readonly XCorrelationId UndefinedCorrelationId = (XCorrelationId)"undefined";

		/// <summary>
		/// Returns the cancellation token that associated with the specified context.
		/// <para>If context (<paramref name="ctx"/>) is <see langword="null"/> then returns <see cref="CancellationToken.None"/>.</para>
		/// </summary>
		/// <param name="ctx">
		/// Context.
		/// <para>Can be null.</para>
		/// </param>
		/// <param name="disposeTolerant">
		/// Special flag indicating the specified context disposing tolerance. When flag is <see langword="true"/> and the specified context is about dispose or disposed, then <see cref="ObjectDisposedException"/> not will thrown and <see cref="CancellationToken.None"/> will returned.
		/// </param>
		/// <returns>Object <see cref="CancellationToken"/>.</returns>
		public static CancellationToken Ct(this IContext ctx, bool disposeTolerant = default)
			=> ctx is null ? CancellationToken.None : CtProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant);

		public static IDisposeRegistry DisposeRegistry(this IContext ctx, bool disposeTolerant = default)
			=> DisposeRegistryProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant)?.Value;

		public static bool HasDisposeRegistry(this IContext ctx, bool disposeTolerant = default)
			=> DisposeRegistryProp.TryGetValue(ctx: ctx, value: out _, disposeTolerant: disposeTolerant);

		public static IAsyncProgress<IFormattable> Progress(this IContext ctx, bool disposeTolerant = default)
			=> ProgressProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant)?.Value;

		public static IFormattable DisplayName(this IContext ctx, bool disposeTolerant = default)
			=> DisplayNameProp.GetValue(ctx: ctx, disposeTolerant: disposeTolerant);

		/// <summary>
		/// Creates new context <see cref="IContext"/>.
		/// </summary>
		/// <param name="fullCorrelationId">Full correlation-id.</param>
		/// <param name="correlationId">Correlation-id of context to be created.</param>
		/// <param name="localTag">Special user defined tag.</param>
		/// <param name="ct">Cancellation token.</param>
		/// <param name="disposeRegistry">Dispose registry.</param>
		/// <param name="progress">Progress reporter.</param>
		/// <param name="displayName">Display name.</param>
		/// <param name="outerCtx">Outer context of context to be created.</param>
		/// <returns></returns>
		public static IContext Create(
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			ArgumentPlaceholder<CancellationToken> ct = default,
			ArgumentPlaceholder<IVh<IDisposeRegistry>> disposeRegistry = default,
			ArgumentPlaceholder<IVh<IAsyncProgress<IFormattable>>> progress = default,
			ArgumentPlaceholder<IFormattable> displayName = default,
			IContext outerCtx = default) {
			//
			var ctx = new GenericContext(fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, outerCtx: outerCtx);
			//
			if (ct.HasExplicitValue)
				CtProp.SetLocalValue(ctx: ctx, value: ct.ExplicitValue);
			if (disposeRegistry.HasExplicitValue)
				DisposeRegistryProp.SetLocalValue(ctx: ctx, value: disposeRegistry.ExplicitValue);
			if (progress.HasExplicitValue)
				ProgressProp.SetLocalValue(ctx: ctx, value: progress.ExplicitValue);
			if (displayName.HasExplicitValue)
				DisplayNameProp.SetLocalValue(ctx: ctx, value: displayName.ExplicitValue);
			//
			return ctx;
		}

		/// <summary>
		/// Creates a new context using context class <typeparamref name="TContext"/> special constructor.
		/// <para>A special constructor must meet the following signature: (<see cref="XFullCorrelationId"/> fullCorrelationId, <see cref="ArgumentPlaceholder{XCorrelationId}"/> correlationId, <see cref="object"/> localTag, <see cref="IContext"/> outerCtx).</para>
		/// <para>Visibility level of a special constructor doesn't matter.</para>
		/// </summary>
		/// <typeparam name="TContext">Type of context to be created.</typeparam>
		/// <param name="fullCorrelationId">
		/// Full correlation-id.
		/// </param>
		/// <param name="correlationId">
		/// Correlation-id of context to be created.
		/// </param>
		/// <param name="localTag">
		/// Special user defined tag.
		/// </param>
		/// <param name="ct">
		/// Cancellation token.
		/// </param>
		/// <param name="disposeRegistry">
		/// Dispose registry.
		/// </param>
		/// <param name="progress">
		/// Progress reporter.
		/// </param>
		/// <param name="displayName">
		/// Display name.
		/// </param>
		/// <param name="outerCtx">Outer context of context to be created.</param>
		public static TContext Create<TContext>(
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			ArgumentPlaceholder<CancellationToken> ct = default,
			ArgumentPlaceholder<IVh<IDisposeRegistry>> disposeRegistry = default,
			ArgumentPlaceholder<IVh<IAsyncProgress<IFormattable>>> progress = default,
			ArgumentPlaceholder<IFormattable> displayName = default,
			IContext outerCtx = default)
			where TContext : class, IContext {
			//
			var ctx =
				ActivationUtilities
				.RequireConstructor<XFullCorrelationId, ArgumentPlaceholder<XCorrelationId>, object, IContext, TContext>()
				(arg1: fullCorrelationId, arg2: correlationId, arg3: localTag, arg4: outerCtx);
			//
			if (ct.HasExplicitValue)
				CtProp.SetLocalValue(ctx: ctx, value: ct.ExplicitValue);
			if (disposeRegistry.HasExplicitValue)
				DisposeRegistryProp.SetLocalValue(ctx: ctx, value: disposeRegistry.ExplicitValue);
			if (progress.HasExplicitValue)
				ProgressProp.SetLocalValue(ctx: ctx, value: progress.ExplicitValue);
			if (displayName.HasExplicitValue)
				DisplayNameProp.SetLocalValue(ctx: ctx, value: displayName.ExplicitValue);
			//
			return ctx;
		}

		public static IContext Create()
			=> new GenericContext();

		public static IContext New<TContext>(
			this IContext outerCtx,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			ArgumentPlaceholder<CancellationToken> ct = default,
			ArgumentPlaceholder<IVh<IDisposeRegistry>> disposeRegistry = default,
			ArgumentPlaceholder<IVh<IAsyncProgress<IFormattable>>> progress = default,
			ArgumentPlaceholder<IFormattable> displayName = default)
			where TContext : class, IContext
			=> Create<TContext>(outerCtx: outerCtx, fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, ct: ct, disposeRegistry: disposeRegistry, progress: progress, displayName: displayName);

		public static IContext New(
			this IContext outerCtx,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			ArgumentPlaceholder<CancellationToken> ct = default,
			ArgumentPlaceholder<IVh<IDisposeRegistry>> disposeRegistry = default,
			ArgumentPlaceholder<IVh<IAsyncProgress<IFormattable>>> progress = default,
			ArgumentPlaceholder<IFormattable> displayName = default)
			=> Create(outerCtx: outerCtx, fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, ct: ct, disposeRegistry: disposeRegistry, progress: progress, displayName: displayName);

		/// <summary>
		/// Sets a property for the specified context locally.
		/// <para>Property can be set once.</para>
		/// </summary>
		/// <typeparam name="TContext">Context type constraint.</typeparam>
		/// <typeparam name="T">Property value type constraint.</typeparam>
		/// <param name="ctx">
		/// Context for which property to be set.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="prop">
		/// Definition of property to be set.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="value">Property value to be set.</param>
		public static TContext Set<TContext, T>(this TContext ctx, ContextProperty<T> prop, T value)
			where TContext : class, IContext {
			//
			if (ctx is null)
				throw new ArgumentNullException(paramName: nameof(ctx));
			else if (prop is null)
				throw new ArgumentNullException(paramName: nameof(prop));
			//
			prop.SetLocalValue(ctx: ctx, value: value);
			return ctx;
		}

		public static TContext Set<TContext, T>(this TContext ctx, ContextProperty<T> prop, ArgumentUtilitiesHandle<T> value)
			where TContext : class, IContext {
			//
			if (ctx is null)
				throw new ArgumentNullException(paramName: nameof(ctx));
			else if (prop is null)
				throw new ArgumentNullException(paramName: nameof(prop));
			//
			prop.SetLocalValue(ctx: ctx, value: value);
			return ctx;
		}

		/// <summary>
		/// Вызывает исключение, если для указанного контекста поступил запрос отмены (см. <see cref="Ct(IContext, bool)"/>).
		/// <para>Наличие запроса отмены проверяется посредством токена отмены, ассоциированного с указанным контекстом (<see cref="Ct(IContext, bool)"/>, <see cref="CancellationToken.IsCancellationRequested"/>).</para>
		/// <para>Если <paramref name="ctx"/> есть <see langword="null"/>, то проверка запроса отмены для контекста, очевидно, не производится.</para>
		/// </summary>
		/// <param name="ctx">
		/// Контекст.
		/// <para>Может быть <see langword="null"/>.</para>
		/// </param>
		public static void ThrowIfCancellationRequested(this IContext ctx) {
			if (!(ctx is null))
				ctx.Ct().ThrowExceptionIfCancellationRequested();
		}

		/// <summary>
		/// Gets whether cancellation has been requested for this context.
		/// <para>If <paramref name="ctx"/> is <see langword="null"/>, then returns <see langword="false"/>.</para>
		/// </summary>
		/// <param name="ctx">
		/// Context.
		/// <para>Can be <see langword="null"/>.</para>
		/// </param>
		/// <returns></returns>
		public static bool IsCancellationRequested(this IContext ctx) {
			CancellationToken ct;
			if (ctx is null || !(ct = ctx.Ct()).CanBeCanceled)
				return false;
			else
				return ct.IsCancellationRequested;
		}

	}

}