using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Linq;

namespace Eon {

	/// <summary>
	/// Представляет исключение в виде дерева, в котором узлы соответствуют внутренним исключениям.
	/// </summary>
	public sealed class ExceptionTree {

		#region Nested types

		/// <summary>
		/// Представляет узел дерева <seealso cref="ExceptionTree"/>.
		/// </summary>
		public sealed class Node {

			readonly Node _parent;

			readonly Exception _exception;

			readonly List<Node> _children;

			readonly int _level;

			internal Node(Exception exception, out List<Node> leafs) {
				exception.EnsureNotNull(nameof(exception));
				//
				_parent = null;
				_exception = exception;
				_children = new List<Node>();
				_level = 1;
				//
				var locLeafs = new List<Node>();
				var traversalBuffer = this.Sequence().ToLinkedList();
				var traversalBufferNode = traversalBuffer.First;
				for (; traversalBufferNode != null;) {
					var previousTraversalBufferLength = traversalBuffer.Count;
					//
					foreach (var innerException in traversalBufferNode.Value.Exception.SelectInner()) {
						Node child;
						traversalBuffer.AddBefore(traversalBufferNode, child = new Node(traversalBufferNode.Value, innerException));
						traversalBufferNode.Value._children.Add(item: child);
					}
					//
					if (previousTraversalBufferLength == traversalBuffer.Count)
						locLeafs.Add(item: traversalBufferNode.Value);
					//
					traversalBuffer.Remove(traversalBufferNode);
					traversalBufferNode = traversalBuffer.First;
				}
				leafs = locLeafs;
			}

			Node(Node parent, Exception exception) {
				parent.EnsureNotNull(nameof(parent));
				exception.EnsureNotNull(nameof(exception));
				//
				_parent = parent;
				_exception = exception;
				_children = new List<Node>();
				_level = parent._level + 1;
			}

			public Node Parent => _parent;

			public Exception Exception => _exception;

			public IEnumerable<Node> Children => _children.AsEnumerable();

			public IEnumerable<Node> Ancestors {
				get {
					var ascendant = _parent;
					for (; ascendant != null;) {
						yield return ascendant;
						ascendant = ascendant._parent;
					}
				}
			}

			public IEnumerable<Node> SelfAndAncestors
				=> this.Sequence().Concat(Ancestors);

			public IEnumerable<Node> Descendants {
				get {
					var traversalBuffer = _children.ToLinkedList();
					var traversalBufferNode = traversalBuffer.First;
					for (; traversalBufferNode != null;) {
						yield return traversalBufferNode.Value;
						//
						foreach (var descendant in traversalBufferNode.Value._children)
							traversalBuffer.AddBefore(traversalBufferNode, descendant);
						//
						traversalBuffer.Remove(traversalBufferNode);
						traversalBufferNode = traversalBuffer.First;
					}
				}
			}

			public IEnumerable<Node> Siblings {
				get {
					if (_parent != null)
						foreach (var sibling in _parent._children) {
							if (ReferenceEquals(sibling, this))
								continue;
							yield return sibling;
						}
				}
			}

			public int Level
				=> _level;

			public int Degree
				=> _children.Count;

			public bool IsLeaf
				=> _children.Count < 1;

			public int Depth
				=> _level - 1;

			// TODO: Put strings into the resources.
			//
			public override string ToString()
				=> $"{nameof(ExceptionTree)}.{nameof(Node)}:{Environment.NewLine}\t{nameof(Level)}: '{Level:d}'{Environment.NewLine}\t{nameof(IsLeaf)}: '{IsLeaf}'{Environment.NewLine}\tExceptionType: '{_exception.GetType()}'{Environment.NewLine}\tExceptionMessage:{Environment.NewLine}{_exception.Message.IndentLines(indentSize: 2)}";

		}

		#endregion

		readonly Node _root;

		readonly List<Node> _leafs;

		public ExceptionTree(Exception exception) {
			_root = new Node(exception: exception, leafs: out _leafs);
		}

		/// <summary>
		/// Возвращает корень дерева, представляющий исходное исключение, указанное при создании (<seealso cref="ExceptionTree.ExceptionTree(Exception)"/>).
		/// </summary>
		public Node Root
			=> _root;

		/// <summary>
		/// Возвращает листья дерева. Каждый лист дерева представляет базовое исключение исходного, указанного при создании дерева (<seealso cref="ExceptionTree.ExceptionTree(Exception)"/>).
		/// </summary>
		public IEnumerable<Node> Leafs
			=> _leafs.AsEnumerable();

	}

}