using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Eon.Collections;
using Eon.Linq;

namespace Eon {

	public class ArgsTuple<TArg1>
		:ArgsTuple, IArgsTuple<TArg1> {

		#region Static members

		static readonly IList<Type> __ArgsTypes = new ListReadOnlyWrap<Type>(collection: new Type[ ] { typeof(TArg1) });

		#endregion

		readonly TArg1 _arg1;

		public ArgsTuple(TArg1 arg1)
			: this(
					arg1: arg1,
					otherArgsConcreteTypes: null) { }

		protected ArgsTuple(TArg1 arg1, IEnumerable<Type> otherArgsConcreteTypes)
			: base(
					otherArgsConcreteTypes: (arg1?.GetType() ?? typeof(TArg1)).Sequence().Concat(otherArgsConcreteTypes.EmptyIfNull())) {
			//
			_arg1 = arg1;
		}

		public override int ArgsCount
			=> 1;

		public override IList<Type> ArgsTypes
			=> __ArgsTypes;

		public TArg1 Arg1
			=> _arg1;

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}Аргумент1:{Environment.NewLine}\tТип:{Environment.NewLine}{typeof(TArg1).FmtStr().G().IndentLines2()}{Environment.NewLine}\tЗначение:{_arg1.FmtStr().GNLI2()}";

	}

}