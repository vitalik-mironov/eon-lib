using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Eon.Collections;
using Eon.Linq;

namespace Eon {

	public class ArgsTuple<TArg1, TArg2, TArg3, TArg4>
		:ArgsTuple<TArg1, TArg2, TArg3>, IArgsTuple<TArg1, TArg2, TArg3, TArg4> {

		#region Static members

		static readonly IList<Type> __ArgsTypes = new ListReadOnlyWrap<Type>(collection: new Type[ ] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) });

		#endregion

		readonly TArg4 _arg4;

		public ArgsTuple(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
			: this(
					arg1: arg1,
					arg2: arg2,
					arg3: arg3,
					arg4: arg4,
					otherArgsConcreteTypes: null) { }

		protected ArgsTuple(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, IEnumerable<Type> otherArgsConcreteTypes)
			: base(
					arg1: arg1,
					arg2: arg2,
					arg3: arg3,
					otherArgsConcreteTypes: (arg4?.GetType() ?? typeof(TArg4)).Sequence().Concat(otherArgsConcreteTypes.EmptyIfNull())) {
			//
			_arg4 = arg4;
		}

		public override int ArgsCount
			=> 4;

		public override IList<Type> ArgsTypes
			=> __ArgsTypes;

		public TArg4 Arg4
			=> _arg4;

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}Аргумент4:{Environment.NewLine}\tТип:{Environment.NewLine}{typeof(TArg4).FmtStr().G().IndentLines2()}{Environment.NewLine}\tЗначение:{_arg4.FmtStr().GNLI2()}";

	}

}