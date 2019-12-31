using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Newtonsoft.Json;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Resources.XResource {

	[DataContract(Name = DataContractName, Namespace = XResourceTree.DataContractNamespace)]
	public sealed class XResourceTextNode {

		#region Nested types

		sealed class P_ResolveFuncPlace {

			public readonly int Index;

			public readonly int Length;

			public readonly XResourceTextNode Node;

			public readonly ReadOnlyCollection<int> ArgIndecies;

			public P_ResolveFuncPlace(XResourceTextNode node, int index, int length, IList<int> argIndecies) {
				if (node is null)
					throw new ArgumentNullException(nameof(node));
				else if (index < 0)
					throw new ArgumentOutOfRangeException(nameof(index));
				else if (length < 1)
					throw new ArgumentOutOfRangeException(nameof(length));
				//
				Index = index;
				Length = length;
				Node = node;
				if (argIndecies is null)
					ArgIndecies = null;
				else
					ArgIndecies = new ReadOnlyCollection<int>(argIndecies);
			}

		}

		#endregion

		#region Static & constant members

		public const string DataContractName = "Node";

		/// <summary>
		/// Значение: '<![CDATA[(\x24\x28resolve\x28(?<path>(\x2f[_\p{L}]+[\p{L}0-9_-]*)+){1}(\x2c(?<arg>[0-9]+))*\x29\x29)]]>'.
		/// </summary>
		public static readonly Regex ResolveFuncRegex = new Regex(pattern: @"(\x24\x28resolve\x28(?<path>(\x2f[_\p{L}]+[\p{L}0-9_-]*)+){1}(\x2c(?<arg>[0-9]+))*\x29\x29)", options: RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

		/// <summary>
		/// Значение: 'path'.
		/// </summary>
		public static readonly string ResolveFuncPathGroupName = "path";

		/// <summary>
		/// Значение: 'arg'.
		/// </summary>
		public static readonly string ResolveFuncArgGroupName = "arg";

		static readonly object[ ] __EmptyObjectArray = new object[ ] { };

		#endregion

		XResourceTextNode _parent;

		XResourceTree _tree;

		ReadOnlyCollection<P_ResolveFuncPlace> _resolveFuncPlaces;

		[DataMember(Name = nameof(Name), IsRequired = true, Order = 0)]
		string _name;

		[DataMember(Name = nameof(Text), IsRequired = false, Order = 1, EmitDefaultValue = false)]
		string _text;

		Dictionary<string, XResourceTextNode> _nodes;

		[DataMember(Name = nameof(Nodes), IsRequired = false, Order = 2, EmitDefaultValue = false)]
		IEnumerable<XResourceTextNode> P_Nodes_DataMember {
			get {
				var nodes = _nodes;
				return nodes is null || nodes.Count < 1 ? null : nodes.Values;
			}
			set {
				if (value is null)
					_nodes = null;
				else {
					var nodes = new Dictionary<string, XResourceTextNode>(comparer: XResourceTree.NodeNameComparer);
					var index = -1;
					foreach (var node in value) {
						index++;
						if (node is null)
							continue;
						var nodeName = node.Name.EnsureNotNull($"{nameof(value)}[{index:d}].{nameof(node.Name)}").Value;
						try {
							nodes.Add(key: nodeName, value: node);
						}
						catch (ArgumentException exception) {
							throw
								new ArgumentException(
									paramName: $"{nameof(value)}[{index:d}].{nameof(node.Name)}",
									message: $"Specified value is not unique.{Environment.NewLine}\tValue:{Environment.NewLine}\t\t{nodeName}.",
									innerException: exception);
						}
					}
					_nodes = nodes;
				}
			}
		}

		string _formatText;

		[JsonConstructor]
		XResourceTextNode() {
			_name = null;
			_text = null;
			_nodes = null;
			_formatText = null;
		}

		public string Name
			=> _name;

		public bool HasText
			=> !(_text is null);

		// TODO: Put strings into the resources.
		//
		public string Text {
			get {
				var result = _text;
				if (result is null)
					throw new InvalidOperationException(message: $"Node have not a text.{Environment.NewLine}\tNode:{Environment.NewLine}\t\t{ToString()}");
				else
					return result;
			}
		}

		public string Path {
			get {
				var parent = _parent;
				if (parent is null)
					return new string(XResourceTree.PathDelimiter, 1) + _name;
				else
					return parent.Path + XResourceTree.PathDelimiterString + _name;
			}
		}

		public XResourceTextNode Parent
			=> _parent;

		// TODO: Put strings into the resources.
		//
		public XResourceTree Tree {
			get {
				var current = this;
				while (!(current._parent is null))
					current = current._parent;
				var tree = current._tree;
				if (tree is null)
					throw new InvalidOperationException(message: "This XResource tree node is not bound to XResource tree.");
				else
					return tree;
			}
		}

		public IEnumerable<XResourceTextNode> Nodes
			=> _nodes?.Values ?? Enumerable.Empty<XResourceTextNode>();

		public XResourceTextNode FindNode(string name) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException(message: Tree.Provider.Format(locator: typeof(string), subpath: "CanNotNullOrEmpty"), paramName: nameof(name));
			//
			var nodes = _nodes;
			if (nodes is null)
				return null;
			else {
				nodes.TryGetValue(name, out var result);
				return result;
			}
		}

		public string Format(IFormatProvider formatProvider = default, IEnumerable<object> args = default)
			=> P_Format(entryNode: this, node: this, formatProvider: formatProvider, args: args, recursionControl: new HashSet<XResourceTextNode>());

		// TODO: Put strings into the resources.
		//
		string P_Format(XResourceTextNode entryNode, XResourceTextNode node, IEnumerable<object> args, HashSet<XResourceTextNode> recursionControl, IFormatProvider formatProvider = default) {
			if (entryNode is null)
				throw new ArgumentNullException(nameof(entryNode));
			else if (node is null)
				throw new ArgumentNullException(nameof(node));
			else if (recursionControl is null)
				throw new ArgumentNullException(nameof(recursionControl));
			else if (!recursionControl.Add(node))
				throw new XResourceException(message: Tree.Provider.Format(locator: typeof(XResourceTextNode), subpath: "CyclicNodeReferenceOccurred", args: new object[ ] { entryNode }));
			var formatArgs = (args is null ? null : (args as object[ ] ?? args.ToArray())) ?? __EmptyObjectArray;
			var format = node.P_EvaluateResolveFunctions(entryNode: entryNode, args: formatArgs, recursionControl: recursionControl, formatProvider: formatProvider);
			try {
				return string.Format(provider: formatProvider, format: format, args: formatArgs);
			}
			catch (FormatException exception) {
				throw
					new FormatException(
						message: string.Format(formatProvider, "Функция форматирования строки вызвала ошибку. Для форматирования была использована форматная строка '{0}', массив аргументов размером '{1}'.", format, formatArgs.Length),
						innerException: exception);
			}
		}

		string P_EvaluateResolveFunctions(XResourceTextNode entryNode, object[ ] args, HashSet<XResourceTextNode> recursionControl, IFormatProvider formatProvider = default) {
			ReadOnlyCollection<P_ResolveFuncPlace> resolveFuncPlaces;
			var sourceText = P_GetFormatText();
			if (string.IsNullOrWhiteSpace(sourceText) || (resolveFuncPlaces = P_GetResolveFuncPlaces()).Count < 1)
				return sourceText;
			else {
				var buffer = new StringBuilder(value: sourceText);
				var currentOffset = 0;
				foreach (var resolveFuncPlace in resolveFuncPlaces) {
					string currentExpandedText;
					if (resolveFuncPlace.ArgIndecies is null || resolveFuncPlace.ArgIndecies.Count < 1)
						currentExpandedText = P_Format(entryNode: entryNode, node: resolveFuncPlace.Node, formatProvider: formatProvider, args: __EmptyObjectArray, recursionControl: recursionControl);
					else if (args is null || args.Length < 1)
						throw new XResourceException(message: Tree.Provider.Format(locator: typeof(XResourceTextNode), subpath: "InsufficientArgsToExpandNodeReference", args: new object[ ] { this }));
					else
						currentExpandedText =
							P_Format(
								entryNode: entryNode,
								node: resolveFuncPlace.Node,
								formatProvider: formatProvider,
								args:
									resolveFuncPlace
									.ArgIndecies
									.Select(
										selector:
											locArgIndex => {
												if (locArgIndex < args.Length)
													return args[ locArgIndex ];
												else
													throw new XResourceException(message: Tree.Provider.Format(locator: typeof(XResourceTextNode), subpath: "InsufficientArgsToExpandNodeReference", args: new object[ ] { this }));
											}),
								recursionControl: recursionControl);
					buffer.Remove(resolveFuncPlace.Index + currentOffset, resolveFuncPlace.Length);
					buffer.Insert(resolveFuncPlace.Index + currentOffset, currentExpandedText);
					currentOffset = buffer.Length - sourceText.Length;
				}
				return buffer.ToString();
			}
		}

		string P_GetFormatText() {
			var result = itrlck.Get(location: ref _formatText);
			if (result is null) {
				var sourceText = Text;
				var buffer = new StringBuilder(value: sourceText);
				buffer.Replace(@"\r\n", Environment.NewLine);
				buffer.Replace(@"\n", Environment.NewLine);
				buffer.Replace(@"\t", "\t");
				result = buffer.ToString();
				result = Interlocked.CompareExchange(location1: ref _formatText, value: result, comparand: null) ?? result;
			}
			return result;
		}

		// TODO: Put strings into the resources.
		//
		ReadOnlyCollection<P_ResolveFuncPlace> P_GetResolveFuncPlaces() {
			var result = itrlck.Get(location: ref _resolveFuncPlaces);
			if (result is null) {
				var buffer = new List<P_ResolveFuncPlace>();
				var text = Text;
				if (!string.IsNullOrWhiteSpace(text)) {
					var matches = ResolveFuncRegex.Matches(input: text);
					P_ResolveFuncPlace currentFuncPlace;
					XResourceTextNode currentNode;
					foreach (var match in matches.Cast<Match>()) {
						if (!match.Success || match.Groups[ ResolveFuncPathGroupName ] is null || !match.Groups[ ResolveFuncPathGroupName ].Success)
							throw new XResourceException(message: Tree.Provider.Format(locator: typeof(XResourceTextNode), subpath: "InvalidTextFormat", args: new object[ ] { this }));
						var argIndecies = default(int[ ]);
						if (match.Groups[ ResolveFuncArgGroupName ] != null && match.Groups[ ResolveFuncArgGroupName ].Success && match.Groups[ ResolveFuncArgGroupName ].Captures.Count > 0) {
							argIndecies = new int[ match.Groups[ ResolveFuncArgGroupName ].Captures.Count ];
							for (var i = 0; i < argIndecies.Length; i++) {
								argIndecies[ i ] = int.Parse(match.Groups[ ResolveFuncArgGroupName ].Captures[ i ].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
								if (argIndecies[ i ] < 0)
									throw new XResourceException(message: Tree.Provider.Format(locator: typeof(XResourceTextNode), subpath: "InvalidTextFormat", args: new object[ ] { this }));
							}
						}
						currentNode = Tree.TryResolvePath(path: match.Groups[ ResolveFuncPathGroupName ].Value);
						if (currentNode is null)
							throw
								new XResourceException(
									message: $"Node referenced by 'resolve' function not found.{Environment.NewLine}\tReferencing node:{Environment.NewLine}\t\t{this}.{Environment.NewLine}{Tree.Provider.Format(locator: typeof(XResourceTree), subpath: "NodeNotFoundByPath", args: new object[ ] { match.Groups[ ResolveFuncPathGroupName ].Value })}");
						currentFuncPlace = new P_ResolveFuncPlace(index: match.Index, length: match.Length, node: currentNode, argIndecies: argIndecies);
						if (ReferenceEquals(currentFuncPlace.Node, this))
							throw
								new XResourceException(
									message: Tree.Provider.Format(locator: typeof(XResourceTextNode), subpath: "CyclicNodeReferenceOccurred", args: new object[ ] { this }));
						buffer.Add(currentFuncPlace);
					}
				}
				//
				result = new ReadOnlyCollection<P_ResolveFuncPlace>(list: buffer);
				result = Interlocked.CompareExchange(location1: ref _resolveFuncPlaces, value: result, comparand: null) ?? result;
			}
			return result;
		}

		public sealed override string ToString()
			=> Path;

		// TODO: Put strings into the resources.
		//
		internal void Internal_SetTree(XResourceTree tree) {
			if (tree is null)
				throw new ArgumentNullException(paramName: nameof(tree));
			//
			var exchangeResult = Interlocked.CompareExchange(location1: ref _tree, value: tree, comparand: null);
			if (!(ReferenceEquals(exchangeResult, tree) || exchangeResult is null))
				throw
					new InvalidOperationException(
						message: "Unable to change the tree object for this node. The tree object set once and cannot be changed.");
		}

		// TODO: Put strings into the resources.
		//
		internal void Internal_SetParent(XResourceTextNode parent) {
			if (parent is null)
				throw new ArgumentNullException(paramName: nameof(parent));
			//
			var exchangeResult = Interlocked.CompareExchange(location1: ref _parent, value: parent, comparand: null);
			if (!(ReferenceEquals(exchangeResult, parent) || exchangeResult is null))
				throw
					new InvalidOperationException(
						message: "Unable to change the parent of this node. The parent set once and cannot be changed.");
		}

		[OnDeserialized]
		void P_OnDeserialized(StreamingContext context) {
			var nodes = _nodes;
			if (!(nodes is null))
				foreach (var node in nodes.Values)
					node.Internal_SetParent(parent: this);
		}

	}

}