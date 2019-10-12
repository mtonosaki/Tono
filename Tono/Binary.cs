using System;
using System.Collections.Generic;
using System.Linq;

namespace Tono
{
    /// <summary>
    /// Binary utility
    /// </summary>
    public static class Binary
    {
        /// <summary>
        /// make int32 value from 4 bytes
        /// </summary>
        /// <param name="d4">higher byte that start 0x01000000</param>
        /// <param name="d3">3rd byte that starts 0x010000</param>
        /// <param name="d2">2nd byte that starts 0x0100 </param>
        /// <param name="d1">lower byte</param>
        /// <returns></returns>
        public static int MakeInt32FromBytes(byte d4, byte d3, byte d2, byte d1)
        {
            unchecked
            {
                return (d4 << 24) | (d3 << 16) | (d2 << 8) | d1;
            }
        }
        public static int MakeInt32FromBytes(IEnumerable<byte> dat)
        {
            var col = dat.Take(4).ToArray();
            if (col.Length >= 4)
            {
                return MakeInt32FromBytes(col[3], col[2], col[1], col[0]);
            }
            else
            {
                throw new ArgumentException("Binary.MakeInt32FromBytes needs 4-bytes array only.");
            }
        }

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
        /// make reverse bytes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ByteReverse(int value)
        {
            //return MakeInt32FromBytes(ByteReverse4(BitConverter.GetBytes(value)));    // 2,125ms for 10M count
            return BitConverter.ToInt32(BitConverter.GetBytes(value).Reverse().ToArray(), 0);   // 1,200ms for 10M count
        }

        /// <summary>
        /// reverse 4 bytes (Use LINQ.Reverse instead of this)
        /// </summary>
        /// <param name="dat"></param>
        /// <returns></returns>
        public static IEnumerable<byte> ByteReverse4(byte[] dat)
        {
            yield return dat[3];
            yield return dat[2];
            yield return dat[1];
            yield return dat[0];
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
