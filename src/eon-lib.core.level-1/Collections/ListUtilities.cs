using System.Collections.Generic;

namespace Eon.Collections {

	public static class ListUtilities {

		public static T RemoveItemAt<T>(this IList<T> list, int index) {
			list.EnsureNotNull(nameof(list));
			//
			var removedItem = list[ index ];
			list.RemoveAt(index);
			return removedItem;
		}

		public static T RemoveLast<T>(this IList<T> list) {
			RemoveLastBool(list: list, removed: out var removed);
			return removed;
		}

		public static bool RemoveLastBool<T>(this IList<T> list)
			=> RemoveLastBool(list: list, removed: out _);

		public static bool RemoveLastBool<T>(this IList<T> list, out T removed) {
			list.EnsureNotNull(nameof(list));
			//
			if (list.Count == 0) {
				removed = default;
				return false;
			}
			else {
				var lastIndex = list.Count - 1;
				var locRemoved = list[ lastIndex ];
				list.RemoveAt(index: lastIndex);
				removed = locRemoved;
				return true;
			}
		}

		public static T RemoveFirst<T>(this IList<T> list) {
			RemoveFirstBool(list: list, removed: out var removed);
			return removed;
		}

		public static bool RemoveFirstBool<T>(this IList<T> list)
			=> RemoveFirstBool(list: list, removed: out _);

		public static bool RemoveFirstBool<T>(this IList<T> list, out T removed) {
			list.EnsureNotNull(nameof(list));
			//
			if (list.Count == 0) {
				removed = default;
				return false;
			}
			else {
				var locRemoved = list[ 0 ];
				list.RemoveAt(index: 0);
				removed = locRemoved;
				return true;
			}
		}

		public static IReadOnlyList<T> ReadOnlyWrap<T>(this IList<T> list) {
			if (list is ListReadOnlyWrap<T> wrapper)
				return wrapper;
			else
				return new ListReadOnlyWrap<T>(list: list);
		}

	}

}