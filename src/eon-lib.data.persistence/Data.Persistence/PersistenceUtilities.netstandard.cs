using System;
using System.Collections.Generic;
using System.Reflection;

using Eon.Collections;
using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Data.Persistence {

	public static partial class PersistenceUtilities {

		static readonly Dictionary<Type, PersistenceEntityReferenceKeyTypeDescriptor> __ReferenceKeyTypeDescriptorCache = new Dictionary<Type, PersistenceEntityReferenceKeyTypeDescriptor>();

		static readonly PrimitiveSpinLock __ReferenceKeyTypeDescriptorCacheSpinLock = new PrimitiveSpinLock();

		/// <summary>
		/// Значение: <see cref="IPersistenceEntity{TReferenceKey}"/>.
		/// </summary>
		static readonly Type __IPersistenceEntityType2GenericDefinition = typeof(IPersistenceEntity<>);

		public static PersistenceEntityReferenceKeyTypeDescriptor GetReferenceKeyTypeDescriptor<TEntity>()
			where TEntity : class, IPersistenceEntity
			=> GetReferenceKeyTypeDescriptor(entityType: typeof(TEntity).Arg(nameof(TEntity)));

		public static PersistenceEntityReferenceKeyTypeDescriptor GetReferenceKeyTypeDescriptor(Type entityType)
			=> GetReferenceKeyTypeDescriptor(entityType: entityType.Arg(nameof(entityType)));

		public static PersistenceEntityReferenceKeyTypeDescriptor GetReferenceKeyTypeDescriptor(ArgumentUtilitiesHandle<Type> entityType) {
			entityType.EnsureNotNull();
			//
			return
				__ReferenceKeyTypeDescriptorCache
				.GetOrAdd(
					spinLock: __ReferenceKeyTypeDescriptorCacheSpinLock,
					key: entityType.Value,
					factory:
						(locEntityType) => {
							locEntityType.Arg(name: entityType.Name).EnsureClass().EnsureNotContainsGenericParameters();
							//
							var locEntityTypeInfo = locEntityType.GetTypeInfo();
							var locFoundInterfaces =
								locEntityTypeInfo
								.FindInterfaces(
									filter: (locT, locCriteria) => {
										var locTInfo = locT.GetTypeInfo();
										return locTInfo.IsGenericType && locTInfo.GetGenericTypeDefinition() == __IPersistenceEntityType2GenericDefinition;
									},
									filterCriteria: null);
							if (locFoundInterfaces.Length == 0)
								throw
									new ArgumentException(
										paramName: entityType.Name,
										message: FormatXResource(locator: typeof(Type), subpath: "NoImplementationOfT", args: new object[ ] { locEntityType, __IPersistenceEntityType2GenericDefinition }));
							else if (locFoundInterfaces.Length > 1)
								throw
									new ArgumentException(
										paramName: entityType.Name,
										message: FormatXResource(locator: typeof(Type), subpath: "AmbiguousImplementationOfT", args: new object[ ] { locEntityType, __IPersistenceEntityType2GenericDefinition }));
							else
								return new PersistenceEntityReferenceKeyTypeDescriptor(entityType: locEntityType, referenceKeyType: locFoundInterfaces[ 0 ].GetTypeInfo().GetGenericArguments()[ 0 ]);
						});
		}

	}

}