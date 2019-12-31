using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Eon.Collections;
using Eon.Linq;
using Eon.Runtime.Serialization;
using Eon.Text;

using Newtonsoft.Json;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Diagnostics {

	/// <summary>
	/// Represents exception immutable DTO.
	/// </summary>
	[DataContract]
	public class ExceptionInfo {

		#region Nested types

		readonly struct P_ExceptionDiscoveryItemState {

			public readonly Exception SourceException;

			public readonly List<ExceptionInfo> ParsedInnerExceptions;

			public P_ExceptionDiscoveryItemState(Exception sourceException) {
				sourceException.EnsureNotNull(nameof(sourceException));
				//
				SourceException = sourceException;
				ParsedInnerExceptions = new List<ExceptionInfo>();
			}

		}

		#endregion

		#region Static members

		static bool P_TryGetInnerExceptionInfo(Exception exception, out ExceptionInfo innerExceptionInfo) {
			exception.EnsureNotNull(nameof(exception));
			//
			ExceptionInfo locInnerExceptionInfo;
			var innerExceptionInfoProvider = exception as IInnerExceptionInfoProvider;
			if (innerExceptionInfoProvider is null)
				locInnerExceptionInfo = null;
			else
				locInnerExceptionInfo = innerExceptionInfoProvider.InnerExceptionInfo;
			innerExceptionInfo = locInnerExceptionInfo;
			return locInnerExceptionInfo != null;
		}

		static ListReadOnlyWrap<ExceptionInfo> P_DiscoverInnerExceptions(Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			if (P_TryGetInnerExceptionInfo(exception: exception, innerExceptionInfo: out var innerExceptionInfo))
				return new ListReadOnlyWrap<ExceptionInfo>(list: new ExceptionInfo[ ] { innerExceptionInfo });
			else {
				List<ExceptionInfo> currentParsedInnerInfoList, resultBuffer;
				P_ExceptionDiscoveryItemState currentQueueEntry, newQueueEntry;
				IExceptionInfoTranslator currentInfoTranslator;
				//
				var discoveryQueue = new Queue<P_ExceptionDiscoveryItemState>();
				discoveryQueue.Enqueue(new P_ExceptionDiscoveryItemState(sourceException: exception));
				resultBuffer = discoveryQueue.Peek().ParsedInnerExceptions;
				//
				for (; discoveryQueue.Count > 0;) {
					currentQueueEntry = discoveryQueue.Dequeue();
					currentParsedInnerInfoList = currentQueueEntry.ParsedInnerExceptions;
					if (P_TryGetInnerExceptionInfo(currentQueueEntry.SourceException, out innerExceptionInfo))
						currentParsedInnerInfoList.Add(innerExceptionInfo);
					else {
						var innerExceptions = currentQueueEntry.SourceException.SelectInner(flatAggregateException: true);
						foreach (var currentInnerException in innerExceptions) {
							currentInfoTranslator = ExceptionInfoUtilities.SelectTranslator(currentInnerException);
							if (currentInfoTranslator == null) {
								newQueueEntry = new P_ExceptionDiscoveryItemState(sourceException: currentInnerException);
								currentParsedInnerInfoList.Add(item: new ExceptionInfo(exception: currentInnerException, innerExceptions: new ListReadOnlyWrap<ExceptionInfo>(list: newQueueEntry.ParsedInnerExceptions), flatAggregateException: false));
								discoveryQueue.Enqueue(newQueueEntry);
							}
							else
								currentParsedInnerInfoList.Add(currentInfoTranslator.FromException(currentInnerException));
						}
					}
				}
				return resultBuffer.Count < 1 ? ListReadOnlyWrap<ExceptionInfo>.Empty : new ListReadOnlyWrap<ExceptionInfo>(list: resultBuffer);
			}
		}

		public static string ToString(IEnumerable<ExceptionInfo> infos, ExceptionInfoFormattingOptions options, Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter = default)
			=> ToString(infos: infos, options: options, exceptionInfoDelimiter: Environment.NewLine, singleInfoFormatter: singleInfoFormatter);

		public static string ToString(IEnumerable<ExceptionInfo> infos, ExceptionInfoFormattingOptions options, string exceptionInfoDelimiter, Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter = default) {
			infos.EnsureNotNull(nameof(infos));
			exceptionInfoDelimiter.EnsureNotNull(nameof(exceptionInfoDelimiter));
			//
			options = options == ExceptionInfoFormattingOptions.Default ? ExceptionInfoFormattingOptions.IncludeMessage | ExceptionInfoFormattingOptions.IncludeNumberingMarker : options;
			var includeNumbering = (options & ExceptionInfoFormattingOptions.IncludeNumberingMarker) == ExceptionInfoFormattingOptions.IncludeNumberingMarker;
			var includeType = (options & ExceptionInfoFormattingOptions.IncludeType) == ExceptionInfoFormattingOptions.IncludeType;
			var includeErrorCodeDescription = (options & ExceptionInfoFormattingOptions.IncludeErrorCodeDescription) == ExceptionInfoFormattingOptions.IncludeErrorCodeDescription;
			var includeErrorCodeIdentifier = (options & ExceptionInfoFormattingOptions.IncludeErrorCodeIdentifier) == ExceptionInfoFormattingOptions.IncludeErrorCodeIdentifier;
			var includeMessage = (options & ExceptionInfoFormattingOptions.IncludeMessage) == ExceptionInfoFormattingOptions.IncludeMessage;
			var includeStackTrace = (options & ExceptionInfoFormattingOptions.IncludeStackTrace) == ExceptionInfoFormattingOptions.IncludeStackTrace;
			var includeInnerExceptions = (options & ExceptionInfoFormattingOptions.ExcludeInnerExceptions) != ExceptionInfoFormattingOptions.ExcludeInnerExceptions;
			var traversal =
				new {
					NumberingMarker = string.Empty,
					Info = (ExceptionInfo)null
				}
				.Sequence(length: 0)
				.ToLinkedList();
			var numberingCounter = 0;
			foreach (var info in infos.Arg(nameof(infos)).EnsureNoNullElements().Value)
				traversal
					.AddLast(
						new {
							NumberingMarker = includeNumbering ? (++numberingCounter).ToString("d") : string.Empty,
							Info = info
						});
			if (includeNumbering && traversal.Count == 1)
				traversal.First.Value =
					new {
						NumberingMarker = string.Empty,
						Info = traversal.First.Value.Info
					};
			using (var resultBuffer = StringBuilderUtilities.AcquireBuffer()) {
				var singleInfoPartTextBuilder = new StringBuilder();
				var singleInfoTextBuilder = new StringBuilder();
				var currentTraverseNode = traversal.First;
				for (; currentTraverseNode != null;) {
					var currentTraverseNodeValue = currentTraverseNode.Value;
					// Формирование текста.
					//
					singleInfoTextBuilder.Clear();
					singleInfoPartTextBuilder.Clear();
					var includedPartsCounter = 0;
					// Нумерация.
					//
					if (includeNumbering && !string.IsNullOrEmpty(currentTraverseNodeValue.NumberingMarker)) {
						singleInfoTextBuilder.AppendFormat("{0})", currentTraverseNodeValue.NumberingMarker);
						includedPartsCounter++;
					}
					// Код ошибки.
					//
					if (includeErrorCodeIdentifier || includeErrorCodeDescription) {
						var errorCode = currentTraverseNodeValue.Info.ErrorCode;
						if (!(errorCode is null)) {
							if (includeErrorCodeIdentifier)
								singleInfoPartTextBuilder.Append(errorCode.Identifier);
							string errorCodeDescription;
							if (includeErrorCodeDescription && !(string.IsNullOrEmpty(errorCodeDescription = errorCode.Description) || (errorCodeDescription.EqualsOrdinalCI(currentTraverseNodeValue.Info.Message) && includeMessage)))
								singleInfoPartTextBuilder.Append((singleInfoPartTextBuilder.Length < 1 ? string.Empty : ", ") + errorCodeDescription);
							else if (singleInfoPartTextBuilder.Length > 0 && options != ExceptionInfoFormattingOptions.IncludeErrorCodeIdentifier)
								singleInfoPartTextBuilder.Append(".");
							if (singleInfoPartTextBuilder.Length > 0) {
								singleInfoTextBuilder.Append(includedPartsCounter > 0 ? " " : string.Empty);
								singleInfoTextBuilder.Append(singleInfoPartTextBuilder.ToString());
								includedPartsCounter++;
								singleInfoPartTextBuilder.Clear();
							}
						}
					}
					// Тип исключения и(или) сообщение.
					//
					if (includeType || includeMessage) {
						if (includeType)
							singleInfoPartTextBuilder.Append(currentTraverseNodeValue.Info.TypeName);
						//
						if (includeMessage) {
							if (singleInfoPartTextBuilder.Length > 0)
								singleInfoPartTextBuilder.Append(new[ ] { ':', ' ' });
							singleInfoPartTextBuilder.Append(currentTraverseNodeValue.Info.Message);
						}
						else if (singleInfoPartTextBuilder.Length > 0)
							singleInfoPartTextBuilder.Append('.');
						if (singleInfoPartTextBuilder.Length > 0) {
							singleInfoTextBuilder.Append(includedPartsCounter > 0 ? " " : string.Empty);
							singleInfoTextBuilder.Append(singleInfoPartTextBuilder.ToString());
							includedPartsCounter++;
							singleInfoPartTextBuilder.Clear();
						}
					}
					// Трасса стека.
					//
					if (includeStackTrace) {
						var stackTrace = currentTraverseNodeValue.Info.StackTrace;
						if (!string.IsNullOrEmpty(stackTrace)) {
							if (includedPartsCounter > 0)
								singleInfoTextBuilder.AppendLine();
							singleInfoTextBuilder.AppendLine("-- Begin of stack trace --");
							singleInfoTextBuilder.AppendLine(stackTrace);
							singleInfoTextBuilder.Append("-- End of stack trace --");
						}
					}
					// Вызов пользовательского форматтера (если он задан).
					//
					var singleInfoText =
						singleInfoFormatter == null
						? singleInfoTextBuilder.ToString()
						: singleInfoFormatter(currentTraverseNodeValue.Info, singleInfoTextBuilder);
					// Поместить результат форматирования одного объекта (singleInfoText) в итоговый результат.
					//
					if (!string.IsNullOrEmpty(singleInfoText)) {
						if (resultBuffer.StringBuilder.Length > 0)
							resultBuffer.StringBuilder.Append(exceptionInfoDelimiter);
						resultBuffer.StringBuilder.Append(singleInfoText);
					}
					// Постановка в очередь обработки внутренних исключений.
					//
					if (includeInnerExceptions) {
						numberingCounter = 0;
						foreach (var innerExceptionInfo in currentTraverseNodeValue.Info.InnerExceptions.SkipNull())
							traversal
								.AddBefore(
									currentTraverseNode,
									new {
										NumberingMarker = includeNumbering ? currentTraverseNodeValue.NumberingMarker + (string.IsNullOrEmpty(currentTraverseNodeValue.NumberingMarker) ? string.Empty : ".") + (++numberingCounter).ToString(format: "d", provider: CultureInfo.InvariantCulture) : string.Empty,
										Info = innerExceptionInfo
									});
					}
					//
					traversal.Remove(currentTraverseNode);
					currentTraverseNode = traversal.First;
				}
				return resultBuffer.StringBuilder.ToString();
			}
		}

		#endregion

		[DataMember(Name = nameof(ErrorCode), IsRequired = false, Order = 0, EmitDefaultValue = false)]
		ErrorCode _errorCode;

		[DataMember(Name = nameof(TypeName), IsRequired = true, Order = 1)]
		string _typeName;

		[DataMember(Name = nameof(Message), IsRequired = false, Order = 2, EmitDefaultValue = false)]
		string _message;

		[DataMember(Name = nameof(StackTrace), IsRequired = false, Order = 3, EmitDefaultValue = false)]
		string _stackTrace;

		ListReadOnlyWrap<ExceptionInfo> _innerExceptions;
		[DataMember(Name = nameof(InnerExceptions), IsRequired = false, Order = 4, EmitDefaultValue = false)]
		IEnumerable<ExceptionInfo> P_InnerExceptions_DataMember {
			get {
				var result = _innerExceptions;
				return result is null || result.Count < 1 ? null : result;
			}
			set => _innerExceptions = ListReadOnlyWrap<ExceptionInfo>.WrapOrEmpty(list: (value?.SkipNull()).EmptyIfNull().ToArray());
		}

		Exception _sourceException;

		ValueHolderClass<SeverityLevel?> _mostHighSeverityLevel;

		ValueHolderClass<SeverityLevel?> _mostLowSeverityLevel;

		ExceptionInfo _parent;

		/// <summary>
		/// Initializes a new instance of the <see cref="Eon.Diagnostics.ExceptionInfo"/>.
		/// </summary>
		/// <param name="typeName">The name of type of the exception.</param>
		/// <param name="message">The message specified in the exception.</param>
		/// <param name="stackTrace">The stack trace specified in the exception.</param>
		/// <param name="innerExceptions">The inner exceptions included in the exception. Can be null as well as an each element of the array can be null, but these null elements will be skipped.</param>
		public ExceptionInfo(string typeName, string message, string stackTrace, params ExceptionInfo[ ] innerExceptions)
			: this(errorCode: null, typeName: typeName, message: message, stackTrace: stackTrace, innerExceptions: innerExceptions) { }

		public ExceptionInfo(ErrorCode errorCode, string typeName, string message, string stackTrace, params ExceptionInfo[ ] innerExceptions) {
			typeName.Arg(nameof(typeName)).EnsureNotNullOrWhiteSpace();
			//
			_errorCode = errorCode;
			_typeName = typeName;
			_message = message;
			_stackTrace = stackTrace;
			_innerExceptions = ListReadOnlyWrap<ExceptionInfo>.WrapOrEmpty(list: (innerExceptions?.SkipNull()).EmptyIfNull().ToArray());
			//
			if (_innerExceptions.Count > 0)
				P_SetParentForInnerExceptions(deep: false);
		}

		public ExceptionInfo(Exception exception)
			: this(exception: exception, innerExceptions: null, flatAggregateException: true) { }

		[JsonConstructor]
		protected ExceptionInfo(SerializationContext ctx) { }

		public ExceptionInfo() {
			_errorCode = null;
			_innerExceptions = ListReadOnlyWrap<ExceptionInfo>.Empty;
			_message = string.Empty;
			_stackTrace = string.Empty;
			_typeName = string.Empty;
			_sourceException = null;
			_parent = null;
		}

		ExceptionInfo(Exception exception, ListReadOnlyWrap<ExceptionInfo> innerExceptions, bool flatAggregateException) {
			exception.EnsureNotNull(nameof(exception));
			//
			if (flatAggregateException)
				exception = (exception as AggregateException)?.Flatten() ?? exception;
			//
			_sourceException = exception;
			_innerExceptions = innerExceptions ?? P_DiscoverInnerExceptions(exception: exception);
			var errorCode = exception.GetErrorCode();
			_errorCode = errorCode is null ? null : new ErrorCode(other: errorCode);
			_typeName = exception.GetType().FullName;
			_message = exception.Message;
			_stackTrace = exception.StackTrace;
			//
			if (innerExceptions is null)
				P_SetParentForInnerExceptions(deep: true);
			else if (innerExceptions.Count != 0)
				P_SetParentForInnerExceptions(deep: false);
		}

		public ExceptionInfo Parent
			=> _parent;

		public int InnerExceptionsCount
			=> _innerExceptions.Count;

		public IEnumerable<ExceptionInfo> InnerExceptions
			=> _innerExceptions;

		public ErrorCode ErrorCode
			=> _errorCode;

		public string TypeName
			=> _typeName;

		public string Message
			=> _message;

		public string StackTrace
			=> _stackTrace;

		public virtual string GetFullMessage(Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter = null)
			=> ToString(options: ExceptionInfoFormattingOptions.IncludeMessage | ExceptionInfoFormattingOptions.IncludeNumberingMarker, singleInfoFormatter: singleInfoFormatter);

		public virtual IEnumerable<ExceptionInfo> BaseWithErrorCode
			=> SelectBase(selector: locItem => locItem.ErrorCode != null);

		public virtual IEnumerable<ExceptionInfo> Base
			=> this.Sequence().TreeNodes().Leafs(childrenSelector: locExceptionInfo => locExceptionInfo.InnerExceptions);

		public virtual IEnumerable<ExceptionInfo> SelectBase(Func<ExceptionInfo, bool> selector) {
			selector.EnsureNotNull(nameof(selector));
			//
			var visitedInfos = new HashSet<ExceptionInfo>(comparer: ReferenceEqualityComparer<ExceptionInfo>.Instance);
			var selectedBaseInfos = new List<ExceptionInfo>();
			foreach (var baseInfo in Base) {
				foreach (var info in baseInfo.TreeNode().SelfAndAncestors(parentSelector: locInfo => locInfo.Parent)) {
					if (visitedInfos.Add(info)) {
						if (selector(info)) {
							selectedBaseInfos.Add(info);
							break;
						}
					}
					else
						break;
				}
			}
			return selectedBaseInfos;
		}

		public virtual string GetBaseMessage(Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter = default)
			=> GetBaseMessage(options: ExceptionInfoFormattingOptions.IncludeMessage | ExceptionInfoFormattingOptions.IncludeNumberingMarker, singleInfoFormatter: singleInfoFormatter);

		public string GetBaseMessage(ExceptionInfoFormattingOptions options, string exceptionInfoDelimiter, Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter = default)
			=> ToString(infos: Base, options: options, exceptionInfoDelimiter: exceptionInfoDelimiter, singleInfoFormatter: singleInfoFormatter);

		public string GetBaseMessage(ExceptionInfoFormattingOptions options, Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter = null)
			=> GetBaseMessage(options: options, exceptionInfoDelimiter: Environment.NewLine, singleInfoFormatter: singleInfoFormatter);

		public int GetSubtreeHeight() {
			// TODO: В дальнейшем сделать вычисляемым на этапе конструирования объекта.
			// Высота поддерева определяется как максимальная длина пути до листа дерева, представляемого данным объектом.
			// 
			var traversalBuffer = new { Depth = 0, Info = this }.Sequence().ToLinkedList();
			var maxDepth = 0;
			//
			var currentTraverseNode = traversalBuffer.First;
			for (; currentTraverseNode != null;) {
				foreach (var currentInnerInfo in currentTraverseNode.Value.Info.InnerExceptions.SkipNull()) {
					int currentInnerExceptionDepth;
					checked { currentInnerExceptionDepth = currentTraverseNode.Value.Depth + 1; }
					maxDepth = Math.Max(currentInnerExceptionDepth, maxDepth);
					if (currentInnerInfo.InnerExceptionsCount > 0)
						traversalBuffer.AddBefore(node: currentTraverseNode, value: new { Depth = currentInnerExceptionDepth, Info = currentInnerInfo });
				}
				traversalBuffer.Remove(currentTraverseNode);
				currentTraverseNode = traversalBuffer.First;
			}
			checked { return maxDepth + 1; }
		}

		public Exception SourceException
			=> _sourceException;

		public SeverityLevel? GetMostHighSeverityLevel() {
			return (itrlck.Get(ref _mostHighSeverityLevel) ?? itrlck.UpdateIfNull(location: ref _mostHighSeverityLevel, value: evaluate())).Value;
			//
			ValueHolderClass<SeverityLevel?> evaluate() {
				try {
					return new ValueHolderClass<SeverityLevel?>(value: this.TreeNode().SelfAndDescendants(i => i.InnerExceptions).Select(i => i.ErrorCode?.SeverityLevel).Where(i => i.HasValue).Max());
				}
				catch (Exception locException) {
					return new ValueHolderClass<SeverityLevel?>(exception: locException);
				}
			}
		}

		public SeverityLevel? GetMostLowSeverityLevel() {
			return (itrlck.Get(ref _mostLowSeverityLevel) ?? itrlck.UpdateIfNull(location: ref _mostLowSeverityLevel, value: evaluate())).Value;
			//
			ValueHolderClass<SeverityLevel?> evaluate() {
				try {
					return new ValueHolderClass<SeverityLevel?>(value: this.TreeNode().SelfAndDescendants(i => i.InnerExceptions).Select(i => i.ErrorCode?.SeverityLevel).Where(i => i.HasValue).Min());
				}
				catch (Exception locException) {
					return new ValueHolderClass<SeverityLevel?>(exception: locException);
				}
			}
		}

		void P_SetParentForInnerExceptions(bool deep) {
			if (deep) {
				foreach (var innerException in InnerExceptions)
					innerException.P_SetParentForInnerExceptions(deep: true);
			}
			//
			foreach (var innerException in InnerExceptions)
				innerException._parent = this;
		}

		[OnDeserialized]
		protected virtual void OnDeserialized(StreamingContext context) {
			_innerExceptions = _innerExceptions ?? ListReadOnlyWrap<ExceptionInfo>.Empty;
			P_SetParentForInnerExceptions(deep: false);
		}

		public virtual string ToString(ExceptionInfoFormattingOptions options, string exceptionInfoDelimiter, Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter = default)
			=> ToString(infos: this.Sequence(), options: options, exceptionInfoDelimiter: exceptionInfoDelimiter, singleInfoFormatter: singleInfoFormatter);

		public string ToString(ExceptionInfoFormattingOptions options, Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter = default)
			=> ToString(options: options, exceptionInfoDelimiter: Environment.NewLine, singleInfoFormatter: singleInfoFormatter);

		public string ToString(Func<ExceptionInfo, StringBuilder, string> singleInfoFormatter)
			=> GetFullMessage(singleInfoFormatter: singleInfoFormatter);

		public override string ToString()
			=> GetFullMessage();

	}

}