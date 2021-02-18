// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Linq;

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

        /// <summary>
        /// Adjust time with input string
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DateTime TimeHelper(DateTime origin, string input)
        {
            try
            {
                var tartime = origin;
                var tardate = new DateTime(tartime.Year, tartime.Month, tartime.Day);
                var cs = input.Split(':')
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToArray();

                // Set new time text
                DateTime settime;
                switch (cs.Length)
                {
                    case 1:
                        settime = TimeHelperProc(cs[0], tartime);
                        break;
                    case 2:
                        settime = TimeHelperProc(cs[0], cs[1], tartime);
                        break;
                    case 3:
                        settime = TimeHelperProc(cs[0], cs[1], cs[2], tartime);
                        break;
                    default:
                        settime = default;
                        break;
                }
                return settime;
            }
            catch
            {
                return origin;
            }
        }

        private static DateTime TimeHelperProc(string str, DateTime tartime)
        {
            var val = Math.Abs(int.Parse(str));
            var settime = tartime;
            var sgn = 0;
            if (str.StartsWith("+"))
            {
                sgn = 1;
            }
            if (str.StartsWith("-"))
            {
                sgn = -1;
            }

            if (sgn == 0)
            {
                if (str.Length <= 3)  // Change second only
                {
                    settime = new DateTime(tartime.Year, tartime.Month, tartime.Day, tartime.Hour, tartime.Minute, 0) + TimeSpan.FromSeconds(val);
                }
                else
                if (str.Length == 4)
                {
                    var M = val / 100;
                    var S = val % 100;
                    settime = new DateTime(tartime.Year, tartime.Month, tartime.Day, tartime.Hour, 0, 0) + +TimeSpan.FromMinutes(M) + TimeSpan.FromSeconds(S);
                }
                else
                {
                    var H = val / 10000;
                    var M = val / 100 % 100;
                    var S = val % 100;
                    settime = new DateTime(tartime.Year, tartime.Month, tartime.Day, 0, 0, 0) + TimeSpan.FromHours(H) + TimeSpan.FromMinutes(M) + TimeSpan.FromSeconds(S);
                }
            }
            else
            {
                try
                {
                    if (str.Length <= 4)  // Change second only
                    {
                        settime = new DateTime(tartime.Year, tartime.Month, tartime.Day, tartime.Hour, tartime.Minute, tartime.Second) + TimeSpan.FromSeconds(val * sgn);
                    }
                    else
                    if (str.Length == 5)
                    {
                        var M = val / 100 * sgn;
                        var S = val % 100 * sgn;
                        settime = new DateTime(tartime.Year, tartime.Month, tartime.Day, tartime.Hour, tartime.Minute, tartime.Second) + TimeSpan.FromSeconds(M * 60 + S);
                    }
                    else
                    {
                        var H = val / 10000 * sgn;
                        var M = val / 100 % 100 * sgn;
                        var S = val % 100 * sgn;
                        settime = new DateTime(tartime.Year, tartime.Month, tartime.Day, tartime.Hour, tartime.Minute, tartime.Second) + TimeSpan.FromSeconds(H * 3600 + M * 60 + S);
                    }
                }
                catch
                {
                    settime = tartime + TimeSpan.FromSeconds(val);
                }
            }
            return settime;
        }
        private static DateTime TimeHelperProc(string str1, string str2, DateTime tartime)
        {
            if (str1.StartsWith("+") || str1.StartsWith("-"))
            {
                return TimeHelperProc($"{str1[0]}{Math.Abs(int.Parse(str1)):00}{int.Parse(str2):00}", tartime);
            }
            else
            {
                return TimeHelperProc($"{int.Parse(str1):00}{int.Parse(str2):00}", tartime);
            }
        }
        private static DateTime TimeHelperProc(string str1, string str2, string str3, DateTime tartime)
        {
            if (str1.StartsWith("+") || str1.StartsWith("-"))
            {
                return TimeHelperProc($"{str1[0]}{Math.Abs(int.Parse(str1)):00}{int.Parse(str2):00}{int.Parse(str3):00}", tartime);
            }
            else
            {
                return TimeHelperProc($"{int.Parse(str1):00}{int.Parse(str2):00}{int.Parse(str3):00}", tartime);
            }
        }
    }
}
