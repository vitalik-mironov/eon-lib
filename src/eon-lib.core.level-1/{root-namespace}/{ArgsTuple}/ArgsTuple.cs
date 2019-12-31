using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Collections;
using Eon.Linq;
using Eon.Reflection;
using Eon.Threading;

namespace Eon {
	using TypeTuple1 = Tuple<Type>;
	using TypeTuple2 = Tuple<Type, Type>;
	using TypeTuple3 = Tuple<Type, Type, Type>;
	using TypeTuple4 = Tuple<Type, Type, Type, Type>;
	using TypeTuple5 = Tuple<Type, Type, Type, Type, Type>;
	using TypeTuple6 = Tuple<Type, Type, Type, Type, Type, Type>;

	public class ArgsTuple
		:IArgsTuple {

		#region Nested types

		abstract class P_CreateFuncBase {

			protected P_CreateFuncBase() { }

			public abstract IArgsTuple<TArg1> Create<TArg1>(TArg1 arg1);

			public abstract IArgsTuple<TArg1, TArg2> Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2);

			public abstract IArgsTuple<TArg1, TArg2, TArg3> Create<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3);

			public abstract IArgsTuple<TArg1, TArg2, TArg3, TArg4> Create<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

			public abstract IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> Create<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);

			public abstract IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);

		}

		sealed class P_CreateFunc<TOutArg1>
			:P_CreateFuncBase {

			internal P_CreateFunc() { }

			public override IArgsTuple<TArg1> Create<TArg1>(TArg1 arg1) {
				return
					new ArgsTuple<TOutArg1>(
						arg1.Cast<TArg1, TOutArg1>()).Cast<IArgsTuple<TOutArg1>, IArgsTuple<TArg1>>();
			}

			public override IArgsTuple<TArg1, TArg2> Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3> Create<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4> Create<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> Create<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
				=> throw new NotSupportedException();

		}

		sealed class P_CreateFunc<TOutArg1, TOutArg2>
			:P_CreateFuncBase {

			internal P_CreateFunc() { }

			public override IArgsTuple<TArg1> Create<TArg1>(TArg1 arg1) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2> Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2) {
				return
					new ArgsTuple<TOutArg1, TOutArg2>(
						arg1.Cast<TArg1, TOutArg1>(),
						arg2.Cast<TArg2, TOutArg2>()).Cast<IArgsTuple<TOutArg1, TOutArg2>, IArgsTuple<TArg1, TArg2>>();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3> Create<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4> Create<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> Create<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
				=> throw new NotSupportedException();

		}

		sealed class P_CreateFunc<TOutArg1, TOutArg2, TOutArg3>
			:P_CreateFuncBase {

			internal P_CreateFunc() { }

			public override IArgsTuple<TArg1> Create<TArg1>(TArg1 arg1) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2> Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3> Create<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
				return
					new ArgsTuple<TOutArg1, TOutArg2, TOutArg3>(
						arg1.Cast<TArg1, TOutArg1>(),
						arg2.Cast<TArg2, TOutArg2>(),
						arg3.Cast<TArg3, TOutArg3>()).Cast<IArgsTuple<TOutArg1, TOutArg2, TOutArg3>, IArgsTuple<TArg1, TArg2, TArg3>>();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4> Create<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> Create<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
				=> throw new NotSupportedException();

		}

		sealed class P_CreateFunc<TOutArg1, TOutArg2, TOutArg3, TOutArg4>
			:P_CreateFuncBase {

			internal P_CreateFunc() { }

			public override IArgsTuple<TArg1> Create<TArg1>(TArg1 arg1) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2> Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3> Create<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4> Create<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
				return
					new ArgsTuple<TOutArg1, TOutArg2, TOutArg3, TOutArg4>(
						arg1.Cast<TArg1, TOutArg1>(),
						arg2.Cast<TArg2, TOutArg2>(),
						arg3.Cast<TArg3, TOutArg3>(),
						arg4.Cast<TArg4, TOutArg4>()).Cast<IArgsTuple<TOutArg1, TOutArg2, TOutArg3, TOutArg4>, IArgsTuple<TArg1, TArg2, TArg3, TArg4>>();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> Create<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
				=> throw new NotSupportedException();

		}

		sealed class P_CreateFunc<TOutArg1, TOutArg2, TOutArg3, TOutArg4, TOutArg5>
			:P_CreateFuncBase {

			internal P_CreateFunc() { }

			public override IArgsTuple<TArg1> Create<TArg1>(TArg1 arg1) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2> Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3> Create<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4> Create<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
				throw new NotSupportedException();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> Create<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) {
				return
					new ArgsTuple<TOutArg1, TOutArg2, TOutArg3, TOutArg4, TOutArg5>(
						arg1.Cast<TArg1, TOutArg1>(),
						arg2.Cast<TArg2, TOutArg2>(),
						arg3.Cast<TArg3, TOutArg3>(),
						arg4.Cast<TArg4, TOutArg4>(),
						arg5.Cast<TArg5, TOutArg5>()).Cast<IArgsTuple<TOutArg1, TOutArg2, TOutArg3, TOutArg4, TOutArg5>, IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5>>();
			}

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
				=> throw new NotSupportedException();

		}

		sealed class P_CreateFunc<TOutArg1, TOutArg2, TOutArg3, TOutArg4, TOutArg5, TOutArg6>
			:P_CreateFuncBase {

			internal P_CreateFunc() { }

			public override IArgsTuple<TArg1> Create<TArg1>(TArg1 arg1)
				=> throw new NotSupportedException();

			public override IArgsTuple<TArg1, TArg2> Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
				=> throw new NotSupportedException();

			public override IArgsTuple<TArg1, TArg2, TArg3> Create<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
				=> throw new NotSupportedException();

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4> Create<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
				=> throw new NotSupportedException();

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> Create<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
				=> throw new NotSupportedException();

			public override IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
				=>
				new ArgsTuple<TOutArg1, TOutArg2, TOutArg3, TOutArg4, TOutArg5, TOutArg6>(
					arg1.Cast<TArg1, TOutArg1>(),
					arg2.Cast<TArg2, TOutArg2>(),
					arg3.Cast<TArg3, TOutArg3>(),
					arg4.Cast<TArg4, TOutArg4>(),
					arg5.Cast<TArg5, TOutArg5>(),
					arg6.Cast<TArg6, TOutArg6>())
				.Cast<IArgsTuple<TOutArg1, TOutArg2, TOutArg3, TOutArg4, TOutArg5, TOutArg6>, IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>>();

		}

		#endregion

		#region Static members

		static readonly IDictionary<TypeTuple1, P_CreateFuncBase> __CreateFuncs1 = new Dictionary<TypeTuple1, P_CreateFuncBase>();

		static readonly IDictionary<TypeTuple2, P_CreateFuncBase> __CreateFuncs2 = new Dictionary<TypeTuple2, P_CreateFuncBase>();

		static readonly IDictionary<TypeTuple3, P_CreateFuncBase> __CreateFuncs3 = new Dictionary<TypeTuple3, P_CreateFuncBase>();

		static readonly IDictionary<TypeTuple4, P_CreateFuncBase> __CreateFuncs4 = new Dictionary<TypeTuple4, P_CreateFuncBase>();

		static readonly IDictionary<TypeTuple5, P_CreateFuncBase> __CreateFuncs5 = new Dictionary<TypeTuple5, P_CreateFuncBase>();

		static readonly IDictionary<TypeTuple6, P_CreateFuncBase> __CreateFuncs6 = new Dictionary<TypeTuple6, P_CreateFuncBase>();

		static readonly PrimitiveSpinLock __CreateFuncsSpinLock = new PrimitiveSpinLock();

		public static Type GetIArgsTupleType(IArgsTuple args)
			=> GetIArgsTupleType(hnd: args.Arg(nameof(args)));

		// TODO: Put strings into the resources.
		//
		public static Type GetIArgsTupleType(ArgumentUtilitiesHandle<IArgsTuple> hnd) {
			hnd.EnsureNotNull();
			//
			var args = hnd.Value;
			if (args.ArgsCount < 1)
				return typeof(IArgsTuple);
			else {
				Type result;
				//
				switch (args.ArgsCount - 1) {
					case 0:
						result = typeof(IArgsTuple<>);
						break;
					case 1:
						result = typeof(IArgsTuple<,>);
						break;
					case 2:
						result = typeof(IArgsTuple<,,>);
						break;
					case 3:
						result = typeof(IArgsTuple<,,,>);
						break;
					case 4:
						result = typeof(IArgsTuple<,,,,>);
						break;
					case 5:
						result = typeof(IArgsTuple<,,,,,>);
						break;
					default:
						throw new ArgumentException(message: $"No generic-type '{nameof(IArgsTuple)}' for the specified tuple of args.{Environment.NewLine}\tTuple:{args.FmtStr().GNLI2()}", paramName: hnd.Name);
				}
				//
				return result.MakeGenericType(typeArguments: args.ArgsTypes.ToArray());
			}
		}

		public static IArgsTuple Create()
			=> new ArgsTuple();

		public static IArgsTuple<TArg1> Create<TArg1>(TArg1 arg1) {
			var typesKey = new TypeTuple1(arg1?.GetType() ?? typeof(TArg1));
			var createFunc = default(P_CreateFuncBase);
			if (!__CreateFuncsSpinLock.Invoke(() => __CreateFuncs1.TryGetValue(typesKey, out createFunc))) {
				createFunc = ActivationUtilities.RequireConstructor<P_CreateFuncBase>(
					typeof(P_CreateFunc<>)
					.MakeGenericType(typesKey.Item1))();
				createFunc = __CreateFuncs1.GetOrAdd(spinLock: __CreateFuncsSpinLock, key: typesKey, factory: key => createFunc);
			}
			return createFunc.Create(arg1);
		}

		public static IArgsTuple<TArg1, TArg2> Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2) {
			var typesKey =
				new TypeTuple2(
					arg1?.GetType() ?? typeof(TArg1),
					arg2?.GetType() ?? typeof(TArg2));
			var createFunc = default(P_CreateFuncBase);
			if (!__CreateFuncsSpinLock.Invoke(() => __CreateFuncs2.TryGetValue(typesKey, out createFunc))) {
				createFunc =
					ActivationUtilities
					.RequireConstructor<P_CreateFuncBase>(concreteType: typeof(P_CreateFunc<,>).MakeGenericType(typesKey.Item1, typesKey.Item2))();
				createFunc = __CreateFuncs2.GetOrAdd(spinLock: __CreateFuncsSpinLock, key: typesKey, factory: key => createFunc);
			}
			return createFunc.Create(arg1, arg2);
		}

		public static IArgsTuple<TArg1, TArg2, TArg3> Create<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
			var typesKey =
				new TypeTuple3(
					arg1?.GetType() ?? typeof(TArg1),
					arg2?.GetType() ?? typeof(TArg2),
					arg3?.GetType() ?? typeof(TArg3));
			var createFunc = default(P_CreateFuncBase);
			if (!__CreateFuncsSpinLock.Invoke(() => __CreateFuncs3.TryGetValue(typesKey, out createFunc))) {
				createFunc = ActivationUtilities.RequireConstructor<P_CreateFuncBase>(typeof(P_CreateFunc<,,>).MakeGenericType(typesKey.Item1, typesKey.Item2, typesKey.Item3))();
				createFunc =
					__CreateFuncs3
					.GetOrAdd(
						spinLock: __CreateFuncsSpinLock,
						key: typesKey,
						factory: key => createFunc);
			}
			return createFunc.Create(arg1, arg2, arg3);
		}

		public static IArgsTuple<TArg1, TArg2, TArg3, TArg4> Create<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) {
			var typesKey = new TypeTuple4(
				arg1?.GetType() ?? typeof(TArg1),
				arg2?.GetType() ?? typeof(TArg2),
				arg3?.GetType() ?? typeof(TArg3),
				arg4?.GetType() ?? typeof(TArg4));
			var createFunc = default(P_CreateFuncBase);
			if (!__CreateFuncsSpinLock.Invoke(() => __CreateFuncs4.TryGetValue(typesKey, out createFunc))) {
				createFunc = ActivationUtilities.RequireConstructor<P_CreateFuncBase>(
					typeof(P_CreateFunc<,,,>)
					.MakeGenericType(
						typesKey.Item1,
						typesKey.Item2,
						typesKey.Item3,
						typesKey.Item4))();
				createFunc =
					__CreateFuncs4
					.GetOrAdd(
						spinLock: __CreateFuncsSpinLock,
						key: typesKey,
						factory: key => createFunc);
			}
			return createFunc.Create(arg1, arg2, arg3, arg4);
		}

		public static IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> Create<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) {
			var typesKey =
				new TypeTuple5(
					arg1?.GetType() ?? typeof(TArg1),
					arg2?.GetType() ?? typeof(TArg2),
					arg3?.GetType() ?? typeof(TArg3),
					arg4?.GetType() ?? typeof(TArg4),
					arg5?.GetType() ?? typeof(TArg5));
			var createFunc = default(P_CreateFuncBase);
			if (!__CreateFuncsSpinLock.Invoke(() => __CreateFuncs5.TryGetValue(typesKey, out createFunc))) {
				createFunc = ActivationUtilities.RequireConstructor<P_CreateFuncBase>(
					typeof(P_CreateFunc<,,,,>)
					.MakeGenericType(
						typesKey.Item1,
						typesKey.Item2,
						typesKey.Item3,
						typesKey.Item4,
						typesKey.Item5))();
				createFunc =
					__CreateFuncs5
					.GetOrAdd(
						spinLock: __CreateFuncsSpinLock,
						key: typesKey,
						factory: key => createFunc);
			}
			return createFunc.Create(arg1, arg2, arg3, arg4, arg5);
		}

		public static IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6) {
			var typesKey =
				new TypeTuple6(
					item1: arg1?.GetType() ?? typeof(TArg1),
					item2: arg2?.GetType() ?? typeof(TArg2),
					item3: arg3?.GetType() ?? typeof(TArg3),
					item4: arg4?.GetType() ?? typeof(TArg4),
					item5: arg5?.GetType() ?? typeof(TArg5),
					item6: arg6?.GetType() ?? typeof(TArg6));
			var createFunc = default(P_CreateFuncBase);
			if (!__CreateFuncsSpinLock.Invoke(() => __CreateFuncs6.TryGetValue(typesKey, out createFunc))) {
				createFunc =
					ActivationUtilities
					.RequireConstructor<P_CreateFuncBase>(concreteType: typeof(P_CreateFunc<,,,,,>).MakeGenericType(typesKey.Item1, typesKey.Item2, typesKey.Item3, typesKey.Item4, typesKey.Item5, typesKey.Item6))
					();
				createFunc = __CreateFuncs6.GetOrAdd(spinLock: __CreateFuncsSpinLock, key: typesKey, factory: key => createFunc);
			}
			return createFunc.Create(arg1: arg1, arg2: arg2, arg3: arg3, arg4: arg4, arg5: arg5, arg6: arg6);
		}

		#endregion

		readonly IList<Type> _argsConcreteTypes;

		public ArgsTuple()
			: this(otherArgsConcreteTypes: default) { }

		protected ArgsTuple(IEnumerable<Type> otherArgsConcreteTypes) {
			otherArgsConcreteTypes = otherArgsConcreteTypes.Arg(nameof(otherArgsConcreteTypes)).EnsureNoNullElements().Value;
			//
			_argsConcreteTypes = new ListReadOnlyWrap<Type>(collection: otherArgsConcreteTypes.EmptyIfNull());
		}

		public virtual int ArgsCount
			=> 0;

		public virtual IList<Type> ArgsTypes
			=> ListReadOnlyWrap<Type>.Empty;

		public IList<Type> ArgsConcreteTypes
			=> _argsConcreteTypes;

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=>
			$"Тип:{GetType().FmtStr().GNLI()}"
			+ $"{Environment.NewLine}Количество аргументов:{ArgsCount.ToString("d").FmtStr().GNLI()}";

	}

}