using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Eon.Collections;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Mathematics {

	// TODO_HIGH: Move and implement «Calculate» methods in IStatsEvaluator.
	//
	public sealed class Stats {

		#region Static members

		/// <summary>
		/// Значение: <see cref="StatsFormattingOptions.MinMax"/> + <see cref="StatsFormattingOptions.Mean"/> + <see cref="StatsFormattingOptions.Median"/> + <see cref="StatsFormattingOptions.IQRValue"/> + <see cref="StatsFormattingOptions.UpperFance"/> + <see cref="StatsFormattingOptions.LowerFance"/> + <see cref="StatsFormattingOptions.StandardDeviation"/>
		/// </summary>
		public static readonly StatsFormattingOptions DefaultFormattingOptions =
			StatsFormattingOptions.MinMax
			| StatsFormattingOptions.Mean
			| StatsFormattingOptions.Median
			| StatsFormattingOptions.IQRValue | StatsFormattingOptions.UpperFance | StatsFormattingOptions.LowerFance
			| StatsFormattingOptions.StandardDeviation;

		public static readonly Stats Empty = new Stats(entire: null);

		public static Stats Calculate(IEnumerable<long> values, Stats entire = default)
			=> Calculate(values: values.Arg(nameof(values)), entire: entire);

		public static Stats Calculate(ArgumentUtilitiesHandle<IEnumerable<long>> values, Stats entire = default)
			=> Calculate(values: values.EnsureNotNull().Select(locValue => (double)locValue), entire: entire);

		public static Stats Calculate(IEnumerable<int> values, Stats entire = default)
			=> Calculate(values: values.Arg(nameof(values)), entire: entire);

		public static Stats Calculate(ArgumentUtilitiesHandle<IEnumerable<int>> values, Stats entire = default)
			=> Calculate(values: values.EnsureNotNull().Select(locValue => (double)locValue), entire: entire);

		public static Stats Calculate(IEnumerable<double> values, Stats entire = default)
			=> Calculate(values: values.Arg(nameof(values)), entire: entire);

		// TODO: Improve exception message.
		//
		public static Stats Calculate(ArgumentUtilitiesHandle<IEnumerable<double>> values, Stats entire = default) {
			values.EnsureNotNull();
			var valuesArray =
				values
				.Value
				.Select(
					(locValue, locIndex) => {
						if (double.IsNaN(locValue))
							throw
								new ArgumentOutOfRangeException(
									paramName: $"{values.Name}[{locIndex:d}]",
									message: FormatXResource(locator: typeof(ArgumentOutOfRangeException), subpath: "CanNotNaN"));
						return locValue;
					})
				.ToArray();
			if (valuesArray.Length == 0)
				return entire is null ? Empty : new Stats(entire: entire);
			else if (!(entire is null) && (entire.IsEmpty || entire.Count < valuesArray.Length))
				throw new ArgumentOutOfRangeException(paramName: nameof(entire));
			else
				return new Stats(valuesArg: values.AsChanged(value: valuesArray), entire: entire);
		}

		#endregion

		readonly bool _isEmpty;

		readonly Stats _entire;

		readonly IReadOnlyList<double> _values;

		readonly int _count;

		readonly double _min;

		readonly double _max;

		readonly double _mean;

		readonly double _quartile1;

		readonly double _median;

		readonly double _quartile3;

		readonly double _interquartileRange;

		readonly double _lowerFence;

		readonly double _upperFence;

		readonly double _variance;

		readonly double _standardDeviation;

		readonly double _standardError;

		readonly double _skewness;

		readonly double _kurtosis;

		Stats(ArgumentUtilitiesHandle<double[ ]> valuesArg, Stats entire) {
			valuesArg.EnsureNotNull().EnsureNotEmpty();
			//
			var values = valuesArg.Value;
			Array.Sort(values);
			_isEmpty = false;
			_entire = entire;
			_values = new ListReadOnlyWrap<double>(list: values);
			_count = values.Length;
			_min = values[ 0 ];
			_max = values[ values.Length - 1 ];
			_mean = values.Average();
			if (_count == 1)
				_quartile1 = _median = _quartile3 = values[ 0 ];
			else {
				_quartile1 = calcMedian(values.PartitionBoundary().Take(count: _count / 2));
				_median = calcMedian(values.PartitionBoundary());
				_quartile3 = calcMedian(values.PartitionBoundary().Skip(count: (_count + 1) / 2));
			}
			_interquartileRange = _quartile3 - _quartile1;
			_lowerFence = _quartile1 - 1.5 * _interquartileRange;
			_upperFence = _quartile3 + 1.5 * _interquartileRange;
			_variance =
				_count == 1
				? 0.0D
				: values.Sum(locValue => Math.Pow(x: locValue - _mean, y: 2)) / (_count - 1);
			_standardDeviation = Math.Sqrt(_variance);
			_standardError = _standardDeviation / Math.Sqrt(_count);
			_skewness = calcCentralMoment(n: 3) / Math.Pow(x: _standardDeviation, y: 3);
			_kurtosis = calcCentralMoment(n: 4) / Math.Pow(x: _standardDeviation, y: 4);
			//
			double calcMedian(ArrayPartitionBoundary partition)
				=>
				partition.Count % 2 == 0
				? (values[ partition.Count / 2 - 1 + partition.Offset ] + values[ partition.Count / 2 + partition.Offset ]) / 2.0D
				: values[ partition.Count / 2 + partition.Offset ];
			double calcCentralMoment(int n)
				=> values.Average(locValue => Math.Pow(x: locValue - _mean, y: n));
		}

		Stats(Stats entire) {
			_isEmpty = true;
			_entire = entire;
			_values = ListReadOnlyWrap<double>.Empty;
			_count = 0;
			_min = double.NaN;
			_max = double.NaN;
			_mean = double.NaN;
			_median = double.NaN;
			_quartile1 = double.NaN;
			_quartile3 = double.NaN;
			_interquartileRange = double.NaN;
			_lowerFence = double.NaN;
			_upperFence = double.NaN;
			_variance = double.NaN;
			_standardDeviation = double.NaN;
			_standardError = double.NaN;
			_skewness = double.NaN;
			_kurtosis = double.NaN;
		}

		public bool IsEmpty
			=> _isEmpty;

		public Stats Entire
			=> _entire;

		public IReadOnlyList<double> Values
			=> _values;

		public int Count
			=> _count;

		public double Min
			=> _min;

		public double Max
			=> _max;

		public double Mean
			=> _mean;

		public double Median
			=> _median;

		public double Quartile1
			=> _quartile1;

		public double Quartile2
			=> _median;

		public double Quartile3
			=> _quartile3;

		public double InterquartileRange
			=> _interquartileRange;

		public double LowerFence
			=> _lowerFence;

		public double UpperFence
			=> _upperFence;

		public double Variance
			=> _variance;

		public double StandardDeviation
			=> _standardDeviation;

		public double StandardError
			=> _standardError;

		public double Skewness
			=> _skewness;

		public double Kurtosis
			=> _kurtosis;

		public bool IsLowerOutlier(double value)
			=> value < _lowerFence;

		public bool IsUpperOutlier(double value)
			=> value > _upperFence;

		public bool IsOutlier(double value)
			=> value < _lowerFence || value > _upperFence;

		public override string ToString()
			=> ToString(options: StatsFormattingOptions.Default);

		public string ToString(int precision) {
			precision.Arg(nameof(precision)).EnsureNotLessThanZero();
			//
			return ToString(options: StatsFormattingOptions.Default, round: locValue => Math.Round(value: locValue, digits: precision), formatter: locValue => locValue.ToString("r"));
		}

		public string ToString(StatsFormattingOptions options, Func<double, string> formatter)
			=> ToString(options: options, round: locValue => Math.Round(value: locValue, digits: 4), formatter: formatter);

		public string ToString(StatsFormattingOptions options)
			=> ToString(options: options, round: locValue => Math.Round(value: locValue, digits: 4), formatter: locValue => locValue.ToString("r"));

		// TODO: Put strings into the resources.
		//
		public string ToString(StatsFormattingOptions options, Func<double, double> round, Func<double, string> formatter) {
			formatter.EnsureNotNull(nameof(formatter));
			round.EnsureNotNull(nameof(round));
			//
			options |= (options & StatsFormattingOptions.Default) == StatsFormattingOptions.Default ? DefaultFormattingOptions : StatsFormattingOptions.None;
			if (IsEmpty)
				return "Н/Д";
			else {
				using (var buffer = EonStringBuilderUtilities.AcquireBuffer()) {
					var sb = buffer.StringBuilder;
					//
					if ((options & StatsFormattingOptions.Count) == StatsFormattingOptions.Count) {
						sb.Append($"Количество: {_count:d}");
						if (!(_entire is null))
							sb.Append($" ({_count / (float)_entire.Count:p2})");
					}
					if ((options & StatsFormattingOptions.MinMax) == StatsFormattingOptions.MinMax) {
						sb.NewLineThenAppend($"Мин.:{formatter(round(_min)).FmtStr().GNLI()}");
						sb.NewLineThenAppend($"Макс.:{formatter(round(_max)).FmtStr().GNLI()}");
					}
					if ((options & StatsFormattingOptions.Mean) == StatsFormattingOptions.Mean)
						sb.NewLineThenAppend($"Среднее:{formatter(round(_mean)).FmtStr().GNLI()}");
					if ((options & StatsFormattingOptions.Median) == StatsFormattingOptions.Median)
						sb.NewLineThenAppend($"Медиана:{formatter(round(_median)).FmtStr().GNLI()}");
					if ((options & StatsFormattingOptions.IQRMask) == StatsFormattingOptions.IQRMask) {
						sb.NewLineThenAppend("Межкв. размах:");
						if ((options & StatsFormattingOptions.Q1) == StatsFormattingOptions.Q1)
							sb.NewLineThenAppend($"\t1ый кв.:{formatter(round(_quartile1)).FmtStr().GNLI2()}");
						if ((options & StatsFormattingOptions.Q2) == StatsFormattingOptions.Q2)
							sb.NewLineThenAppend($"\t2ой кв.:{formatter(round(_median)).FmtStr().GNLI2()}");
						if ((options & StatsFormattingOptions.Q3) == StatsFormattingOptions.Q3)
							sb.NewLineThenAppend($"\t3ий кв.:{formatter(round(_quartile3)).FmtStr().GNLI2()}");
						if ((options & StatsFormattingOptions.IQRValue) == StatsFormattingOptions.IQRValue)
							sb.NewLineThenAppend($"\tРазмах:{formatter(round(_interquartileRange)).FmtStr().GNLI2()}");
						if ((options & StatsFormattingOptions.LowerFance) == StatsFormattingOptions.LowerFance)
							sb.NewLineThenAppend($"\tН. порог:{formatter(round(_lowerFence)).FmtStr().GNLI2()}");
						if ((options & StatsFormattingOptions.UpperFance) == StatsFormattingOptions.UpperFance)
							sb.NewLineThenAppend($"\tВ. порог:{formatter(round(_upperFence)).FmtStr().GNLI2()}");
					}
					if ((options & StatsFormattingOptions.Variance) == StatsFormattingOptions.Variance)
						sb.NewLineThenAppend($"Дисперсия:{formatter(round(_variance)).FmtStr().GNLI()}");
					if ((options & StatsFormattingOptions.StandardDeviation) == StatsFormattingOptions.StandardDeviation)
						sb.NewLineThenAppend($"Ст. отклонение:{formatter(round(_standardDeviation)).FmtStr().GNLI()}");
					if ((options & StatsFormattingOptions.StandardError) == StatsFormattingOptions.StandardError)
						sb.NewLineThenAppend($"Ст. ошибка:{formatter(round(_standardError)).FmtStr().GNLI()}");
					if ((options & StatsFormattingOptions.Skewness) == StatsFormattingOptions.Skewness)
						sb.NewLineThenAppend($"Ассиметрия:{formatter(round(_skewness)).FmtStr().GNLI()}");
					if ((options & StatsFormattingOptions.Kurtosis) == StatsFormattingOptions.Kurtosis)
						sb.NewLineThenAppend($"Эксцесс:{formatter(round(_kurtosis)).FmtStr().GNLI()}");
					return sb.ToString();
				}
			}
		}

	}

}