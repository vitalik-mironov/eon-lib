namespace Eon {

	/// <summary>
	/// Представляет тэг, описывающий доступность редактирования объекта.
	/// </summary>
	public sealed class ReadOnlyStateTag {

		public readonly bool IsReadOnly;

		public readonly bool IsPermanent;

		public ReadOnlyStateTag() {
			IsReadOnly = false;
			IsPermanent = false;
		}

		public ReadOnlyStateTag(bool isReadOnly, bool isPermanent) {
			IsReadOnly = isReadOnly;
			IsPermanent = isPermanent;
		}

		public bool IsPermanentReadOnly
			=> IsReadOnly && IsPermanent;

	}

}