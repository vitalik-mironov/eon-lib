using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

using Eon.Xml.Schema;

using Newtonsoft.Json;

namespace Eon.Resources.XResource {

	[DataContract(Name = DataContractName, Namespace = DataContractNamespace)]
	public sealed class XResourceTree {

		#region Static & constant members

		public const string DataContractName = "Tree";

		/// <summary>
		/// Значение: <see cref="EonXmlNamespaces.Resources.XResource.Tree.Ns"/>.
		/// </summary>
		public const string DataContractNamespace = EonXmlNamespaces.Resources.XResource.Tree.Ns;

		/// <summary>
		/// Значение: '/'.
		/// </summary>
		public static readonly char PathDelimiter = '/';

		/// <summary>
		/// Значение: '/'.
		/// </summary>
		public static readonly string PathDelimiterString = "/";

		/// <summary>
		/// Значение: <see cref="StringComparer.Ordinal"/>.
		/// </summary>
		public static readonly StringComparer NodeNameComparer = StringComparer.Ordinal;

		#endregion

		Dictionary<string, XResourceTextNode> _nodes;

		// TODO: Put strings into the resources.
		//
		[DataMember(Name = nameof(Nodes), IsRequired = false, Order = 0, EmitDefaultValue = false)]
		IEnumerable<XResourceTextNode> P_Nodes_DataMember {
			get {
				var nodes = _nodes;
				return nodes is null || nodes.Count < 1 ? null : nodes.Values;
			}
			set {
				if (value is null)
					_nodes = null;
				else {
					var nodes = new Dictionary<string, XResourceTextNode>(comparer: NodeNameComparer);
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

		IXResourceService _provider;

		[JsonConstructor]
		XResourceTree() { }

		public IEnumerable<XResourceTextNode> Nodes
			=> _nodes?.Values ?? Enumerable.Empty<XResourceTextNode>();

		public IXResourceService Provider {
			get {
				var provider = _provider;
				if (provider is null)
					throw new InvalidOperationException(message: "This XResource tree is not bound to XResource service provider.");
				else
					return provider;
			}
		}

		public XResourceTextNode TryResolvePath(string path) {
			string[ ] names;
			if (string.IsNullOrEmpty(path))
				throw new ArgumentException(message: Provider.Format(locator: typeof(string), subpath: "CanNotNullOrEmpty"), paramName: nameof(path));
			else if (path[ 0 ] != PathDelimiter)
				throw new ArgumentException(message: Provider.Format(locator: typeof(XResourceTree), subpath: "NodePathNotValid", args: new object[ ] { path }), paramName: nameof(path));
			else if ((names = path.Split(separator: PathDelimiter)).Length < 2)
				throw new ArgumentException(message: Provider.Format(locator: typeof(XResourceTree), subpath: "NodePathNotValid", args: new object[ ] { path }), paramName: nameof(path));
			//
			var nodes = _nodes;
			XResourceTextNode node = default;
			for (var i = 1; i < names.Length; i++) {
				if (names[ i ] == string.Empty)
					throw new ArgumentException(message: Provider.Format(locator: typeof(XResourceTree), subpath: "NodePathNotValid", args: new object[ ] { path }), paramName: nameof(path));
				else if (i == 1)
					nodes?.TryGetValue(key: names[ i ], value: out node);
				else
					node = node.FindNode(name: names[ i ]);
				//
				if (node is null)
					break;
			}
			return node;
		}

		[OnDeserialized]
		void P_OnDeserialized(StreamingContext context) {
			var nodes = _nodes;
			if (!(nodes is null))
				foreach (var node in nodes.Values)
					node.Internal_SetTree(tree: this);
		}

		// TODO: Put strings into the resources.
		//
		public void SetProvider(IXResourceService provider) {
			if (provider is null)
				throw new ArgumentNullException(paramName: nameof(provider));
			//
			var exchangeResult = Interlocked.CompareExchange(location1: ref _provider, value: provider, comparand: null);
			if (!(ReferenceEquals(exchangeResult, provider) || exchangeResult is null))
				throw
					new InvalidOperationException(
						message: "Unable to change the XResource service provider for this XResource tree. The provider set once and cannot be changed.");
		}

	}

}