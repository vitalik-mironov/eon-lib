using System;
using System.Collections.Generic;
using Eon.Linq;

namespace Eon {

	public static class XInstanceScopeUtilities {

		public static IEnumerable<IXInstance> SelectSubscopes(this IXInstance scope, Func<IXInstance, bool, bool> predicate, bool disposeTolerant) {
			scope.EnsureNotNull(nameof(scope));
			predicate.EnsureNotNull(nameof(predicate));
			//
			if (disposeTolerant)
				return scope.TreeNode().Subtrees(predicate: localPredicate, childrenSelector: childrenSelectorDisposeTolerant);
			else
				return scope.TreeNode().Subtrees(predicate: localPredicate, childrenSelector: childrenSelector);
			//
			bool localPredicate(IXInstance locItem) => predicate(arg1: locItem, arg2: disposeTolerant);
			IEnumerable<IXInstance> childrenSelector(IXInstance locScope) => locScope.ScopedInstances;
			IEnumerable<IXInstance> childrenSelectorDisposeTolerant(IXInstance locScope) => locScope.ScopedInstancesDisposeTolerant;
		}

		public static IEnumerable<IXInstance> SelectSubscopes(this IXInstance scope, Func<IXInstance, bool> predicate) {
			predicate.EnsureNotNull(nameof(predicate));
			//
			return SelectSubscopes(scope: scope, predicate: localPredicate, disposeTolerant: false);
			//
			bool localPredicate(IXInstance locItem, bool locDisposeTolerant) => predicate(arg: locItem);
		}

		public static IXInstance RootScope(this IXInstance scope) {
			scope.EnsureNotNull(nameof(scope));
			//
			var root = scope;
			for (var next = root.Scope; next != null; next = root.Scope)
				root = next;
			return root;
		}

		public static IEnumerable<IXInstance> SelectSelfAndDescendantScopes(this IXInstance scope, bool disposeTolerant) {
			scope.EnsureNotNull(nameof(scope));
			//
			if (disposeTolerant)
				return scope.TreeNode().SelfAndDescendants(childrenSelector: childrenSelectorDisposeTolerant);
			else
				return scope.TreeNode().SelfAndDescendants(childrenSelector: childrenSelector);
			//
			IEnumerable<IXInstance> childrenSelector(IXInstance locScope) => locScope.ScopedInstances;
			IEnumerable<IXInstance> childrenSelectorDisposeTolerant(IXInstance locScope) => locScope.ScopedInstancesDisposeTolerant;
		}

		public static IEnumerable<IXInstance> SelectSelfAndDescendantScopes(this IXInstance scope)
			=> SelectSelfAndDescendantScopes(scope: scope, disposeTolerant: false);

		public static IEnumerable<IXInstance> SelectSelfAndAncestorScopes(this IXInstance scope, bool disposeTolerant) {
			scope.EnsureNotNull(nameof(scope));
			//
			if (disposeTolerant)
				return scope.TreeNode().SelfAndAncestors(parentSelector: parentSelectorDisposeTolerant);
			else
				return scope.TreeNode().SelfAndAncestors(parentSelector: parentSelector);
			//
			IXInstance parentSelectorDisposeTolerant(IXInstance locScope) => locScope.ScopeDisposeTolerant;
			IXInstance parentSelector(IXInstance locScope) => locScope.Scope;
		}

		public static IEnumerable<IXInstance> SelectSelfAndAncestorScopes(this IXInstance scope)
			=> SelectSelfAndAncestorScopes(scope: scope, disposeTolerant: false);

	}

}