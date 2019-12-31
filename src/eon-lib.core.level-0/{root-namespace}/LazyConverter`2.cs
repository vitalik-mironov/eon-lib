using System;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	public sealed class LazyConverter<TFrom, TTo> {

		ValueHolderClass<TTo> _toHolder;

		ValueHolderClass<TFrom> _fromHolder;

		readonly Func<TTo, TFrom> _backwardConvert;

		readonly Func<TFrom, TTo> _convert;

		public LazyConverter(TTo to, Func<TTo, TFrom> backwardConvert) {
			backwardConvert.EnsureNotNull(nameof(backwardConvert));
			//
			_toHolder = new ValueHolderClass<TTo>(value: to);
			_fromHolder = null;
			_backwardConvert = backwardConvert;
			_convert = null;
		}

		public LazyConverter(TFrom from, Func<TFrom, TTo> convert) {
			convert.EnsureNotNull(nameof(convert));
			//
			_toHolder = null;
			_fromHolder = new ValueHolderClass<TFrom>(value: from);
			_backwardConvert = null;
			_convert = convert;
		}

		public TFrom From {
			get {
				var holder = itrlck.Get(location: ref _fromHolder);
				if (holder is null) {
					try {
						holder = new ValueHolderClass<TFrom>(value: _backwardConvert(arg: _toHolder.Value));
					}
					catch (Exception exception) {
						holder = new ValueHolderClass<TFrom>(exception: exception);
					}
					holder = itrlck.UpdateIfNull(location: ref _fromHolder, value: holder);
				}
				return holder.Value;
			}
		}

		public TTo To {
			get {
				var holder = itrlck.Get(location: ref _toHolder);
				if (holder is null) {
					try {
						holder = new ValueHolderClass<TTo>(value: _convert(arg: _fromHolder.Value));
					}
					catch (Exception exception) {
						holder = new ValueHolderClass<TTo>(exception: exception);
					}
					holder = itrlck.UpdateIfNull(location: ref _toHolder, value: holder);
				}
				return holder.Value;
			}
		}

	}

}