using System;
using System.Collections.Generic;

namespace Eon {

	public static class CloneableUtilities {

		#region Nested types

		sealed class P_SetCloneOriginCloneContext
			:CloneContextBase {

			internal P_SetCloneOriginCloneContext(object cloneOrigin, bool setCloneOrigin)
				: base(cloneOrigin, setCloneOrigin) { }

		}

		#endregion

		public static T CloneAs<T>(this T cloneOrigin)
			where T : IEonCloneable
			=> (T)cloneOrigin?.Clone();

		public static T CloneAs<T>(this T cloneOrigin, bool setCloneOrigin)
			where T : IEonCloneableOrigin {
			if (cloneOrigin == null)
				return default;
			else
				using (var cloneContext = new P_SetCloneOriginCloneContext(cloneOrigin, setCloneOrigin))
					return (T)cloneOrigin.Clone(cloneContext);
		}

		// TODO: Put strings into the resources.
		//
		public static T[ ] CloneAs<T>(this T[ ] array, CloneMode itemsCloneMode = CloneMode.Original)
			where T : IEonCloneable {
			if (itemsCloneMode == CloneMode.Root) {
				if (array is null)
					return null;
				else {
					var clonedArray = (T[ ])array.Clone();
					for (var i = 0; i < clonedArray.Length; i++)
						clonedArray[ i ] = (T)clonedArray[ i ]?.Clone();
					return clonedArray;
				}
			}
			else if (itemsCloneMode == CloneMode.Original)
				return (T[ ])array?.Clone();
			else
				throw new NotSupportedException($"Режим клонирования '{itemsCloneMode}' не поддерживается.");
		}

		public static T[ ] CloneAs<T>(this T[ ] array)
			=> (T[ ])array?.Clone();

		public static ISet<T> Clone<T>(this ISet<T> set) {
			if (set == null)
				return null;
			var hashSet = set as HashSet<T>;
			if (hashSet == null)
				return new HashSet<T>(set);
			else
				return new HashSet<T>(set, hashSet.Comparer);
		}

	}

}