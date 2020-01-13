// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Tono.Logic
{
    /// <summary>
    /// Interpolation function
    /// </summary>
    public static class Interpolation
    {
        /// <summary>
        /// ease-in curve interpolation
        /// </summary>
        /// <param name="x">input time(0～1.0)</param>
        /// <param name="exponent">curve exponent 2...</param>
        /// <returns>output value(0.0～1.0)</returns>
        public static double EaseIn(double x, double exponent = 2)
        {
            Debug.Assert(exponent > 0 && exponent < double.PositiveInfinity);
            if (x < double.Epsilon)
            {
                return 0;
            }
            if (x > 1.0)
            {
                return 1.0;
            }
            return Math.Pow(x, exponent);
        }

        /// <summary>
        /// ease-out curve interpolation
        /// </summary>
        /// <param name="x">input time(0～1.0)</param>
        /// <param name="exponent">curve exponent 2...</param>
        /// <returns>output value(0.0～1.0)</returns>
        public static double EaseOut(double x, double exponent = 2)
        {
            Debug.Assert(exponent > 0 && exponent < double.PositiveInfinity);
            if (x < double.Epsilon)
            {
                return 0;
            }

            if (x > 1.0)
            {
                return 1.0;
            }

            return 1.0 - Math.Pow(1.0 - x, exponent);
        }

        /// <summary>
        /// ease-in-out curve interpolation
        /// </summary>
        /// <param name="x">input time(0～1.0)</param>
        /// <param name="exponent">curve exponent 2...</param>
        /// <returns>output value(0.0～1.0)</returns>
        public static double EaseInOut(double x, double exponent = 2)
        {
            if (x < double.Epsilon)
            {
                return 0;
            }

            if (x > 1.0)
            {
                return 1.0;
            }

            if (x <= 0.5)
            {
                return EaseIn(x * 2, exponent) / 2;
            }
            else
            {
                return EaseOut((x - 0.5) * 2, exponent) / 2 + 0.5;
            }
        }
    }
}
