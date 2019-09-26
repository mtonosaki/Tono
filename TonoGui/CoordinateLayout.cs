using System;
using System.Diagnostics;

namespace Tono.Gui
{
    /// <summary>
    /// Layout coodinate type
    /// </summary>
    /// <remarks>
    /// This GUI framework separates coodinates to Code, Layout and Screen.
    /// Layout is logical space converted from code.
    /// </remarks>
    public struct LayoutX
    {
        /// <summary>
        /// Layout X
        /// </summary>
        public double Lx { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is LayoutX pos)
            {
                return pos.Lx == Lx;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(Lx), 0);
        }

        public override string ToString()
        {
            return $"Lx={Lx}";
        }

        /// <summary>
        /// v1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutX operator +(LayoutX v1, LayoutX v2)
        {
            return new LayoutX { Lx = v1.Lx + v2.Lx };
        }

        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutX operator -(LayoutX v1, LayoutX v2)
        {
            return new LayoutX { Lx = v1.Lx - v2.Lx };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutX operator *(LayoutX v1, double v2)
        {
            return new LayoutX { Lx = v1.Lx * v2 };
        }

        /// <summary>
        /// v1 divided by v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutX operator /(LayoutX v1, double v2)
        {
            return new LayoutX { Lx = v1.Lx / v2 };
        }

        public static LayoutX From(double value)
        {
            return new LayoutX { Lx = value };
        }

        /// <summary>
        /// Convert layout value to Screen coodinate
        /// </summary>
        /// <param name="pane">target pane control</param>
        /// <param name="pos">screen position</param>
        /// <returns></returns>
        public static LayoutX From(IDrawArea pane, ScreenX pos)
        {
            var ppos = pos - pane.Rect.LT.X;
            return new LayoutX
            {
                Lx = ppos.Sx / pane.ZoomX - pane.ScrollX,
            };
        }
    }

    /// <summary>
    /// Layout coodinate type
    /// </summary>
    /// <remarks>
    /// This GUI framework separates coodinates to Code, Layout and Screen.
    /// Layout is logical space converted from code.
    /// </remarks>
    public struct LayoutY
    {
        /// <summary>
        /// Layout Y
        /// </summary>
        public double Ly { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is LayoutY pos)
            {
                return pos.Ly == Ly;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(Ly), 0);
        }

        public override string ToString()
        {
            return $"Ly={Ly}";
        }

        public static LayoutY From(double value)
        {
            return new LayoutY { Ly = value };
        }

        /// <summary>
        /// Convert layout value to Screen coodinate
        /// </summary>
        /// <param name="pane">target pane control</param>
        /// <param name="pos">screen position</param>
        /// <returns></returns>
        public static LayoutY From(IDrawArea pane, ScreenY pos)
        {
            var ppos = pos - pane.Rect.LT.Y;
            return new LayoutY
            {
                Ly = ppos.Sy / pane.ZoomY - pane.ScrollY,
            };
        }

        /// <summary>
        /// v1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutY operator +(LayoutY v1, LayoutY v2)
        {
            return new LayoutY { Ly = v1.Ly + v2.Ly };
        }

        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutY operator -(LayoutY v1, LayoutY v2)
        {
            return new LayoutY { Ly = v1.Ly - v2.Ly };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutY operator *(LayoutY v1, double v2)
        {
            return new LayoutY { Ly = v1.Ly * v2 };
        }

        /// <summary>
        /// v1 divided by v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutY operator /(LayoutY v1, double v2)
        {
            return new LayoutY { Ly = v1.Ly / v2 };
        }
    }

    /// <summary>
    /// Size object of Layout coodinate
    /// </summary>
    public struct LayoutSize
    {
        public LayoutX Width { get; set; }
        public LayoutY Height { get; set; }

        /// <summary>
        /// Create a new instance from double values
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static LayoutSize From(double width, double height)
        {
            return new LayoutSize
            {
                Width = new LayoutX { Lx = width },
                Height = new LayoutY { Ly = height },
            };
        }

        /// <summary>
        /// Create a new instance from layout X,Y values
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static LayoutSize From(LayoutX width, LayoutY height)
        {
            return new LayoutSize
            {
                Width = width,
                Height = height,
            };
        }

        public override string ToString()
        {
            return $"L(w,h)=({Width},{Height})";
        }

        public override bool Equals(object obj)
        {
            if (obj is LayoutSize size)
            {
                return Width.Equals(size.Width) && Height.Equals(size.Height);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var h1 = Width.GetHashCode();
            var h2 = Height.GetHashCode();
            return h1 ^ h2;
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutSize operator *(LayoutSize v1, double v2)
        {
            return new LayoutSize { Width = v1.Width * v2, Height = v1.Height * v2, };
        }

        /// <summary>
        /// cast operator from LayoutSize to LayoutPos
        /// </summary>
        /// <param name="size"></param>
        [DebuggerHidden]
        public static explicit operator LayoutPos(LayoutSize size)
        {
            return new LayoutPos { X = size.Width, Y = size.Height, };
        }
    }

    /// <summary>
    /// Position object of Layout coodinate
    /// </summary>
    public struct LayoutPos
    {
        public LayoutX X { get; set; }
        public LayoutY Y { get; set; }

        /// <summary>
        /// create a new instance from double values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static LayoutPos From(double x, double y)
        {
            return new LayoutPos
            {
                X = new LayoutX { Lx = x },
                Y = new LayoutY { Ly = y },
            };
        }

        /// <summary>
        /// create a new instance from Layout X,Y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static LayoutPos From(LayoutX x, LayoutY y)
        {
            return new LayoutPos
            {
                X = x,
                Y = y,
            };
        }

        /// <summary>
        /// Converet from screen to layout coodinate
        /// </summary>
        /// <param name="pane">target pane control</param>
        /// <param name="pos">screen position</param>
        /// <returns></returns>
        public static LayoutPos From(IDrawArea pane, ScreenPos pos)
        {
            return new LayoutPos
            {
                X = LayoutX.From(pane, pos.X),
                Y = LayoutY.From(pane, pos.Y),
            };
        }

        public override string ToString()
        {
            return $"L(x,y)=({X},{Y})";
        }

        public override bool Equals(object obj)
        {
            if (obj is LayoutPos pos)
            {
                return X.Equals(pos.X) && Y.Equals(pos.Y);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var h1 = X.GetHashCode();
            var h2 = Y.GetHashCode();
            return h1 ^ h2;
        }

        /// <summary>
        /// v1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutPos operator +(LayoutPos v1, LayoutPos v2)
        {
            return new LayoutPos { X = v1.X + v2.X, Y = v1.Y + v2.Y, };
        }

        /// <summary>
        /// v1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutPos operator +(LayoutPos v1, LayoutSize v2)
        {
            return new LayoutPos { X = v1.X + v2.Width, Y = v1.Y + v2.Height, };
        }

        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutSize operator -(LayoutPos v1, LayoutPos v2)
        {
            return new LayoutSize { Width = v1.X - v2.X, Height = v1.Y - v2.Y, };
        }

        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutPos operator -(LayoutPos v1, LayoutSize v2)
        {
            return new LayoutPos { X = v1.X - v2.Width, Y = v1.Y - v2.Height, };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static LayoutPos operator *(LayoutPos v1, double v2)
        {
            return new LayoutPos { X = v1.X * v2, Y = v1.Y * v2, };
        }
    }
}
