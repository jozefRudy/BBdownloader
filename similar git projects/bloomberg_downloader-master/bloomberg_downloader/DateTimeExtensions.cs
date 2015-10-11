using System;
using System.Linq;

namespace bloomberg_downloader
{
    public static class DateTimeExtensions
    {
        public static DateTime AddWorkDays(this DateTime date, int workingDays)
        {
            int direction = workingDays < 0 ? -1 : 1;
            DateTime newDate = date;
            while (workingDays != 0)
            {
                newDate = newDate.AddDays(direction);
                if (newDate.DayOfWeek != DayOfWeek.Saturday &&
                    newDate.DayOfWeek != DayOfWeek.Sunday &&
                    !newDate.IsHoliday())
                {
                    workingDays -= direction;
                }
            }
            return newDate;
        }

        public static bool IsHoliday(this DateTime date)
        {
            // You'd load/cache from a DB or file somewhere rather than hardcode
            DateTime[] holidays =
            {
                new DateTime(2010,12,27),
                new DateTime(2010,12,28),
                new DateTime(2011,01,03),
                new DateTime(2011,01,12),
                new DateTime(2011,01,13)
            };

            return holidays.Contains(date.Date);
        }

        public static DateTime PreviousWorkingDay(this DateTime date)
        {
            return date.AddWorkDays(-1);
        }

        public static int PreviousDateId(this DateTime date)
        {
            return date.PreviousWorkingDay().ToDateId();
        }

        public static int ToDateId(this DateTime date)
        {
            return int.Parse(date.ToString("yyyyMMdd"));
        }

        public static int ToDateId(this Bloomberglp.Blpapi.Datetime date)
        {
            return date.ToSystemDateTime().ToDateId();
        }
    }
}