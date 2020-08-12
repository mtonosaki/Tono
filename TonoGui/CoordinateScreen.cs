// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;

namespace Tono.Gui
{
    /// <summary>
    /// Screen coodinate type
    /// </summary>
    /// <remarks>
    /// This GUI framework separates coodinates to Code, Layout and Screen.
    /// Screen is display space such as mouse pointer.
    /// </remarks>
    public struct ScreenX : IComparable<ScreenX>
    {
        public float Sx { get; set; }

        public ScreenX Clone()
        {
            return new ScreenX
            {
                Sx = Sx,
            };
        }

        /// <summary>
        /// Auto cast to float
        /// </summary>
        /// <param name="val"></param>
        public static implicit operator float(ScreenX val)
        {
            return val.Sx;
        }

        public static explicit operator int(ScreenX val)
        {
            return (int)val.Sx;
        }

        public static explicit operator short(ScreenX val)
        {
            return (short)val.Sx;
        }

        public override bool Equals(object obj)
        {
            if (obj is ScreenX)
            {
                return ((ScreenX)obj).Sx == Sx;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(Sx), 0);
        }

        public override string ToString()
        {
            return $"Sx={Sx}";
        }

        /// <summary>
        /// cast from layout to screen
        /// </summary>
        /// <param name="pos"></param>

        public static explicit operator ScreenX(LayoutX pos) { return ScreenX.From(pos.Lx); }

        public static bool operator <(ScreenX v1, ScreenX v2)
        {
            return v1.Sx < v2.Sx;
        }

        public static bool operator >(ScreenX v1, ScreenX v2)
        {
            return v1.Sx > v2.Sx;
        }

        public static bool operator <=(ScreenX v1, ScreenX v2)
        {
            return v1.Sx <= v2.Sx;
        }

        public static bool operator >=(ScreenX v1, ScreenX v2)
        {
            return v1.Sx >= v2.Sx;
        }

