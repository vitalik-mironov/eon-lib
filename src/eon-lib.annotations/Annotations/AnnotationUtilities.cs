using System;
using System.Collections.Generic;

using Eon.Threading;

namespace Eon.Annotations {

	public static class AnnotationUtilities {

		static readonly Dictionary<Type, AnnotationReader> __AnnotationCache;

		static readonly PrimitiveSpinLock __AnnotationCacheSpinLock;

		static AnnotationUtilities() {
			__AnnotationCache = new Dictionary<Type, AnnotationReader>();
			__AnnotationCacheSpinLock = new PrimitiveSpinLock();
		}

		public static EffectiveAnnotations GetAnnotations(Type annotatedType) {
			annotatedType.EnsureNotNull(nameof(annotatedType));
			//
			var reader = default(AnnotationReader);
			if (!__AnnotationCacheSpinLock.Invoke(() => __AnnotationCache.TryGetValue(annotatedType, out reader))) {
				var newReader = new AnnotationReader(annotatedType: annotatedType);
				reader =
					__AnnotationCacheSpinLock
					.Invoke(
						func:
							() => {
								if (__AnnotationCache.ContainsKey(key: annotatedType))
									return __AnnotationCache[ annotatedType ];
								else {
									__AnnotationCache.Add(key: annotatedType, value: newReader);
									return newReader;
								}
							});
			}
			return reader.Read();
		}

	}

}