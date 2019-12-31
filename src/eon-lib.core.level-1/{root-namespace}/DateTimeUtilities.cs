using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static class DateTimeUtilities {

		/// <summary>
		/// Gets the ordinal number of the day of week (in range [1; 7]) relatively of the specified first day of week.
		/// </summary>
		/// <param name="dayOfWeek"></param>
		/// <param name="firstDayOfWeek"></param>
		public static int NumberOfDayOfWeek(DayOfWeek dayOfWeek, DayOfWeek firstDayOfWeek) {
			var orderedDaysOfWeek =
				Enum
				.GetValues(typeof(DayOfWeek))
				.Cast<DayOfWeek>()
				.OrderBy(i => (int)i).ToArray();
			var correction = Array.IndexOf(orderedDaysOfWeek, firstDayOfWeek);
			var result = Array.IndexOf(orderedDaysOfWeek, dayOfWeek);
			if ((result - correction) < 0)
				return orderedDaysOfWeek.Length - correction + result + 1;
			else
				return result - correction + 1;
		}

		// TODO: Put exception messages into the resources.
		//
		/// <summary>
		/// Ensures that two specified dates are in the same format (Local time, Coordinated Universal Time (UTC) or Unspecified).
		/// <para>See <see cref="System.DateTime.Kind"/>.</para>
		/// <para>WARNING! If one of specified date is null, then no check occurs.</para>
		/// </summary>
		/// <param name="dateA">The dateA.</param>
		/// <param name="dateB">The dateB.</param>
		/// <param name="altDateAArgName">The alternative name of the argument specifying the <paramref name="dateA"/>.</param>
		/// <param name="altDateBArgName">The alternative name of the argument specifying the <paramref name="dateB"/>.</param>
		/// <exception cref="ArgumentException">When two dates aren't in the same format (Local time, Coordinated Universal Time (UTC) or Unspecified).</exception>
		public static void EnsureSameKind(this DateTime? dateA, DateTime? dateB, string altDateAArgName = "dateA", string altDateBArgName = "dateB") {
			if (dateA.HasValue && dateB.HasValue && dateA.Value.Kind != dateB.Value.Kind)
				throw
					new ArgumentException(
						message: $"Указанные значения дат представлены в различных форматах. Дата, указанная параметром '{(altDateAArgName ?? "dateA")}' ('{dateA.Value.ToString("o")}') имеет формат '{dateA.Value.Kind}', отличный от формата '{dateB.Value.Kind}' даты, указанной параметром '{(altDateBArgName ?? "dateB")}' ('{dateB.Value.ToString("o")}').");
		}

		/// <summary>
		/// Ensures that two specified dates are in the same format (Local time, Coordinated Universal Time (UTC) or Unspecified).
		/// <para>See <see cref="System.DateTime.Kind"/>.</para>
		/// </summary>
		/// <param name="dateA">The dateA.</param>
		/// <param name="dateB">The dateB.</param>
		/// <param name="altDateAArgName">The alternative name of the argument specifying the <paramref name="dateA"/>.</param>
		/// <param name="altDateBArgName">The alternative name of the argument specifying the <paramref name="dateB"/>.</param>
		/// <exception cref="System.ArgumentException">When two dates aren't in the same format (Local time, Coordinated Universal Time (UTC) or Unspecified).</exception>
		public static void EnsureSameKind(this DateTime dateA, DateTime dateB, string altDateAArgName = "dateA", string altDateBArgName = "dateB")
			=> EnsureSameKind(dateA, (DateTime?)dateB, altDateAArgName: altDateAArgName, altDateBArgName: altDateBArgName);

		public static void EnsureSameOffset(this DateTimeOffset dateA, DateTimeOffset dateB, string altDateAArgName = "dateA", string altDateBArgName = "dateB") { EnsureSameOffset(dateA, (DateTimeOffset?)dateB, altDateAArgName: altDateAArgName, altDateBArgName: altDateBArgName); }

		public static void EnsureSameOffset(this DateTimeOffset? dateA, DateTimeOffset? dateB, string altDateAArgName = "dateA", string altDateBArgName = "dateB") {
			if (dateA == null || dateB == null)
				return;
			if (dateA.Value.Offset != dateB.Value.Offset)
				throw
					new ArgumentException(
						message: $"Указанные значения дат представлены в различных часовых поясах. Дата, указанная параметром '{(altDateAArgName ?? "dateA")}' ('{dateA.Value.ToString("o")}') указана в часовом поясе '{dateA.Value.Offset.ToString("g")}', отличном от часового пояса '{dateB.Value.Offset.ToString("g")}' даты, указанной параметром '{(altDateBArgName ?? "dateB")}' ('{dateB.Value.ToString("o")}').");
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset? Min(this DateTimeOffset? a, DateTimeOffset? b)
			=> a.HasValue && b.HasValue ? (a < b ? a : b) : null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset Min(this DateTimeOffset a, DateTimeOffset b)
			=> a < b ? a : b;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime Min(this DateTime a, DateTime b)
			=> a < b ? a : b;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime? Min(this DateTime? a, DateTime? b)
			=> b.HasValue && a.HasValue ? (a > b ? b : a) : null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime? Max(this DateTime? a, DateTime? b)
			=> b.HasValue && a.HasValue ? (a < b ? b : a) : null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime Max(this DateTime a, DateTime b)
			=> a > b ? a : b;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset? Max(this DateTimeOffset? a, DateTimeOffset? b)
			=> a.HasValue && b.HasValue ? (a > b ? a : b) : null;

		/// <summary>
		/// Возвращает компонент даты (см. <seealso cref="DateTimeOffset.Date"/>).
		/// </summary>
		/// <param name="value">
		/// Значение даты и времени.
		/// <para>Может быть <see langword="null"/>. В этом случае метод возвращает <see langword="null"/>.</para>
		/// </param>
		/// <returns>
		/// Значение <see cref="Nullable{DateTimeOffset}"/>.
		/// </returns>
		public static DateTime? GetDate(this DateTimeOffset? value)
			=> value.HasValue ? value.Value.Date : default(DateTime?);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime? GetDate(this DateTime? value)
			=> value.HasValue ? value.Value.Date : default(DateTime?);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime? NewDate(this DateTime? value, DateTimeKind kind) { return value == null ? (DateTime?)null : new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 0, 0, 0, kind); }

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime? NewDateUtc(this DateTime? value) { return value == null ? (DateTime?)null : new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 0, 0, 0, DateTimeKind.Utc); }

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime? NewDateLocal(this DateTime? value) { return value == null ? (DateTime?)null : new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 0, 0, 0, DateTimeKind.Local); }

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime NewDate(this DateTime value, DateTimeKind kind) { return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, kind); }

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime NewDateUtc(this DateTime value) { return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc); }

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTime NewDateLocal(this DateTime value) { return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Local); }

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset? ToZeroOffsetDateTimeOffset(this DateTime? value)
			=> value.HasValue ? new DateTimeOffset(value.Value.Ticks, TimeSpan.Zero) : (DateTimeOffset?)null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset TrimToSeconds(this DateTimeOffset value)
			=> new DateTimeOffset(year: value.Year, month: value.Month, day: value.Day, hour: value.Hour, minute: value.Minute, second: value.Second, offset: value.Offset);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset? TrimToSeconds(this DateTimeOffset? value)
			=> value.HasValue ? TrimToSeconds(value.Value) : (DateTimeOffset?)null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset TrimToSeconds(this DateTime value)
			=> new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: value.Hour, minute: value.Minute, second: value.Second, kind: value.Kind);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset? TrimToSeconds(this DateTime? value)
			=> value.HasValue ? TrimToSeconds(value.Value) : (DateTimeOffset?)null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset TrimToMinutes(this DateTimeOffset value)
			=> new DateTimeOffset(year: value.Year, month: value.Month, day: value.Day, hour: value.Hour, minute: value.Minute, second: 0, offset: value.Offset);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset? TrimToMinutes(this DateTimeOffset? value)
			=> value.HasValue ? TrimToMinutes(value.Value) : (DateTimeOffset?)null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset TrimToMinutes(this DateTime value)
			=> new DateTime(year: value.Year, month: value.Month, day: value.Day, hour: value.Hour, minute: value.Minute, second: 0, kind: value.Kind);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset? TrimToMinutes(this DateTime? value)
			=> value.HasValue ? TrimToMinutes(value.Value) : (DateTimeOffset?)null;

		// TODO: Put exception messages into the resources.
		//
		public static DateTime ParseDateIso8601(string dateString, DateTimeKind kind, string altDateStringArgName = null) {
			altDateStringArgName = altDateStringArgName ?? "dateString";
			if (string.IsNullOrEmpty(dateString))
				throw new ArgumentException(FormatXResource(typeof(string), "CanNotNullOrEmpty"), altDateStringArgName);
			DateTime result;
			if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				throw new ArgumentException(string.Format("{0} The specified value doesn't conform to ISO-8601 format.", FormatXResource(typeof(string), "InvalidFormat")), altDateStringArgName);
			return new DateTime(result.Ticks, kind);
		}

		/// <summary>
		/// Конвертирует значение <see cref="DateTime"/> в строковое представление даты (без компонента времени) в соответствии со стандартом ISO-8601 — YYYY-MM-DD.
		/// </summary>
		/// <param name="dateTime">Значение даты и времени.</param>
		/// <returns>Значение <see cref="string"/>.</returns>
		public static string ToIso8601DatePart(this DateTime dateTime) { return string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd}", dateTime); }

		public static DateTime? GetLocalDateTime(this DateTimeOffset? dateTimeOffset) { return dateTimeOffset == null ? (DateTime?)null : dateTimeOffset.Value.LocalDateTime; }

		public static void EnsureLessOrEqualThan(this DateTime? dateA, DateTime? dateB, string altDateAArgName = "dateA", string altDateBArgName = "dateB") {
			if (dateA == null || dateB == null)
				return;
			dateA.EnsureSameKind(dateB, altDateAArgName: altDateAArgName, altDateBArgName: altDateBArgName);
			if (dateA > dateB)
				throw new ArgumentOutOfRangeException((altDateAArgName ?? "dateA"), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", (altDateBArgName ?? "dateB")));
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool? HasZeroTimeComponent(this DateTime? date)
			=> date.HasValue ? date.Value.TimeOfDay == TimeSpan.Zero : (bool?)null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool HasZeroTimeComponent(this DateTime date)
			=> date.TimeOfDay == TimeSpan.Zero;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool? HasZeroTimeComponent(this DateTimeOffset? date)
			=> date.HasValue ? date.Value.TimeOfDay == TimeSpan.Zero : (bool?)null;

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool HasZeroTimeComponent(this DateTimeOffset date)
			=> date.TimeOfDay == TimeSpan.Zero;

		public static DateTime? AddDays(this DateTime? dateTime, double days) {
			if (dateTime.HasValue)
				return dateTime.Value.AddDays(days);
			return dateTime;
		}

		// TODO: Put strings into the resources.
		//
		public static void GetDatePeriod(this DateTimeOffset referenceDateTime, DatePeriodKind kind, out DateTimeOffset periodFrom, out DateTimeOffset periodUntil, ArgumentPlaceholder<DayOfWeek> firstDayOfWeek = default) {
			if (kind == DatePeriodKind.Any)
				throw new ArgumentOutOfRangeException(nameof(kind));
			//
			DateTimeOffset locPeriodFrom, locPeriodUntil;
			var locFirstDayOfWeek = firstDayOfWeek.Substitute(value: CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
			var referenceDate =
				new DateTimeOffset(
					year: referenceDateTime.Year,
					month: referenceDateTime.Month,
					day: referenceDateTime.Day,
					hour: 0,
					minute: 0,
					second: 0,
					offset: referenceDateTime.Offset);
			var referenceDateUntil = referenceDate.AddDays(1.0D);
			switch (kind) {
				case DatePeriodKind.CurrentMonth:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, referenceDate.Month, 1, 0, 0, 0, referenceDate.Offset);
					locPeriodUntil = locPeriodFrom.AddMonths(1);
					break;
				case DatePeriodKind.CurrentDay:
					locPeriodFrom = referenceDate;
					locPeriodUntil = locPeriodFrom.AddDays(1.0);
					break;
				case DatePeriodKind.CurrentWeek:
					locFirstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
					locPeriodFrom = referenceDate.AddDays(-(NumberOfDayOfWeek(referenceDate.DayOfWeek, locFirstDayOfWeek) - 1));
					locPeriodUntil = locPeriodFrom.AddDays(7.0);
					break;
				case DatePeriodKind.CurrentQuarter:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, (int)Math.Ceiling(referenceDate.Month / 3.0D) * 3, 1, 0, 0, 0, referenceDate.Offset).AddMonths(-2);
					locPeriodUntil = locPeriodFrom.AddMonths(3);
					break;
				case DatePeriodKind.CurrentHalfOfYear:
					locPeriodFrom = referenceDate.Month > 6 ? new DateTimeOffset(referenceDate.Year, 7, 1, 0, 0, 0, referenceDate.Offset) : new DateTimeOffset(referenceDate.Year, 1, 1, 0, 0, 0, referenceDate.Offset);
					locPeriodUntil = locPeriodFrom.AddMonths(6);
					break;
				case DatePeriodKind.CurrentYear:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, 1, 1, 0, 0, 0, referenceDate.Offset);
					locPeriodUntil = locPeriodFrom.AddYears(1);
					break;
				case DatePeriodKind.BeginOfMonth:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, referenceDate.Month, 1, 0, 0, 0, referenceDate.Offset);
					locPeriodUntil = referenceDateUntil;
					break;
				case DatePeriodKind.BeginOfWeek:
					locPeriodFrom = referenceDate.AddDays(-(DateTimeUtilities.NumberOfDayOfWeek(referenceDate.DayOfWeek, locFirstDayOfWeek) - 1));
					locPeriodUntil = referenceDateUntil;
					break;
				case DatePeriodKind.BeginOfQuarter:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, (int)Math.Ceiling(referenceDate.Month / 3.0D) * 3, 1, 0, 0, 0, referenceDate.Offset).AddMonths(-2);
					locPeriodUntil = referenceDate;
					break;
				case DatePeriodKind.BeginOfHalfOfYear:
					locPeriodFrom = referenceDate.Month > 6 ? new DateTimeOffset(referenceDate.Year, 7, 1, 0, 0, 0, referenceDate.Offset) : new DateTimeOffset(referenceDate.Year, 1, 1, 0, 0, 0, referenceDate.Offset);
					locPeriodUntil = referenceDateUntil;
					break;
				case DatePeriodKind.BeginOfYear:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, 1, 1, 0, 0, 0, referenceDate.Offset);
					locPeriodUntil = referenceDateUntil;
					break;
				case DatePeriodKind.PreviousDay:
					locPeriodFrom = referenceDate.AddDays(-1);
					locPeriodUntil = referenceDate;
					break;
				case DatePeriodKind.PreviousWeek:
					locPeriodFrom = referenceDate.AddDays(-(NumberOfDayOfWeek(referenceDate.DayOfWeek, locFirstDayOfWeek) + 6));
					locPeriodUntil = locPeriodFrom.AddDays(7.0);
					break;
				case DatePeriodKind.PreviousMonth:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, referenceDate.Month, 1, 0, 0, 0, 0, referenceDate.Offset).AddMonths(-1);
					locPeriodUntil = locPeriodFrom.AddMonths(1);
					break;
				case DatePeriodKind.PreviousQuarter:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, (int)Math.Ceiling(referenceDate.Month / 3.0D) * 3, 1, 0, 0, 0, referenceDate.Offset).AddMonths(-5);
					locPeriodUntil = locPeriodFrom.AddMonths(3);
					break;
				case DatePeriodKind.PreviousHalfOfYear:
					locPeriodFrom = referenceDate.Month > 6 ? new DateTimeOffset(referenceDate.Year, 7, 1, 0, 0, 0, referenceDate.Offset).AddMonths(-6) : new DateTimeOffset(referenceDate.Year, 1, 1, 0, 0, 0, referenceDate.Offset).AddMonths(-6);
					locPeriodUntil = locPeriodFrom.AddMonths(6);
					break;
				case DatePeriodKind.PreviousYear:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, 1, 1, 0, 0, 0, referenceDate.Offset).AddYears(-1);
					locPeriodUntil = locPeriodFrom.AddYears(1);
					break;
				case DatePeriodKind.NextDay:
					locPeriodFrom = referenceDate.AddDays(1);
					locPeriodUntil = locPeriodFrom.AddDays(1);
					break;
				case DatePeriodKind.NextWeek:
					locPeriodFrom = referenceDate.AddDays(-(NumberOfDayOfWeek(referenceDate.DayOfWeek, locFirstDayOfWeek) - 8));
					locPeriodUntil = locPeriodFrom.AddDays(7.0);
					break;
				case DatePeriodKind.NextMonth:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, referenceDate.Month, 1, 0, 0, 0, 0, referenceDate.Offset).AddMonths(1);
					locPeriodUntil = locPeriodFrom.AddMonths(1);
					break;
				case DatePeriodKind.NextQuarter:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, (int)Math.Ceiling(referenceDate.Month / 3.0D) * 3, 1, 0, 0, 0, referenceDate.Offset).AddMonths(1);
					locPeriodUntil = locPeriodFrom.AddMonths(3);
					break;
				case DatePeriodKind.NextHalfOfYear:
					locPeriodFrom = referenceDate.Month > 6 ? new DateTimeOffset(referenceDate.Year, 7, 1, 0, 0, 0, referenceDate.Offset).AddMonths(6) : new DateTimeOffset(referenceDate.Year, 1, 1, 0, 0, 0, referenceDate.Offset).AddMonths(6);
					locPeriodUntil = locPeriodFrom.AddMonths(6);
					break;
				case DatePeriodKind.NextYear:
					locPeriodFrom = new DateTimeOffset(referenceDate.Year, 1, 1, 0, 0, 0, referenceDate.Offset).AddYears(1);
					locPeriodUntil = locPeriodFrom.AddYears(1);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(kind), $"Значение '{kind}' (тип '{kind.GetType().FullName}' не поддерживается).");
			}
			periodFrom = locPeriodFrom;
			periodUntil = locPeriodUntil;
		}

		public static (DateTimeOffset? from, DateTimeOffset? until) GetDatePeriod(this DateTimeOffset referenceDateTime, DatePeriodKind kind, ArgumentPlaceholder<DayOfWeek> firstDayOfWeek = default) {
			GetDatePeriod(referenceDateTime: referenceDateTime, kind: kind, periodFrom: out var periodFrom, periodUntil: out var periodUntil, firstDayOfWeek: firstDayOfWeek);
			return (from: periodFrom, until: periodUntil);
		}

		public static (DateTimeOffset? from, DateTimeOffset? until) GetDatePeriod(this DateTimeOffset? referenceDateTime, DatePeriodKind? kind, ArgumentPlaceholder<DayOfWeek> firstDayOfWeek = default) {
			if (referenceDateTime.HasValue && kind.HasValue) {
				GetDatePeriod(referenceDateTime: referenceDateTime.Value, kind: kind.Value, periodFrom: out var locPeriodFrom, periodUntil: out var locPeriodUntil, firstDayOfWeek: firstDayOfWeek);
				return (from: locPeriodFrom, until: locPeriodUntil);
			}
			else
				return (from: null, until: null);
		}

	}

}