        /// <summary>
        /// v1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenX operator +(ScreenX v1, ScreenX v2)
        {
            return new ScreenX { Sx = v1.Sx + v2.Sx };
        }

        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenX operator -(ScreenX v1, ScreenX v2)
        {
            return new ScreenX { Sx = v1.Sx - v2.Sx };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenX operator *(ScreenX v1, float v2)
        {
            return new ScreenX { Sx = v1.Sx * v2 };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenX operator *(ScreenX v1, double v2)
        {
            return new ScreenX { Sx = (float)(v1.Sx * v2) };
        }

        /// <summary>
        /// v1 divided by v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenX operator /(ScreenX v1, float v2)
        {
            return new ScreenX { Sx = v1.Sx / v2 };
        }

        /// <summary>
        /// v1 divided by v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenX operator /(ScreenX v1, double v2)
        {
            return new ScreenX { Sx = (float)(v1.Sx / v2) };
        }

        /// <summary>
        /// make a new instance from float value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenX From(float value)
        {
            return new ScreenX { Sx = value, };
        }

        /// <summary>
        /// make a new instance from double value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenX From(double value)
        {
            return new ScreenX { Sx = (float)value, };
        }

        /// <summary>
        /// make a new instance from layout X in specific pane control
        /// </summary>
        /// <param name="pane">target pane</param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static ScreenX From(IDrawArea pane, LayoutX val)
        {
            return new ScreenX
            {
                Sx = (float)((val.Lx + pane.ScrollX) * pane.ZoomX + pane.Rect.LT.X.Sx),
            };
        }

        /// <summary>
        /// return smaller value in this or val
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public ScreenX Min(ScreenX val)
        {
            if (this > val)
            {
                return val;
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// return bigger value in this or val
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public ScreenX Max(ScreenX val)
        {
            if (this < val)
            {
                return val;
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// comparison
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ScreenX other)
        {
            return Compare.Normal(Sx, other.Sx);
        }

        public static bool operator ==(ScreenX left, ScreenX right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ScreenX left, ScreenX right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Screen coodinate type
    /// </summary>
    /// <remarks>
    /// This GUI framework separates coodinates to Code, Layout and Screen.
    /// Screen is display space such as mouse pointer.
    /// </remarks>
    public struct ScreenY
    {
        public float Sy { get; set; }

        public ScreenY Clone()
        {
            return new ScreenY
            {
                Sy = Sy,
            };
        }

        /// <summary>
        /// auto cast to float type
        /// </summary>
        /// <param name="val"></param>
        public static implicit operator float(ScreenY val) { return val.Sy; }

        public static explicit operator int(ScreenY val)
        {
            return (int)val.Sy;
        }

        public static explicit operator short(ScreenY val)
        {
            return (short)val.Sy;
        }

        public static bool operator ==(ScreenY left, ScreenY right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ScreenY left, ScreenY right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is ScreenY)
            {
                return ((ScreenY)obj).Sy == Sy;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(Sy), 0);
        }

        public override string ToString()
        {
            return $"Sy={Sy}";
        }

        /// <summary>
        /// cast support to LayoutY
        /// </summary>
        /// <param name="pos"></param>

        public static explicit operator ScreenY(LayoutY pos)
        {
            return ScreenY.From(pos.Ly);
        }

        /// <summary>
        /// v1 is less than v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static bool operator <(ScreenY v1, ScreenY v2)
        {
            return v1.Sy < v2.Sy;
        }

        /// <summary>
        /// v1 is greater than v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static bool operator >(ScreenY v1, ScreenY v2)
        {
            return v1.Sy > v2.Sy;
        }

        /// <summary>
        /// v1 + v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenY operator +(ScreenY v1, ScreenY v2)
        {
            return new ScreenY { Sy = v1.Sy + v2.Sy };
        }

        /// <summary>
        /// v1 - v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenY operator -(ScreenY v1, ScreenY v2)
        {
            return new ScreenY { Sy = v1.Sy - v2.Sy };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenY operator *(ScreenY v1, float v2)
        {
            return new ScreenY { Sy = v1.Sy * v2 };
        }

        /// <summary>
        /// v1 * v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenY operator *(ScreenY v1, double v2)
        {
            return new ScreenY { Sy = (float)(v1.Sy * v2) };
        }

        /// <summary>
        /// v1 is divided by v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenY operator /(ScreenY v1, float v2)
        {
            return new ScreenY { Sy = v1.Sy / v2 };
        }

        /// <summary>
        /// v1 is divided by v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenY operator /(ScreenY v1, double v2)
        {
            return new ScreenY { Sy = (float)(v1.Sy / v2) };
        }

        /// <summary>
        /// make a new instance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenY From(ScreenX value)
        {
            return new ScreenY { Sy = value.Sx, };
        }

        /// <summary>
        /// make a new instance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenY From(float value)
        {
            return new ScreenY { Sy = value, };
        }

        /// <summary>
        /// make a new instance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenY From(double value)
        {
            return new ScreenY { Sy = (float)value, };
        }

        /// <summary>
        /// make a new instance
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static ScreenY From(IDrawArea pane, LayoutY val)
        {
            return new ScreenY
            {
                Sy = (float)((val.Ly + pane.ScrollY) * pane.ZoomY + pane.Rect.LT.Y.Sy),
            };
        }

    }

    /// <summary>
    /// Position object of screen coodinate
    /// </summary>
    public struct ScreenPos
    {
        /// <summary>
        /// Zero value instance
        /// </summary>
        public static readonly ScreenPos Zero = ScreenPos.From(0, 0);

        public ScreenX X { get; set; }
        public ScreenY Y { get; set; }

        public void SetX(ScreenX x) { X = x; }
        public void SetX(double x) { X = ScreenX.From(x); }
        public void SetY(ScreenY y) { Y = y; }
        public void SetY(double y) { Y = ScreenY.From(y); }

        /// <summary>
        /// instance type change to ScreenSize
        /// </summary>
        /// <returns></returns>
        public ScreenSize ToSize()
        {
            return new ScreenSize
            {
                Width = X,
                Height = Y,
            };
        }

        /// <summary>
        /// calculate distance from p0 to p
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public double LengthTo(ScreenPos p)
        {
            return Math.Sqrt((p.X.Sx - X.Sx) * (p.X.Sx - X.Sx) + (p.Y.Sy - Y.Sy) * (p.Y.Sy - Y.Sy));
        }

        /// <summary>
        /// IsOnline for Screen coodinate
        /// </summary>
        /// <param name="line0"></param>
        /// <param name="line1"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public bool IsOnline(ScreenPos line0, ScreenPos line1, double width = 1.0)
        {
            return GeoEu.IsOnline(line0.X.Sx, line0.Y.Sy, line1.X.Sx, line1.Y.Sy, X.Sx, Y.Sy, width);
        }

        /// <summary>
        /// auto cast to Vestor2 for a lot of Graphics functions
        /// </summary>
        /// <param name="pos"></param>
        public static implicit operator System.Numerics.Vector2(ScreenPos pos)
        {
            return new System.Numerics.Vector2
            {
                X = pos.X,
                Y = pos.Y,
            };
        }

        /// <summary>
        /// make angle from s0 to s1
        /// </summary>
        /// <param name="s0"></param>
        /// <param name="s1"></param>
        /// <returns></returns>
        public Angle AngleTo(ScreenPos s1)
        {
            return GeoEu.Angle(X.Sx, Y.Sy, s1.X.Sx, s1.Y.Sy);
        }

        /// <summary>
        /// check instance position is in betweeb tar
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public bool IsIn((ScreenX L, ScreenX R) tar)
        {
            return X >= tar.L && X < tar.R;
        }

        /// <summary>
        /// check instance position is in betweeb tar
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public bool IsIn((ScreenY L, ScreenY R) tar)
        {
            return Y >= tar.L && Y < tar.R;
        }

        /// <summary>
        /// make ScreenPos collection from tuple collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ScreenPos[] ToCollection((double X, double Y)[] collection)
        {
            return collection.Select(a => new ScreenPos { X = ScreenX.From(a.X), Y = ScreenY.From(a.Y) }).ToArray();
        }

        /// <summary>
        /// make new instancde from float x,y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static ScreenPos From(float x, float y)
        {
            return new ScreenPos
            {
                X = new ScreenX { Sx = x },
                Y = new ScreenY { Sy = y },
            };
        }

        /// <summary>
        /// make a new instance from ScreenX and Y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static ScreenPos From(ScreenX x, ScreenY y)
        {
            return new ScreenPos
            {
                X = x,
                Y = y,
            };
        }


        /// <summary>
        /// convert from code coodinate
        /// </summary>
        /// <param name="cpos"></param>
        /// <returns></returns>
        public static ScreenPos From(CodePos<ScreenX, ScreenY> cpos)
        {
            return new ScreenPos
            {
                X = cpos.X.Cx,
                Y = cpos.Y.Cy,
            };
        }

        /// <summary>
        /// make a new instance from double x,y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static ScreenPos From(double x, double y)
        {
            return new ScreenPos
            {
                X = new ScreenX { Sx = (float)x },
                Y = new ScreenY { Sy = (float)y },
            };
        }

        /// <summary>
        /// make a new instance from layout coodinate of the target pane control
        /// </summary>
        /// <param name="pane">target pane control</param>
        /// <param name="pos">position</param>
        /// <returns></returns>
        public static ScreenPos From(IDrawArea pane, LayoutPos pos)
        {
            Debug.Assert(pane != null);
            return new ScreenPos
            {
                X = ScreenX.From(pane, pos.X),
                Y = ScreenY.From(pane, pos.Y),
            };
        }

        /// <summary>
        /// Check Zero value
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return Equals(Zero);
        }

        /// <summary>
        /// change instance value from x, y to select smaller one
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TrimMinimum(ScreenX x, ScreenY y)
        {
            X = ScreenX.From(Math.Max(x.Sx, X.Sx));
            Y = ScreenY.From(Math.Max(y.Sy, Y.Sy));
        }

        /// <summary>
        /// change instance value from x, y to select smaller one
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TrimMaximum(ScreenX x, ScreenY y)
        {
            X = ScreenX.From(Math.Min(x.Sx, X.Sx));
            Y = ScreenY.From(Math.Min(y.Sy, Y.Sy));
        }

        /// <summary>
        public override string ToString()
        {
            return $"S({X:0},{Y:0})";
        }

        public override bool Equals(object obj)
        {
            if (obj is ScreenPos sp)
            {
                return X.Equals(sp.X) && Y.Equals(sp.Y);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int h1 = X.GetHashCode();
            int h2 = Y.GetHashCode();
            return h1 ^ h2;
        }

        /// <summary>
        /// convert instance to code coodinate
        /// </summary>
        /// <returns></returns>
        public CodePos<ScreenX, ScreenY> ToCodePos()
        {
            return CodePos<ScreenX, ScreenY>.From(X, Y);
        }

        public ScreenPos Clone()
        {
            return new ScreenPos
            {
                X = ScreenX.From(X.Sx),
                Y = ScreenY.From(Y.Sy),
            };
        }

        public static ScreenPos operator +(ScreenPos v1, ScreenPos v2)
        {
            return new ScreenPos { X = v1.X + v2.X, Y = v1.Y + v2.Y, };
        }

        public static ScreenPos operator +(ScreenPos v1, ScreenSize v2)
        {
            return new ScreenPos { X = v1.X + v2.Width, Y = v1.Y + v2.Height, };
        }

        public static ScreenPos operator +(ScreenPos v1, ScreenX v2)
        {
            return new ScreenPos { X = v1.X + v2, Y = v1.Y, };
        }

        public static ScreenPos operator +(ScreenPos v1, ScreenY v2)
        {
            return new ScreenPos { X = v1.X, Y = v1.Y + v2, };
        }

        public static ScreenSize operator -(ScreenPos v1, ScreenPos v2)
        {
            return new ScreenSize { Width = v1.X - v2.X, Height = v1.Y - v2.Y };
        }

        public static ScreenPos operator -(ScreenPos v1, ScreenSize v2)
        {
            return new ScreenPos { X = v1.X - v2.Width, Y = v1.Y - v2.Height, };
        }

        public static ScreenPos operator -(ScreenPos v1, ScreenX v2)
        {
            return new ScreenPos { X = v1.X - v2, Y = v1.Y, };
        }

        public static ScreenPos operator -(ScreenPos v1, ScreenY v2)
        {
            return new ScreenPos { X = v1.X, Y = v1.Y - v2, };
        }

        public static ScreenPos operator *(ScreenPos v1, double v2)
        {
            return new ScreenPos { X = v1.X * v2, Y = v1.Y * v2, };
        }

        public static ScreenPos operator *(ScreenPos v1, ScreenY v2)
        {
            return new ScreenPos { X = v1.X, Y = v1.Y * v2.Sy, };
        }

        public static ScreenPos operator *(ScreenPos v1, ScreenX v2)
        {
            return new ScreenPos { X = v1.X * v2.Sx, Y = v1.Y, };
        }

        public static ScreenPos operator *(ScreenPos v1, ScreenPos v2)
        {
            return new ScreenPos { X = v1.X * v2.X, Y = v1.Y * v2.Y, };
        }

        public static ScreenPos operator /(ScreenPos v1, ScreenPos v2)
        {
            return new ScreenPos { X = v1.X / v2.X, Y = v1.Y / v2.Y, };
        }

        public static ScreenPos operator /(ScreenPos v1, double v2)
        {
            return new ScreenPos { X = v1.X / v2, Y = v1.Y / v2, };
        }

        /// <summary>
        /// auto cast instance to tuple of double type
        /// </summary>
        /// <param name="v1"></param>

        public static implicit operator (double X, double Y)(ScreenPos v1)
        {
            return (v1.X.Sx, v1.Y.Sy);
        }

        /// <summary>
        /// select corner or center position of rectangle to destination position
        /// </summary>
        /// <param name="rect">target rectangle</param>
        /// <param name="dest">target position</param>
        /// <returns></returns>
        public static ScreenPos SelectEdgePosition(ScreenRect rect, ScreenPos dest)
        {
            if (dest.X < rect.LT.X)
            {
                if (dest.Y < rect.LT.Y)
                {
                    return rect.LT;
                }

                if (dest.Y > rect.RB.Y)
                {
                    return rect.LB;
                }

                return rect.LC;
            }
            if (dest.X > rect.R)
            {
                if (dest.Y < rect.LT.Y)
                {
                    return rect.RT;
                }

                if (dest.Y > rect.RB.Y)
                {
                    return rect.RB;
                }

                return rect.RC;
            }
            if (dest.Y < rect.LT.Y)
            {
                return rect.CT;
            }

            if (dest.Y > rect.RB.Y)
            {
                return rect.CB;
            }

            return dest;
        }

        /// <summary>
        /// left equal to right
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ScreenPos left, ScreenPos right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// left not equal to right
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ScreenPos left, ScreenPos right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Size object of screen coodinate system
    /// </summary>
    public struct ScreenSize
    {
        public ScreenX Width { get; set; }
        public ScreenY Height { get; set; }

        /// <summary>
        /// make clone instance
        /// </summary>
        /// <returns></returns>
        public ScreenSize Clone()
        {
            return new ScreenSize
            {
                Width = ScreenX.From(Width.Sx),
                Height = ScreenY.From(Height.Sy),
            };
        }

        /// <summary>
        /// make a new instance from layout coodinate of the target pane control
        /// </summary>
        /// <param name="pane">target pane control</param>
        /// <param name="pos">position</param>
        /// <returns></returns>
        public static ScreenSize From(IDrawArea pane, LayoutSize size)
        {
            return new ScreenSize
            {
                Width = ScreenX.From(size.Width.Lx * pane.ZoomX),
                Height = ScreenY.From(size.Height.Ly * pane.ZoomY),
            };
        }

        /// <summary>
        /// diagonal length of Width-Height
        /// </summary>
        public double Length => GeoEu.Length((Width.Sx, Height.Sy));

        public ScreenPos ToPos()
        {
            return new ScreenPos
            {
                X = Width,
                Y = Height,
            };
        }

        /// <summary>
        /// Convert to touple
        /// </summary>
        /// <returns></returns>
        public (double X, double Y) ToDoubles()
        {
            return (Width.Sx, Height.Sy);
        }

        /// <summary>
        /// make a new instance from float
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>

        public static ScreenSize From(float w, float h)
        {
            return new ScreenSize
            {
                Width = new ScreenX { Sx = w },
                Height = new ScreenY { Sy = h },
            };
        }

        /// <summary>
        /// make a new instance from doubles (NOTE: digit loss because screen coodinate is using float values)
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>

        public static ScreenSize From(double w, double h)
        {
            return new ScreenSize
            {
                Width = new ScreenX { Sx = (float)w },
                Height = new ScreenY { Sy = (float)h },
            };
        }


        public override string ToString()
        {
            return $"S(W={Width.Sx},H={Height.Sy})";
        }

        public override bool Equals(object obj)
        {
            if (obj is ScreenSize tar)
            {
                return Width.Equals(tar.Width) && Height.Equals(tar.Height);
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

        public static ScreenSize operator +(ScreenSize v1, ScreenSize v2)
        {
            return new ScreenSize { Width = v1.Width + v2.Width, Height = v1.Height + v2.Height };
        }

        public static ScreenSize operator +(ScreenSize v1, ScreenX v2)
        {
            return new ScreenSize { Width = v1.Width + v2, Height = v1.Height };
        }

        public static ScreenSize operator +(ScreenSize v1, ScreenY v2)
        {
            return new ScreenSize { Width = v1.Width, Height = v1.Height + v2 };
        }

        public static ScreenSize operator -(ScreenSize v1, ScreenSize v2)
        {
            return new ScreenSize { Width = v1.Width - v2.Width, Height = v1.Height - v2.Height };
        }

        public static ScreenSize operator -(ScreenSize v1, ScreenX v2)
        {
            return new ScreenSize { Width = v1.Width - v2, Height = v1.Height };
        }

        public static ScreenSize operator -(ScreenSize v1, ScreenY v2)
        {
            return new ScreenSize { Width = v1.Width, Height = v1.Height - v2 };
        }

        public static ScreenSize operator *(ScreenSize v1, float v2)
        {
            return new ScreenSize { Width = v1.Width * v2, Height = v1.Height * v2 };
        }

        public static ScreenSize operator *(ScreenSize v1, double v2)
        {
            return new ScreenSize { Width = v1.Width * v2, Height = v1.Height * v2 };
        }

        public static ScreenSize operator *(ScreenSize v1, (double X, double Y) v2)
        {
            return new ScreenSize { Width = v1.Width * v2.X, Height = v1.Height * v2.Y };
        }

        public static ScreenSize operator /(ScreenSize v1, float v2)
        {
            return new ScreenSize { Width = v1.Width / v2, Height = v1.Height / v2 };
        }

        public static ScreenSize operator /(ScreenSize v1, double v2)
        {
            return new ScreenSize { Width = v1.Width / v2, Height = v1.Height / v2 };
        }

        public static ScreenSize operator /(ScreenSize v1, ScreenPos v2)
        {
            return new ScreenSize { Width = v1.Width / v2.X, Height = v1.Height / v2.Y };
        }

        /// <summary>
        /// cast support to ScreenPos
        /// </summary>
        /// <param name="size"></param>

        public static explicit operator ScreenPos(ScreenSize size)
        {
            return ScreenPos.From(size.Width.Sx, size.Height.Sy);
        }

        /// <summary>
        /// left is equal to right
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ScreenSize left, ScreenSize right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// left is not equal to right
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ScreenSize left, ScreenSize right)
        {
            return !(left == right);
        }
    }


    /// <summary>
    /// Rectangle object of screen coodinate
    /// </summary>
    public class ScreenRect
    {
        /// <summary>
        /// Left-Top
        /// </summary>
        public ScreenPos LT { get; set; }

        /// <summary>
        /// Right-Bottom
        /// </summary>
        public ScreenPos RB { get; set; }

        /// <summary>
        /// make clone instance
        /// </summary>
        /// <returns></returns>
        public ScreenRect Clone()
        {
            return new ScreenRect
            {
                LT = LT.Clone(),
                RB = RB.Clone(),
            };
        }

        /// <summary>
        /// Left-Bottom (only refference)
        /// </summary>
        public ScreenPos LB => ScreenPos.From(LT.X, RB.Y);

        /// <summary>
        /// horizontal span
        /// </summary>
        public (ScreenX L, ScreenX R) LR => (L, R);

        /// <summary>
        /// vertical span
        /// </summary>
        public (ScreenY T, ScreenY B) TB => (T, B);

        /// <summary>
        /// Left (only refference)
        /// </summary>
        public ScreenX L => LT.X;

        /// <summary>
        /// Right (only refference)
        /// </summary>
        public ScreenX R => RB.X;

        /// <summary>
        /// Top (only refference)
        /// </summary>
        public ScreenY T => LT.Y;

        /// <summary>
        /// Bottom (only refference)
        /// </summary>
        public ScreenY B => RB.Y;

        /// <summary>
        /// Right-Top (only refference)
        /// </summary>
        public ScreenPos RT => ScreenPos.From(RB.X, LT.Y);

        /// <summary>
        /// Empty insntance
        /// </summary>
        public static readonly ScreenRect Empty = ScreenRect.FromLTWH(0, 0, 0, 0);

        /// <summary>
        /// increment RB x and y
        /// </summary>
        /// <returns></returns>
        public ScreenRect GetRBPlus1()
        {
            return new ScreenRect
            {
                LT = LT,
                RB = ScreenPos.From(R.Sx + 1, B.Sy + 1),
            };
        }

        /// <summary>
        /// normalize instance value from negative to positive rectangle
        /// </summary>
        /// <returns></returns>
        public ScreenRect Normalize()
        {
            float l = Math.Min(L.Sx, R.Sx);
            float t = Math.Min(T.Sy, B.Sy);
            float r = Math.Max(L.Sx, R.Sx);
            float b = Math.Max(T.Sy, B.Sy);
            LT = new ScreenPos
            {
                X = new ScreenX { Sx = l },
                Y = new ScreenY { Sy = t },
            };
            RB = new ScreenPos
            {
                X = new ScreenX { Sx = r },
                Y = new ScreenY { Sy = b },
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
        public bool IsIn(ScreenPos pos)
        {
            return LT.X <= pos.X && RB.X >= pos.X && LT.Y <= pos.Y && RB.Y >= pos.Y;
        }


        /// <summary>
        /// check anogher rectangle is in this rectangle
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public bool IsIn(ScreenRect tar)
        {
            if (tar != null)
            {
                if (LT.X <= tar.RB.X && RB.X >= tar.LT.X && LT.Y <= tar.RB.Y && RB.Y >= tar.LT.Y)
                {
                    return true;
                }
                if (LT.X >= tar.LT.X && RB.X <= tar.RB.X && LT.Y >= tar.LT.Y && RB.Y <= tar.RB.Y)
                {
                    return true;
                }
                if (tar.LT.X >= LT.X && tar.RB.X <= RB.X && tar.LT.Y >= LT.Y && tar.RB.Y <= RB.Y)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check negative or not.
        /// </summary>
        /// <returns></returns>
        public bool IsNegative()
        {
            return Width.Sx < 0 || Height.Sy < 0;
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

        public static ScreenRect FromLTRB(float left, float top, float right, float bottom)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = left },
                    Y = new ScreenY { Sy = top },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = right },
                    Y = new ScreenY { Sy = bottom },
                },
            };
        }

        /// <summary>
        /// make a new instance from LT and RB
        /// </summary>
        /// <param name="lt">Left-Top</param>
        /// <param name="rb">Right-Bottom</param>
        /// <returns></returns>

        public static ScreenRect FromLTRB(ScreenPos lt, ScreenPos rb)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = lt.X },
                    Y = new ScreenY { Sy = lt.Y },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = rb.X },
                    Y = new ScreenY { Sy = rb.Y },
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

        public static ScreenRect FromLTWH(float left, float top, float width, float height)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = left },
                    Y = new ScreenY { Sy = top },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = left + width },
                    Y = new ScreenY { Sy = top + height },
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

        public static ScreenRect FromLTWH(ScreenPos lt, float width, float height)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = lt.X },
                    Y = new ScreenY { Sy = lt.Y },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = lt.X + width },
                    Y = new ScreenY { Sy = lt.Y + height },
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
        public static ScreenRect FromCWH(ScreenPos center, float width, float height)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = center.X - width / 2 },
                    Y = new ScreenY { Sy = center.Y - height / 2 },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = center.X + width / 2 },
                    Y = new ScreenY { Sy = center.Y + height / 2 },
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
        public static ScreenRect FromCbWH(ScreenPos centerbottom, float width, float height)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = centerbottom.X - width / 2 },
                    Y = new ScreenY { Sy = centerbottom.Y - height },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = centerbottom.X + width / 2 },
                    Y = new ScreenY { Sy = centerbottom.Y },
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

