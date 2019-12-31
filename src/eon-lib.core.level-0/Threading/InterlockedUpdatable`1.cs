using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Threading {

	public sealed class InterlockedUpdatable<T>
		where T : class {

		T _value;

		public InterlockedUpdatable(T initialValue) {
			_value = initialValue;
		}

		public void Clear()
			=> itrlck.SetNull(ref _value);

		public UpdateResult<T> Update(Transform<T> transform)
			=> itrlck.Update(location: ref _value, transform: transform);

		public UpdateResult<T> Update(Transform<T> transform, out bool isUpdated)
			=> itrlck.Update(location: ref _value, transform: transform, isUpdated: out isUpdated);

		public UpdateResult<T> Update(Transform2<T> transform)
			=> itrlck.Update(location: ref _value, transform: transform);

		public UpdateResult<T> Update(Transform2<T> transform, out bool isUpdated)
			=> itrlck.Update(location: ref _value, transform: transform, isUpdated: out isUpdated);

		public T Value
			=> itrlck.Get(location: ref _value);

	}

}