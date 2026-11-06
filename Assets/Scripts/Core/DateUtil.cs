using System;
using System.Globalization;

namespace RuoJingo
{
    /// <summary>Culture-independent calendar-day helpers for the save file.</summary>
    public static class DateUtil
    {
        public const string Format = "yyyy-MM-dd";

        public static string ToDayString(DateTime date) => date.Date.ToString(Format, CultureInfo.InvariantCulture);

        public static DateTime? ParseDay(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d.Date
                : (DateTime?)null;
        }

        /// <summary>Monday of the week containing <paramref name="date"/>.</summary>
        public static DateTime WeekStart(DateTime date)
        {
            int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
            return date.Date.AddDays(-daysSinceMonday);
        }
    }
}
