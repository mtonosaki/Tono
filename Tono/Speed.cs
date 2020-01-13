// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Text.RegularExpressions;

namespace Tono
{
    /// <summary>
    /// Speed type for safe caluclation 
    /// </summary>
    public struct Speed : IEquatable<Speed>
    {
        public static readonly Speed Zero = From_meter_per_second(0);

        private double mps;    // Meter Per Second

        /// <summary>
        /// make instance from distance divided by time
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Speed From(Distance dist, TimeSpan time)
        {
            return new Speed
            {
                mps = dist.m / time.TotalSeconds,
            };
        }

        /// <summary>
        /// make instance from value[meter/second]
        /// </summary>
        /// <param name="mps">m/second</param>
        /// <returns></returns>
        public static Speed From_meter_per_second(double mps)
        {
            return new Speed
            {
                mps = mps
            };
        }

        /// <summary>
        /// make instance from value[kilometer/hour]
        /// </summary>
        /// <param name="kmph">km/ｈ</param>
        /// <returns></returns>
        public static Speed From_km_per_hour(double kmph)
        {
            return new Speed
            {
                mps = kmph / 3.6
            };
        }

        /// <summary>
        /// make instance from value[meter/minute]
        /// </summary>
        /// <param name="mps">meter/minute</param>
        /// <returns></returns>
        public static Speed From_meter_per_minute(double mpm)
        {
            return new Speed
            {
                mps = mpm * 60
            };
        }

        /// <summary>
        /// make string of [km/h]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}[km/h]", mps * 3.6);
        }

        /// <summary>
        /// make distance of distance/hour
        /// </summary>
        /// <returns></returns>
        public Distance DistancePerHour => Tono.Distance.FromMeter(mps * 3600);

        /// <summary>
        /// make new instance of distance/minute
        /// </summary>
        /// <returns></returns>
        public Distance DistancePerMinute => Tono.Distance.FromMeter(mps * 60);

        /// <summary>
        /// make distance of distance/second
        /// </summary>
        /// <returns></returns>
        public Distance DistancePerSecond => Tono.Distance.FromMeter(mps);

        /// <summary>
        /// make distance of distance/span
        /// </summary>
        /// <returns></returns>
        public Distance Distance(TimeSpan span)
        {
            return Tono.Distance.FromMeter(mps * span.TotalSeconds);
        }

        /// <summary>
        /// make time of the distance
        /// </summary>
        /// <returns></returns>
        public TimeSpan Time(Distance dist)
        {
            return TimeSpan.FromSeconds(dist.m / mps);
        }

        /// <summary>
        /// make time from acceleration
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public TimeSpan Time(Acceleration y)
        {
            return TimeSpan.FromSeconds(Math.Abs(mps) / (2 * Math.Abs(y.To_m_per_s_per_s)));
        }

        /// <summary>
        /// x + y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Speed operator +(Speed x, Speed y)
        {
            return new Speed { mps = x.mps + y.mps };
        }

        /// <summary>
        /// x - y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Speed operator -(Speed x, Speed y)
        {
            return new Speed { mps = x.mps - y.mps };
        }

        /// <summary>
        /// x * y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Speed operator *(Speed x, double y)
        {
            return new Speed { mps = x.mps * y };
        }

        /// <summary>
        /// x / y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Speed operator /(Speed x, double y)
        {
            return new Speed { mps = x.mps / y };
        }

        /// <summary>
        /// x * y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Distance operator *(Speed x, TimeSpan y)
        {
            return Tono.Distance.From(x, y);
        }

        /// <summary>
        /// Check negative value
        /// </summary>
        /// <returns>true=negative(minus) speed</returns>
        public bool IsNegative()
        {
            return mps < 0;
        }

        public override bool Equals(object obj)
        {
            return obj is Speed && Equals((Speed)obj);
        }

        public bool Equals(Speed other)
        {
            return mps == other.mps;
        }

        public override int GetHashCode()
        {
            return 563816345 + mps.GetHashCode();
        }

        /// <summary>
        /// v1 less than v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <(Speed v1, Speed v2)
        {
            return (v1.mps < v2.mps);
        }

        /// <summary>
        /// v1 greater than v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >(Speed v1, Speed v2)
        {
            return (v1.mps > v2.mps);
        }

        /// <summary>
        /// v1 less than or equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <=(Speed v1, Speed v2)
        {
            return (v1.mps <= v2.mps);
        }

        /// <summary>
        /// v1 greater than or equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >=(Speed v1, Speed v2)
        {
            return (v1.mps >= v2.mps);
        }

        /// <summary>
        /// v1 equal to v2 (NOTE: compare double type)
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator ==(Speed v1, Speed v2)
        {
            return (v1.mps == v2.mps);
        }

        /// <summary>
        /// v1 not equal to v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator !=(Speed v1, Speed v2)
        {
            return (v1.mps != v2.mps);
        }

        /// <summary>
        /// make instance from string: support unit m/s, m/m, m/h, km/s, km/m, km/h, cm/s, cm/m, cm/h
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Speed Parse(string str)
        {
            var match = Regex.Match(str, "([0-9]+(?:.[0-9]+)?)([ck]?m)/([hms])");
            if (!match.Success)
            {
                throw new FormatException("not Matching Format '([0-9]+(?:.[0-9]+)?)([ck]?m)/([hms])'");
            }
            var value = double.Parse(match.Groups[1].Value);
            switch (match.Groups[2].Value)
            {
                case "cm":
                    value /= 100;
                    break;
                case "km":
                    value *= 1000;
                    break;
            }
            switch (match.Groups[3].Value)
            {
                case "h":
                    value /= 3600;
                    break;
                case "m":
                    value /= 60;
                    break;
            }
            return Speed.From_meter_per_second(value);
        }
    }
}
