using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Linq {

	public static partial class QueryableUtilities {

		static readonly IDictionary<Type, object> __EmptyQueryableRepository = new Dictionary<Type, object>();

		static readonly PrimitiveSpinLock __EmptyQueryableRepositorySpinLock = new PrimitiveSpinLock();

		static readonly MethodInfo __Queryable_OfTypeMethodDefinition = typeof(Queryable).GetMethod(nameof(Queryable.OfType), BindingFlags.Public | BindingFlags.Static);

		#region Nested types

		interface P_IEmptyQueryable<out T>
		 :IQueryable<T> { }

		sealed class P_EmptyQueryable<T>
			:P_IEmptyQueryable<T> {

			readonly IEnumerable<T> _sequence;

			readonly Expression _expression;

			readonly IQueryProvider _provider;

			readonly Type _elementType;

			internal P_EmptyQueryable() {
				var sequenceAsQueryable = (_sequence = Enumerable.Empty<T>()).AsQueryable();
				_expression = sequenceAsQueryable.Expression;
				_provider = sequenceAsQueryable.Provider;
				_elementType = sequenceAsQueryable.ElementType;
			}

			#region IEnumerable<T> Members

			IEnumerator<T> IEnumerable<T>.GetEnumerator() { return _sequence.GetEnumerator(); }

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator() { return _sequence.GetEnumerator(); }

			#endregion

			#region IQueryable Members

			Type IQueryable.ElementType => _elementType;

			Expression IQueryable.Expression => _expression;

			IQueryProvider IQueryable.Provider => _provider;

			#endregion

		}

		#endregion

		public static bool IsEmptyQueryable<T>(this IQueryable<T> queryable) {
			queryable.EnsureNotNull(nameof(queryable));
			//
			return queryable is P_IEmptyQueryable<T>;
		}

		public static IQueryable<T> GetEmptyQueryable<T>() {
			var typeOfT = typeof(T);
			var fromRepositoryResult = default(object);
			if (__EmptyQueryableRepositorySpinLock.Invoke(() => __EmptyQueryableRepository.TryGetValue(typeOfT, out fromRepositoryResult)))
				return (IQueryable<T>)fromRepositoryResult;
			else {
				var newEmptyQueryable = new P_EmptyQueryable<T>();
				return
					__EmptyQueryableRepositorySpinLock
					.Invoke(
						() => {
							if (__EmptyQueryableRepository.TryGetValue(typeOfT, out fromRepositoryResult))
								return (IQueryable<T>)fromRepositoryResult;
							else {
								__EmptyQueryableRepository.Add(typeOfT, newEmptyQueryable);
								return newEmptyQueryable;
							}
						});
			}
		}

		public static IQueryable OfType(this IQueryable queryable, Type type) {
			queryable.EnsureNotNull(nameof(queryable));
			type.EnsureNotNull(nameof(type));
			if (queryable.ElementType.IsAssignableFrom(type))
				return (IQueryable)__Queryable_OfTypeMethodDefinition.MakeGenericMethod(type).Invoke(null, new object[ ] { queryable });
			else
				throw new ArgumentException(message: FormatXResource(typeof(Type), "InvalidDerivation", type, queryable.ElementType), paramName: nameof(type));
		}

		/// <summary>
		/// Возвращает <see langword="null"/> приведенный к типу <see cref="IQueryable{T}"/>.
		/// </summary>
		/// <typeparam name="T">Тип элемента данных запроса.</typeparam>
		/// <param name="typeExpression">
		/// Выражение, определяющее тип <typeparamref name="T"/>.
		/// <para>Может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="IQueryable{T}"/>.</returns>
		public static IQueryable<T> AsQueryableOf<T>(this Expression<Func<T>> typeExpression)
			=> default;

	}

}