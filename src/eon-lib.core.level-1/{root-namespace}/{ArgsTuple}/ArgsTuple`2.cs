using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Eon.Collections;
using Eon.Linq;

namespace Eon {

	public class ArgsTuple<TArg1, TArg2>
		:ArgsTuple<TArg1>, IArgsTuple<TArg1, TArg2> {

		#region Static members

		static readonly IList<Type> __ArgsTypes = new ListReadOnlyWrap<Type>(collection: new Type[ ] { typeof(TArg1), typeof(TArg2) });

		#endregion

		readonly TArg2 _arg2;

		public ArgsTuple(TArg1 arg1, TArg2 arg2)
			: this(
					arg1: arg1,
					arg2: arg2,
					otherArgsConcreteTypes: null) { }

		protected ArgsTuple(TArg1 arg1, TArg2 arg2, IEnumerable<Type> otherArgsConcreteTypes)
			: base(
					arg1: arg1,
					otherArgsConcreteTypes: (arg2?.GetType() ?? typeof(TArg2)).Sequence().Concat(otherArgsConcreteTypes.EmptyIfNull())) {
			//
			_arg2 = arg2;
		}

		public override int ArgsCount
			=> 2;

		public override IList<Type> ArgsTypes
			=> __ArgsTypes;

		public TArg2 Arg2
			=> _arg2;

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}Аргумент2:{Environment.NewLine}\tТип:{Environment.NewLine}{typeof(TArg2).FmtStr().G().IndentLines2()}{Environment.NewLine}\tЗначение:{_arg2.FmtStr().GNLI2()}";

	}

}