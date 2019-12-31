using System;

namespace Eon.Collections.Trees {

	public class Link<TSource, TTarget>
		:ILink<TSource, TTarget> {

		readonly TSource _source;

		readonly TTarget _target;

		readonly object _tag;

		// TODO: Put strings into the resources.
		//
		public Link(TSource source, TTarget target, object tag = null) {
			source.EnsureNotNull(nameof(source));
			target.EnsureNotNull(nameof(target));
			if (ReferenceEquals(source, target))
				throw
					new ArgumentException(
						message: $"Параметр '{nameof(source)}' (представляет источник связи) и параметр '{nameof(target)}' (представляет приёмник связи) не могут указывать на один и тот же объект.",
						paramName: nameof(target));
			//
			_source = source;
			_target = target;
			_tag = tag;
		}

		public TSource Source
			=> _source;

		public TTarget Target
			=> _target;

		public object Tag
			=> _tag;

	}

}