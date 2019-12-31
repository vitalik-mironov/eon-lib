using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Collections;
using Eon.Reflection;

namespace Eon {

	public static class AsReadOnlyUtilities {

		#region Nested types

		static class P_ConvertAsReadOnly<T, TResult>
			where T : class, IAsReadOnly<T>
			where TResult : class, T {

			static readonly ValueHolderStruct<Func<T, bool, TResult>> __Ctor;

			static P_ConvertAsReadOnly() {
				try {
					__Ctor = new ValueHolderStruct<Func<T, bool, TResult>>(value: ActivationUtilities.RequireConstructor<T, bool, TResult>());
				}
				catch (Exception exception) {
					__Ctor = new ValueHolderStruct<Func<T, bool, TResult>>(exception: exception);
				}
			}

			public static TResult Convert(T value, bool isReadOnly) {
				Convert(source: value, isReadOnly: isReadOnly, result: out var result);
				return result;
			}

			public static void Convert(T source, bool isReadOnly, out TResult result) {
				if (source is null)
					result = null;
				else if (source is TResult sourceAsResult) {
					if (isReadOnly) {
						sourceAsResult = sourceAsResult.AsReadOnly() as TResult;
						if (sourceAsResult is null)
							result = __Ctor.Value(arg1: source, arg2: true);
						else
							result = sourceAsResult;
					}
					else
						result = sourceAsResult;
				}
				else
					result = __Ctor.Value(arg1: source, arg2: isReadOnly);
			}

			public static IEnumerable<TResult> Convert(IEnumerable<T> source, bool isReadOnly) {
				if (source is null)
					return null;
				else
					return
						isReadOnly
						? source.Select(selector: locItem => { Convert(source: locItem, isReadOnly: true, result: out var locResult); return locResult; })
						: source.Select(selector: locItem => { Convert(source: locItem, isReadOnly: false, result: out var locResult); return locResult; });
			}

		}

		#endregion

		public static T AsReadOnlyIf<T>(this T value, bool condition)
			where T : class, IAsReadOnly<T>
			=> condition ? value?.AsReadOnly() : value;

		public static IList<T> AsReadOnlyIf<T>(this IEnumerable<T> source, bool condition, bool emptyIfNull = default)
			where T : class, IAsReadOnly<T> {
			if (source is null)
				return emptyIfNull ? (condition ? (IList<T>)ListReadOnlyWrap<T>.Empty : new List<T>()) : null;
			else if (condition) {
				var traversal = new List<T>(collection: source.Select(locItem => locItem?.AsReadOnly()));
				if (traversal.Count == 0)
					return ListReadOnlyWrap<T>.Empty;
				else
					return new ListReadOnlyWrap<T>(list: traversal);
			}
			else
				return new List<T>(collection: source);
		}

		public static TResult AsReadOnlyIf<T, TResult>(this T value, bool condition)
			where T : class, IAsReadOnly<T>
			where TResult : class, T
			=> P_ConvertAsReadOnly<T, TResult>.Convert(value: value, isReadOnly: condition);

		public static void AsReadOnlyIf<T, TResult>(this T value, bool condition, out TResult result)
			where T : class, IAsReadOnly<T>
			where TResult : class, T
			=> result = P_ConvertAsReadOnly<T, TResult>.Convert(value: value, isReadOnly: condition);

		public static IList<TResult> AsReadOnlyIf<T, TResult>(this IEnumerable<T> source, bool condition, bool emptyIfNull = default)
			where T : class, IAsReadOnly<T>
			where TResult : class, T {
			//
			if (source is null)
				return emptyIfNull ? (condition ? (IList<TResult>)ListReadOnlyWrap<TResult>.Empty : new List<TResult>()) : null;
			else if (condition) {
				var traversal = P_ConvertAsReadOnly<T, TResult>.Convert(source: source, isReadOnly: true).ToArray();
				if (traversal.Length == 0)
					return ListReadOnlyWrap<TResult>.Empty;
				else
					return new ListReadOnlyWrap<TResult>(list: traversal);
			}
			else
				return new List<TResult>(collection: P_ConvertAsReadOnly<T, TResult>.Convert(source: source, isReadOnly: false));
		}

		public static IList<TResult> AsReadOnlyIf<T, TResult>(this IEnumerable<T> source, bool condition, out IList<TResult> result, bool emptyIfNull = default)
			where T : class, IAsReadOnly<T>
			where TResult : class, T
			=> result = AsReadOnlyIf<T, TResult>(source: source, condition: condition, emptyIfNull: emptyIfNull);

	}

}