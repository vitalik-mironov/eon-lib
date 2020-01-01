using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

using Eon.Globalization;
using Eon.Reflection;
using Eon.Threading;

namespace Eon.Resources.XResource {

	public class DefaultXResourceService
		:IXResourceService {

		#region Nested types

		protected class ResolvedPointer
			:XResourcePointer {

			readonly XResourceTextNode _node;

			readonly CultureInfo _culture;

			protected internal ResolvedPointer(Type locator, CultureInfo culture, string subpath, XResourceTextNode node)
				: base(locator: locator, subpath: subpath) {
				if (culture is null)
					throw new ArgumentNullException(paramName: nameof(culture));
				else if (node is null)
					throw new ArgumentNullException(paramName: nameof(node));
				//
				_node = node;
				_culture = culture;
			}

			public XResourceTextNode Node
				=> _node;

			public CultureInfo Culture
				=> _culture;

		}

		#endregion

		#region Static members

		public static readonly DefaultXResourceService Instance;

		static readonly Type __TypeOfIXResourceSource;

		/// <summary>
		/// Value: <see cref="CultureNameUtilities.Comparer"/>.
		/// </summary>
		protected static readonly StringComparer CultureTreeRepoKeyComparer;

		static DefaultXResourceService() {
			__TypeOfIXResourceSource = typeof(IXResourceSourcePointer);
			CultureTreeRepoKeyComparer = CultureNameUtilities.Comparer;
			Instance = new DefaultXResourceService();
		}

		#endregion

		readonly Dictionary<Type, IXResourceSourcePointer> _sourcePointerByLocatorRepo;

		readonly PrimitiveSpinLock _sourceTypeByLocatorRepoSpinLock;

		protected readonly Dictionary<string, Dictionary<IXResourceSourcePointer, ValueHolderStruct<XResourceTree>>> CultureTreeRepo;

		protected readonly object CultureTreeRepoSyncRoot;

		protected internal DefaultXResourceService() {
			_sourcePointerByLocatorRepo = new Dictionary<Type, IXResourceSourcePointer>();
			_sourceTypeByLocatorRepoSpinLock = new PrimitiveSpinLock();
			//
			CultureTreeRepo = new Dictionary<string, Dictionary<IXResourceSourcePointer, ValueHolderStruct<XResourceTree>>>(comparer: CultureTreeRepoKeyComparer);
			CultureTreeRepoSyncRoot = new object();
		}

		// TODO: Put strings into the resources.
		//
		public virtual IXResourceSourcePointer GetSource(Type locator) {
			locator.EnsureNotNull(nameof(locator));
			//
			var source = default(IXResourceSourcePointer);
			if (_sourceTypeByLocatorRepoSpinLock.Invoke(() => _sourcePointerByLocatorRepo.TryGetValue(key: locator, value: out source)))
				return source;
			else {
				var definedSources = locator.GetCustomAttributes(inherit: false).OfType<IXResourceSourcePointer>().Take(count: 2).ToArray();
				if (definedSources.Length > 1)
					throw
						new XResourceException(
							message: $"Обнаружен конфликт при определении XResource-источника. Тип '{locator}' по меньшей мере дважды аннотирован атрибутами, реализующими тип '{__TypeOfIXResourceSource}': '{definedSources[ 0 ]}' и '{definedSources[ 1 ]}'.");
				else if (definedSources.Length == 1)
					source = definedSources[ 0 ];
				else {
					definedSources = locator.Assembly.GetCustomAttributes().OfType<IXResourceSourcePointer>().Take(count: 2).ToArray();
					if (definedSources.Length > 1)
						throw
							new XResourceException(
								message: $"Обнаружен конфликт при определении XResource-источника. Сборка '{locator.Assembly}' по меньшей мере дважды аннотирована атрибутами, реализующими тип '{__TypeOfIXResourceSource}': '{definedSources[ 0 ]}' и '{definedSources[ 1 ]}'.");
					else if (definedSources.Length == 1)
						source = definedSources[ 0 ];
					else
						source = XResourceUtilities.EonSource;
				}
				//
				_sourceTypeByLocatorRepoSpinLock
					.Invoke(
						action:
							() => {
								if (_sourcePointerByLocatorRepo.TryGetValue(key: locator, value: out var locSource))
									source = locSource;
								else
									_sourcePointerByLocatorRepo.Add(key: locator, value: source);
							});
				return source;
			}
		}

		public bool EnsureLocatorValid(Type locator, bool notThrow = default) {
			locator.EnsureNotNull(nameof(locator));
			//
			var errorMessagePrologue = notThrow ? null : $"Указанный локатор ресурса (XResource) не является валидным.{Environment.NewLine}\tЛокатор:{Environment.NewLine}\t\t{locator}";
			//
			if (locator.IsArray) {
				if (notThrow)
					return false;
				else
					throw
						new ArgumentException(
							message: $"{errorMessagePrologue}{Environment.NewLine}{Format(locator: typeof(Type), subpath: "ArrayTypeNotAllowed", args: new[ ] { locator })}",
							paramName: nameof(locator));
			}
			else if (locator.IsNested) {
				if (notThrow)
					return false;
				else
					throw
						new ArgumentException(
							message: $"{errorMessagePrologue}{Environment.NewLine}{Format(locator: typeof(Type), subpath: "NestedTypeNotAllowed", args: new[ ] { locator })}",
							paramName: nameof(locator));
			}
			else if (locator.IsSpecialName) {
				if (notThrow)
					return false;
				else
					throw
						new ArgumentException(
							message: $"{errorMessagePrologue}{Environment.NewLine}{Format(locator: typeof(Type), subpath: "SpecialNamedTypeNotAllowed", args: new[ ] { locator })}",
							paramName: nameof(locator));
			}
			else if (locator.IsGenericType && !locator.IsGenericTypeDefinition) {
				if (notThrow)
					return false;
				else
					throw
						new ArgumentException(
							message: $"{errorMessagePrologue}{Environment.NewLine}{Format(locator: typeof(Type), subpath: "GenericTypeWithArgsNotAllowed", args: new[ ] { locator })}",
							paramName: nameof(locator));
			}
			else
				return true;
		}

		public Type GetLocator(Type type) {
			type.EnsureNotNull(nameof(type));
			//
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
				type = type.GetGenericTypeDefinition();
			EnsureLocatorValid(locator: type);
			return type;
		}

		public bool TryGetLocator(Type type, out Type locator) {
			type.EnsureNotNull(nameof(type));
			//
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
				type = type.GetGenericTypeDefinition();
			if (EnsureLocatorValid(locator: type, notThrow: true)) {
				locator = type;
				return true;
			}
			else {
				locator = null;
				return false;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected virtual XResourceTree LocateTree(IXResourceSourcePointer source, CultureInfo culture, bool throwIfMissing = default) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			else if (culture is null)
				throw new ArgumentNullException(nameof(culture));
			//
			ValueHolderStruct<XResourceTree> treeHolder;
			lock (CultureTreeRepoSyncRoot) {
				if (!CultureTreeRepo.TryGetValue(key: culture.Name, value: out var cultureTrees))
					CultureTreeRepo.Add(key: culture.Name, value: cultureTrees = new Dictionary<IXResourceSourcePointer, ValueHolderStruct<XResourceTree>>());
				//
				if (!cultureTrees.TryGetValue(key: source, value: out treeHolder)) {
					Stream resourceStream = default;
					try {
						resourceStream = source.GetStream(culture: culture, throwIfMissing: false);
						if (resourceStream is null)
							treeHolder = new ValueHolderStruct<XResourceTree>(value: null);
						else
							using (
								var xmlReader =
									XmlReader
									.Create(
										input: resourceStream,
										settings:
											new XmlReaderSettings() {
												ConformanceLevel = ConformanceLevel.Document,
												DtdProcessing = DtdProcessing.Prohibit,
												IgnoreComments = true,
												IgnoreProcessingInstructions = true,
												CheckCharacters = true
											})) {
								//
								try {
									var serializer = new DataContractSerializer(type: typeof(XResourceTree));
									treeHolder = new ValueHolderStruct<XResourceTree>(value: (XResourceTree)serializer.ReadObject(reader: xmlReader, verifyObjectName: true));
								}
								catch (Exception exception) {
									throw
										new XResourceException(
											message: $"An error occurred while deserializing XResource tree from the resource.{Environment.NewLine}Culture = '{culture.Name}', Source = '{source}'.", innerException: exception);
								}
								treeHolder.Value?.SetProvider(provider: this);
							}
					}
					catch (Exception exception) {
						treeHolder = new ValueHolderStruct<XResourceTree>(exception: exception);
					}
					finally {
						resourceStream?.Dispose();
					}
					cultureTrees.Add(key: source, value: treeHolder);
				}
			}
			var tree = treeHolder.Value;
			if (tree is null) {
				if (throwIfMissing)
					throw new XResourceException(message: $"XResource tree is missing.{Environment.NewLine}Culture = '{culture.Name}', Source = '{source}'.");
				else
					return null;
			}
			else
				return tree;
		}

		protected virtual XResourceTextNode LocateTreeNode(Type locator, CultureInfo culture, string subpath = default, bool throwIfMissing = default) {
			var tree = LocateTree(source: GetSource(locator: locator), culture: culture, throwIfMissing: false);
			var treeNodePath =
				XResourceTree.PathDelimiterString
				+ locator.FullName.Replace(oldChar: EonTypeUtilities.TypeNameDelimiter, newChar: XResourceTree.PathDelimiter)
				+ (string.IsNullOrEmpty(subpath)
					? string.Empty
					: (subpath.StartsWith(value: XResourceTree.PathDelimiterString) ? subpath : XResourceTree.PathDelimiterString + subpath));
			if (tree is null) {
				if (throwIfMissing)
					throw new XResourceException(message: Format(locator: typeof(XResourceTree), subpath: "NodeNotFoundByPath", args: new object[ ] { treeNodePath }));
				else
					return null;
			}
			else {
				var treeNode = tree.TryResolvePath(path: treeNodePath);
				if (treeNode is null) {
					if (throwIfMissing)
						throw new ArgumentException(message: Format(locator: typeof(XResourceTree), subpath: "NodeNotFoundByPath", args: new[ ] { treeNodePath }), paramName: nameof(locator));
					else
						return null;
				}
				else
					return treeNode;
			}
		}

		public virtual bool Locate(Type locator, CultureInfo culture, out IXResourcePointer pointer, string subpath = default) {
			var node = LocateTreeNode(locator: locator, culture: culture, subpath: subpath);
			if (node is null) {
				pointer = null;
				return false;
			}
			else {
				pointer = new ResolvedPointer(locator: locator, culture: culture, subpath: subpath, node: node);
				return true;
			}
		}

		protected virtual string FormatInner(Type locator, CultureInfo culture = default, string subpath = default, bool throwIfMissing = default, IEnumerable<object> args = default) {
			EnsureLocatorValid(locator: locator);
			//
			culture = culture ?? CultureInfo.CurrentCulture;
			var treeNode = LocateTreeNode(locator: locator, culture: culture, subpath: subpath, throwIfMissing: throwIfMissing);
			if (treeNode is null)
				return null;
			else
				return treeNode.Format(formatProvider: culture, args: args);
		}

		public string Format(Type locator, CultureInfo culture = default, string subpath = default, bool throwIfMissing = default, IEnumerable<object> args = default)
			=> FormatInner(locator: locator, subpath: subpath, culture: culture, throwIfMissing: throwIfMissing, args: args);

		public string FormatUI(Type locator, string subpath = default, bool throwIfMissing = default, IEnumerable<object> args = default)
			=> FormatInner(locator: locator, subpath: subpath, culture: CultureInfo.CurrentUICulture, throwIfMissing: throwIfMissing, args: args);

		public virtual string Format(IXResourcePointer pointer, IEnumerable<object> args = default) {
			if (pointer is null)
				throw new ArgumentNullException(paramName: nameof(pointer));
			//
			if (pointer is ResolvedPointer resolvedPointer)
				return resolvedPointer.Node.Format(formatProvider: resolvedPointer.Culture, args: args);
			else
				return Format(locator: pointer.Locator, subpath: pointer.Subpath, args: args);
		}

	}

}