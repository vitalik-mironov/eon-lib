using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Collections;
using Eon.Linq;

namespace Eon {

	public class ArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5>
		:ArgsTuple<TArg1, TArg2, TArg3, TArg4>, IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> {

		#region Static members

		static readonly IList<Type> __ArgTypes = new ListReadOnlyWrap<Type>(collection: new Type[ ] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5) });

		#endregion

		readonly TArg5 _arg5;

		public ArgsTuple(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
			: this(arg1: arg1, arg2: arg2, arg3: arg3, arg4: arg4, arg5: arg5, otherArgsConcreteTypes: default) { }

		protected ArgsTuple(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, IEnumerable<Type> otherArgsConcreteTypes)
			: base(arg1: arg1, arg2: arg2, arg3: arg3, arg4: arg4, otherArgsConcreteTypes: (arg5?.GetType() ?? typeof(TArg5)).Sequence().Concat(otherArgsConcreteTypes.EmptyIfNull())) {
			//
			_arg5 = arg5;
		}

		public override int ArgsCount
			=> 5;

		public override IList<Type> ArgsTypes
			=> __ArgTypes;

		public TArg5 Arg5
			=> _arg5;

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}Аргумент5:{Environment.NewLine}\tТип:{Environment.NewLine}{typeof(TArg5).FmtStr().G().IndentLines2()}{Environment.NewLine}\tЗначение:{_arg5.FmtStr().GNLI2()}";

	}

}