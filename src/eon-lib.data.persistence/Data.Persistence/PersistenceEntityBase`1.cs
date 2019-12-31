using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;

using Eon.ComponentModel.Properties.Annotations;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Data.Persistence {

	[DataContract]
	[DebuggerDisplay("{ToString()}")]
	[PropertyValueCopyTypeOptions]
	public abstract class PersistenceEntityBase<TReferenceKey>
		:IPersistenceEntity<TReferenceKey>
		where TReferenceKey : struct {

		#region Static & constant members

		const int __TrueFlag = 1;

		const int __FalseFlag = 0;

		static readonly IEqualityComparer<TReferenceKey> __ReferenceKeyEqualityComparer = EqualityComparer<TReferenceKey>.Default;

		static readonly Type __ReferenceKeyType = typeof(TReferenceKey);

		#endregion

		int _isReferenceKeySetFlag;

		TReferenceKey? _referenceKey;

		long? _etagInt64;

		protected PersistenceEntityBase() {
			_isReferenceKeySetFlag = __FalseFlag;
		}

		public Type ReferenceKeyType
			=> __ReferenceKeyType;

		// TODO: Put strings into the resources.
		//
		[DataMember(Order = 0, IsRequired = true)]
		[PropertyValueCopyOptions]
		public TReferenceKey? ReferenceKey {
			get => _referenceKey;
			set {
				if (value.HasValue) {
					if (Interlocked.CompareExchange(ref _isReferenceKeySetFlag, __TrueFlag, __FalseFlag) == __FalseFlag)
						_referenceKey = value;
					else {
						var referenceKey = _referenceKey;
						if (!(referenceKey.HasValue && __ReferenceKeyEqualityComparer.Equals(x: value.Value, y: referenceKey.Value)))
							throw
								new InvalidOperationException(
									message: $"Ссылочный ключ не может быть изменён.{Environment.NewLine}\tСущность:{this.FmtStr().GNLI2()}");
					}
				}
				else
					throw new ArgumentNullException(paramName: nameof(value));
			}
		}

		[DataMember(Order = 1, IsRequired = true)]
		[PropertyValueCopyOptions(IsNotCopyableFrom = true, IsNotCopyableTo = true)]
		public byte[ ] Etag {
			get => ByteArrayUtilities.GetBytesBigEndian(value: _etagInt64);
			set {
				if (value is null)
					_etagInt64 = null;
				else if (value.Length > 8)
					throw new ArgumentOutOfRangeException(paramName: nameof(value), message: FormatXResource(typeof(Array), "TooBig/ExpectedMaxLength", "8"));
				else
					_etagInt64 = ByteArrayUtilities.ToInt64BigEndian(input: value);
			}
		}

		[IgnoreDataMember]
		[PropertyValueCopyOptions(IsNotCopyableFrom = true, IsNotCopyableTo = true)]
		public long? EtagInt64 {
			get => _etagInt64;
			set => _etagInt64 = value;
		}

		[IgnoreDataMember]
		[NotMapped]
		public bool IsNew
			=> Interlocked.CompareExchange(ref _isReferenceKeySetFlag, __FalseFlag, __FalseFlag) == __FalseFlag;

		// TODO: Put strings into the resources.
		//
		public virtual string ToString(string format, IFormatProvider provider) {
			if (string.IsNullOrEmpty(format)) {
				return
					$"Тип:{GetType().FmtStr().GNLI()}"
					+ $"{Environment.NewLine}Ссылка:{(_referenceKey?.ToString()).FmtStr().GNLI()}";
			}
			else
				throw
					new ArgumentOutOfRangeException(paramName: nameof(format), message: FormatXResource(typeof(FormatException), "InvalidFormatSpecifier", format));
		}

		public sealed override string ToString()
			=> ToString(format: null, provider: null);

	}

}