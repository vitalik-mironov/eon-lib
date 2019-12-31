using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eon.Collections {

	public static class LinkedListUtilities {

		public static bool TryRemove<T>(this LinkedListNode<T> node) {
			node.EnsureNotNull(nameof(node));
			//
			var list = node.List;
			if (list is null)
				return false;
			else
				try {
					list.Remove(node);
					return true;
				} catch (InvalidOperationException) {
					return false;
				}
		}

		public static LinkedList<T> Copy<T>(this LinkedList<T> source) {
			if (source is null)
				return null;
			else {
				var copy = new LinkedList<T>();
				var node = source.First;
				for (; node != null;) {
					copy.AddLast(node.Value);
					node = node.Next;
				}
				return copy;
			}
		}

		public static LinkedList<T> AddRangeLast<T>(this LinkedList<T> list, IEnumerable<T> range) {
			list.EnsureNotNull(nameof(list));
			//
			if (!(range is null))
				foreach (var item in range)
					list.AddLast(item);
			//
			return list;
		}

		public static void ForEach<T>(this LinkedList<T> list, Action<T> action) {
			list.EnsureNotNull(nameof(list));
			action.EnsureNotNull(nameof(action));
			//
			var node = list.First;
			for (; node != null;) {
				action(node.Value);
				node = node.Next;
			}
		}

	}

}