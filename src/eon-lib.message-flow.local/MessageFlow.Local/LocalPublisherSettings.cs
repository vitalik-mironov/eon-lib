using System.Runtime.Serialization;
using Eon.Runtime.Options;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.MessageFlow.Local {

	[DataContract]
	public class LocalPublisherSettings
		:AsReadOnlyValidatableBase, ILocalPublisherSettings, IAsReadOnly<LocalPublisherSettings> {

		#region Static & constant members

		/// <summary>
		/// Gets the current default local publisher settings set by <see cref="MessageFlowLocalPublisherSettingsOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static ILocalPublisherSettings Default
			=> MessageFlowLocalPublisherSettingsOption.Require();

		#endregion

		[DataMember(Order = 0, Name = nameof(PostingDop), IsRequired = true, EmitDefaultValue = true)]
		int _postingDop;

		public LocalPublisherSettings()
			: this(postingDop: default) { }

		public LocalPublisherSettings(int postingDop = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			//
			_postingDop = postingDop;
		}

		public LocalPublisherSettings(ILocalPublisherSettings other, bool isReadOnly = false)
			: this(postingDop: other.EnsureNotNull(nameof(other)).Value.PostingDop, isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected LocalPublisherSettings(SerializationContext ctx)
			: base(ctx: ctx) { }

		bool IAbilityOption.IsDisabled
			=> false;

		public int PostingDop {
			get => _postingDop;
			set {
				EnsureNotReadOnly();
				_postingDop = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy) {
			CreateReadOnlyCopy(out var locCopy);
			copy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out LocalPublisherSettings copy)
			=> copy = new LocalPublisherSettings(other: this, isReadOnly: true);

		public new LocalPublisherSettings AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var сopy);
				return сopy;
			}
		}

		ILocalPublisherSettings IAsReadOnlyMethod<ILocalPublisherSettings>.AsReadOnly()
			=> AsReadOnly();

		protected override void OnValidate()
			=> PostingDop.ArgProp(name: nameof(PostingDop)).EnsureNotLessThan(operand: 1);

	}

}