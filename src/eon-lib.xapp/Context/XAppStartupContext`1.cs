using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Description;
using Eon.Threading;
using Eon.Threading.Tasks;

namespace Eon.Context {

	/// <summary>
	/// XApp startup context.
	/// </summary>
	public class XAppStartupContext<TDescription>
		:GenericContext, IXAppStartupContext<TDescription>
		where TDescription : class, IXAppDescription {

		readonly XAppStartupContextHostHints _hostHints;

		DescriptionLocator _locator;

		DisposableLazy<IXAppContainerControl> _containerControlLazy;

		AsyncOperator<TDescription> _descriptionLoader;

		public XAppStartupContext(
			XAppStartupContextHostHints hostHints,
			DescriptionLocator locator,
			ArgumentPlaceholder<IXAppContainerControl> containerControl = default,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			IContext outerCtx = default)
			: base(fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, outerCtx: outerCtx) {
			//
			locator.EnsureNotNull(nameof(locator));
			//
			_hostHints = hostHints;
			_locator = locator;
			_descriptionLoader =
				new AsyncOperator<TDescription>(
					asyncFactory:
						async locCtxArg => {
							// Arg 'locCtxArg' can be null.
							//
							var locLinkedCts = default(CancellationTokenSource);
							try {
								CancellationToken locCt;
								var locThisCtxCt = this.Ct();
								if (ReferenceEquals(objA: this, objB: locCtxArg) || !locThisCtxCt.CanBeCanceled)
									locCt = locCtxArg.Ct();
								else
									locCt = CancellationTokenUtilities.SingleOrLinked(ct1: locCtxArg.Ct(), ct2: locThisCtxCt, linkedCts: out locLinkedCts);
								var locDescriptionLocator = ReadDA(location: ref _locator);
								var locSvc = this.Dependencies().RequireService<IDescriptionPackageService>();
								using (var locCtx = ContextUtilities.Create(outerCtx: locCtxArg, ct: locCt))
									return await locSvc.LoadDescriptionAsync<TDescription>(locator: locDescriptionLocator, ctx: locCtx).ConfigureAwait(false);
							}
							finally {
								locLinkedCts?.Dispose();
							}
						},
					ownsResult: false);
			P_CtorInitializer(containerControl: containerControl);
		}

		public XAppStartupContext(
			XAppStartupContextHostHints hostHints,
			TDescription description,
			ArgumentPlaceholder<IXAppContainerControl> containerControl = default,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			IContext outerCtx = default)
			: base(fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, outerCtx: outerCtx) {
			//
			description.EnsureNotNull(nameof(description));
			//
			_hostHints = hostHints;
			_locator = null;
			_descriptionLoader = new AsyncOperator<TDescription>(result: description, ownsResult: false);
			P_CtorInitializer(containerControl: containerControl);
		}

		IXAppStartupContext P_OuterXAppStartupContext
			=> HasOuterContext ? OuterContext as IXAppStartupContext : null;

		public XAppStartupContextHostHints HostHints {
			get {
				EnsureNotDisposeState();
				return _hostHints;
			}
		}

		public DescriptionLocator DescriptionLocator
			=> ReadDA(ref _locator);

		public IXAppContainerControl ContainerControl
			=> ReadDA(ref _containerControlLazy).Value;

		void P_CtorInitializer(ArgumentPlaceholder<IXAppContainerControl> containerControl) {
			if (containerControl.HasExplicitValue)
				_containerControlLazy = new DisposableLazy<IXAppContainerControl>(value: containerControl.ExplicitValue, ownsValue: false);
			else
				_containerControlLazy = new DisposableLazy<IXAppContainerControl>(factory: () => P_OuterXAppStartupContext?.ContainerControl, ownsValue: false);
		}

		public virtual async Task<TDescription> LoadDescriptionAsync(IContext ctx = default)
			=> await ReadDA(ref _descriptionLoader).ExecuteAsync(ctx: ctx).ConfigureAwait(false);

		ITaskWrap<TDescription> IXAppStartupContext<TDescription>.LoadDescriptionAsync(IContext ctx)
			=> TaskUtilities.Wrap(factory: async () => await LoadDescriptionAsync(ctx: ctx).ConfigureAwait(false));

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_descriptionLoader?.Dispose();
			}
			_locator = null;
			_descriptionLoader = null;
			_containerControlLazy = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}