        public static ScreenRect FromLWH(ScreenPos leftcenter, float width, float height)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = leftcenter.X },
                    Y = new ScreenY { Sy = leftcenter.Y - height / 2 },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = leftcenter.X + width },
                    Y = new ScreenY { Sy = leftcenter.Y + height / 2 },
                },
            };
        }

        /// <summary>
        /// make a new instance from center x0, y0 and size
        /// </summary>
        /// <param name="x0">center X (screen)</param>
        /// <param name="y0">center Y (screen)</param>
        /// <param name="size"></param>
        /// <returns></returns>

        public static ScreenRect From(float x0, float y0, ScreenSize size)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = x0 },
                    Y = new ScreenY { Sy = y0 },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = x0 + size.Width.Sx },
                    Y = new ScreenY { Sy = y0 + size.Height.Sy },
                },
            };
        }

        /// <summary>
        /// make a new instance from center and size
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <returns></returns>

        public static ScreenRect FromCS(ScreenPos center, ScreenSize size)
        {
            return FromCWH(center, size.Width, size.Height);
        }

        /// <summary>
        /// make a new instance from Left-Top and size
        /// </summary>
        /// <param name="lt"></param>
        /// <param name="size"></param>
        /// <returns></returns>

        public static ScreenRect From(ScreenPos lt, ScreenSize size)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = lt.X.Sx },
                    Y = new ScreenY { Sy = lt.Y.Sy },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = lt.X.Sx + size.Width.Sx },
                    Y = new ScreenY { Sy = lt.Y.Sy + size.Height.Sy },
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

        public static ScreenRect From(ScreenPos lt, float width, float height)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = lt.X.Sx },
                    Y = new ScreenY { Sy = lt.Y.Sy },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = lt.X.Sx + width },
                    Y = new ScreenY { Sy = lt.Y.Sy + height },
                },
            };
        }

        /// <summary>
        /// Width (reference only)
        /// </summary>
        public ScreenX Width => RB.X - LT.X;

        /// <summary>
        /// Height (reference only)
        /// </summary>
        public ScreenY Height => RB.Y - LT.Y;

        /// <summary>
        /// Center position (reference only)
        /// </summary>
        public ScreenPos C => new ScreenPos { X = (LT.X + RB.X) / 2, Y = (LT.Y + RB.Y) / 2 };

        /// <summary>
        /// Left-Center (reference only)
        /// </summary>
        public ScreenPos LC => new ScreenPos { X = LT.X, Y = (LT.Y + RB.Y) / 2 };

        /// <summary>
        /// Right-Center (reference only)
        /// </summary>
        public ScreenPos RC => new ScreenPos { X = RB.X, Y = (LT.Y + RB.Y) / 2 };

        /// <summary>
        /// Center-Top (reference only)
        /// </summary>
        public ScreenPos CT => new ScreenPos { X = (LT.X + RB.X) / 2, Y = LT.Y };

        /// <summary>
        /// Center-Bottom (reference only)
        /// </summary>
        public ScreenPos CB => new ScreenPos { X = (LT.X + RB.X) / 2, Y = RB.Y };

        /// <summary>
        /// infrate instance
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public ScreenRect Inflate(ScreenSize size)
        {
            LT -= size / 2;
            RB += size / 2;
            return this;
        }

        /// <summary>
        /// deflate instance
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public ScreenRect Deflate(ScreenSize size)
        {
            LT += size / 2;
            RB -= size / 2;
            return this;
        }


        public override bool Equals(object obj)
        {
            if (obj is ScreenRect sr)
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
            return $"S({LT.X.Sx:0},{LT.Y.Sy:0})-({RB.X.Sx:0},{RB.Y.Sy:0}) W={Width.Sx:0}, H={Height.Sy:0}";
        }

        /// <summary>
        /// make a new size instance
        /// </summary>
        /// <returns></returns>
        public ScreenSize ToSize()
        {
            return ScreenSize.From(Width, Height);
        }

        /// <summary>
        /// make overlapped arrea rectangle
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static ScreenRect operator &(ScreenRect r1, ScreenRect r2)
        {
            if (r1 == null || r2 == null)
            {
                return null;
            }
            var ret = ScreenRect.FromLTRB(
                r1.LT.X >= r2.LT.X ? r1.LT.X : r2.LT.X,
                r1.LT.Y >= r2.LT.Y ? r1.LT.Y : r2.LT.Y,
                r1.RB.X <= r2.RB.X ? r1.RB.X : r2.RB.X,
                r1.RB.Y <= r2.RB.Y ? r1.RB.Y : r2.RB.Y
            );
            if (ret.IsEmptyNegative() == false)
            {
                return null;
            }
            else
            {
                return ret;
            }
        }

        /// <summary>
        /// r * value (= LT * value, RB * value)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenRect operator *(ScreenRect r, double value)
        {
            return new ScreenRect
            {
                LT = ScreenPos.From(r.L.Sx * value, r.T.Sy * value),
                RB = ScreenPos.From(r.R.Sx * value, r.B.Sy * value),
            };
        }
        public static ScreenRect operator *(ScreenRect r, ScreenX value)
        {
            return new ScreenRect
            {
                LT = ScreenPos.From(r.L.Sx * value.Sx, r.T.Sy),
                RB = ScreenPos.From(r.R.Sx * value.Sx, r.B.Sy),
            };
        }
        public static ScreenRect operator *(ScreenRect r, ScreenY value)
        {
            return new ScreenRect
            {
                LT = ScreenPos.From(r.L.Sx, r.T.Sy * value.Sy),
                RB = ScreenPos.From(r.R.Sx, r.B.Sy * value.Sy),
            };
        }

        public static ScreenRect operator *(ScreenRect r, ScreenPos value)
        {
            return new ScreenRect
            {
                LT = ScreenPos.From(r.L.Sx * value.X.Sx, r.T.Sy * value.Y.Sy),
                RB = ScreenPos.From(r.R.Sx * value.X.Sx, r.B.Sy * value.Y.Sy),
            };
        }

        public static ScreenRect operator /(ScreenRect r, double value)
        {
            return new ScreenRect
            {
                LT = ScreenPos.From(r.L.Sx / value, r.T.Sy / value),
                RB = ScreenPos.From(r.R.Sx / value, r.B.Sy / value),
            };
        }

        public static ScreenRect operator /(ScreenRect r, ScreenX value)
        {
            return new ScreenRect
            {
                LT = ScreenPos.From(r.L.Sx / value.Sx, r.T.Sy),
                RB = ScreenPos.From(r.R.Sx / value.Sx, r.B.Sy),
            };
        }
        public static ScreenRect operator /(ScreenRect r, ScreenY value)
        {
            return new ScreenRect
            {
                LT = ScreenPos.From(r.L.Sx, r.T.Sy / value.Sy),
                RB = ScreenPos.From(r.R.Sx, r.B.Sy / value.Sy),
            };
        }

        public static ScreenRect operator /(ScreenRect r, ScreenPos value)
        {
            return new ScreenRect
            {
                LT = ScreenPos.From(r.L.Sx / value.X.Sx, r.T.Sy / value.Y.Sy),
                RB = ScreenPos.From(r.R.Sx / value.X.Sx, r.B.Sy / value.Y.Sy),
            };
        }

        /// <summary>
        /// offset horizontal position
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenRect operator +(ScreenRect v1, ScreenPos v2)
        {
            return new ScreenRect { LT = ScreenPos.From(v1.LT.X + v2.X, v1.LT.Y + v2.Y), RB = ScreenPos.From(v1.R + v2.X, v1.RB.Y + v2.Y), };
        }
        public static ScreenRect operator +(ScreenRect v1, ScreenSize v2)
        {
            return new ScreenRect { LT = ScreenPos.From(v1.LT.X + v2.Width, v1.LT.Y + v2.Height), RB = ScreenPos.From(v1.R + v2.Width, v1.RB.Y + v2.Height), };
        }

        /// <summary>
        /// offset horizontal position
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenRect operator +(ScreenRect v1, ScreenX v2)
        {
            return new ScreenRect { LT = ScreenPos.From(v1.LT.X + v2, v1.LT.Y), RB = ScreenPos.From(v1.R + v2, v1.RB.Y), };
        }

        /// <summary>
        /// offset vertical position
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenRect operator +(ScreenRect v1, ScreenY v2)
        {
            return new ScreenRect { LT = ScreenPos.From(v1.LT.X, v1.LT.Y + v2), RB = ScreenPos.From(v1.R, v1.RB.Y + v2), };
        }


        /// <summary>
        /// offset position
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static ScreenRect operator -(ScreenRect v1, ScreenPos v2)
        {
            return new ScreenRect { LT = ScreenPos.From(v1.LT.X - v2.X, v1.LT.Y - v2.Y), RB = ScreenPos.From(v1.R - v2.X, v1.RB.Y - v2.Y), };
        }

        public static ScreenRect operator -(ScreenRect v1, ScreenSize v2)
        {
            return new ScreenRect { LT = ScreenPos.From(v1.LT.X - v2.Width, v1.LT.Y - v2.Height), RB = ScreenPos.From(v1.R - v2.Width, v1.RB.Y - v2.Height), };
        }

        /// <summary>
        /// offset horizontal position
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static ScreenRect operator -(ScreenRect v1, ScreenX v2)
        {
            return new ScreenRect { LT = ScreenPos.From(v1.LT.X - v2, v1.LT.Y), RB = ScreenPos.From(v1.R - v2, v1.RB.Y), };
        }

        /// <summary>
        /// offset vertical position 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>

        public static ScreenRect operator -(ScreenRect v1, ScreenY v2)
        {
            return new ScreenRect { LT = ScreenPos.From(v1.LT.X, v1.LT.Y - v2), RB = ScreenPos.From(v1.R, v1.RB.Y - v2), };
        }
    }
}
