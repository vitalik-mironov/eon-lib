using System.Runtime.Serialization;

using Eon.Description;

namespace Eon.Text {

	[DataContract]
	public class StringBuilderBufferSetSettings
		:AsReadOnlyValidatableBase, IStringBuilderBufferSetSettings, ISettings {

		[DataMember(Order = 0, Name = nameof(BufferMaxCapacity))]
		int? _bufferMaxCapacity;

		[DataMember(Order = 1, Name = nameof(BufferMinCapacity))]
		int? _bufferMinCapacity;

		[DataMember(Order = 2, Name = nameof(BufferSetMaxSize))]
		int? _bufferSetMaxSize;

		public StringBuilderBufferSetSettings() { }

		public StringBuilderBufferSetSettings(int? bufferMaxCapacity = default, int? bufferMinCapacity = default, int? bufferSetMaxSize = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			_bufferMaxCapacity = bufferMaxCapacity;
			_bufferMinCapacity = bufferMinCapacity;
			_bufferSetMaxSize = bufferSetMaxSize;
		}

		public StringBuilderBufferSetSettings(IStringBuilderBufferSetSettings other, bool isReadOnly = default)
			: this(bufferSetMaxSize: other.EnsureNotNull(nameof(other)).Value.BufferSetMaxSize, bufferMaxCapacity: other.BufferMaxCapacity, bufferMinCapacity: other.BufferMinCapacity, isReadOnly: isReadOnly) { }

		public int? BufferMaxCapacity {
			get => _bufferMaxCapacity;
			set {
				EnsureNotReadOnly();
				_bufferMaxCapacity = value;
			}
		}

		public int? BufferMinCapacity {
			get => _bufferMinCapacity;
			set {
				EnsureNotReadOnly();
				_bufferMinCapacity = value;
			}
		}

		public int? BufferSetMaxSize {
			get => _bufferSetMaxSize;
			set {
				EnsureNotReadOnly();
				_bufferSetMaxSize = value;
			}
		}

		bool IAbilityOption.IsDisabled
			=> false;

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy) {
			CreateReadOnlyCopy(copy: out var locCopy);
			copy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out StringBuilderBufferSetSettings copy)
			=> copy = new StringBuilderBufferSetSettings(other: this, isReadOnly: true);

		public new StringBuilderBufferSetSettings AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(copy: out var copy);
				return copy;
			}
		}

		IStringBuilderBufferSetSettings IAsReadOnlyMethod<IStringBuilderBufferSetSettings>.AsReadOnly()
			=> AsReadOnly();

		protected override void OnValidate() {
			var maxCapacity = BufferMaxCapacity;
			var minCapacity = BufferMinCapacity;
			var setSize = BufferSetMaxSize;
			maxCapacity.ArgProp(nameof(BufferMaxCapacity)).EnsureNotLessThan(operand: 1).EnsureNotLessThan(operand: minCapacity ?? maxCapacity ?? 1);
			minCapacity.ArgProp(nameof(BufferMaxCapacity)).EnsureNotLessThan(operand: 1).EnsureNotGreaterThan(operand: maxCapacity ?? minCapacity ?? 1);
			setSize.ArgProp(nameof(BufferSetMaxSize)).EnsureNotLessThan(operand: 1);
		}

	}

}