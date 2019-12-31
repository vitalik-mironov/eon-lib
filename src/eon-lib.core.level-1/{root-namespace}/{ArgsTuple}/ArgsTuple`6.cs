using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Collections;
using Eon.Linq;

namespace Eon {

	public class ArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>
		:ArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5>, IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> {

		#region Static members

		static readonly IList<Type> __ArgTypes = new ListReadOnlyWrap<Type>(collection: new Type[ ] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6) });

		#endregion

		readonly TArg6 _arg6;

		public ArgsTuple(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
			: this(arg1: arg1, arg2: arg2, arg3: arg3, arg4: arg4, arg5: arg5, arg6: arg6, otherArgsConcreteTypes: default) { }

		protected ArgsTuple(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, IEnumerable<Type> otherArgsConcreteTypes)
			: base(arg1: arg1, arg2: arg2, arg3: arg3, arg4: arg4, arg5: arg5, otherArgsConcreteTypes: (arg6?.GetType() ?? typeof(TArg6)).Sequence().Concat(otherArgsConcreteTypes.EmptyIfNull())) {
			//
			_arg6 = arg6;
		}

		public override int ArgsCount
			=> 6;

		public override IList<Type> ArgsTypes
			=> __ArgTypes;

		public TArg6 Arg6
			=> _arg6;

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}Аргумент6:{Environment.NewLine}\tТип:{typeof(TArg6).FmtStr().GNLI2()}{Environment.NewLine}\tЗначение:{_arg6.FmtStr().GNLI2()}";

	}

}