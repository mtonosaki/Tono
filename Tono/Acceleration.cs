// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono
{
    /// <summary>
    /// Acceleration type for safe caluclation 
    /// </summary>
    public class Acceleration : IEquatable<Acceleration>
    {
        /// <summary>
        /// m/s/s
        /// </summary>
        private double m_psps;
        public static readonly Acceleration Zero = new Acceleration { m_psps = 0, };

        public double To_m_per_s_per_s => m_psps;

        /// <summary>
        /// Create an instance from value[m/s/s]
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Acceleration FromMeterPerScondPerScond(double val)
        {
            return new Acceleration
            {
                m_psps = val,
            };
        }

        /// <summary>
        /// Create an instance from string.
        /// </summary>
        /// <param name="str">string ends with m/s/s</param>
        /// <returns></returns>
        public static Acceleration Parse(string str)
        {
            string s = str.Trim();
            if (s.EndsWith("m/s/s"))
            {
                return FromMeterPerScondPerScond(double.Parse(StrUtil.LeftBefore(s, "m/s")));
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Get speed acc * time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Speed operator *(Acceleration acc, TimeSpan time)
        {
            return Speed.From_meter_per_second(acc.m_psps * time.Seconds);
        }

        /// <summary>
        /// Get distance of the time span.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Distance Distance(TimeSpan time)
        {
            return Tono.Distance.FromMeter(Math.Abs(m_psps) * time.Seconds * time.Seconds / 2);
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Acceleration tar)
            {
                return m_psps == tar.m_psps;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Acceleration other)
        {
            return other != null &&
                   m_psps == other.m_psps;
        }

        /// <summary>
        /// Make hash code of instance
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = -1710505915;
            hashCode = hashCode * -1521134295 + m_psps.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// v1 less than v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <(Acceleration v1, Acceleration v2)
        {
            return (v1.m_psps < v2.m_psps);
        }

        /// <summary>
        /// v1 greater than v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >(Acceleration v1, Acceleration v2)
        {
            return (v1.m_psps > v2.m_psps);
        }

        /// <summary>
        /// v1 less than or equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <=(Acceleration v1, Acceleration v2)
        {
            return (v1.m_psps <= v2.m_psps);
        }

        /// <summary>
        /// v1 greater than or equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >=(Acceleration v1, Acceleration v2)
        {
            return (v1.m_psps >= v2.m_psps);
        }

        /// <summary>
        /// v1 equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator ==(Acceleration v1, Acceleration v2)
        {
            if (v1 == null && v2 == null) return true;

            return v1?.Equals(v2) ?? false;
        }

        /// <summary>
        /// v1 not equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator !=(Acceleration v1, Acceleration v2)
        {
            if (v1 == null && v2 == null) return false;

            return !(v1?.Equals(v2) ?? false);
        }

        public override string ToString()
        {
            return $"{To_m_per_s_per_s}m/s/s";
        }
    }
}
