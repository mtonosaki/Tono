using System;

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

        public static bool operator <(LayoutX v1, LayoutX v2)
        {
            return v1.Lx < v2.Lx;
        }

        public static bool operator >(LayoutX v1, LayoutX v2)
        {
            return v1.Lx > v2.Lx;
        }

        public static bool operator <=(LayoutX v1, LayoutX v2)
        {
            return v1.Lx <= v2.Lx;
        }

        public static bool operator >=(LayoutX v1, LayoutX v2)
        {
            return v1.Lx >= v2.Lx;
        }

        /// <summary>
        /// v1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

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

        public static LayoutY operator +(LayoutY v1, LayoutY v2)
        {
            return new LayoutY { Ly = v1.Ly + v2.Ly };
        }

        public static bool operator <(LayoutY v1, LayoutY v2)
        {
            return v1.Ly < v2.Ly;
        }

        public static bool operator >(LayoutY v1, LayoutY v2)
        {
            return v1.Ly > v2.Ly;
        }

        public static bool operator <=(LayoutY v1, LayoutY v2)
        {
            return v1.Ly <= v2.Ly;
        }

        public static bool operator >=(LayoutY v1, LayoutY v2)
        {
            return v1.Ly >= v2.Ly;
        }

        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

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

        public static LayoutSize operator *(LayoutSize v1, double v2)
        {
            return new LayoutSize { Width = v1.Width * v2, Height = v1.Height * v2, };
        }

        public static LayoutSize operator /(LayoutSize v1, double v2)
        {
            return new LayoutSize { Width = v1.Width / v2, Height = v1.Height / v2, };
        }

        /// <summary>
        /// cast operator from LayoutSize to LayoutPos
        /// </summary>
        /// <param name="size"></param>

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

        public static LayoutPos operator *(LayoutPos v1, double v2)
        {
            return new LayoutPos { X = v1.X * v2, Y = v1.Y * v2, };
        }
    }

    [Serializable]
    /// <summary>
    /// Rectangle object of screen coodinate
    /// </summary>
    public class LayoutRect
    {
        /// <summary>
        /// Left-Top
        /// </summary>
        public LayoutPos LT { get; set; }

        /// <summary>
        /// Right-Bottom
        /// </summary>
        public LayoutPos RB { get; set; }

        /// <summary>
        /// Left-Bottom (only refference)
        /// </summary>
        public LayoutPos LB => LayoutPos.From(LT.X, RB.Y);

        /// <summary>
        /// Left (only refference)
        /// </summary>
        public LayoutX L => LT.X;

        /// <summary>
        /// Right (only refference)
        /// </summary>
        public LayoutX R => RB.X;

        /// <summary>
        /// Top (only refference)
        /// </summary>
        public LayoutY T => LT.Y;

        /// <summary>
        /// Bottom (only refference)
        /// </summary>
        public LayoutY B => RB.Y;

        /// <summary>
        /// Right-Top (only refference)
        /// </summary>
        public LayoutPos RT => LayoutPos.From(RB.X, LT.Y);

        /// <summary>
        /// Empty insntance
        /// </summary>
        public static readonly LayoutRect Empty = LayoutRect.FromLTWH(0, 0, 0, 0);

        public LayoutRect Clone()
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = LT.X.Lx },
                    Y = new LayoutY { Ly = LT.Y.Ly },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = RB.X.Lx },
                    Y = new LayoutY { Ly = RB.Y.Ly },
                },
            };
        }

        /// <summary>
        /// normalize instance value from negative to positive rectangle
        /// </summary>
        /// <returns></returns>
        public LayoutRect Normalize()
        {
            var l = Math.Min(L.Lx, R.Lx);
            var t = Math.Min(T.Ly, B.Ly);
            var r = Math.Max(L.Lx, R.Lx);
            var b = Math.Max(T.Ly, B.Ly);
            LT = new LayoutPos
            {
                X = new LayoutX { Lx = l },
                Y = new LayoutY { Ly = t },
            };
            RB = new LayoutPos
            {
                X = new LayoutX { Lx = r },
                Y = new LayoutY { Ly = b },
            };
            return this;
        }

        /// <summary>
        /// Check equal to empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Equals(Empty);
        }

        /// <summary>
        /// check position is in this space
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool IsIn(LayoutPos pos)
        {
            return LT.X <= pos.X && RB.X >= pos.X && LT.Y <= pos.Y && RB.Y >= pos.Y;
        }

        /// <summary>
        /// Check negative or not.
        /// </summary>
        /// <returns></returns>
        public bool IsNegative()
        {
            return Width.Lx < 0 || Height.Ly < 0;
        }

        /// <summary>
        /// Check negative or empty
        /// </summary>
        /// <returns>true = negative or empty</returns>
        public bool IsEmptyNegative()
        {
            return IsEmpty() || IsNegative();
        }

        /// <summary>
        /// make a new instance from float
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>

        public static LayoutRect FromLTRB(double left, double top, double right, double bottom)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = left },
                    Y = new LayoutY { Ly = top },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = right },
                    Y = new LayoutY { Ly = bottom },
                },
            };
        }

        /// <summary>
        /// make a new instance from LT and RB
        /// </summary>
        /// <param name="lt">Left-Top</param>
        /// <param name="rb">Right-Bottom</param>
        /// <returns></returns>

        public static LayoutRect FromLTRB(LayoutPos lt, LayoutPos rb)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = lt.X.Lx },
                    Y = new LayoutY { Ly = lt.Y.Ly },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = rb.X.Lx },
                    Y = new LayoutY { Ly = rb.Y.Ly },
                },
            };
        }

        /// <summary>
        /// make a new instance from Left-Top, width and height
        /// </summary>
        /// <param name="left">left</param>
        /// <param name="top">top</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <returns></returns>

        public static LayoutRect FromLTWH(double left, double top, double width, double height)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = left },
                    Y = new LayoutY { Ly = top },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = left + width },
                    Y = new LayoutY { Ly = top + height },
                },
            };
        }

        /// <summary>
        /// make a new instance from Left-Top, width and height
        /// </summary>
        /// <param name="lt"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>

        public static LayoutRect FromLTWH(LayoutPos lt, double width, double height)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = lt.X.Lx },
                    Y = new LayoutY { Ly = lt.Y.Ly },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = lt.X.Lx + width },
                    Y = new LayoutY { Ly = lt.Y.Ly + height },
                },
            };
        }

        /// <summary>
        /// make a new instance from center(x,y), width and height
        /// </summary>
        /// <param name="center">center position</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static LayoutRect FromCWH(LayoutPos center, double width, double height)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = center.X.Lx - width / 2 },
                    Y = new LayoutY { Ly = center.Y.Ly - height / 2 },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = center.X.Lx + width / 2 },
                    Y = new LayoutY { Ly = center.Y.Ly + height / 2 },
                },
            };
        }

        /// <summary>
        /// make a new instance from center x, bottom y, width and height
        /// </summary>
        /// <param name="centerbottom"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static LayoutRect FromCbWH(LayoutPos centerbottom, double width, double height)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = centerbottom.X.Lx - width / 2 },
                    Y = new LayoutY { Ly = centerbottom.Y.Ly - height },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = centerbottom.X.Lx + width / 2 },
                    Y = new LayoutY { Ly = centerbottom.Y.Ly },
                },
            };
        }

        /// <summary>
        /// make a new instance from left, center y, width and height
        /// </summary>
        /// <param name="leftcenter"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>

        public static LayoutRect FromLWH(LayoutPos leftcenter, double width, double height)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = leftcenter.X.Lx },
                    Y = new LayoutY { Ly = leftcenter.Y.Ly - height / 2 },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = leftcenter.X.Lx + width },
                    Y = new LayoutY { Ly = leftcenter.Y.Ly + height / 2 },
                },
            };
        }

        /// <summary>
        /// make a new instance from center x0, y0 and size
        /// </summary>
        /// <param name="x0">center X (Layout)</param>
        /// <param name="y0">center Y (Layout)</param>
        /// <param name="size"></param>
        /// <returns></returns>

        public static LayoutRect From(double x0, double y0, LayoutSize size)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = x0 },
                    Y = new LayoutY { Ly = y0 },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = x0 + size.Width.Lx },
                    Y = new LayoutY { Ly = y0 + size.Height.Ly },
                },
            };
        }

        /// <summary>
        /// make a new instance from center and size
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <returns></returns>

        public static LayoutRect FromCS(LayoutPos center, LayoutSize size)
        {
            return FromCWH(center, size.Width.Lx, size.Height.Ly);
        }

        /// <summary>
        /// make a new instance from Left-Top and size
        /// </summary>
        /// <param name="lt"></param>
        /// <param name="size"></param>
        /// <returns></returns>

        public static LayoutRect From(LayoutPos lt, LayoutSize size)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = lt.X.Lx },
                    Y = new LayoutY { Ly = lt.Y.Ly },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = lt.X.Lx + size.Width.Lx },
                    Y = new LayoutY { Ly = lt.Y.Ly + size.Height.Ly },
                },
            };
        }

        /// <summary>
        /// make a new instance from Left-Top, width and height
        /// </summary>
        /// <param name="lt"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>

        public static LayoutRect From(LayoutPos lt, double width, double height)
        {
            return new LayoutRect
            {
                LT = new LayoutPos
                {
                    X = new LayoutX { Lx = lt.X.Lx },
                    Y = new LayoutY { Ly = lt.Y.Ly },
                },
                RB = new LayoutPos
                {
                    X = new LayoutX { Lx = lt.X.Lx + width },
                    Y = new LayoutY { Ly = lt.Y.Ly + height },
                },
            };
        }

        /// <summary>
        /// Width (reference only)
        /// </summary>
        public LayoutX Width => RB.X - LT.X;

        /// <summary>
        /// Height (reference only)
        /// </summary>
        public LayoutY Height => RB.Y - LT.Y;

        /// <summary>
        /// Center position (reference only)
        /// </summary>
        public LayoutPos C => new LayoutPos { X = (LT.X + RB.X) / 2, Y = (LT.Y + RB.Y) / 2 };

        /// <summary>
        /// Left-Center (reference only)
        /// </summary>
        public LayoutPos LC => new LayoutPos { X = LT.X, Y = (LT.Y + RB.Y) / 2 };

        /// <summary>
        /// Right-Center (reference only)
        /// </summary>
        public LayoutPos RC => new LayoutPos { X = RB.X, Y = (LT.Y + RB.Y) / 2 };

        /// <summary>
        /// Center-Top (reference only)
        /// </summary>
        public LayoutPos CT => new LayoutPos { X = (LT.X + RB.X) / 2, Y = LT.Y };

        /// <summary>
        /// Center-Bottom (reference only)
        /// </summary>
        public LayoutPos CB => new LayoutPos { X = (LT.X + RB.X) / 2, Y = RB.Y };

        /// <summary>
        /// infrate instance
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public void Inflate(LayoutSize size)
        {
            LT -= size / 2.0;
            RB += size / 2.0;
        }

        /// <summary>
        /// deflate instance
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public void Deflate(LayoutSize size)
        {
            LT += size / 2;
            RB -= size / 2;
        }


        public override bool Equals(object obj)
        {
            if (obj is LayoutRect sr)
            {
                return sr.LT.Equals(LT) && sr.RB.Equals(RB);
            }
            else
            {
                return false;
            }
        }


        public override int GetHashCode()
        {
            return LT.GetHashCode() ^ RB.GetHashCode();
        }

        public override string ToString()
        {
            return $"S({LT.X.Lx:0},{LT.Y.Ly:0})-({RB.X.Lx:0},{RB.Y.Ly:0}) W={Width.Lx:0}, H={Height.Ly:0}";
        }

        /// <summary>
        /// make a new size instance
        /// </summary>
        /// <returns></returns>
        public LayoutSize ToSize()
        {
            return LayoutSize.From(Width, Height);
        }

        /// <summary>
        /// offset horizontal position
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static LayoutRect operator +(LayoutRect v1, LayoutPos v2)
        {
            return new LayoutRect { LT = LayoutPos.From(v1.LT.X + v2.X, v1.LT.Y + v2.Y), RB = LayoutPos.From(v1.R + v2.X, v1.RB.Y + v2.Y), };
        }

        /// <summary>
        /// offset horizontal position
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static LayoutRect operator +(LayoutRect v1, LayoutX v2)
        {
            return new LayoutRect { LT = LayoutPos.From(v1.LT.X + v2, v1.LT.Y), RB = LayoutPos.From(v1.R + v2, v1.RB.Y), };
        }

        /// <summary>
        /// offset vertical position
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static LayoutRect operator +(LayoutRect v1, LayoutY v2)
        {
            return new LayoutRect { LT = LayoutPos.From(v1.LT.X, v1.LT.Y + v2), RB = LayoutPos.From(v1.R, v1.RB.Y + v2), };
        }

        /// <summary>
        /// offset vertical position 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static LayoutRect operator -(LayoutRect v1, LayoutY v2)
        {
            return new LayoutRect { LT = LayoutPos.From(v1.LT.X, v1.LT.Y - v2), RB = LayoutPos.From(v1.R, v1.RB.Y - v2), };
        }
    }
}
