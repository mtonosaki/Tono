// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// Euclidean geometry utility
    /// </summary>
    public class GeoEu
    {

        /// <summary>
        /// Equatorial radius[mm]
        /// </summary>
        public const double EarthRadiusX = 6378137000;

        /// <summary>
        /// Polar radius[mm]
        /// </summary>
        public const double EarthRadiusY = 6356752000;

        /// <summary>
        /// Make positon (0,0)=Left-Top
        /// </summary>
        /// <param name="org">origin position</param>
        /// <param name="angle"></param>
        /// <param name="length">length no unit</param>
        /// <returns></returns>
        public static (double X, double Y) Position((double X, double Y) org, Angle angle, float length)
        {
            var x = (float)(org.X + Math.Cos(angle.Rad) * length);
            var y = (float)(org.Y - Math.Sin(angle.Rad) * length);
            return (x, y);
        }

        /// <summary>
        /// Caluclate distance between (lon0,lat0) - (lon1, lat1) with grate circle method (NOT Accuracy)
        /// </summary>
        /// <param name="lon0"></param>
        /// <param name="lat0"></param>
        /// <param name="lon1"></param>
        /// <param name="lat1"></param>
        /// <returns></returns>
        /// <remarks>Especially for Japan Tokyo area</remarks>
        public static Distance Distance(Longitude lon0, Latitude lat0, Longitude lon1, Latitude lat1)
        {
            var dx = lon1.Lon.Deg - lon0.Lon.Deg;
            var dy = lat1.Lat.Deg - lat0.Lat.Deg;
            dx = dx / 360 * 2 * Math.PI * EarthRadiusX / 1000 * Math.Cos((lat0.Lat.Deg + lat1.Lat.Deg) / 2 * Math.PI / 180);
            dy = dy / 360 * 2 * Math.PI * EarthRadiusY / 1000;
            var ret = Math.Sqrt(dx * dx + dy * dy);

            // LonLat間の直線距離は、地面を潜る分だけ短く出る。東京本社→トヨタ本社で、65ｍの差異。この値で簡易的に補正
            ret = ret * 246060.92770516232 / 245998.87656939359;

            return Tono.Distance.FromKm(ret / 1000);
        }

        /// <summary>
        /// Caluclate position of circle
        /// </summary>
        /// <param name="x">orign position</param>
        /// <param name="y">origin position</param>
        /// <param name="angle"></param>
        /// <param name="length">radius</param>
        /// <param name="rx">test position X</param>
        /// <param name="ry">test position Y</param>
        public static void Position(double x, double y, Angle angle, double length, out double rx, out double ry)
        {
            rx = x + Math.Cos(angle.Rad) * length;
            ry = y - Math.Sin(angle.Rad) * length;
        }

        /// <summary>
        /// Caluclate length (0,0)-(X,Y) (Pythagorean theorem)
        /// </summary>
        /// <param name="position">position from (0,0)</param>
        /// <returns></returns>
        public static double Length((double X, double Y) position)
        {
            return Length((0, 0), position);
        }

        /// <summary>
        /// Caluclate diagonal length of rectangle
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <returns></returns>
        public static double Length(double w, double h)
        {
            return Length(0, 0, w, h);
        }

        /// <summary>
        /// Caluclate length between p0 and p (Pythagorean theorem)
        /// </summary>
        /// <param name="p0">origin</param>
        /// <param name="p">position</param>
        /// <returns></returns>
        public static double Length((double X, double Y) p0, (double X, double Y) p)
        {
            return Math.Sqrt((p.X - p0.X) * (p.X - p0.X) + (p.Y - p0.Y) * (p.Y - p0.Y));
        }

        /// <summary>
        /// Caluclate length between (x0,y0)-(x1,y1) (Pythagorean theorem)
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static double Length(double x0, double y0, double x1, double y1)
        {
            return Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
        }

        /// <summary>
        /// Caluclate angle from (x0,y0) to (x1, y1)  (3 O'clock=0[deg], 12 O'clock=90[deg], 6 O'clock=270[deg])
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static Angle Angle(double x0, double y0, double x1, double y1)
        {
            double lx = x1 - x0;
            double ly = -(y1 - y0);
            double at = Math.Atan2(ly, lx) * 180 / Math.PI;
            at %= 360;
            if (at < 0)
            {
                at = 360 + at;
            }

            return Tono.Angle.FromDeg((float)at);
        }

        /// <summary>
        /// Caluclate angle from p0 to p1  (3 O'clock=0[deg], 12 O'clock=90[deg], 6 O'clock=270[deg])
        /// </summary>
        /// <param name="p0">origin</param>
        /// <param name="p">position</param>
        /// <returns></returns>
        public static Angle Angle((double X, double Y) p0, (double X, double Y) p)
        {
            return Angle(p0.X, p0.Y, p.X, p.Y);
        }

        /// <summary>
        /// Caluclate angle from (lon0,lat0) to (lon1,lat1)  (3 O'clock=0[deg], 12 O'clock=90[deg], 6 O'clock=270[deg])
        /// </summary>
        /// <param name="lon0"></param>
        /// <param name="lat0"></param>
        /// <param name="lon1"></param>
        /// <param name="lat1"></param>
        /// <returns></returns>
        public static Angle Angle(Longitude lon0, Latitude lat0, Longitude lon1, Latitude lat1)
        {
            return Angle(lon0.Lon.Deg, lat0.Lat.Deg, lon1.Lon.Deg, lat1.Lat.Deg);
        }

        /// <summary>
        /// Check (px,py) is on line (x0,y0)-(x1,y1)
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="width"></param>
        /// <returns>true=ON</returns>
        public static bool IsOnline(double x0, double y0, double x1, double y1, double px, double py, double width = 1.0)
        {
            var R = Length(x0, y0, x1, y1);
            var A = Angle(x0, y0, x1, y1);
            var l = Length(x0, y0, px, py);
            var a = Angle(x0, y0, px, py);
            Position(0, 0, a - A, l, out var mpx, out var mpy);
            var T = -width / 2;
            var B = +width / 2;
            return 0.0 <= mpx && R >= mpx && T <= mpy && B >= mpy;
        }

        /// <summary>
        /// Check pos is on line p0-p1
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static bool IsOnline((double X, double Y) p0, (double X, double Y) p1, (double X, double Y) pos, double width = 1.0) => IsOnline(p0.X, p0.Y, p1.X, p1.Y, pos.X, pos.Y, width);

        /// <summary>
        /// Result mode when the coordinates of the straight line are inside the circle
        /// </summary>
        public enum IntersectionResultMode
        {
            /// <summary>
            /// No result
            /// </summary>
            None,

            /// <summary>
            /// Result of the position(s) on the line
            /// </summary>
            LinePosition,

            /// <summary>
            /// Result of the positions on the extention line
            /// </summary>
            Intersection,
        }

        /// <summary>
        /// Get X,Y position of inscribed square in a circle
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static (double X, double Y) GetLocationOfInscribedSquareInCircle(Angle angle)
        {
            const double cos45 = 0.7071067811865297; // Cos(45deg)

            if (angle.Deg >= 360 - 45 || angle.Deg < 45.0)
            {
                var a = Math.Sin(angle.Rad) / Math.Cos(angle.Rad);
                var x = cos45;
                var y = a * x;
                return (x, y);
            }
            else if (angle.Deg >= 45.0 && angle.Deg < 135.0)
            {
                var a = Math.Cos(angle.Rad) / Math.Sin(angle.Rad);
                var y = cos45;
                var x = a * y;
                return (x, y);
            }
            else if (angle.Deg >= 135.0 && angle.Deg < 225.0)
            {
                var a = Math.Sin(angle.Rad) / Math.Cos(angle.Rad);
                var x = -cos45;
                var y = a * x;
                return (x, y);
            }
            else
            {
                var a = Math.Cos(angle.Rad) / Math.Sin(angle.Rad);
                var y = -cos45;
                var x = a * y;
                return (x, y);
            }
        }

        /// <summary>
        /// Calculate the intersection of a circle and a straight line
        /// </summary>
        /// <param name="center">center position of circle</param>
        /// <param name="radius">radius of circle</param>
        /// <param name="linestart">line start position</param>
        /// <param name="lineend">line end position</param>
        /// <param name="mode">IntersectionResultMode</param>
        /// <returns>the intersection point(s)</returns>
        public static (double X, double Y)[] GetPointsOfIntersection((double X, double Y) center, double radius, (double X, double Y) linestart, (double X, double Y) lineend, IntersectionResultMode mode)
        {
            return GetPointsOfIntersection(center.X, center.Y, radius, linestart.X, linestart.Y, lineend.X, lineend.Y, mode);
        }

        /// <summary>
        /// Calculate the intersection of a circle and a straight line
        /// </summary>
        /// <param name="cx">center X of circle</param>
        /// <param name="cy">center Y of circle</param>
        /// <param name="cr">radius of circle</param>
        /// <param name="lsx">line start position X</param>
        /// <param name="lsy">line start position Y</param>
        /// <param name="lex">line end position X</param>
        /// <param name="ley">line end position Y</param>
        /// <param name="mode">IntersectionResultMode</param>
        /// <returns>the intersection point(s)</returns>
        public static (double X, double Y)[] GetPointsOfIntersection(
            double cx, double cy, double cr,
            double lsx, double lsy, double lex, double ley,
            IntersectionResultMode mode
        )
        {
            var rets = new List<(double X, double Y)>();

            // ax + by + c = 0
            var a = ley - lsy;
            var b = lsx - lex;
            var c = -(a * lsx + b * lsy);

            // hanging the vertical straight line from the center of the circle 
            var L = Math.Sqrt((lex - lsx) * (lex - lsx) + (ley - lsy) * (ley - lsy));

            // vector(ex,ey)
            var vector_x = (lex - lsx) / L;
            var vector_y = (ley - lsy) / L;
            var ra_vector_x = -vector_y;
            var ra_vector_y = vector_x;
            var k = -(a * cx + b * cy + c) / (a * ra_vector_x + b * ra_vector_y);

            if (cr < k) // not cross to circle line
            {
                return rets.ToArray();
            }

            var virtical_x = cx + k * ra_vector_x;
            var virtical_y = cy + k * ra_vector_y;
            var distance_between_cross_and_vertical = Math.Sqrt(cr * cr - k * k);

            var x1 = virtical_x - distance_between_cross_and_vertical * vector_x;
            var y1 = virtical_y - distance_between_cross_and_vertical * vector_y;
            var r1 = Length(cx, cy, lsx, lsy);

            var x2 = virtical_x + distance_between_cross_and_vertical * vector_x;
            var y2 = virtical_y + distance_between_cross_and_vertical * vector_y;
            var r2 = Length(cx, cy, lex, ley);

            if (r1 > cr && r2 > cr)
            {
                var ds1 = Length(x1, y1, lsx, lsy);
                var ds2 = Length(x2, y2, lsx, lsy);
                var de1 = Length(x1, y1, lex, ley);
                var de2 = Length(x2, y2, lex, ley);
                if (ds1 < ds2 && de1 < de2)
                {
                    return rets.ToArray();
                }
                if (ds1 > ds2 && de1 > de2)
                {
                    return rets.ToArray();
                }
            }

            if (r1 <= cr)
            {
                switch (mode)
                {
                    case IntersectionResultMode.LinePosition:
                        rets.Add((lsx, lsy));
                        break;
                    case IntersectionResultMode.Intersection:
                        rets.Add((x1, y1));
                        break;
                    default:
                        break;
                }
            }
            else
            {
                rets.Add((x1, y1));
            }

            if (r2 < cr)
            {
                switch (mode)
                {
                    case IntersectionResultMode.LinePosition:
                        rets.Add((lex, ley));
                        break;
                    case IntersectionResultMode.Intersection:
                        rets.Add((x2, y2));
                        break;
                    default:
                        break;
                }
            }
            else
            {
                rets.Add((x2, y2));
            }
            if (lsx < lex)
            {
                if (rets[0].X > rets[1].X)
                {
                    rets.Reverse();
                }
            }
            else
            {
                if (rets[0].X < rets[1].X)
                {
                    rets.Reverse();
                }
            }
            return rets.ToArray();
        }
    }
}
