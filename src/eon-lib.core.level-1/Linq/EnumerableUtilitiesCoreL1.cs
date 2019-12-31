using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Collections;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Linq {

	public static class EnumerableUtilitiesCoreL1 {

		#region Nested types

		public readonly struct Partition<T> {

			internal Partition(ListReadOnlyWrap<T> elements, int index) {
				if (index < 0)
					throw new ArgumentOutOfRangeException(paramName: nameof(index));
				else if (elements is null)
					throw new ArgumentNullException(paramName: nameof(elements));
				//
				Index = index;
				Elements = elements;
			}

			public int Index { get; }

			public ListReadOnlyWrap<T> Elements { get; }

			public int Count
				=> (Elements ?? throw ArgumentUtilitiesCoreL1.NewNullReferenceException(varName: nameof(Elements), component: null)).Count;

		}

		public readonly struct TreeNodesUtilitiesHandle<T> {

			readonly IEnumerable<T> _source;

			public TreeNodesUtilitiesHandle(IEnumerable<T> source) {
				source.EnsureNotNull(nameof(source));
				//
				_source = source;
			}

			public IEnumerable<T> Source {
				get {
					var source = _source;
					if (source is null)
						throw
							new InvalidOperationException(
								message: $"Объект не указывает на последовательность узлов (см. св-во '{nameof(Source)}').{Environment.NewLine}\tОбъект:{Environment.NewLine}\t\t{ToString()}");
					else
						return source;
				}
			}

		}

		public readonly struct SingleTreeNodeUtilitiesHandle<T> {

			readonly T _source;

			public SingleTreeNodeUtilitiesHandle(T source) {
				if (source == null)
					throw new ArgumentNullException(paramName: nameof(source));
				//
				_source = source;
			}

			// TODO: Put strings into the resources.
			//
			public T Source {
				get {
					var source = _source;
					if (source == null)
						throw
							new InvalidOperationException(
								message: $"Объект не указывает на узел (св-во '{nameof(Source)}').{Environment.NewLine}\tОбъект:{Environment.NewLine}\t\t{ToString()}");
					else
						return source;
				}
			}

		}

		public struct SequenceNode<T> {

			readonly bool _notEmpty;

			readonly T _value;

			readonly int? _position;

			public SequenceNode(T value, int? position) {
				_value = value;
				_position = position;
				_notEmpty = true;
			}

			public bool IsEmpty
				=> !_notEmpty;

			public T Value {
				get {
					P_EnsureNotEmpty();
					return _value;
				}
			}

			public int? Position {
				get {
					P_EnsureNotEmpty();
					return _position;
				}
			}

			// TODO: Put strings into the resources.
			//
			void P_EnsureNotEmpty() {
				if (!_notEmpty)
					throw new InvalidOperationException($"Узел последовательности является пустым (не представляет реальный элемент к.л. последовательности).{Environment.NewLine}\tУзел:{ToString().FmtStr().GNLI2()}");
			}

			public static implicit operator T(SequenceNode<T> value) {
				try {
					value.P_EnsureNotEmpty();
				}
				catch (Exception exception) {
					throw new InvalidCastException(message: FormatXResource(typeof(InvalidCastException), null, args: null), innerException: exception);
				}
				return value._value;
			}

		}

		class P_TreeNodeTransformState<TSource, TResult> {

			public readonly P_TreeNodeTransformState<TSource, TResult> Parent;

			public readonly TSource Source;

			public TSource[ ] SourceChildren;

			public readonly List<TResult> TransformedSourceChildren;

			public P_TreeNodeTransformState(TSource source) {
				if (source == null)
					throw new ArgumentNullException(paramName: nameof(source));
				//
				Parent = null;
				Source = source;
				SourceChildren = null;
				TransformedSourceChildren = new List<TResult>();
			}

			public P_TreeNodeTransformState(P_TreeNodeTransformState<TSource, TResult> parent, TSource source) {
				parent.EnsureNotNull(nameof(parent));
				if (source == null)
					throw new ArgumentNullException(paramName: nameof(source));
				//
				Parent = parent;
				Source = source;
				SourceChildren = null;
				TransformedSourceChildren = new List<TResult>();
			}

		}

		#endregion

		public static ISet<T> AsReadOnly<T>(this ISet<T> source, Func<T, T> transform = default) {
			source.EnsureNotNull(nameof(source));
			//
			var set =
				new HashSet<T>(
					collection: transform is null ? source : source.Select(transform),
					comparer: (source as HashSet<T>)?.Comparer);
			return new SetReadOnlyWrap<T>(set: set);
		}

		public static IEnumerable<T> Ancestors<T>(this SingleTreeNodeUtilitiesHandle<T> hnd, Func<T, T> parentSelector) {
			parentSelector.EnsureNotNull(nameof(parentSelector));
			//
			var parent = parentSelector(hnd.Source);
			for (; parent != null;) {
				yield return parent;
				parent = parentSelector(parent);
			}
		}

		public static IEnumerable<T> SelfAndAncestors<T>(this SingleTreeNodeUtilitiesHandle<T> hnd, Func<T, T> parentSelector)
			=> Enumerable.Concat(first: hnd.Source.Sequence(), second: Ancestors(hnd: hnd, parentSelector: parentSelector));

		public static IEnumerable<T> Descendants<T>(this SingleTreeNodeUtilitiesHandle<T> hnd, Func<T, IEnumerable<T>> childrenSelector) {
			childrenSelector.EnsureNotNull(nameof(childrenSelector));
			//
			var traversalBuffer = new Queue<T>();
			foreach (var child in childrenSelector(arg: hnd.Source).EmptyIfNull())
				traversalBuffer.Enqueue(child);
			for (; traversalBuffer.Count > 0;) {
				var current = traversalBuffer.Dequeue();
				foreach (var child in childrenSelector(current).EmptyIfNull())
					traversalBuffer.Enqueue(child);
				yield return current;
			}
		}

		public static IEnumerable<T> SelfAndDescendants<T>(this SingleTreeNodeUtilitiesHandle<T> hnd, Func<T, IEnumerable<T>> childrenSelector)
			=> Enumerable.Concat(first: hnd.Source.Sequence(), second: Descendants(hnd: hnd, childrenSelector: childrenSelector));

		/// <summary>
		/// Производит выборку поддеревьев узла, указанного <paramref name="hnd"/>.
		/// <para>Порядок обхода — breadth-first.</para>
		/// </summary>
		/// <typeparam name="T">Тип узла дерева.</typeparam>
		/// <param name="hnd">
		/// Узел.
		/// </param>
		/// <param name="predicate">Предикат выборки поддерева.</param>
		/// <param name="childrenSelector">Селектор дочерных узлов.</param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<T> Subtrees<T>(this SingleTreeNodeUtilitiesHandle<T> hnd, Func<T, bool> predicate, Func<T, IEnumerable<T>> childrenSelector) {
			predicate.EnsureNotNull(nameof(predicate));
			childrenSelector.EnsureNotNull(nameof(childrenSelector));
			//
			if (predicate(arg: hnd.Source))
				yield return hnd.Source;
			else {
				var traversal = new Queue<T>(collection: hnd.Source.Sequence());
				for (; traversal.Count > 0;) {
					var current = traversal.Dequeue();
					foreach (var child in childrenSelector(arg: current).EmptyIfNull()) {
						if (predicate(child))
							yield return child;
						else
							traversal.Enqueue(child);
					}
				}
			}
		}

		/// <summary>
		/// Производит выборку листьев деревьев.
		/// </summary>
		/// <typeparam name="T">Тип элемента дерева.</typeparam>
		/// <param name="hnd">
		/// Корневые узлы деревьев (см. <seealso cref="TreeNodesUtilitiesHandle{T}"/>).
		/// </param>
		/// <param name="childrenSelector">
		/// Функция-селектор дочерних узлов.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <seealso cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<T> Leafs<T>(this TreeNodesUtilitiesHandle<T> hnd, Func<T, IEnumerable<T>> childrenSelector) {
			childrenSelector.EnsureNotNull(nameof(childrenSelector));
			//
			var traversal = new Stack<T>();
			foreach (var sourceItem in hnd.Source) {
				traversal.Push(sourceItem);
				for (; traversal.Count != 0;) {
					var current = traversal.Pop();
					var previousSize = traversal.Count;
					foreach (var child in childrenSelector(arg: current).EmptyIfNull())
						traversal.Push(child);
					if (previousSize == traversal.Count)
						yield return current;
				}
			}
		}


		public static TResult Transform<TSource, TResult>(this SingleTreeNodeUtilitiesHandle<TSource> hnd, Func<TSource, IEnumerable<TSource>> childrenSelector, Func<TSource, IEnumerable<TResult>, TResult> transform) {
			childrenSelector.EnsureNotNull(nameof(childrenSelector));
			transform.EnsureNotNull(nameof(transform));
			//
			var root = new P_TreeNodeTransformState<TSource, TResult>(hnd.Source);
			var traversalStack = new Stack<P_TreeNodeTransformState<TSource, TResult>>(root.Sequence());
			for (; traversalStack.Count > 0;) {
				var currentTraversalEntry = traversalStack.Peek();
				//
				var initialSizeOfTraversalStack = traversalStack.Count;
				if (currentTraversalEntry.SourceChildren == null) {
					currentTraversalEntry.SourceChildren =
						childrenSelector(currentTraversalEntry.Source)
						.EmptyIfNull()
						.ToArray();
					for (var i = currentTraversalEntry.SourceChildren.Length - 1; i > -1; i--)
						traversalStack
							.Push(
								new P_TreeNodeTransformState<TSource, TResult>(
									currentTraversalEntry,
									currentTraversalEntry.SourceChildren[ i ]));
				}
				//
				if (initialSizeOfTraversalStack == traversalStack.Count) {
					traversalStack.Pop();
					if (!ReferenceEquals(currentTraversalEntry, root))
						currentTraversalEntry
							.Parent
							.TransformedSourceChildren
							.Add(transform(currentTraversalEntry.Source, currentTraversalEntry.TransformedSourceChildren));
				}
			}
			return transform(root.Source, root.TransformedSourceChildren);
		}

		/// <summary>
		/// Создает дескриптор доступа к утилитам одиночного узла к.л. дерева.
		/// </summary>
		/// <typeparam name="T">Тип узла дерева.</typeparam>
		/// <param name="node">Узел дерева.</param>
		/// <returns>Значение <seealso cref="SingleTreeNodeUtilitiesHandle{T}"/>.</returns>
		public static SingleTreeNodeUtilitiesHandle<T> TreeNode<T>(this T node)
			=> new SingleTreeNodeUtilitiesHandle<T>(node);

		/// <summary>
		/// Создает дескриптор доступа к утилитам одиночного узла к.л. дерева.
		/// </summary>
		/// <typeparam name="T">Тип узла дерева.</typeparam>
		/// <param name="hnd">Дескриптор доступа к утилитам аргумента, представляющий узел дерева (см. <see cref="ArgumentUtilitiesHandle{T}.Value"/>).</param>
		/// <returns>Значение <seealso cref="SingleTreeNodeUtilitiesHandle{T}"/>.</returns>
		public static SingleTreeNodeUtilitiesHandle<T> TreeNode<T>(this ArgumentUtilitiesHandle<T> hnd)
			=> new SingleTreeNodeUtilitiesHandle<T>(source: hnd.Value);

		public static TreeNodesUtilitiesHandle<T> TreeNodes<T>(this IEnumerable<T> source)
			=> new TreeNodesUtilitiesHandle<T>(source);

		public static IEnumerable<Partition<T>> Split<T>(this IEnumerable<T> source, int maxSize, Func<T, bool> predicate = default)
			=> Split<T, T>(source: source, maxSize: maxSize, predicate: predicate, selector: locItem => locItem);

		public static IEnumerable<Partition<TResult>> Split<TSource, TResult>(this IEnumerable<TSource> source, int maxSize, Func<TSource, TResult> selector, Func<TSource, bool> predicate = default) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			else if (maxSize < 1)
				throw new ArgumentOutOfRangeException(paramName: nameof(maxSize), message: FormatXResource(locator: typeof(ArgumentOutOfRangeException), subpath: "CanNotLessThanOne"));
			else if (selector is null)
				throw new ArgumentNullException(paramName: nameof(selector));
			//
			TResult[ ] portion;
			var partitionUpperBound = maxSize - 1;
			var buffer = new List<TResult>();
			var bufferTailIndex = -1;
			var partitionIndex = -1;
			foreach (var item in source) {
				if (bufferTailIndex == partitionUpperBound) {
					portion = new TResult[ maxSize ];
					buffer.CopyTo(index: 0, array: portion, arrayIndex: 0, count: maxSize);
					bufferTailIndex = -1;
					yield return new Partition<TResult>(elements: ListReadOnlyWrap<TResult>.WrapOrEmpty(list: portion), index: checked(++partitionIndex));
				}
				if (predicate?.Invoke(item) ?? true) {
					if (buffer.Count > ++bufferTailIndex)
						buffer[ bufferTailIndex ] = selector(item);
					else
						buffer.Add(selector(item));
				}
			}
			if (bufferTailIndex > -1) {
				portion = new TResult[ bufferTailIndex + 1 ];
				buffer.CopyTo(index: 0, array: portion, arrayIndex: 0, count: bufferTailIndex + 1);
				buffer.Clear();
				yield return new Partition<TResult>(elements: ListReadOnlyWrap<TResult>.WrapOrEmpty(list: portion), index: checked(++partitionIndex));
			}
			else
				buffer.Clear();
		}

		// TODO: Put exception messages into the resources.
		//
		public static Partition<T>[ ] Split<T>(this IEnumerable<T> source, int count, Func<T, int> partitioner) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			else if (count < 1)
				throw new ArgumentOutOfRangeException(paramName: nameof(count), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanOne"));
			else if (partitioner is null)
				throw new ArgumentNullException(paramName: nameof(partitioner));
			var partitionsBuffer = new List<T>[ count ];
			for (var i = 0; i < partitionsBuffer.Length; i++)
				partitionsBuffer[ i ] = new List<T>();
			var partitionMaxIndex = count - 1;
			var itemIndex = -1;
			foreach (var item in source) {
				itemIndex++;
				var partitionBufferIndex = partitioner(item);
				if (partitionBufferIndex < 0 || partitionBufferIndex > partitionMaxIndex)
					throw
						new EonException(message: $"Указанная функция определения последовательного индекса секции возвратила недопустимый индекс секции '{partitionBufferIndex.ToString("d")}' для указанного элемента последовательности (позиция элемента {itemIndex.ToString("d")}). Возвращенный индекс не попадает в ожидаемый диапазон [0; {partitionMaxIndex.ToString("d")}]{Environment.NewLine}\tЭлемент:{item.FmtStr().GNLI2()}");
				partitionsBuffer[ partitionBufferIndex ].Add(item);
			}
			var result = new Partition<T>[ partitionsBuffer.Length ];
			for (var i = 0; i < partitionsBuffer.Length; i++)
				result[ i ] = new Partition<T>(elements: new ListReadOnlyWrap<T>(readOnlyList: partitionsBuffer[ i ]), index: i);
			return result;
		}

		public static SequenceNode<T> FindFirstNonUnique<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, bool skinNull = default) {
			source.EnsureNotNull(nameof(source));
			keySelector.EnsureNotNull(nameof(keySelector));
			//
			var buffer = new HashSet<TKey>();
			var positionCounter = -1;
			foreach (var item in (skinNull ? source.SkipNull() : source)) {
				positionCounter++;
				if (!buffer.Add(keySelector(item)))
					return new SequenceNode<T>(item, positionCounter);
			}
			return default;
		}

		public static SequenceNode<T> FindFirstNonUnique<T>(this IEnumerable<T> source, bool skinNull = default) {
			source.EnsureNotNull(nameof(source));
			//
			var buffer = new HashSet<T>();
			var positionCounter = -1;
			foreach (var item in (skinNull ? source.SkipNull() : source)) {
				positionCounter++;
				if (!buffer.Add(item))
					return new SequenceNode<T>(item, positionCounter);
			}
			return default;
		}

	}

}