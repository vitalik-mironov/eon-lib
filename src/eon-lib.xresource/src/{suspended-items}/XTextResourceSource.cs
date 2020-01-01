using System;
using System.Globalization;

using Eon.ComponentModel;
using Eon.Globalization;

namespace Eon.Resources.XResource {

	public class XTextResourceSource
		:DisposableNotifyPropertyChangedBase, ILocalizable<string> {

		Type _resourceTreeNodePathType;

		string _resourceTreeNodeSubPath;

		XResourceTreeNoArgsTextNodeRef _resourceTreeNodeRef;

		public XTextResourceSource() { }

		public Type ResourcePathType {
			get { return ReadDA(ref _resourceTreeNodePathType); }
			set {
				var originalValue = WriteDA(ref _resourceTreeNodePathType, value);
				if (originalValue != value) {
					NotifyPropertyChanged(nameof(ResourcePathType));
					P_UpdateResourceNodeText(value, ResourceSubPath);
				}
			}
		}

		public string ResourceSubPath {
			get { return ReadDA(ref _resourceTreeNodeSubPath); }
			set {
				var originalValue = WriteDA(ref _resourceTreeNodeSubPath, value);
				if (!string.Equals(originalValue, value, StringComparison.Ordinal)) {
					NotifyPropertyChanged(nameof(ResourceSubPath));
					P_UpdateResourceNodeText(ResourcePathType, value);
				}
			}
		}

		public string Text
			=> ReadDA(ref _resourceTreeNodeRef)?.Value;

		void P_UpdateResourceNodeText(Type pathType, string subPath) {
			XResourceTreeNoArgsTextNodeRef newResourceNodeRef = null;
			XResourceTreeNoArgsTextNodeRef oldResourceNodeRef;
			try {
				if (pathType == null)
					newResourceNodeRef = null;
				else
					newResourceNodeRef = new XResourceTreeNoArgsTextNodeRef(pathType, subPath);
				oldResourceNodeRef = WriteDA(ref _resourceTreeNodeRef, newResourceNodeRef);
			}
			catch (Exception firstException) {
				DisposableUtilities.DisposeMany(firstException, newResourceNodeRef);
				throw;
			}
			oldResourceNodeRef.TryDispose();
			NotifyPropertyChanged(nameof(Text));
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_resourceTreeNodeRef.TryDispose();
			_resourceTreeNodePathType = null;
			_resourceTreeNodeRef = null;
			_resourceTreeNodeSubPath = null;
			base.Dispose(explicitDispose);
		}

		#region ILocalizable<string> Members

		public string Value
			=> Text;

		public string GetForCulture(CultureInfo culture) {
			var resourceTreeNodeRef = ReadDA(ref _resourceTreeNodeRef);
			return resourceTreeNodeRef == null ? null : ((ILocalizable<string>)resourceTreeNodeRef).GetForCulture(culture);
		}

		#endregion

	}

}