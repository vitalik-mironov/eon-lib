using System;
using System.Collections;

namespace Eon {

	public static class TupleUtilities {

		#region Nested types

		public interface IDisposeNotifyingTuple<out T1, out T2>
			:IDisposeNotifying, IStructuralComparable, IStructuralEquatable, IComparable {

			T1 Item1 { get; }

			T2 Item2 { get; }

		}

		sealed class P_DisposeNotifyingTuple<T1, T2>
			:DisposeNotifying, IDisposeNotifyingTuple<T1, T2> {

			readonly Tuple<T1, T2> _innerTuple;

			internal P_DisposeNotifyingTuple(T1 item1, T2 item2) {
				_innerTuple = new Tuple<T1, T2>(item1: item1, item2: item2);
				//
				var item1AsIDisposeNotifying = item1 as IDisposeNotifying;
				if (!(item1AsIDisposeNotifying is null))
					item1AsIDisposeNotifying.AfterDisposed += P_EH_Item_AfterDisposed;
				var item2AsIDisposeNotifying = item2 as IDisposeNotifying;
				if (!(item2AsIDisposeNotifying is null) && !ReferenceEquals(item1AsIDisposeNotifying, item2AsIDisposeNotifying))
					item2AsIDisposeNotifying.AfterDisposed += P_EH_Item_AfterDisposed;
			}

			public T1 Item1
				=> _innerTuple.Item1;

			public T2 Item2
				=> _innerTuple.Item2;

			void P_EH_Item_AfterDisposed(object sender, DisposeEventArgs eventArgs) {
				eventArgs.EnsureNotNull(nameof(eventArgs));
				//
				if (eventArgs.ExplicitDispose)
					Dispose();
			}

			public int CompareTo(object other, IComparer comparer)
				=> ((IStructuralComparable)_innerTuple).CompareTo(other: other, comparer: comparer);

			public int CompareTo(object obj)
				=> ((IComparable)_innerTuple).CompareTo(obj: obj);

			public bool Equals(object other, IEqualityComparer comparer)
				=> ((IStructuralEquatable)_innerTuple).Equals(other: other, comparer: comparer);

			public int GetHashCode(IEqualityComparer comparer)
				=> ((IStructuralEquatable)_innerTuple).GetHashCode(comparer: comparer);

			public override bool Equals(object obj)
				=> _innerTuple.Equals(obj: obj);

			public override int GetHashCode()
				=> _innerTuple.GetHashCode();

		}

		#endregion

		public static Tuple<T1, T2, T3> Tuple<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
			=> new Tuple<T1, T2, T3>(item1, item2, item3);

		public static Tuple<T1, T2> Tuple<T1, T2>(T1 item1, T2 item2)
			=> new Tuple<T1, T2>(item1, item2);

		public static Tuple<T1> Tuple<T1>(T1 item1)
			=> new Tuple<T1>(item1);

		public static IDisposeNotifyingTuple<T1, T2> DisposeNotifyingTuple<T1, T2>(T1 item1, T2 item2)
			=> new P_DisposeNotifyingTuple<T1, T2>(item1: item1, item2: item2);

	}

}