using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.Metadata;

namespace Eon {

	public static class XInstanceFactoryUtilities {

		#region Nested types

		public readonly struct NewAppScopeInstanceHandle<TInstance>
			where TInstance : class, IXAppScopeInstance {

			static readonly Type __InstanceType = typeof(TInstance);

			internal readonly IXAppScopeInstance Scope;

			internal readonly IDescription Description;

			internal readonly Type InstanceTypeConstraint;

			internal readonly bool IgnoreDisabilityOption;

			public NewAppScopeInstanceHandle(IXAppScopeInstance scope, IDescription description, Type instanceTypeConstraint = null, bool ignoreDisabilityOption = false) {
				scope.EnsureNotNull(nameof(scope));
				description.EnsureNotNull(nameof(description));
				instanceTypeConstraint
					.Arg(nameof(instanceTypeConstraint))
					.EnsureCompatible(type: __InstanceType);
				//
				Scope = scope;
				Description = description;
				InstanceTypeConstraint = instanceTypeConstraint;
				IgnoreDisabilityOption = ignoreDisabilityOption;
			}

		}

		#endregion

		// TODO: Put strings into the resources.
		//
		static void P_EnsureHandleValid<TInstance>(this NewAppScopeInstanceHandle<TInstance> hnd)
			where TInstance : class, IXAppScopeInstance {
			//
			if (hnd.Scope is null)
				throw new ArgumentException(message: $"Указатель не валиден: отсутствует значение поля '{nameof(hnd.Scope)}'.", paramName: nameof(hnd));
			else if (hnd.Description is null)
				throw new ArgumentException(message: $"Указатель не валиден: отсутствует значение поля '{nameof(hnd.Description)}'.", paramName: nameof(hnd));
		}

