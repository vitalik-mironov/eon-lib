
namespace Eon {

	public delegate T TransformStateful<T, TState>(T current, ref TState state);

}