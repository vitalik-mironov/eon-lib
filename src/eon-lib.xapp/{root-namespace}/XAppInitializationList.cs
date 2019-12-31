#region Compilation conditional symbols

#define DO_NOT_USE_OXY_LOGGING_API

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eon.Context;
using Eon.Description;
using Eon.Linq;

namespace Eon {

	public class XAppInitializationList<TDescription>
		:XAppScopeInstanceBase<TDescription>, IXAppInitializationList<TDescription>
		where TDescription : class, IXAppInitializationListDescription {

		readonly bool _isDisabled;

		IXAppScopeInstance[ ] _initializedItems;

		public XAppInitializationList(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) {
			_isDisabled = description.IsDisabled;
		}

		// TODO: Put strings into the resources.
		//
		protected override async Task OnInitializeAsync(IContext ctx = default) {
			var initializedItems = default(IXAppScopeInstance[ ]);
			try {
				if (IsDisabled) {
#if !DO_NOT_USE_OXY_LOGGING_API
					this
						.IssueWarning(
							message: "Данный список инициализации приложения отключен. Инициализация компонентов, входящих в этот список, не выполняется.",
							severityLevel: Diagnostics.SeverityLevel.Lowest);
#endif
					initializedItems = new IXAppScopeInstance[ ] { };
				}
				else {
					var initializablesDescriptions =
						Description
						.InitializableItems
						.ToArray();
					if (initializablesDescriptions.Length > 0) {
						initializedItems = new IXAppScopeInstance[ initializablesDescriptions.Length ];
						for (var i = 0; i < initializablesDescriptions.Length; i++) {
							initializedItems[ i ] = this.CreateAppScopeInstance<IXAppScopeInstance>(initializablesDescriptions[ i ]);
							await initializedItems[ i ].InitializeAsync(ctx: ctx).ConfigureAwait(false);
						}
					}
				}
				WriteDA(location: ref _initializedItems, value: initializedItems);
			}
			catch (Exception firstException) {
				foreach (var item in initializedItems.EmptyIfNull())
					item?.Dispose(firstException);
				throw;
			}
		}

		public bool IsDisabled {
			get {
				EnsureNotDisposeState();
				return _isDisabled;
			}
		}

		public IEnumerable<IXInstance> InitializedItems
			=> EnumerateDA(ReadIA(ref _initializedItems));

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_initializedItems.DisposeAndClearArray();
			_initializedItems = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}