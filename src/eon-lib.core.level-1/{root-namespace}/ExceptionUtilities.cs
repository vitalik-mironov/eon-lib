using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Eon.Diagnostics;
using Eon.Linq;
using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	public static class ExceptionUtilities {

		#region Nested types

		sealed class P_ExceptionData {

			readonly IErrorCode _errorCode;

			readonly bool _isObserved;

			internal P_ExceptionData(P_ExceptionData source = default, ArgumentPlaceholder<IErrorCode> errorCode = default, ArgumentPlaceholder<bool> isObserved = default) {
				_errorCode = errorCode.HasExplicitValue ? errorCode.ExplicitValue : source?._errorCode;
				_isObserved = isObserved.HasExplicitValue ? isObserved.ExplicitValue : (source?._isObserved ?? false);
			}

			public IErrorCode ErrorCode
				=> _errorCode;

			public bool IsObserved
				=> _isObserved;

		}

		sealed class P_ExceptionDataBacking {

			P_ExceptionData _data;

			internal P_ExceptionDataBacking() { }

			public P_ExceptionData Data
				=> itrlck.Get(ref _data);

			public void UpdateData(Transform<P_ExceptionData> transform)
				=> itrlck.Update(location: ref _data, transform: transform);

		}

		#endregion

		public static readonly string TypeFullName = typeof(ExceptionUtilities).FullName;

		static readonly object __ExceptionTreeDataKey = new object();

		static readonly object __IsCriticalTestFaultMark = new object();

		static readonly ConditionalWeakTable<Exception, object> __IsCriticalTestFaultRepo = new ConditionalWeakTable<Exception, object>();

		static readonly ConditionalWeakTable<Exception, P_ExceptionDataBacking> __ExceptionDataRepo = new ConditionalWeakTable<Exception, P_ExceptionDataBacking>();

		public static readonly SeverityLevel MostHighSeverityLevel = Enum.GetValues(typeof(SeverityLevel)).Cast<SeverityLevel>().Max();

		public static readonly SeverityLevel MostLowSeverityLevel = Enum.GetValues(typeof(SeverityLevel)).Cast<SeverityLevel>().Min();

		public static readonly ExceptionFactoryDelegate2 InvalidOperationExceptionFactory =
			(locMessage, locInnerException, locCode)
			=>
			new EonException(message: locMessage, innerException: locInnerException).SetErrorCode(code: locCode);

		const int __CreateObjectDisposedException_CallCounterLimitPlusOne = 5;

		[ThreadStatic]
		static int __CreateObjectDisposedException_CallCounter;

		static P_ExceptionDataBacking P_GetExceptionDataBacking(Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			var result =
				__ExceptionDataRepo
				.GetValue(
					key: exception,
					createValueCallback: locKey => new P_ExceptionDataBacking());
			return result;
		}

		/// <summary>
		/// Устанавливает отметку для исключения, указывающую, что исключение обработано.
		/// <para>Если указанное исключение является <seealso cref="AggregateException"/>, то отметка устанавливается только для исходных исключений (см. <seealso cref="AggregateException.Flatten"/>).</para>
		/// </summary>
		/// <typeparam name="TException">Тип исключения.</typeparam>
		/// <param name="exception">
		/// Исключение.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Объект <typeparamref name="TException"/> (параметр <paramref name="exception"/>).</returns>
		public static TException MarkAsObserved<TException>(this TException exception)
			where TException : Exception {
			//
			foreach (var currentException in P_SelectObservationMarkableExceptions(exception))
				P_GetExceptionDataBacking(currentException)
					.UpdateData(
						locCurrentData => locCurrentData?.IsObserved == true ? locCurrentData : new P_ExceptionData(source: locCurrentData, isObserved: true));
			//
			return exception;
		}

		/// <summary>
		/// Снимает отметку для исключения, указывающую, что исключение обработано (см. <seealso cref="MarkAsObserved{TException}(TException)"/>).
		/// </summary>
		/// <typeparam name="TException">Тип исключения.</typeparam>
		/// <param name="exception">
		/// Исключение.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Объект <typeparamref name="TException"/> (параметр <paramref name="exception"/>).</returns>
		public static TException MarkAsUnobserved<TException>(this TException exception)
			where TException : Exception {
			//
			foreach (var currentException in P_SelectObservationMarkableExceptions(exception))
				P_GetExceptionDataBacking(currentException)
					.UpdateData(
						locCurrentData => locCurrentData?.IsObserved == false ? locCurrentData : new P_ExceptionData(source: locCurrentData, isObserved: false));
			//
			return exception;
		}

		/// <summary>
		/// Проверяет, установлена ли для исключения отметка, указывающая, что исключение обработано (см. <seealso cref="MarkAsObserved{TException}(TException)"/>).
		/// </summary>
		/// <param name="exception">Тип исключения.</param>
		/// <returns>Значение <seealso cref="bool"/>.</returns>
		public static bool IsObserved(this Exception exception) {
			foreach (var currentException in P_SelectObservationMarkableExceptions(exception)) {
				var currentInnerExceptionMark = P_GetExceptionDataBacking(currentException).Data?.IsObserved ?? false;
				if (!currentInnerExceptionMark)
					return false;
			}
			return true;
		}

		static IEnumerable<Exception> P_SelectObservationMarkableExceptions(Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			if (!(exception is AggregateException aggregateException))
				yield return exception;
			else
				// Поскольку AggregateException по факту реализации является сугубо специальным для TPL, то оно не рассматривается как исключение, несущее полезную инфу об ошибке.
				//
				foreach (var innerException in aggregateException.Flatten().InnerExceptions)
					yield return innerException;
		}

		public static ExceptionInfo ToExceptionInfo(this Exception exception) {
			var translator = ExceptionInfoUtilities.SelectTranslator(exception: exception);
			if (translator is null)
				return new ExceptionInfo(exception: exception);
			else
				return translator.FromException(exception: exception);
		}

		public static string GetFullMessage(this Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			return exception.ToExceptionInfo().GetFullMessage();
		}

		public static string ToString(this Exception exception, ExceptionInfoFormattingOptions options) {
			exception.EnsureNotNull(nameof(exception));
			//
			return exception.FmtStr().Info(options: options);
		}

		public static string ToString(this Exception exception, Func<ExceptionInfo, IEnumerable<ExceptionInfo>> selector, ExceptionInfoFormattingOptions options) {
			exception.EnsureNotNull(nameof(exception));
			selector.EnsureNotNull(nameof(selector));
			//
			var info = exception.ToExceptionInfo();
			return ExceptionInfo.ToString(infos: selector(arg: info), options: options, singleInfoFormatter: null);
		}


		public static string ToStringFull(this Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			return exception.FmtStr().InfoFull();
		}

		public static string GetBaseMessage(this Exception exception)
			=> GetBaseMessage(exception, ExceptionInfoFormattingOptions.IncludeMessage | ExceptionInfoFormattingOptions.IncludeNumberingMarker);

		public static string GetBaseMessage(this Exception exception, ExceptionInfoFormattingOptions options) {
			exception.EnsureNotNull(nameof(exception));
			//
			return exception.ToExceptionInfo().GetBaseMessage(options);
		}

		public static IEnumerable<Exception> SelectAllInner(this Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			Exception currentQueueEntry;
			var traversalQueue = new Queue<Exception>();
			traversalQueue.Enqueue(exception);
			for (; traversalQueue.Count > 0;) {
				currentQueueEntry = traversalQueue.Dequeue();
				foreach (var innerException in currentQueueEntry.SelectInner(flatAggregateException: false)) {
					yield return innerException;
					traversalQueue.Enqueue(innerException);
				}
			}
		}

		public static AggregateException ToAggregateException(this Exception firstInnerException, IEnumerable<Exception> innerExceptions)
			=> ToAggregateException(firstInnerException: firstInnerException, innerExceptions: innerExceptions.ToArray());

		public static AggregateException ToAggregateException(this Exception firstInnerException, Exception secondInnerException)
			=> ToAggregateException(firstInnerException: firstInnerException, innerExceptions: new[ ] { secondInnerException });

		public static AggregateException ToAggregateException(this Exception firstInnerException, params Exception[ ] innerExceptions)
			=> new AggregateException(innerExceptions: firstInnerException.EnsureNotNull(nameof(firstInnerException)).Value.Sequence().Concat(second: innerExceptions.Arg(nameof(innerExceptions)).EnsureNoNullElements().Value));

		public static bool HasSingleBaseExceptionOf<TException>(this Exception exception)
			where TException : Exception
			=> HasSingleBaseExceptionOf<TException>(exception, out _);

		public static bool HasSingleBaseExceptionOf<TException>(this Exception exception, out TException baseException)
			where TException : Exception {
			exception.EnsureNotNull(nameof(exception));
			//
			var firstBaseException = default(Exception);
			foreach (var locBaseException in exception.SelectBase()) {
				if (firstBaseException is null)
					firstBaseException = locBaseException;
				else {
					baseException = default;
					return false;
				}
			}
			TException locSingleBaseException;
			baseException = locSingleBaseException = firstBaseException as TException;
			return !(locSingleBaseException is null);
		}

		public static bool HasSingleBaseException(this Exception exception, Func<Exception, bool> creterion, out Exception baseException) {
			exception.EnsureNotNull(nameof(exception));
			creterion.EnsureNotNull(nameof(creterion));
			//
			Exception firstBaseException = null;
			foreach (var locBaseException in exception.SelectBase()) {
				if (firstBaseException == null)
					firstBaseException = locBaseException;
				else {
					baseException = null;
					return false;
				}
			}
			if (firstBaseException == null || !creterion(firstBaseException)) {
				baseException = null;
				return false;
			}
			else {
				baseException = firstBaseException;
				return true;
			}
		}

		#region «Error code» impls.

		public static TException SetErrorCode<TException>(this TException exception, string identifier)
			where TException : Exception
			=> SetErrorCode(exception: exception, identifier: identifier, description: null, severityLevel: null);

		public static TException SetErrorCode<TException>(this TException exception, string identifier, string description)
			where TException : Exception
			=> SetErrorCode(exception: exception, identifier: identifier, description: description, severityLevel: null);

		public static TException SetErrorCode<TException>(this TException exception, string identifier, string description, SeverityLevel? severityLevel)
			where TException : Exception
			=> SetErrorCode(exception: exception, code: new ErrorCode(identifier: identifier, severityLevel: severityLevel, description: description));

		// TODO: Put strings into the resources.
		//
		public static TException SetErrorCode<TException>(
			this TException exception,
			IErrorCode code = default,
			bool notThrowOnDisallowedChange = default,
			bool notSetIfPresent = default)
			where TException : Exception {
			//
			if (!(code is null)) {
				P_GetExceptionDataBacking(exception: exception)
					.UpdateData(
						locCurrentData => {
							if ((notSetIfPresent && !(locCurrentData?.ErrorCode is null)) || ReferenceEquals(locCurrentData?.ErrorCode, code))
								return locCurrentData;
							else
								return new P_ExceptionData(source: locCurrentData, errorCode: new ArgumentPlaceholder<IErrorCode>(code));
						});
			}
			return exception;
		}

		[Obsolete(message: "Use '" + nameof(SetErrorCode) + "' instead.", error: true)]
		public static TException TrySetErrorCode<TException>(this TException exception, string identifier, string description = null, SeverityLevel? severityLevel = null)
			where TException : Exception
			=>
			SetErrorCode(exception: exception.EnsureNotNull(nameof(exception)).Value, code: new ErrorCode(identifier: identifier, severityLevel: severityLevel, description: description));

		/// <summary>
		/// Gets error code of the specified exception(see <see cref="SetErrorCode{TException}(TException, IErrorCode, bool, bool)"/>).
		/// <para>If the specified exception have not a code, then returns <see langword="null"/>.</para>
		/// </summary>
		/// <param name="exception">
		/// Exception.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static IErrorCode GetErrorCode(this Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			if (__ExceptionDataRepo.TryGetValue(key: exception, value: out var exceptionDataBacking))
				return exceptionDataBacking.Data?.ErrorCode;
			else
				return null;
		}

		public static bool HasErrorCode(this Exception exception)
			=> HasErrorCode(exception: exception, deep: false);

		public static bool HasErrorCode(this Exception exception, bool deep) {
			if (deep)
				return GetAllErrorsCodes(exception: exception).Any();
			else {
				exception.EnsureNotNull(nameof(exception));
				//
				return hasErrorCode(locException: exception);
			}
			//
			bool hasErrorCode(Exception locException) {
				if (__ExceptionDataRepo.TryGetValue(key: locException, value: out var exceptionDataBacking))
					return !(exceptionDataBacking.Data?.ErrorCode is null);
				else
					return false;
			}
		}

		/// <summary>
		/// Возвращает перечислитель выборки всех кодов ошибок, определенных для указанного исключения (включая внутренние).
		///<para>Результат выборки не содержит <see langword="null"/>-элементов.</para>
		/// </summary>
		/// <param name="exception">
		/// Объект исключения.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<IErrorCode> GetAllErrorsCodes(this Exception exception)
			=>
			exception
			.EnsureNotNull(nameof(exception))
			.TreeNode()
			.SelfAndDescendants(childrenSelector: locException => locException.SelectInner())
			.Select(selector: locException => locException.GetErrorCode())
			.SkipNull();

		public static SeverityLevel GetMostHighSeverityLevel(this Exception exception, SeverityLevel baseLevel) {
			if (exception == null || baseLevel == MostHighSeverityLevel)
				return baseLevel;
			else {
				var mostHighLevel = GetMostHighSeverityLevel(exception);
				return !mostHighLevel.HasValue || mostHighLevel.Value < baseLevel ? baseLevel : mostHighLevel.Value;
			}
		}

		public static SeverityLevel? GetMostHighSeverityLevel(this Exception exception) {
			if (exception == null)
				return null;
			else {
				var result = default(SeverityLevel?);
				foreach (var severityLevel in exception.GetAllErrorsCodes().Select(locCode => locCode.SeverityLevel).SkipNull()) {
					if (result.HasValue) {
						if (result < severityLevel)
							result = severityLevel;
					}
					else
						result = severityLevel;
					//
					if (result == MostHighSeverityLevel)
						return result;
				}
				return result;
			}
		}

		public static SeverityLevel GetMostLowSeverityLevel(this Exception exception, SeverityLevel baseLevel) {
			if (exception == null || baseLevel == MostLowSeverityLevel)
				return baseLevel;
			else {
				var mostLowLevel = GetMostLowSeverityLevel(exception);
				return !mostLowLevel.HasValue || mostLowLevel.Value > baseLevel ? baseLevel : mostLowLevel.Value;
			}
		}

		public static SeverityLevel? GetMostLowSeverityLevel(this Exception exception) {
			if (exception == null)
				return null;
			else {
				var result = default(SeverityLevel?);
				foreach (var severityLevel in exception.GetAllErrorsCodes().Select(locCode => locCode.SeverityLevel).SkipNull()) {
					if (result.HasValue) {
						if (result > severityLevel)
							result = severityLevel;
					}
					else
						result = severityLevel;
					//
					if (result == MostLowSeverityLevel)
						return result;
				}
				return result;
			}
		}

		#endregion

		/// <summary>
		/// Определяет, является ли указанное исключение <paramref name="exception"/> исключением отмены операции, с которой ассоциирован указанный токен отмены <paramref name="ct"/>.
		/// <para>Выполнение одного из условий для указанного исключения <paramref name="exception"/> является необходимым:</para>
		/// <para>	• Исключение имеет тип <see cref="OperationCanceledException"/> и его токен отмены равен указанному <paramref name="ct"/>.</para>
		/// <para>	• Исключение имеет тип <see cref="AggregateException"/> с единственным базовым исключением типа <see cref="OperationCanceledException"/>, токен отмены которого равен указанному <paramref name="ct"/>.</para>
		/// <para>	• Исключение имеет любой тип и его базовое исключение имеет тип <see cref="OperationCanceledException"/> а также токен отмены, равный указанному <paramref name="ct"/>.</para>
		/// </summary>
		/// <param name="exception">Исключение.</param>
		/// <param name="ct">Токен отмены.</param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		public static bool IsOperationCancellationOf(this Exception exception, CancellationToken ct) {
			exception.EnsureNotNull(nameof(exception));
			//
			OperationCanceledException operationCanceledException;
			return
				((operationCanceledException = exception as OperationCanceledException) != null
					&& operationCanceledException.CancellationToken == ct)
				|| (HasSingleBaseExceptionOf(exception, out operationCanceledException)
					&& operationCanceledException.CancellationToken == ct)
				|| ((operationCanceledException = exception.GetBaseException() as OperationCanceledException) != null
					&& operationCanceledException.CancellationToken == ct);
		}

		public static bool IsOperationCancellation(this Exception exception)
			=> HasSingleBaseExceptionOf<OperationCanceledException>(exception: exception);

		public static IEnumerable<Exception> GetBase(this Exception exception)
			=> ToExceptionTree(exception: exception).Leafs.Select(locItem => locItem.Exception);

		public static IEnumerable<TResult> GetBase<TResult>(this Exception exception, Func<Exception, (bool skip, TResult output)> selector)
			=> SelectBase(exception: exception, selector: selector);

		public static IEnumerable<IErrorCode> GetBaseErrorCodes(this Exception exception)
			=> SelectBase(exception: exception, selector: locException => { var locCode = locException.GetErrorCode(); return (skip: locCode is null, output: locCode); });

		public static IEnumerable<Exception> SelectBase(this Exception exception)
			=> ToExceptionTree(exception: exception).Leafs.Select(locItem => locItem.Exception);

		public static IEnumerable<TResult> SelectBase<TResult>(this Exception exception, Func<Exception, (bool skip, TResult output)> selector) {
			selector.EnsureNotNull(nameof(selector));
			//
			var exceptionTree = ToExceptionTree(exception: exception);
			var visitedExceptionTreeNodes = new HashSet<ExceptionTree.Node>();
			foreach (var leaf in exceptionTree.Leafs) {
				foreach (var node in leaf.SelfAndAncestors) {
					if (visitedExceptionTreeNodes.Add(node)) {
						var (skip, output) = selector(arg: node.Exception);
						if (!skip) {
							yield return output;
							break;
						}
					}
					else
						break;
				}
			}
		}

		/// <summary>
		/// Создает дерево исключения <see cref="ExceptionTree"/> для указанного <paramref name="exception"/>.
		/// </summary>
		/// <param name="exception">Исключение, для которого создается дерево.</param>
		/// <returns>Дерево исключения <see cref="ExceptionTree"/>.</returns>
		public static ExceptionTree ToExceptionTree(this Exception exception)
			=> new ExceptionTree(exception: exception);

		/// <summary>
		/// Выводит все базовые исключения указанного исключения на один уровень, включая их в набор внутренних исключений <see cref="AggregateException"/>.
		/// </summary>
		/// <param name="exception">Исходное исключение.</param>
		/// <param name="selector">Селектор (предикат) выбора базовых исключений.</param>
		/// <returns>Исключение <see cref="AggregateException"/>.</returns>
		public static AggregateException FlattenBase(this Exception exception, Func<Exception, bool> selector) {
			selector.EnsureNotNull(nameof(selector));
			//
			return new AggregateException(innerExceptions: SelectBase(exception: exception, selector: locException => (skip: !selector(locException), output: locException)));
		}

		public static AggregateException FlattenBase(this Exception exception)
			=> new AggregateException(innerExceptions: SelectBase(exception: exception));

		public static AggregateException AggregateBase(this Exception exception, Func<Exception, bool> selector)
			=> FlattenBase(exception: exception, selector: selector);

		public static AggregateException AggregateBase(this Exception exception)
			=> FlattenBase(exception: exception);

		public static string GetExceptionIdentityString(this Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			//
			using (var acquiredBuffer = EonStringBuilderUtilities.AcquireBuffer()) {
				var sb = acquiredBuffer.StringBuilder;
				sb.Append(exception.GetHashCode().ToString("d", CultureInfo.InvariantCulture));
				foreach (var innerException in exception.SelectAllInner()) {
					sb.Append('.');
					sb.Append(innerException.GetHashCode().ToString("d", CultureInfo.InvariantCulture));
				}
				return sb.ToString();
			}
		}

		/// <summary>
		/// Возвращает перечислитель выборки всех внутгренних исключений указанного объекта исключения.
		/// <para>Результат выборки не содержит <see langword="null"/>-элементов.</para>
		/// </summary>
		/// <param name="exception">
		/// Исключение.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="flatAggregateException">
		/// <para>Если == <see langword="null"/>, то перед выборкой внутренних исключений производится сведение исключений типа <seealso cref="AggregateException"/> в новый экземпляр (см. <seealso cref="AggregateException.Flatten"/>).</para>
		/// </param>
		/// <returns>Объект <seealso cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<Exception> SelectInner(this Exception exception, bool flatAggregateException) {
			exception.EnsureNotNull(nameof(exception));
			//
			IEnumerable<Exception> result;
			var aggregateException = exception as AggregateException;
			if (aggregateException is null) {
				var innerExceptionsGetter = exception as IInnerExceptionsGetter;
				if (innerExceptionsGetter is null) {
					var reflectionTypeLoadException = exception as ReflectionTypeLoadException;
					if (reflectionTypeLoadException is null)
						result = exception.InnerException.Sequence();
					else
						result = reflectionTypeLoadException.InnerException.Sequence().Concat(second: reflectionTypeLoadException.LoaderExceptions);
				}
				else
					result = innerExceptionsGetter.InnerExceptions;
			}
			else
				result = (flatAggregateException ? aggregateException.Flatten() : aggregateException).InnerExceptions;
			//
			if (flatAggregateException && aggregateException is null)
				result = result.Select(locException => (locException as AggregateException)?.Flatten() ?? locException);
			return result.SkipNull();
		}

		/// <summary>
		/// Возвращает перечислитель выборки всех внутгренних исключений указанного объекта исключения.
		/// <para>Результат выборки не содержит <see langword="null"/>-элементов.</para>
		/// </summary>
		/// <param name="exception">
		/// Исключение.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<Exception> SelectInner(this Exception exception)
			=> SelectInner(exception: exception, flatAggregateException: true);

		public static void TryCatch(Action op, Exception exception) {
			op.EnsureNotNull(nameof(op));
			//
			try {
				op();
			}
			catch (Exception secondException) {
				if (exception is null)
					throw;
				else
					throw new AggregateException(exception, secondException);
			}
		}

		public static async Task TryCatchAsync(Func<Task> op, Exception exception) {
			op.EnsureNotNull(nameof(op));
			//
			try {
				await op().ConfigureAwait(false);
			}
			catch (Exception caughtException) {
				if (exception is null)
					throw;
				else
					throw new AggregateException(exception, caughtException);
			}
		}

		public static async Task TryCatchAsync(Task op, Exception exception) {
			if (op != null)
				try {
					await op.ConfigureAwait(false);
				}
				catch (Exception caughtException) {
					if (exception is null)
						throw;
					else
						throw new AggregateException(exception, caughtException);
				}
		}

		public static NullReferenceException NewNullReferenceException(string varName, object component)
			=> ArgumentUtilitiesCoreL1.NewNullReferenceException(varName: varName, component: component);

		public static ObjectDisposedException NewObjectDisposedException(object disposable, bool disposeRequestedException, string disposableName = default) {
			ObjectDisposedException exception;
			//
			if (disposable is null) {
				if (string.IsNullOrEmpty(disposableName))
					exception =
						new ObjectDisposedException(
							message: FormatXResource(locator: typeof(ObjectDisposedException), subpath: disposeRequestedException ? "DisposeRequested" : null),
							innerException: null)
						.SetErrorCode(disposeRequestedException ? DisposableErrorCodes.Object_DisposeRequested : DisposableErrorCodes.Object_DisposingOrDisposed);
				else
					exception =
						new ObjectDisposedException(
							objectName: disposableName,
							message: FormatXResource(locator: typeof(ObjectDisposedException), subpath: disposeRequestedException ? "DisposeRequested" : null))
						.SetErrorCode(disposeRequestedException ? DisposableErrorCodes.Object_DisposeRequested : DisposableErrorCodes.Object_DisposingOrDisposed);
			}
			else {
				__CreateObjectDisposedException_CallCounter++;
				try {
					var disposableText = default(string);
					var disposableHashCode = default(int?);
					//
					if (__CreateObjectDisposedException_CallCounter < __CreateObjectDisposedException_CallCounterLimitPlusOne) {
						try {
							disposableHashCode = RuntimeHelpers.GetHashCode(o: disposable);
							disposableText = disposable.ToString();
						}
						catch (ObjectDisposedException) { }
					}
					//
					if (string.IsNullOrEmpty(disposableText))
						exception =
							new ObjectDisposedException(
								objectName: $"{(string.IsNullOrEmpty(disposableName) ? disposable.GetType().ToString() : disposableName)}{(disposableHashCode.HasValue ? $" (хэш-код {disposableHashCode.Value.ToString("d")})" : string.Empty)}",
								message: FormatXResource(locator: typeof(ObjectDisposedException), subpath: disposeRequestedException ? "DisposeRequested" : null))
							.SetErrorCode(disposeRequestedException ? DisposableErrorCodes.Object_DisposeRequested : DisposableErrorCodes.Object_DisposingOrDisposed);
					else
						exception =
							new ObjectDisposedException(
								objectName: $"{(string.IsNullOrEmpty(disposableName) ? disposable.GetType().ToString() : disposableName)}{(disposableHashCode.HasValue ? $" (хэш-код {disposableHashCode.Value.ToString("d")})" : string.Empty)}",
								message: $"{FormatXResource(locator: typeof(ObjectDisposedException), subpath: disposeRequestedException ? "DisposeRequested" : null)}{Environment.NewLine}\tПредставление объекта:{disposableText.FmtStr().GNLI2()}")
							.SetErrorCode(disposeRequestedException ? DisposableErrorCodes.Object_DisposeRequested : DisposableErrorCodes.Object_DisposingOrDisposed);
				}
				finally {
					__CreateObjectDisposedException_CallCounter--;
				}
			}
			//
			return exception;
		}

	}

}