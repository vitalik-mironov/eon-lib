using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public abstract class StringCriterionOperatorBase
		:CriterionOperatorBase, IStringCriterionOperator {

		#region Static & constant members

		/// <summary>
		/// Значение: <see cref="StringComparison.Ordinal"/>.
		/// </summary>
		public static readonly StringComparison DefaultComparison = StringComparison.Ordinal;

		#endregion

		StringComparison? _comparison;
		[DataMember(Order = 0, Name = nameof(Comparison), IsRequired = true)]
		string P_Comparison_DataMember {
			get {
				var comparison = _comparison;
				if (comparison.HasValue)
					return Enum.GetName(enumType: typeof(StringComparison), value: comparison.Value);
				else
					return null;

			}
			set {
				try {
					_comparison = EnumUtilities.Parse<StringComparison>(value);
				}
				catch (Exception exception) {
					throw new MemberDeserializationException(GetType(), nameof(Comparison), exception);
				}
			}
		}

		protected StringCriterionOperatorBase()
			: this(comparison: null, isReadOnly: false) { }

		public StringCriterionOperatorBase(StringComparison? comparison, bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			_comparison = comparison;
		}

		protected StringCriterionOperatorBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public StringComparison? Comparison {
			get => _comparison;
			set {
				EnsureNotReadOnly();
				_comparison = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out CriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected abstract void CreateReadOnlyCopy(out StringCriterionOperatorBase readOnlyCopy);

		public new StringCriterionOperatorBase AsReadOnly()
			=> (StringCriterionOperatorBase)base.AsReadOnly();

		IStringCriterionOperator IStringCriterionOperator.AsReadOnly()
			=> AsReadOnly();

	}

}