using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eon.Linq;

namespace Eon.Metadata.Tree {

	//[DebuggerStepThrough]
	//[DebuggerNonUserCode]
	public static class MetadataTreeNodeUtilities {

		public static bool IsDescendantOf(this IMetadataTreeStructureComponent descendant, IMetadataTreeStructureComponent ancestor) {
			descendant.EnsureNotNull(nameof(descendant));
			ancestor.EnsureNotNull(nameof(ancestor));
			//
			if (ReferenceEquals(descendant, ancestor))
				return false;
			else {
				var parent = descendant.Parent;
				for (; !(parent is null);) {
					if (ReferenceEquals(parent, ancestor))
						return true;
					parent = parent.Parent;
				}
				return false;
			}
		}

		public static IEnumerable<IMetadataTreeNode> Descendants(this IMetadataTreeNode node)
			=> node.EnsureNotNull(nameof(node)).Value.TreeNode().Descendants(i => i?.Children);

		// TODO: Put strings into the resources.
		//
		public static IMetadataTreeElement EnsureHasMetadataElement(this IMetadataTreeNode node) {
			node.EnsureNotNull(nameof(node));
			//
			var element = node.MetadataElement;
			if (element == null)
				throw new EonException($"Для узла дерева метаданных '{node}' не установлен элемент метаданных.");
			else
				return element;
		}

		// TODO: Put strings into the resources.
		//
		public static TRootNode EnsureHasRootOfType<TRootNode>(this IMetadataTreeNode node)
			where TRootNode : class, IMetadataTreeNode {
			node.EnsureNotNull(nameof(node));
			//
			TRootNode rootAsTRootNode;
			var root = node.Root;
			if (root == null)
				throw
					new EonException(
						$"По указанному узлу дерева метаданных не определен корень дерева метаданных.{Environment.NewLine}\tУказанный узел:{node.FmtStr().GNLI2()}");
			else if ((rootAsTRootNode = root as TRootNode) == null)
				throw
					new EonException(
						$"Корневой узел дерева метаданных имеет тип, который не совместим с требуемым типом.{Environment.NewLine}\tКорневой узел:{root.FmtStr().GNLI2()}{Environment.NewLine}\tТип корневого узла:{root.GetType().FmtStr().GNLI2()}{Environment.NewLine}\tТребуемый тип:{typeof(TRootNode).FmtStr().GNLI2()}");
			else
				return rootAsTRootNode;
		}

	}

}