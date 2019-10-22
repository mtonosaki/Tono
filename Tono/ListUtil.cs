// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// List Utility
    /// </summary>
    public static class ListUtil
    {
        /// <summary>
        /// Find index of value with binary search. return index of smaller value if not hit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="comparer"></param>
        /// <returns>list index of the result</returns>
        /// <example>
        /// var list = new List＜KeyValue＞{ ... }
        /// var ret = uCollection＜KeyValue＞.BinarySearchIndex(list, tar => 987 - tar.Key);
        /// → find number 987 from KeyValue.Keys
        /// </example>
        public static int BinarySearchIndex<T>(IReadOnlyList<T> list, Func<T, int> keyComparer)
        {
            if (list.Count == 1)
            {
                return 0;
            }

            if (list.Count == 0)
            {
                return -1;
            }

            var r = list.Count - 1;
            var l = 0;

            for (; ; )
            {
                var c = (r + l) / 2;
                if (c == l)
                {
                    for (; c >= 0 && keyComparer(list[c]) < 0; c--)
                    {
                        ;
                    }

                    if (c < 0)
                    {
                        c = 0;
                    }

                    return c;
                }
                var com = keyComparer(list[c]);
                if (com == 0)
                {
                    return c;
                }

                if (com < 0)
                {
                    r = c - 1;
                }
                else
                {
                    l = c + 1;
                }
            }
        }
    }

    /// <summary>
    /// List extentions
    /// </summary>
    public static class ListExtention
    {
        /// <summary>
        /// Find value with binary search. return value of smaller one if not hit.(Extention)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="keyComparer"></param>
        /// <returns></returns>
        /// <example>
        /// var list = new List＜KeyValue＞{ ... }
        /// var ret = uCollection＜KeyValue＞.BinarySearchIndex(list, tar => 987 - tar.Key);
        /// → find number 987 from KeyValue.Keys
        /// </example>
        public static T BinarySearch<T>(this List<T> list, Func<T, int> keyComparer)
        {
            var id = BinarySearchIndex(list, keyComparer);
            if (id < 0)
            {
                return default(T);
            }

            return list[id];
        }


        /// <summary>
        /// Find index of value with binary search. return index of smaller value if not hit.(Extention)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="keyComparer"></param>
        /// <returns></returns>
        public static int BinarySearchIndex<T>(this List<T> list, Func<T, int> keyComparer)
        {
            return ListUtil.BinarySearchIndex<T>(list, keyComparer);
        }

        /// <summary>
        /// Collect equal objects (use binary search)
        /// </summary>
        /// <param name="list">sorted list</param>
        /// <param name="keyComparer">CompareTo method. minus:key＜T, zero:key==T, plus:key＞T</param>
        /// <returns></returns>
        public static IEnumerable<T> CollectItems<T>(this List<T> list, Func<T, int> keyComparer)
        {
            var l = 0;
            var r = list.Count - 1;
            var c = 0;
            var isFound = false;
            for (; l <= r;)
            {
                c = (l + r) / 2;
                int comp = keyComparer(list[c]);
                if (comp == 0)
                {
                    isFound = true;
                    break;
                }
                else
                if (comp > 0)
                {
                    l = c + 1;
                }
                else
                if (comp < 0)
                {
                    r = c - 1;
                }
            }
            if (isFound)
            {
                int s;
                for (s = c; s >= 0; s--)
                {
                    int eq = keyComparer(list[s]);
                    if (eq != 0)
                    {
                        s++;
                        break;
                    }
                }
                if (s < 0)
                {
                    s = 0;
                }

                var ret = new List<T>();
                for (int i = s; i < list.Count; i++)
                {
                    int eq = keyComparer(list[i]);
                    if (eq == 0)
                    {
                        ret.Add(list[i]);
                    }
                    else
                    {
                        break;
                    }
                }
                return ret;
            }
            return new List<T>();
        }

        /// <summary>
        /// Check the all values are same
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool EqualsAll<T>(this List<T> list)
        {
            var isFirst = true;
            T first = default;
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
        /// Check the all values are same with val
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool EqualsAll<T>(this List<T> list, T val)
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
        /// Check the all values are same with deffered execution
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="equalFunc"></param>
        /// <returns></returns>
        public static bool EqualsAll<T>(this List<T> list, Func<T, bool> equalFunc)
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
}
