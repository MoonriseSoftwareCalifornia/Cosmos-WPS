using System;

namespace Cosmos.Cms.Common.Services
{
    /// <summary>
    ///     Time zone conversion utility
    /// </summary>
    public static class TimeZoneUtility
    {
        /// <summary>
        ///     Converts a UTC date to Pacific Standard Time
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertUtcDateTimeToPst(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTime(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
        }

        /// <summary>
        ///     Converts a PST date to UTC date
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertPstDateTimeToUtc(DateTime dateTime)
        {
            //if (dateTime.Kind == DateTimeKind.Unspecified)
            //{
            //    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            //}
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified),
                TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
        }
    }
}