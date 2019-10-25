// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Runtime.Serialization;

namespace Tono
{
    /// <summary>
    /// Volume type for safe caluclation 
    /// </summary>
    public struct Volume
    {
        /// <summary>
        /// cubic meter
        /// </summary>
        public double m3 { get; set; }

        /// <summary>
        /// cubic centimeter
        /// </summary>
        [IgnoreDataMember]
        public double cm3 => m3 * 1e6;

        /// <summary>
        /// cubic milimeter
        /// </summary>
        [IgnoreDataMember]
        public double mm3 => m3 * 1e9;

        public override string ToString()
        {
            return $"{m3:0.###}[m3]";
        }

        /// <summary>
        /// x + y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Volume operator +(Volume x, Volume y)
        {
            return new Volume { m3 = x.m3 + y.m3, };
        }

        /// <summary>
        /// x - y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Volume operator -(Volume x, Volume y)
        {
            return new Volume { m3 = x.m3 - y.m3, };
        }

        /// <summary>
        /// x * y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Volume operator *(Volume x, double y)
        {
            return new Volume { m3 = x.m3 * y, };
        }

        /// <summary>
        /// x divided by y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Volume operator /(Volume x, double y)
        {
            return new Volume { m3 = x.m3 / y, };
        }
    }
}
