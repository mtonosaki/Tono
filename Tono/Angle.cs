// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;

namespace Tono
{
    /// <summary>
    /// Angle type for safe caluclation 
    /// </summary>
    public struct Angle
    {
        public static readonly Angle Zero = Angle.FromDeg(0);

        /// <summary>
        /// Pi = 3.14159...
        /// </summary>
        public const double PI = Math.PI;

        /// <summary>
        /// Convert to radian
        /// </summary>
        public double Rad { get; set; }

        /// <summary>
        /// Convert to degree
        /// </summary>
        [IgnoreDataMember]
        public double Deg
        {
            get => Rad * 180 / PI;
            set => Rad = value * PI / 180;
        }

        /// <summary>
        /// Get Normalized instance 0-360 Deg
        /// </summary>
        public Angle Normalized
        {
            get
            {
                var d = Deg;
                d = d % 360.0;
                if( d < 0)
                {
                    d = 360.0 - d;
                }
                return Angle.FromDeg(d);
            }
        }

        /// <summary>
        /// Create an instance from degree
        /// </summary>
        /// <param name="deg"></param>
        /// <returns></returns>
        public static Angle FromDeg(double deg)
        {
            return new Angle { Deg = deg };
        }

        /// <summary>
        /// Create an instance from radian
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static Angle FromRad(double rad)
        {
            return new Angle { Rad = rad };
        }

        /// <summary>
        /// make string of the instance
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Rad} radian ({Deg:0.0} degree)";
        }

        /// <summary>
        /// Check it is zero instance
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            if (Math.Abs(Rad) < 0.000001f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// v0 + v1
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Angle operator +(Angle v0, Angle v1)
        {
            return new Angle { Rad = v0.Rad + v1.Rad };
        }

        /// <summary>
        /// v0 - v1
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Angle operator -(Angle v0, Angle v1)
        {
            return new Angle { Rad = v0.Rad - v1.Rad };
        }

        /// <summary>
        /// v0 * v1
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Angle operator *(Angle v0, double v1)
        {
            return new Angle { Rad = v0.Rad * v1 };
        }

        /// <summary>
        /// v0 devide by v1
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Angle operator /(Angle v0, double v1)
        {
            return new Angle { Rad = v0.Rad / v1 };
        }

        /// <summary>
        /// Make angle from (x0, y0) to (x1, y1). (Y-Axis Plus = bottom)
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        /// <remarks>0=3 O'clock, 90=12 O'clock, 270=6 O'clock</remarks>
        public static Angle From(double x0, double y0, double x1, double y1)
        {
            var lx = x1 - x0;
            var ly = -(y1 - y0);
            var at = Math.Atan2(ly, lx) * 180 / Math.PI;
            at = at % 360;
            if (at < 0)
            {
                at = 360 + at;
            }
            return new Angle { Deg = at };
        }

        /// <summary>
        /// Make angle from (lon0, lat0) to (lon1, lat1) (Y-Axis Plus = bottom, North latitude coordinate system)
        /// </summary>
        /// <param name="lon0"></param>
        /// <param name="lat0"></param>
        /// <param name="lon1"></param>
        /// <param name="lat1"></param>
        /// <returns></returns>
        public static Angle From(Longitude lon0, Latitude lat0, Longitude lon1, Latitude lat1)
        {
            return Angle.FromDeg(360) - From(lon0.Lon.Deg, lat0.Lat.Deg, lon1.Lon.Deg, lat1.Lat.Deg);
        }

        /// <summary>
        /// Make angle from s0 to s1
        /// </summary>
        /// <param name="s0"></param>
        /// <param name="s1"></param>
        /// <returns></returns>
        public static Angle From((double X, double Y) s0, (double X, double Y) s1)
        {
            return Angle.From(s0.X, s0.Y, s1.X, s1.Y);
        }
    }
}
