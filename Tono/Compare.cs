// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono
{
    /// <summary>
    /// Comparer utility
    /// </summary>
    public static class Compare
    {
        /// <summary>
        /// Compare long type to make -1, 0, +1
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Normal(long a, long b)
        {
            if (a == b)
            {
                return 0;
            }
            if (a < b)
            {
                return -1;
            }
            return 1;
        }

        /// <summary>
        /// Compare float type to make -1, 0, +1
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Normal(float a, float b)
        {
            if (Math.Abs(a - b) < float.Epsilon)
            {
                return 0;
            }

            if (a < b)
            {
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// Compare double type to make -1, 0, +1
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Normal(double a, double b)
        {
            if (Math.Abs(a - b) < double.Epsilon)
            {
                return 0;
            }

            if (a < b)
            {
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// compare DateTime with MONTH accuracy (means ignore less than month accuracy)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int TotalMonth(DateTime a, DateTime b)
        {
            var x = a.Year * 12 + a.Month;
            var y = b.Year * 12 + b.Month;
            return x - y;
        }

        /// <summary>
        /// compare DateTime with DAY accuracy (means ignore less than day accuracy)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        /// <remarks>時以下の差は見ない</remarks>
        public static int TotalDays(DateTime a, DateTime b)
        {
            var da = (long)((a - b).TotalDays);
            if (da < 0)
            {
                return -1;
            }

            if (da > 0)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// compare DateTime with HOUR accuracy (means ignore less than hour accuracy)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int TotalHours(DateTime a, DateTime b)
        {
            var da = (long)((a - b).TotalHours);
            if (da < 0)
            {
                return -1;
            }

            if (da > 0)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// compare DateTime with MINUTE accuracy (means ignore less than minute accuracy)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int TotalMinutes(DateTime a, DateTime b)
        {
            var da = (long)((a - b).TotalMinutes);
            if (da < 0)
            {
                return -1;
            }

            if (da > 0)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// compare DateTime with SECOND accuracy (means ignore less than SECOND accuracy such as milli second)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int TotalSeconds(DateTime a, DateTime b)
        {
            var da = (long)((a - b).TotalSeconds);
            if (da < 0)
            {
                return -1;
            }

            if (da > 0)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// compare DateTime.Tick
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Ticks(DateTime a, DateTime b)
        {
            return Normal(a.Ticks, b.Ticks);
        }
    }
}
