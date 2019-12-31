using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Eon.Collections;
using Eon.Linq;

namespace Eon {

	public class ArgsTuple<TArg1, TArg2, TArg3>
		:ArgsTuple<TArg1, TArg2>,
		IArgsTuple<TArg1, TArg2, TArg3> {

		#region Static members

		static readonly IList<Type> __ArgsTypes = new ListReadOnlyWrap<Type>(collection: new Type[ ] { typeof(TArg1), typeof(TArg2), typeof(TArg3) });

		#endregion

		readonly TArg3 _arg3;

		public ArgsTuple(TArg1 arg1, TArg2 arg2, TArg3 arg3)
			: this(
					arg1: arg1,
					arg2: arg2,
					arg3: arg3,
					otherArgsConcreteTypes: null) { }

		protected ArgsTuple(TArg1 arg1, TArg2 arg2, TArg3 arg3, IEnumerable<Type> otherArgsConcreteTypes)
			: base(
					arg1: arg1,
					arg2: arg2,
					otherArgsConcreteTypes: (arg3?.GetType() ?? typeof(TArg3)).Sequence().Concat(otherArgsConcreteTypes.EmptyIfNull())) {
			//
			_arg3 = arg3;
		}

		public override int ArgsCount
			=> 3;

		public override IList<Type> ArgsTypes
			=> __ArgsTypes;

		public TArg3 Arg3
			=> _arg3;

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}Аргумент3:{Environment.NewLine}\tТип:{Environment.NewLine}{typeof(TArg3).FmtStr().G().IndentLines2()}{Environment.NewLine}\tЗначение:{_arg3.FmtStr().GNLI2()}";

	}

}