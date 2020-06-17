// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Tono
{
    /// <summary>
    /// Collection utility 1 of 2 (Generic version)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Collection<T>
    {
        private static readonly T[] _zero = Array.Empty<T>();

        /// <summary>
        /// Zero collection
        /// </summary>
        public static ICollection<T> ZeroCollection => _zero;

        /// <summary>
        /// Remove specified object from Queue
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="queue">Queue object</param>
        /// <param name="equaler">lambda expression of filter</param>
        /// <example>
        /// Remove＜Object＞(queue, item ==> object.ReferenceEquals(item, obj));
        /// This example remove item that have same pointer with "obj" object.
        /// </example>
        public static void Remove(Queue<T> queue, Func<T, bool> equaler)
        {
            var tmp = new Queue<T>(queue);
            queue.Clear();
            while (tmp.Count > 0)
            {
                T t = tmp.Dequeue();
                if (equaler(t) == false)
                {
                    queue.Enqueue(t);
                }
            }
        }

        /// <summary>
        /// Make repeat value collection
        /// </summary>
        /// <param name="val">value</param>
        /// <param name="count">repeat count</param>
        /// <returns>The collection</returns>
        public static IEnumerable<T> Rep(T val, int count)
        {
            for (var i = count; i > 0; i--)
            {
                yield return val;
            }
        }

        /// <summary>
        /// Check contains val in collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="val"></param>
        /// <returns>true=含まれる</returns>
        /// <remarks>
        /// slow method. (Do sequencial compare)
        /// </remarks>
        public static bool Contains(IEnumerable<T> collection, T val)
        {
            foreach (var v in collection)
            {
                if (v.Equals(val))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check the all values are same in list
        /// </summary>
        /// <param name="list"></param>
        /// <returns>tue = All same</returns>
        public static bool EqualsAll(IEnumerable<T> list)
        {
            var isFirst = true;
            var first = default(T);
            foreach (var item in list)
            {
                if (isFirst)
                {
                    first = item;
                    isFirst = false;
                }
                else
                {
                    if (first.Equals(item) == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Check the all values in the list equals to val.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool EqualsAll(IEnumerable<T> list, T val)
        {
            foreach (var item in list)
            {
                if (item.Equals(val) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check the all values in the list equals to val. (deferred comarison with "eaualFunc")
        /// </summary>
        /// <param name="list"></param>
        /// <param name="equalFunc"></param>
        /// <returns></returns>
        public static bool EqualsAll(IEnumerable<T> list, Func<T, bool> equalFunc)
        {
            foreach (var item in list)
            {
                if (equalFunc?.Invoke(item) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Collection utility 2 of 2 (Object version)
    /// </summary>
    public static class Collection
    {
        private static readonly object[] _zero = Array.Empty<object>();

        /// <summary>
        /// Zero collection
        /// </summary>
        public static System.Collections.ICollection ZeroCollection => _zero;

        /// <summary>
        /// LINQ Extention : break by CancellationToken
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public static IEnumerable<T> CancelBy<T>(this IEnumerable<T> collection, CancellationToken stoppingToken)
        {
            foreach (var item in collection)
            {
                if (stoppingToken.IsCancellationRequested) break;

                yield return item;
            }
        }

        /// <summary>
        /// LINQ Extention : break by the lazy function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="conditionChecker"></param>
        /// <returns></returns>
        public static IEnumerable<T> When<T>(this IEnumerable<T> collection, Func<bool> conditionChecker)
        {
            foreach (var item in collection)
            {
                if (!conditionChecker.Invoke()) break;

                yield return item;
            }
        }

        /// <summary>
        /// check any null object
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static bool ContainsNull(params object[] objs)
        {
            foreach (var obj in objs)
            {
                if (obj == null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Make sequencial numbers
        /// </summary>
        /// <param name="start">Start number</param>
        /// <param name="count">Count</param>
        /// <param name="step">Step value from previous number</param>
        /// <returns>The collection</returns>
        public static IEnumerable<int> Seq(int start, int count, int step = 1)
        {
            var n = start;
            for (var i = 0; i < count; i++)
            {
                yield return n;
                n += step;
            }
        }

        /// <summary>
        /// Make sequencial numbers that start with zero
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<int> Seq(int count)
        {
            return Enumerable.Range(0, count);
        }

        /// <summary>
        /// 0, 1, 2, 3
        /// </summary>
        public static IEnumerable<int> Seq4 { get { yield return 0; yield return 1; yield return 2; yield return 3; } }

        /// <summary>
        /// 0, 1, 2, 3, 4, 5, 6, 7
        /// </summary>
        public static IEnumerable<int> Seq8 { get { yield return 0; yield return 1; yield return 2; yield return 3; yield return 4; yield return 5; yield return 6; yield return 7; } }

        /// <summary>
        /// Make sequencial double numbers.
        /// </summary>
        /// <param name="start">start number</param>
        /// <param name="count">count</param>
        /// <param name="step">step value</param>
        /// <returns>the sequencial collection</returns>
        public static IEnumerable<double> Seq(double start, double count, double step = 1.0)
        {
            var n = start;
            for (var i = 0; i < count; i++)
            {
                yield return n;
                n += step;
            }
        }

        /// <summary>
        /// make repeat collection (double type)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<double> Rep(double val, int count)
        {
            for (; count > 0; count--)
            {
                yield return val;
            }
        }

        /// <summary>
        /// make repeat collection (int type)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<int> Rep(int val, int count)
        {
            for (; count > 0; count--)
            {
                yield return val;
            }
        }

        /// <summary>
        /// make null collection start with zero (int)
        /// </summary>
        /// <param name="count">collection count</param>
        /// <returns></returns>
        public static IEnumerable<object> Rep(int count)
        {
            for (; count > 0; count--)
            {
                yield return null;
            }
        }

        /// <summary>
        /// make random value collection (0.0 ... 1.0)
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <remarks>Using MathUtil.Rand() method.</remarks>
        public static IEnumerable<double> Rand(int count)
        {
            for (; count > 0; count--)
            {
                yield return MathUtil.Rand();
            }
        }

        private static readonly Random randomizer = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Make random values of normal distotion (Box–Muller's method)
        /// </summary>
        /// <param name="count">Sample count</param>
        /// <param name="μ">average</param>
        /// <param name="σ">standard deviation</param>
        /// <returns></returns>
        /// <remarks>using random seed (from DateTime.Now.Ticks)</remarks>
        public static IEnumerable<double> NormalDistribution(int count, double μ = 0.0, double σ = 1.0)
        {
            for (; count > 0; count--)
            {
                double r;
                for (r = randomizer.NextDouble(); r == 0.0; r = randomizer.NextDouble())
                {
                    ;    // avoid zero value
                }

                var r2 = randomizer.NextDouble();
                var val = Math.Sqrt(-2.0 * Math.Log(r)) * Math.Cos(2.0 * Math.PI * r2);
                val *= σ + μ;
                yield return val;
            }
        }

        /// <summary>
        /// Make specific seed's random values of normal distotion (Box–Muller's method)
        /// </summary>
        /// <param name="seed">random seed value</param>
        /// <param name="count">sample count</param>
        /// <param name="μ">average</param>
        /// <param name="σ">standard deviation</param>
        /// <returns></returns>
        public static IEnumerable<double> NormalDistribution(int seed, int count, double μ = 0.0, double σ = 1.0)
        {
            var randomizer2 = new Random(seed);
            for (; count > 0; count--)
            {
                double r;
                for (r = randomizer2.NextDouble(); r == 0.0; r = randomizer2.NextDouble())
                {
                    ;    // 0を避ける
                }

                var r2 = randomizer2.NextDouble();
                var val = Math.Sqrt(-2.0 * Math.Log(r)) * Math.Cos(2.0 * Math.PI * r2);
                val *= σ + μ;
                yield return val;
            }
        }
    }
}
