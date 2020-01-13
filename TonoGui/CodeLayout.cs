// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.
using System;

namespace Tono.Gui
{
    /// <summary>
    /// Coodinate convert utility
    /// </summary>
    public static class CodeLayout
    {
        private static readonly double R = 128.0 / Angle.PI;

        public static LayoutX PositionerSx(CodeX<ScreenX> cx, CodeY<ScreenY> cy)
        {
            return new LayoutX
            {
                Lx = cx.Cx,
            };
        }

        public static LayoutY PositionerSy(CodeX<ScreenX> cx, CodeY<ScreenY> cy)
        {
            return new LayoutY
            {
                Ly = cy.Cy,
            };
        }

        /// <summary>
        /// Longitude to Layout(0-256) positioner
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <returns></returns>
        public static LayoutX PositionerLon(CodeX<Longitude> lon, CodeY<Latitude> lat)
        {
            return new LayoutX
            {
                Lx = R * (lon.Cx.Lon.Rad + Angle.PI)
            };
        }

        /// <summary>
        /// Latitude to Layout(0-256) positioner
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <returns></returns>
        public static LayoutY PositionerLat(CodeX<Longitude> lon, CodeY<Latitude> lat)
        {
            return new LayoutY
            {
                Ly = -R / 2 * Math.Log((1 + Math.Sin(lat.Cy.Lat.Rad)) / (1 - Math.Sin(lat.Cy.Lat.Rad))) + 128
            };
        }

        /// <summary>
        /// Code as Screen coodinate
        /// </summary>
        /// <param name="lx"></param>
        /// <param name="ly"></param>
        /// <returns></returns>
        public static CodeX<ScreenX> CoderSx(LayoutX lx, LayoutY ly)
        {
            return new CodeX<ScreenX>
            {
                Cx = ScreenX.From(lx.Lx),
            };
        }

        /// <summary>
        /// Code as Screen coodinate
        /// </summary>
        /// <param name="lx"></param>
        /// <param name="ly"></param>
        /// <returns></returns>
        public static CodeY<ScreenY> CoderSy(LayoutX lx, LayoutY ly)
        {
            return new CodeY<ScreenY>
            {
                Cy = ScreenY.From(ly.Ly),
            };
        }


        /// <summary>
        /// layout(0-256) to longitude coder
        /// </summary>
        /// <param name="lx"></param>
        /// <param name="ly"></param>
        /// <returns></returns>
        public static CodeX<Longitude> CoderLon(LayoutX lx, LayoutY ly)
        {
            return new CodeX<Longitude>
            {
                Cx = Longitude.FromRad(lx.Lx / R - Angle.PI)
            };
        }

        /// <summary>
        /// layout(0-256) to longitude coder
        /// </summary>
        /// <param name="lx"></param>
        /// <param name="ly"></param>
        /// <returns></returns>
        public static CodeY<Latitude> CoderLat(LayoutX lx, LayoutY ly)
        {
            return new CodeY<Latitude>
            {
                Cy = Latitude.FromRad(Math.Atan(Math.Sinh((128 - ly.Ly) / R)))
            };
        }
    }
}
