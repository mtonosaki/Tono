using System;

namespace Tono.Gui
{
    /// <summary>
    /// GUI convert utility
    /// </summary>
    public class GuiExtentions
    {
        /// <summary>
        /// make angle from s0 to s1
        /// </summary>
        /// <param name="s0"></param>
        /// <param name="s1"></param>
        /// <returns></returns>
        //public static Angle From(ScreenPos s0, ScreenPos s1)
        //{
        //    return Angle(s0.X.Sx, s0.Y.Sy, s1.X.Sx, s1.Y.Sy);
        //}

        /// <summary>
        /// make angle from po to p1
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        //public static Angle Angle(CodePos<Longitude, Latitude> p0, CodePos<Longitude, Latitude> p1)
        //{
        //    return Angle(p0.X.Cx, p0.Y.Cy, p1.X.Cx, p1.Y.Cy);
        //}

        /// <summary>
        /// make distance from lon/lat of code coodinate
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        //public static Distance From(CodePos<Longitude, Latitude> p0, CodePos<Longitude, Latitude> p1)
        //{
        //    return Distance(p0.X.Cx, p0.Y.Cy, p1.X.Cx, p1.Y.Cy);
        //}

        /// <summary>
        /// measure length of diagonal
        /// </summary>
        /// <param name="xy"></param>
        /// <returns></returns>
        //public static double Length(ScreenSize xy)
        //{
        //    return Length(xy.Width, xy.Height);
        //}

        /// <summary>
        /// calculate distance from p0 to p
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double Length(ScreenPos p0, ScreenPos p)
        {
            return Math.Sqrt((p.X.Sx - p0.X.Sx) * (p.X.Sx - p0.X.Sx) + (p.Y.Sy - p0.Y.Sy) * (p.Y.Sy - p0.Y.Sy));
        }
    }
}
