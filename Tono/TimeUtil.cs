// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono
{
    /// <summary>
    /// Time utility
    /// </summary>
    public struct TimeUtil
    {
        /// <summary>
        /// date time format "yyyy/MM/dd HH:mm:ss.fff"
        /// </summary>
        public const string FormatYMDHMSms = "yyyy/MM/dd HH:mm:ss.fff";
        /// <summary>
        /// date time format "yyyy/MM/dd HH:mm:ss"
        /// </summary>
        public const string FormatYMDHMS = "yyyy/MM/dd HH:mm:ss";
        /// <summary>
        /// date time format "HH:mm:ss"
        /// </summary>
        public const string FormatHMS = "HH:mm:ss";
        /// <summary>
        /// date time format "HH:mm"
        /// </summary>
        public const string FormatHM = "HH:mm";

        /// <summary>
        /// change specific value (means keep the other values)
        /// </summary>
        /// <param name="dt">input</param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="millisecond"></param>
        /// <returns>new date time</returns>
        public static DateTime Set(DateTime dt, int year = -1, int month = -1, int day = -1, int hour = -1, int minute = -1, int second = -1, int millisecond = -1)
        {
            return new DateTime(year > 1899 ? year : dt.Year, month > 0 ? month : dt.Month, day > 0 ? day : dt.Day, hour >= 0 ? hour : dt.Hour, minute >= 0 ? minute : dt.Minute, second >= 0 ? second : dt.Second, millisecond >= 0 ? millisecond : dt.Millisecond);
        }

        /// <summary>
        /// set 0 to hh:mm:ss (means keep date value)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        /// <example>
        /// ClearTime( 2019/09/25 13:34:45 ) → 2019/09/25 00:00:00
        /// </example>
        public static DateTime ClearTime(DateTime dt)
        {
            return Set(dt, hour: 0, minute: 0, second: 0, millisecond: 0);
        }

        /// <summary>
        /// set 0 to mm:ss
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        /// ClearMinutes( 2019/09/25 13:34:45 ) → 2019/09/25 13:00:00
        public static DateTime ClearMinutes(DateTime dt)
        {
            return Set(dt, minute: 0, second: 0, millisecond: 0);
        }

        /// <summary>
        /// set 0 to seconds
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        /// ClearSeconds( 2019/09/25 13:34:45 ) → 2019/09/25 13:34:00
        public static DateTime ClearSeconds(DateTime dt)
        {
            return Set(dt, second: 0, millisecond: 0);
        }
    }
}
