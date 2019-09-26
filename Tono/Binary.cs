using System;
using System.Collections.Generic;

namespace Tono
{
    public class Binary
    {
        /// <summary>
        /// Compare binary foreach bytes
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static int Comparer(byte[] d1, byte[] d2)
        {
            foreach (var i in Collection.Seq(Math.Max(d1.Length, d2.Length)))
            {
                var v1 = i >= d1.Length ? 0 : d1[i];
                var v2 = i >= d2.Length ? 0 : d2[i];
                if (v1 < v2)
                {
                    return -1;
                }

                if (v1 > v2)
                {
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Right rotate n bits
        /// </summary>
        /// <param name="value"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int BitRotateRight(int value, int n)
        {
            unchecked
            {
                return (int)(((uint)value >> n) | ((uint)value << (32 - n)));
            }
        }

        /// <summary>
        /// Right shift n bits
        /// </summary>
        /// <param name="value"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int BitShiftRight(int value, int n)
        {
            return value >> n;
        }

        /// <summary>
        /// make binary collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<bool> GetBits(int value)
        {
            unchecked
            {
                for (uint i = (uint)int.MinValue; i != 0; i >>= 1)
                {
                    yield return (value & i) != 0 ? true : false;
                }
            }
        }
    }
}
