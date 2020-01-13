// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.
using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Tono
{
    /// <summary>
    /// Distance type for safe caluclation 
    /// </summary>
    /// <remarks>
    /// Euclidean geometry
    /// </remarks>
    public struct Distance : IEquatable<Distance>
    {
        /// <summary>
        /// Zero value
        /// </summary>
        public static readonly Distance Zero = new Distance { _val = 0 };

        /// <summary>
        /// Infinity
        /// </summary>
        public static readonly Distance PositiveInfinity = new Distance { _val = double.PositiveInfinity };

        /// <summary>
        /// instance value [meter]
        /// </summary>
        [IgnoreDataMember]
        private double _val;

        /// <summary>
        /// v1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Distance operator +(Distance v1, Distance v2)
        {
            return new Distance { _val = v1._val + v2._val };
        }

        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Distance operator -(Distance v1, Distance v2)
        {
            return new Distance { _val = v1._val - v2._val };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Distance operator *(Distance v1, double v2)
        {
            return new Distance { _val = v1._val * v2 };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Distance operator *(Distance v1, int v2)
        {
            return new Distance { _val = v1._val * v2 };
        }

        /// <summary>
        /// v1 divided by v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Distance operator /(Distance v1, double v2)
        {
            return new Distance { _val = v1._val / v2 };
        }

        /// <summary>
        /// v1 divided by v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Distance operator /(Distance v1, int v2)
        {
            return new Distance { _val = v1._val / v2 };
        }

        /// <summary>
        /// get time span from distance and speed
        /// </summary>
        /// <param name="distance">distance</param>
        /// <param name="speed">speed</param>
        /// <returns></returns>
        public static TimeSpan operator /(Distance distance, Speed speed)
        {
            return TimeSpan.FromSeconds(distance.m / speed.DistancePerSecond.m);
        }

        /// <summary>
        /// get speed from distance and time
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Speed operator /(Distance v1, TimeSpan v2)
        {
            return Speed.From(v1, v2);
        }

        /// <summary>
        /// v1 less than v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <(Distance v1, Distance v2)
        {
            return (v1._val < v2._val);
        }

        /// <summary>
        /// v1 greater than v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >(Distance v1, Distance v2)
        {
            return (v1._val > v2._val);
        }

        /// <summary>
        /// v1 less than or equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <=(Distance v1, Distance v2)
        {
            return (v1._val <= v2._val);
        }

        /// <summary>
        /// v1 greater than or equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >=(Distance v1, Distance v2)
        {
            return (v1._val >= v2._val);
        }

        /// <summary>
        /// v1 equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator ==(Distance v1, Distance v2)
        {
            return (v1._val == v2._val);
        }

        /// <summary>
        /// v1 not equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator !=(Distance v1, Distance v2)
        {
            return (v1._val != v2._val);
        }

        /// <summary>
        /// Get kilometer value
        /// </summary>
        [IgnoreDataMember]
        public double km
        {
            get => _val / 1000.0;
            set => _val = value * 1000.0;
        }

        /// <summary>
        /// Get meter value
        /// </summary>
        public double m
        {
            get => _val;
            set => _val = value;
        }

        /// <summary>
        /// Get meter value round off to meter
        /// </summary>
        /// <returns></returns>
        public Distance ToRound_m(double min_m = 1.0)
        {
            return new Distance
            {
                m = Math.Round(m / min_m) * min_m,
            };
        }

        /// <summary>
        /// get centimeter value
        /// </summary>
        [IgnoreDataMember]
        public double cm
        {
            get => _val * 100.0;
            set => _val = value / 100.0;
        }

        /// <summary>
        /// make distance from speed and time.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Distance From(Speed speed, TimeSpan time)
        {
            return new Distance
            {
                _val = speed.DistancePerSecond.m * time.TotalSeconds,
            };
        }

        /// <summary>
        /// make instance from kilometer value
        /// </summary>
        /// <param name="kilometer"></param>
        /// <returns></returns>
        public static Distance FromKm(double kilometer)
        {
            return new Distance
            {
                _val = kilometer * 1000.0
            }; ;
        }

        /// <summary>
        /// make value from kilometer string but use default value if parse error.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static Distance FromKm(string v, double def = 0)
        {
            return new Distance
            {
                _val = (double.TryParse(v, out double val) ? val : def) * 1000.0
            };
        }

        /// <summary>
        /// make Distance from meter value
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        public static Distance FromMeter(double meter)
        {
            return new Distance
            {
                m = meter,
            };
        }

        /// <summary>
        /// make instance from meter string but return default value if parse error
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static Distance FromMeter(string meter, double def)
        {
            return new Distance
            {
                m = double.TryParse(meter, out double val) ? val : def,
            };
        }

        /// <summary>
        /// make good unit string km / m / cm
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (km >= 1)
            {
                return $"{km:0.0}km";
            }
            if (m >= 1)
            {
                return $"{m:0.0}m";
            }
            return $"{cm:0}cm";
        }

        public override bool Equals(object obj)
        {
            return obj is Distance && Equals((Distance)obj);
        }

        public bool Equals(Distance other)
        {
            return _val == other._val;
        }

        public override int GetHashCode()
        {
            return _val.GetHashCode();
        }

        /// <summary>
        /// make Distance instance from string (suppert unit : m, cm, km)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Distance Parse(string str)
        {
            var match = Regex.Match(str, "([0-9]+(?:.[0-9]+)?)([ck]?m)");
            if (!match.Success)
            {
                throw new FormatException("not Matching Format '([0-9]+(?:.[0-9]+)?)([ck]?m)'");
            }
            var value = double.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value;

            switch (unit)
            {
                case "cm":
                    value /= 100;
                    break;
                case "km":
                    value *= 1000;
                    break;
            }
            return new Distance { _val = value };
        }
    }
}
