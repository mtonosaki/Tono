using System;
using Windows.UI;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Color change utility
    /// </summary>
    public static class ColorUtil
    {
        /// <summary>
        /// make color from value between range
        /// </summary>
        /// <param name="value"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Color GetColorByNumber(int value, int range)
        {
            if (value < 0)
            {
                value = -value;
            }

            if (range <= 10)
            {
                switch (value % 10)
                {
                    case 0: return Color.FromArgb(255, 8, 8, 8); // 黒い礼（０）服
                    case 1: return Colors.Brown;                 // 茶を一杯
                    case 2: return Colors.Red;                   // 赤いに（２）んじん
                    case 3: return Colors.Orange;                // 第（橙）三者
                    case 4: return Colors.Yellow;                // 岸（黄４）恵子
                    case 5: return Colors.Green;                 // 緑子（５）
                    case 6: return Colors.Blue;                  // ろく（６）でなしの青二才
                    case 7: return Colors.Purple;                // 紫式（７）部
                    case 8: return Colors.Gray;                  // ハイ（灰）ヤー（８）
                    case 9: return Colors.White;                 // ホワイトク（９）リスマス
                }
                return Colors.DarkGray;
            }
            else
            {
                var h = (value * 129) % 360;       // 120 cycle
                var s = (value / 120) % 20;        // 20 step
                var v = (value / 2400) % 20;       // 20 step
                var hsv = new HSV(h, 1f - (float)s / 20, 1f - (float)v / 20);
                return hsv.ToColor();   // 48000 color cycle
            }
        }

        /// <summary>
        /// Blend two colors
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Color Blend(Color c1, Color c2)
        {
            if (c1 == null)
            {
                c1 = Colors.Transparent;
            }

            if (c2 == null)
            {
                c2 = Colors.Transparent;
            }

            return Color.FromArgb((byte)((c1.A + c2.A) / 2), (byte)((c1.R + c2.R) / 2), (byte)((c1.G + c2.G) / 2), (byte)((c1.B + c2.B) / 2));
        }

        /// <summary>
        /// change transparent volume
        /// </summary>
        /// <param name="color">original color</param>
        /// <param name="value">1.0f = same with original color / 0.5f = double transparent / 2.0f = double opacity</param>
        /// <returns></returns>
        public static Color ChangeAlpha(Color color, float value)
        {
            float a = color.A;
            a *= value;
            if (a < 0)
            {
                a = 0;
            }

            if (a > 255)
            {
                a = 255;
            }

            return Color.FromArgb((byte)a, color.R, color.G, color.B);
        }

        /// <summary>
        /// reset transparent volume
        /// </summary>
        /// <param name="color"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color SetAlpha(Color color, double value)
        {
            return Color.FromArgb((byte)(value * 255), color.R, color.G, color.B);
        }

        /// <summary>
        /// change brightness
        /// </summary>
        /// <param name="color">original color</param>
        /// <param name="value">1.0f = same to original color / 2.0f = double brightness</param>
        /// <returns></returns>
        public static Color ChangeBrightness(Color color, float value)
        {
            var r = (byte)(color.R * value);
            var g = (byte)(color.G * value);
            var b = (byte)(color.B * value);
            if (r < 0)
            {
                r = 0;
            }

            if (r > 255)
            {
                r = 255;
            }

            if (g < 0)
            {
                g = 0;
            }

            if (g > 255)
            {
                g = 255;
            }

            if (b < 0)
            {
                b = 0;
            }

            if (b > 255)
            {
                b = 255;
            }

            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// make negative color
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color GetNegativeColor(Color c)
        {
            return Color.FromArgb(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));
        }

        /// <summary>
        /// check similar color or not
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool IsNearColor(Color c1, Color c2)
        {
            var hsv1 = HSV.FromColor(c1);
            var hsv2 = HSV.FromColor(c2);
            var dh = Math.Abs(hsv1.H - hsv2.H);
            if (dh > 180)
            {
                dh = Math.Abs(dh - 360);
            }
            if (dh < 45)
            {
                if (Math.Abs(hsv1.V - hsv2.V) < 64)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// HSV model color
        /// </summary>
        public class HSV
        {
            /// <summary>
            /// Hue 色相
            /// </summary>
            public float H = 0; // 0-360f

            /// <summary>
            /// Saturation 彩度
            /// </summary>
            public float S = 0; // 0-1.0f

            /// <summary>
            /// Value 明度
            /// </summary>
            public float V = 0; // 0-1.0f

            public HSV(float h, float s, float v)
            {
                H = h;
                S = s;
                V = v;
            }

            public HSV()
            {
                H = 0;
                S = 0;
                V = 0;
            }

            /// <summary>
            /// Convert to Color object
            /// </summary>
            /// <returns></returns>
            public Color ToColor()
            {
                float R = 0, G = 0, B = 0;
                var Hi = Math.Abs(((int)H) / 60) % 6;
                var f = H / 60 - Hi;
                var p = V * (1 - S);
                var q = V * (1 - f * S);
                var t = V * (1 - (1 - f) * S);
                if (Hi == 0)
                {
                    R = V;
                    G = t;
                    B = p;
                }
                if (Hi == 1)
                {
                    R = q;
                    G = V;
                    B = p;
                }
                if (Hi == 2)
                {
                    R = p;
                    G = V;
                    B = t;
                }
                if (Hi == 3)
                {
                    R = p;
                    G = q;
                    B = V;
                }
                if (Hi == 4)
                {
                    R = t;
                    G = p;
                    B = V;
                }
                if (Hi == 5)
                {
                    R = V;
                    G = p;
                    B = q;
                }
                return Color.FromArgb(255, (byte)(R * 255), (byte)(G * 255), (byte)(B * 255));
            }

            /// <summary>
            /// Convert to Color specified alpha channel volume
            /// </summary>
            /// <param name="alpha">アルファ 0-1.0</param>
            /// <returns></returns>
            public Color ToColor(double alpha)
            {
                var col = ToColor();
                return Color.FromArgb((byte)(alpha * 255), col.R, col.G, col.B);
            }

            /// <summary>
            /// make a new instance from Color object
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            public static HSV FromColor(Color c)
            {
                var ret = new HSV();
                float Z;
                float r, g, b;
                var R = c.R / 255f;
                var G = c.G / 255f;
                var B = c.B / 255f;

                ret.V = max_color(R, G, B);
                Z = min_color(R, G, B);
                if (ret.V != 0.0)
                {
                    ret.S = (ret.V - Z) / ret.V;
                }
                else
                {
                    ret.S = 0.0f;
                }

                if ((ret.V - Z) != 0)
                {
                    r = (ret.V - R) / (ret.V - Z);
                    g = (ret.V - G) / (ret.V - Z);
                    b = (ret.V - B) / (ret.V - Z);
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                if (ret.V == R)
                {
                    ret.H = 60 * (b - g);       // 60 = PI/3
                }
                else if (ret.V == G)
                {
                    ret.H = 60 * (2 + r - b);
                }
                else
                {
                    ret.H = 60 * (4 + g - r);
                }
                if (ret.H < 0.0)
                {
                    ret.H = ret.H + 360;
                }
                return ret;
            }
            private static float max_color(float r, float g, float b)
            {
                float ret;
                if (r > g)
                {
                    if (r > b)
                    {
                        ret = r;
                    }
                    else
                    {
                        ret = b;
                    }
                }
                else
                {
                    if (g > b)
                    {
                        ret = g;
                    }
                    else
                    {
                        ret = b;
                    }
                }
                return ret;
            }
            private static float min_color(float r, float g, float b)
            {
                float ret;
                if (r < g)
                {
                    if (r < b)
                    {
                        ret = r;
                    }
                    else
                    {
                        ret = b;
                    }
                }
                else
                {
                    if (g < b)
                    {
                        ret = g;
                    }
                    else
                    {
                        ret = b;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Delete alpha channel to make opacity color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color DeleteAlpha(Color color)
        {
            return Color.FromArgb(255, color.R, color.G, color.B);
        }
    }
}
