using System.Globalization;
using System.Runtime.Serialization;

namespace Eon.Globalization {

	public sealed class InvariantLocalizable<T>
		:ILocalizable<T>
		where T : class {

		[DataMember(Name = nameof(Value), IsRequired = true, Order = 0)]
		T _value;

		public InvariantLocalizable(T value) {
			_value = value;
		}

		InvariantLocalizable() { }

		public T Value
			=> _value;

		T ILocalizable<T>.GetForCulture(CultureInfo culture)
			=> _value;

	}

}