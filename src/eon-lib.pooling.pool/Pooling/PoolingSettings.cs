using System.Runtime.Serialization;

using Eon.Threading;

namespace Eon.Pooling {

	[DataContract]
	public class PoolingSettings
		:AsReadOnlyValidatableBase, IPoolingSettings, IAsReadOnly<PoolingSettings> {

		#region Static members

		/// <summary>
		/// Value: <see cref="Pool.DisplayNameMaxLength"/>.
		/// </summary>
		public static readonly int PoolDisplayNameMaxLength = Pool.DisplayNameMaxLength;

		#endregion

		[DataMember(Order = 0, Name = nameof(PoolDisplayName), IsRequired = false, EmitDefaultValue = false)]
		string _poolDisplayName;

		[DataMember(Order = 1, Name = nameof(IsDisabled), IsRequired = true)]
		bool _isDisabled;

		[DataMember(Order = 2, Name = nameof(PoolSize), IsRequired = true)]
		int? _poolSize;

		[DataMember(Order = 3, Name = nameof(PreferredSlidingTtl), IsRequired = true)]
		TimeoutDuration _preferredSlidingTtl;

		public PoolingSettings()
			: this(isReadOnly: default) { }

		public PoolingSettings(string poolDisplayName = default, bool isDisabled = default, int? poolSize = default, TimeoutDuration preferredSlidingTtl = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			_poolDisplayName = poolDisplayName;
			_isDisabled = isDisabled;
			_poolSize = poolSize;
			_preferredSlidingTtl = preferredSlidingTtl;
		}

		public PoolingSettings(IPoolingSettings other, bool isReadOnly = default)
			: this(poolDisplayName: other.EnsureNotNull(nameof(other)).Value.PoolDisplayName, isDisabled: other.IsDisabled, poolSize: other.PoolSize, preferredSlidingTtl: other.PreferredSlidingTtl, isReadOnly: isReadOnly) { }

		public string PoolDisplayName {
			get => _poolDisplayName;
			set {
				EnsureNotReadOnly();
				_poolDisplayName = value;
			}
		}

		public bool IsDisabled {
			get => _isDisabled;
			set {
				EnsureNotReadOnly();
				_isDisabled = value;
			}
		}

		public int? PoolSize {
			get => _poolSize;
			set {
				EnsureNotReadOnly();
				_poolSize = value;
			}
		}

		public TimeoutDuration PreferredSlidingTtl {
			get => _preferredSlidingTtl;
			set {
				EnsureNotReadOnly();
				_preferredSlidingTtl = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy) {
			CreateReadOnlyCopy(copy: out var locCopy);
			copy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out PoolingSettings copy)
			=> copy = new PoolingSettings(other: this, isReadOnly: true);

		public new PoolingSettings AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(copy: out var copy);
				return copy;
			}
		}

		IPoolingSettings IAsReadOnlyMethod<IPoolingSettings>.AsReadOnly()
			=> AsReadOnly();

		protected override void OnValidate() {
			if (IsDisabled)
				PoolSize.PropArg(nameof(PoolSize)).EnsureNotLessThan(operand: 1);
			else {
				PoolSize.PropArg(nameof(PoolSize)).EnsureNotNull().EnsureNotLessThan(operand: 1);
				PreferredSlidingTtl.PropArg(nameof(PreferredSlidingTtl)).EnsureNotNull();
			}
			PoolDisplayName.PropArg(nameof(PoolDisplayName)).EnsureNotEmpty().EnsureHasMaxLength(maxLength: PoolDisplayNameMaxLength);
		}

	}

}