		public static NewAppScopeInstanceHandle<TInstance> AppScopeInstanceOf<TInstance>(this IXAppScopeInstance scope, IDescription description, Type instanceTypeConstraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXAppScopeInstance
			=> new NewAppScopeInstanceHandle<TInstance>(scope: scope, description: description, instanceTypeConstraint: instanceTypeConstraint, ignoreDisabilityOption: ignoreDisabilityOption);

		public static TInstance Create<TInstance>(this NewAppScopeInstanceHandle<TInstance> hnd)
			where TInstance : class, IXAppScopeInstance {
			hnd.P_EnsureHandleValid();
			return hnd.Scope.CreateScopedInstance<TInstance>(description: hnd.Description, constraint: hnd.InstanceTypeConstraint, ignoreDisabilityOption: hnd.IgnoreDisabilityOption);
		}

		public static TInstance Create<TArg1, TInstance>(this NewAppScopeInstanceHandle<TInstance> hnd, TArg1 arg1)
			where TInstance : class, IXAppScopeInstance {
			hnd.P_EnsureHandleValid();
			return hnd.Scope.CreateScopedInstance<TArg1, TInstance>(description: hnd.Description, arg1: arg1, constraint: hnd.InstanceTypeConstraint, ignoreDisabilityOption: hnd.IgnoreDisabilityOption);
		}

		public static TInstance Create<TArg1, TArg2, TInstance>(this NewAppScopeInstanceHandle<TInstance> hnd, TArg1 arg1, TArg2 arg2)
			where TInstance : class, IXAppScopeInstance {
			hnd.P_EnsureHandleValid();
			return hnd.Scope.CreateScopedInstance<TArg1, TArg2, TInstance>(description: hnd.Description, arg1: arg1, arg2: arg2, constraint: hnd.InstanceTypeConstraint, ignoreDisabilityOption: hnd.IgnoreDisabilityOption);
		}

		public static TInstance Create<TArg1, TArg2, TArg3, TInstance>(this NewAppScopeInstanceHandle<TInstance> hnd, TArg1 arg1, TArg2 arg2, TArg3 arg3)
			where TInstance : class, IXAppScopeInstance {
			hnd.P_EnsureHandleValid();
			return
				hnd.Scope.CreateScopedInstance<TArg1, TArg2, TArg3, TInstance>(description: hnd.Description, arg1: arg1, arg2: arg2, arg3: arg3, constraint: hnd.InstanceTypeConstraint, ignoreDisabilityOption: hnd.IgnoreDisabilityOption);
		}

		public static TInstance Create<TArg1, TArg2, TArg3, TArg4, TInstance>(this NewAppScopeInstanceHandle<TInstance> hnd, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
			where TInstance : class, IXAppScopeInstance {
			hnd.P_EnsureHandleValid();
			return
				hnd.Scope.CreateScopedInstance<TArg1, TArg2, TArg3, TArg4, TInstance>(description: hnd.Description, arg1: arg1, arg2: arg2, arg3: arg3, arg4: arg4, constraint: hnd.InstanceTypeConstraint, ignoreDisabilityOption: hnd.IgnoreDisabilityOption);
		}

		public static TInstance CreateAppScopeInstance<TInstance>(this IXAppScopeInstance scope, IDescription description, Type instanceTypeConstraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXAppScopeInstance
			=>
			scope
			.AppScopeInstanceOf<TInstance>(description: description, instanceTypeConstraint: instanceTypeConstraint, ignoreDisabilityOption: ignoreDisabilityOption)
			.Create();

		public static TInstance CreateAppScopeInstance<TInstance>(this IXAppScopeInstance scope, ArgumentUtilitiesHandle<string> descriptionFullName, Type instanceTypeConstraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXAppScopeInstance {
			//
			scope.EnsureNotNull(nameof(scope));
			descriptionFullName.EnsureNotNull(nameof(descriptionFullName));
			//
			var descriptionPath = MetadataPathName.Parse(metadataPathName: descriptionFullName.Arg(nameof(descriptionFullName)));
			var description = scope.Description.Package.RequireMetadata<IDescription>(fullName: descriptionPath);
			return CreateAppScopeInstance<TInstance>(scope: scope, description: description, ignoreDisabilityOption: ignoreDisabilityOption, instanceTypeConstraint: instanceTypeConstraint);
		}

		public static TInstance CreateAppScopeInstance<TInstance>(this IXAppScopeInstance scope, string descriptionFullName, Type instanceTypeConstraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXAppScopeInstance
			=> CreateAppScopeInstance<TInstance>(scope: scope, descriptionFullName: descriptionFullName.Arg(nameof(descriptionFullName)), instanceTypeConstraint: instanceTypeConstraint, ignoreDisabilityOption: ignoreDisabilityOption);

		public static async Task<TInstance> CreateInitializeAppScopeInstanceAsync<TInstance>(this IXAppScopeInstance scope, IDescription description, IContext ctx = default, bool ignoreDisabilityOption = false)
			where TInstance : class, IXAppScopeInstance {
			var createdInstance = default(TInstance);
			try {
				createdInstance = scope.CreateAppScopeInstance<TInstance>(description: description, ignoreDisabilityOption: ignoreDisabilityOption);
				await createdInstance.InitializeAsync(ctx: ctx).ConfigureAwait(false);
				return createdInstance;
			}
			catch (Exception exception) {
				createdInstance?.Dispose(exception);
				throw;
			}
		}

		public static async Task<IList<TInstance>> CreateInitializeAppScopeInstanceAsync<TInstance>(this IXAppScopeInstance scope, ArgumentUtilitiesHandle<IEnumerable<IDescription>> descriptions, IContext ctx = default, bool ignoreDisabilityOption = false)
			where TInstance : class, IXAppScopeInstance {
			scope.EnsureNotNull(nameof(scope));
			descriptions.EnsureNotNull();
			//
			var descriptionsArray = descriptions.EnsureNoNullElements().Value.ToArray();
			var result = new List<TInstance>();
			var createdInstance = default(TInstance);
			try {
				for (var i = 0; i < descriptionsArray.Length; i++) {
					var description = descriptionsArray[ i ];
					createdInstance = scope.CreateAppScopeInstance<TInstance>(description: description, ignoreDisabilityOption: ignoreDisabilityOption);
					await createdInstance.InitializeAsync(ctx: ctx).ConfigureAwait(false);
					result.Add(createdInstance);
				}
				return result;
			}
			catch (Exception exception) {
				createdInstance?.Dispose(exception);
				result?.DisposeMany(exception);
				result?.Clear();
				throw;
			}
		}

		public static Task<IList<TInstance>> CreateInitializeAppScopeInstanceAsync<TInstance>(this IXAppScopeInstance scope, IEnumerable<IDescription> descriptions, IContext ctx = default, bool ignoreDisabilityOption = false)
			where TInstance : class, IXAppScopeInstance
			=> CreateInitializeAppScopeInstanceAsync<TInstance>(scope: scope, descriptions: descriptions.Arg(nameof(descriptions)), ctx: ctx, ignoreDisabilityOption: ignoreDisabilityOption);

	}

}