// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Runtime.Serialization;

namespace Tono
{
    /// <summary>
    /// Weight type for safe caluclation with Newtonian mechanics
    /// </summary>
    public struct Weight
    {
        /// <summary>
        /// gram
        /// </summary>
        public double g { get; set; }

        /// <summary>
        /// make instance from kilogram
        /// </summary>
        /// <param name="kg"></param>
        /// <returns></returns>
        public static Weight FromKg(double kg)
        {
            return new Weight
            {
                g = kg * 1000,
            };
        }

        /// <summary>
        /// kilogram
        /// </summary>
        [IgnoreDataMember]
        public double kg => g / 1e3;

        /// <summary>
        /// miligram
        /// </summary>
        [IgnoreDataMember]
        public double mg => g * 1e3;

        /// <summary>
        /// microgram
        /// </summary>
        [IgnoreDataMember]
        public double μg => g * 1e6;

        /// <summary>
        /// ton = 1,000kg
        /// </summary>
        [IgnoreDataMember]
        public double ton => g / 1e6;

        /// <summary>
        /// mega ton = 1,000,000kg
        /// </summary>
        [IgnoreDataMember]
        public double Mton => g / 1e12;

        /// <summary>
        /// make good unit string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (g >= 1e3 && g < 1e6)
            {
                return $"{kg:0.###}[kg]";
            }
            if (g >= 1e12)
            {
                return $"{Mton:0.###}[Mton]";
            }
            if (g >= 1e6)
            {
                return $"{ton:0.###}[ton]";
            }
            if (g >= 1e-3 && g < 1)
            {
                return $"{mg:0.###}[mg]";
            }
            if (g >= 1e-6 && g < 1e-3)
            {
                return $"{μg:0.###}[μg]";
            }
            return $"{g:0.###}[g]";
        }
    }
}
