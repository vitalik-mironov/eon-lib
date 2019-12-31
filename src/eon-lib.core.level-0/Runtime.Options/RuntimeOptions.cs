using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;

using Eon.Threading;

using static Eon.Reflection.MemberText;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Runtime.Options {

	/// <summary>
	/// Defines runtime options utilities.
	/// </summary>
	public static class RuntimeOptions {

		#region Nested types

		[DebuggerDisplay("{ToString(),nq}")]
		public sealed class Identity {

			readonly Type _type;

			internal Identity(Type type) {
				type.EnsureNotNull(nameof(type));
				//
				_type = type;
			}

			public override string ToString()
				=> _type.ToString();

		}

		public static class Option<TOption>
			where TOption : RuntimeOptionBase {

			public static readonly Identity Identity;

			static TOption __Fallback;

			static TOption __Explicit;

			static Option() {
				Identity = new Identity(type: typeof(TOption));
				__Fallback = null;
				__Explicit = null;
				itrlck.Update(location: ref __FallbackSet, transform: locCurrent => locCurrent.Add(value: P_GetFallback));
			}

			public static TOption Fallback
				=> itrlck.Get(location: ref __Fallback);

			public static TOption Explicit
				=> itrlck.Get(location: ref __Explicit);

			static RuntimeOptionBase P_GetFallback()
				=> itrlck.Get(location: ref __Fallback);

			public static TOption Require() {
				var @explicit = itrlck.Get(location: ref __Explicit);
				if (@explicit is null || itrlck.Get(location: ref __IsExplicitSetReadyToUse) != __TrueFlag) {
					var fallback = itrlck.Get(location: ref __Fallback);
					if (fallback is null)
						throw new InvalidOperationException(message: $"The required option is missing (neither explicit value nor fallback value has set).{Environment.NewLine}\tOption:{Environment.NewLine}\t\t{typeof(TOption)}");
					else
						return fallback;
				}
				else
					return @explicit;
			}

			internal static void Set(RuntimeOptionSet set) {
				if (set is null)
					throw new ArgumentNullException(paramName: nameof(set));
				//
				itrlck.Set(location: ref __Explicit, value: (TOption)set[ Identity ]);
			}

			public static UpdateResult<TOption> SetFallback(TOption option) {
				if (option is null)
					throw new ArgumentNullException(paramName: nameof(option));
				//
				var current = itrlck.Get(ref __Fallback);
				if (ReferenceEquals(objA: current, objB: option))
					return new UpdateResult<TOption>(current: current, original: current);
				else
					return SetFallback(factory: () => option);
			}

			public static UpdateResult<TOption> SetFallback(Func<TOption> factory) {
				if (factory is null)
					throw new ArgumentNullException(paramName: nameof(factory));
				//
				TOption madeByFactory = default;
				for (; ; ) {
					var current = itrlck.Get(ref __Fallback);
					if (current is null) {
						if (madeByFactory is null) {
							madeByFactory = factory();
							if (madeByFactory is null)
								throw new InvalidOperationException(message: $"The specified factory returned invalid result — 'null'.{Environment.NewLine}\tFactory:{Environment.NewLine}\t\t:{GetMethodSignatureText(func: factory)}");
						}
						var exchangeResult = Interlocked.CompareExchange(location1: ref __Fallback, value: madeByFactory, comparand: null);
						if (exchangeResult is null)
							return new UpdateResult<TOption>(current: madeByFactory, original: null);
						else if (ReferenceEquals(objA: exchangeResult, objB: madeByFactory))
							return new UpdateResult<TOption>(current: madeByFactory, original: madeByFactory);
					}
					else
						return new UpdateResult<TOption>(current: current, original: current);
				}
			}

		}

		internal abstract class OptionApiInvoker {

			#region Static members

			public static OptionApiInvoker Create(Type optionType) {
				if (optionType is null)
					throw new ArgumentNullException(paramName: nameof(optionType));
				else if (!typeof(RuntimeOptionBase).IsAssignableFrom(c: optionType))
					throw new ArgumentOutOfRangeException(paramName: nameof(optionType));
				//
				return (OptionApiInvoker)Activator.CreateInstance(type: typeof(OptionApiInvoker<>).MakeGenericType(typeArguments: new Type[ ] { optionType }), nonPublic: true);
			}

			#endregion

			protected OptionApiInvoker() { }

			public abstract void Set(RuntimeOptionSet set);

			public abstract Identity Identity { get; }

		}

		internal sealed class OptionApiInvoker<TOption>
			:OptionApiInvoker
			where TOption : RuntimeOptionBase {

			internal OptionApiInvoker() { }

			public override Identity Identity
				=> Option<TOption>.Identity;

			public override void Set(RuntimeOptionSet set)
				=> Option<TOption>.Set(set: set);

		}

		#endregion

		const int __TrueFlag = 1;

		const int __FalseFlag = 0;

		static RuntimeOptionSet __ExplicitSet;

		static int __IsExplicitSetReadyToUse;

		static ImmutableList<Func<RuntimeOptionBase>> __FallbackSet;

		static RuntimeOptions() {
			__ExplicitSet = null;
			__IsExplicitSetReadyToUse = __FalseFlag;
			__FallbackSet = ImmutableList<Func<RuntimeOptionBase>>.Empty;
		}

		public static UpdateResult<RuntimeOptionSet> Set(Func<RuntimeOptionSet> factory) {
			if (factory is null)
				throw new ArgumentNullException(paramName: nameof(factory));
			//
			RuntimeOptionSet madeByFactory = default;
			for (; ; ) {
				var current = itrlck.Get(ref __ExplicitSet);
				if (current is null) {
					if (madeByFactory is null) {
						madeByFactory = factory();
						if (madeByFactory is null)
							throw new InvalidOperationException(message: $"The specified factory returned invalid result — 'null'.{Environment.NewLine}\tFactory:{Environment.NewLine}\t\t:{GetMethodSignatureText(func: factory)}");
					}
					var exchangeResult = Interlocked.CompareExchange(location1: ref __ExplicitSet, value: madeByFactory, comparand: null);
					if (exchangeResult is null) {
						try {
							foreach (var option in madeByFactory.Options)
								OptionApiInvoker.Create(optionType: option.GetType()).Set(set: madeByFactory);
							Interlocked.Exchange(location1: ref __IsExplicitSetReadyToUse, value: __TrueFlag);
						}
						catch {
							Interlocked.Exchange(location1: ref __IsExplicitSetReadyToUse, value: __FalseFlag);
							Interlocked.CompareExchange(location1: ref __ExplicitSet, value: null, comparand: madeByFactory);
							throw;
						}
						return new UpdateResult<RuntimeOptionSet>(current: madeByFactory, original: null);
					}
					else if (ReferenceEquals(objA: exchangeResult, objB: madeByFactory))
						return new UpdateResult<RuntimeOptionSet>(current: madeByFactory, original: madeByFactory);
				}
				else
					return new UpdateResult<RuntimeOptionSet>(current: current, original: current);
			}
		}

		public static UpdateResult<RuntimeOptionSet> Set(RuntimeOptionSet options) {
			if (options is null)
				throw new ArgumentNullException(paramName: nameof(options));
			//
			var current = itrlck.Get(ref __ExplicitSet);
			if (ReferenceEquals(objA: current, objB: options))
				return new UpdateResult<RuntimeOptionSet>(current: current, original: current);
			else
				return Set(factory: () => options);
		}

		public static RuntimeOptionSet Explicit {
			get {
				var @explicit = itrlck.Get(location: ref __ExplicitSet);
				return itrlck.Get(location: ref __IsExplicitSetReadyToUse) == __TrueFlag ? @explicit : RuntimeOptionSet.Empty;
			}
		}

		public static IEnumerable<RuntimeOptionBase> Fallback {
			get {
				var current = itrlck.Get(location: ref __FallbackSet);
				foreach (var item in current) {
					var fallback = item();
					if (!(fallback is null))
						yield return fallback;
				}
			}
		}

	}

}