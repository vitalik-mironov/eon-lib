using System;
using System.Collections.Generic;
using System.Globalization;

using Eon.Globalization;
using Eon.Threading;

namespace Eon.Resources.XResource {

	public class ByTypeTextXResourceAccessor
		:ByTypeXResourceAccessor {

		#region Nested types

		sealed class P_Localizable
			:ILocalizable<string> {

			readonly ByTypeTextXResourceAccessor _accessor;

			readonly string _subpath;


			internal P_Localizable(ByTypeTextXResourceAccessor accessor, string subpath = default) {
				if (accessor is null)
					throw new ArgumentNullException(paramName: nameof(accessor));
				//
				_accessor = accessor;
				_subpath = subpath;
			}

			public string Value
				=> GetForCulture(culture: null);

			public string GetForCulture(CultureInfo culture)
				=> _accessor.GetText(culture: culture, subpath: _subpath);

		}

		#endregion

		readonly PrimitiveSpinLock _repoSpinLock;

		readonly Dictionary<string, Dictionary<string, ValueHolderClass<IXResourcePointer>>> _repo;

		readonly Dictionary<string, P_Localizable> _localizablesRepo;

		public ByTypeTextXResourceAccessor(Type type)
			: this(type: type, provider: null) { }

		protected internal ByTypeTextXResourceAccessor(Type type, IXResourceService provider)
			: base(type: type, provider: provider) {
			_repoSpinLock = new PrimitiveSpinLock();
			_repo = new Dictionary<string, Dictionary<string, ValueHolderClass<IXResourcePointer>>>(comparer: CultureNameUtilities.Comparer);
			_localizablesRepo = new Dictionary<string, P_Localizable>(comparer: CultureNameUtilities.Comparer);
		}

		// TODO: Put strings into the resources.
		//
		public string GetText(CultureInfo culture = default, string subpath = default) {
			culture = culture ?? CultureInfo.CurrentCulture;
			subpath = subpath ?? string.Empty;
			var (repo, holder) =
				_repoSpinLock
				.Invoke(
					func:
						() => {
							if (!_repo.TryGetValue(key: culture.Name, value: out var locCultureRepo))
								_repo.Add(key: culture.Name, value: locCultureRepo = new Dictionary<string, ValueHolderClass<IXResourcePointer>>(comparer: XResourceTree.NodeNameComparer));
							locCultureRepo.TryGetValue(key: subpath, value: out var locHolder);
							return (repo: locCultureRepo, holder: locHolder);
						});
			//
			var provider = GetProvider();
			if (holder is null) {
				var newPointer = default(IXResourcePointer);
				var locatorOrder = GetLocatorOrder();
				for (var y = 0; y < locatorOrder.Count; y++) {
					if (provider.Locate(locator: locatorOrder[ y ], culture: culture, subpath: subpath, pointer: out newPointer))
						break;
				}
				//
				holder =
					_repoSpinLock
					.Invoke(
						func:
							() => {
								if (!repo.TryGetValue(key: subpath, value: out var locHolder))
									repo.Add(key: subpath, value: locHolder = new ValueHolderClass<IXResourcePointer>(value: newPointer));
								return locHolder;
							});
			}
			//
			if (holder.Value is null)
				throw
					new XResourceException(
						message: $"Не удалось получить ресурс по указанному пути (тип '{Type}', подпуть '{subpath}').{Environment.NewLine}{provider.Format(locator: typeof(XResourceTree), subpath: "NodeNotFoundByPath", args: new[ ] { subpath })}");
			else
				return provider.Format(pointer: holder.Value);
		}

		public ILocalizable<string> GetLocalizableText(string subpath = default) {
			subpath = subpath ?? string.Empty;
			//
			P_Localizable localizable = default;
			if (_repoSpinLock.Invoke(() => _localizablesRepo.TryGetValue(key: subpath, value: out localizable)))
				return localizable;
			else {
				localizable = new P_Localizable(accessor: this, subpath: subpath);
				return
					_repoSpinLock
					.Invoke(
						func:
							() => {
								if (_localizablesRepo.TryGetValue(key: subpath, value: out var locLocalizable))
									return locLocalizable;
								else {
									_localizablesRepo.Add(key: subpath, value: localizable);
									return localizable;
								}
							});
			}
		}

	}

}