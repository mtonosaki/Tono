// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// Mathematical utility
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// Calc FNV Hash code
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>hash code</returns>
        public static int GetFnvHash(string str)
        {
            unchecked
            {
                const UInt32 key = 16777619;
                UInt32 hash = 2166136261;
                foreach (var c in str)
                {
                    hash = (key * hash) ^ c;
                }
                return (int)hash;
            }
        }

        /// <summary>
        /// Trim val between min and max
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double Trim(double val, double min = double.NegativeInfinity, double max = double.PositiveInfinity)
        {
            if (val < min)
            {
                val = min;
            }

            if (val > max)
            {
                val = max;
            }

            return val;
        }

        /// <summary>
        /// Gratest Common Divisor (Euclidean Algorithm)
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>Gratest Common Divisor</returns>
        public static int Gcd(int v1, int v2)
        {
            if (v1 < v2)
            {
                Swap(ref v1, ref v2);
            }
            int a = v1 % v2;
            if (a == 0)
            {
                return v2;
            }
            else
            {
                return Gcd(v2, a);
            }
        }

        /// <summary>
        /// swap the referenced values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public static void Swap<T>(ref T v1, ref T v2)
        {
            var tmp = v2;
            v2 = v1;
            v1 = tmp;
        }

        private static readonly Random _rand0 = new Random(0);
        private static readonly Random _rand = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// make random value between 0.0 - 1.0 (Random seed = 0, set at App startup)
        /// </summary>
        /// <param name="isInclude_100percent">true=1.0を含む。 false=1.0を含まない</param>
        /// <returns></returns>
        public static double Rand0(bool isInclude_100percent = true)
        {
            double ret = _rand0.Next(0, int.MaxValue - (isInclude_100percent ? 0 : 1));
            ret = ret / int.MaxValue;
            return ret;
        }

        /// <summary>
        /// make random value between 0.0 - 1.0 (Random seed = Time.Tick)
        /// </summary>
        /// <returns></returns>
        public static double Rand()
        {
            double ret = _rand.Next(0, int.MaxValue);
            ret = ret / int.MaxValue;
            return ret;
        }

        /// <summary>
        /// make random value between "start" and "end"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static double Rand(double start, double end)
        {
            return Rand() * Math.Abs(end - start) + Math.Min(start, end);
        }

        /// <summary>
        /// make integer random value between "start" and "end"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int Rand(int start, int end)
        {
            return _rand.Next(start, end);
        }

        /// <summary>
        /// check a-b ＜ nonzero
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="nonzero"></param>
        /// <returns></returns>
        public static bool Equals(double a, double b, double nonzero)
        {
            return Math.Abs(a - b) < nonzero;
        }


        /// <summary>
        /// Get sign -1, +1, 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Sgn(double value)
        {
            if (value == 0)
            {
                return 0;
            }
            else
            {
                return value < 0 ? -1 : +1;
            }
        }

        /// <summary>
        /// Get sign -1, +1, 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Sgn(int value)
        {
            if (value == 0)
            {
                return 0;
            }
            else
            {
                return value < 0 ? -1 : +1;
            }
        }

        /// <summary>
        /// get the maximum value in col
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static T Max<T>(params T[] col) where T : IComparable<T>
        {
            var max = default(T);
            var isFirst = true;
            foreach (var val in col)
            {
                if (isFirst)
                {
                    max = val;
                    isFirst = false;
                }
                else
                {
                    if (val.CompareTo(max) > 0)
                    {
                        max = val;
                    }
                }
            }
            return max;
        }

        /// <summary>
        /// get the maximum value in col
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static double Max(IEnumerable<double> col)
        {
            var max = double.NaN;
            foreach (var val in col)
            {
                if (double.IsNaN(max))
                {
                    max = val;
                }
                else
                {
                    if (val > max)
                    {
                        max = val;
                    }
                }
            }
            return max;
        }

        /// <summary>
        /// get larger(future) time
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static DateTime Max(DateTime t1, DateTime t2)
        {
            if (t1.Ticks >= t2.Ticks)
            {
                return t1;
            }
            else
            {
                return t2;
            }
        }

        /// <summary>
        /// get larger time span
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static TimeSpan Max(TimeSpan t1, TimeSpan t2)
        {
            if (t1.TotalSeconds >= t2.TotalSeconds)
            {
                return t1;
            }
            else
            {
                return t2;
            }
        }

        /// <summary>
        /// get smaller(older) time
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static DateTime Min(DateTime t1, DateTime t2)
        {
            if (t1.Ticks <= t2.Ticks)
            {
                return t1;
            }
            else
            {
                return t2;
            }
        }

        /// <summary>
        /// get smaller time span
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static TimeSpan Min(TimeSpan t1, TimeSpan t2)
        {
            if (t1.TotalSeconds <= t2.TotalSeconds)
            {
                return t1;
            }
            else
            {
                return t2;
            }
        }

        /// <summary>
        /// make integer collection that sum of value is same with the original one
        /// </summary>
        /// <param name="dats"></param>
        /// <returns></returns>
        public static IList<int> ToIntegerKeepSameTotal(List<double> dats)
        {
            var ret = new List<int>();
            var rui = new List<double>();
            var val = 0.0;
            for (var i = 0; i < dats.Count; i++)
            {
                val += dats[i];
                rui.Add(val);
            }
            ret.Add((int)Math.Floor(Math.Round(rui[0]) + 0.0001));
            for (var i = 1; i < rui.Count; i++)
            {
                ret.Add((int)Math.Floor(Math.Round(rui[i]) - Math.Round(rui[i - 1])));
            }
            return ret;
        }
    }
}
