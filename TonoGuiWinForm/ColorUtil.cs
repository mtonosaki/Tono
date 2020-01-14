// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Color Utility
    /// </summary>
    public static class ColorUtil
    {
        /// <summary>
        /// 指定レンジいっぱいに使って、色で数値を分けることができるようにする。
        /// </summary>
        /// <param name="no"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Color GetColorByNumber(int no, int range)
        {
            if (no < 0)
            {
                no = -no;
            }

            if (range <= 10)
            {
                switch (no % 10)
                {
                    case 0: return Color.FromArgb(8, 8, 8); // 黒い礼（０）服
                    case 1: return Color.Brown;             // 茶を一杯
                    case 2: return Color.Red;               // 赤いに（２）んじん
                    case 3: return Color.Orange;            // 第（橙）三者
                    case 4: return Color.Yellow;            // 岸（黄４）恵子
                    case 5: return Color.Green;             // 緑子（５）
                    case 6: return Color.Blue;              // ろく（６）でなしの青二才
                    case 7: return Color.Purple;            // 紫式（７）部
                    case 8: return Color.Gray;              // ハイ（灰）ヤー（８）
                    case 9: return Color.White;             // ホワイトク（９）リスマス
                    default:
                        break;
                }
                return Color.DarkGray;
            }
            else
            {
                var h = (no * 129) % 360;       // 120サイクル
                var s = (no / 120) % 20;        // 20ステップ
                var v = (no / 2400) % 20;       // 20ステップ
                var hsv = new HSV(h, 1f - (float)s / 20, 1f - (float)v / 20);
                return hsv.ToColor();   // 48000色サイクル
            }
        }

        /// <summary>
        /// ２色を均等に混ぜる
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Color Blend(Color c1, Color c2)
        {
            if (c1 == null)
            {
                c1 = Color.Transparent;
            }

            if (c2 == null)
            {
                c2 = Color.Transparent;
            }

            return Color.FromArgb((c1.A + c2.A) / 2, (c1.R + c2.R) / 2, (c1.G + c2.G) / 2, (c1.B + c2.B) / 2);
        }

        /// <summary>
        /// 透明度を変更する（Color.FromArgb(int, Color)との違いは、アルファ値を比で指定する点）
        /// </summary>
        /// <param name="color">元の色</param>
        /// <param name="value">透明度 1.0f = 元と同じ / 0.5f = 倍の透明度 / 2.0f = 倍の不透明</param>
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

            return Color.FromArgb((int)a, color);
        }

        /// <summary>
        /// 明るさを変更する
        /// </summary>
        /// <param name="color">元の色</param>
        /// <param name="value">明るさ 1.0f = 元と同じ / 2.0f = 二倍の明るさ</param>
        /// <returns></returns>
        public static Color ChangeBrightness(Color color, float value)
        {
            var r = (int)(color.R * value);
            var g = (int)(color.G * value);
            var b = (int)(color.B * value);
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
        /// ネガ値を返す
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color GetNegativeColor(Color c)
        {
            return Color.FromArgb(c.A, 255 - c.R, 255 - c.G, 255 - c.B);
        }

        /// <summary>
        /// 近い色で区別しにくいかどうか調べる
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
        /// 色相・彩度・輝度
        /// </summary>
        public class HSV
        {
            /// <summary>
            /// 色相
            /// </summary>
            public float H = 0; // 0-360f

            /// <summary>
            /// 彩度
            /// </summary>
            public float S = 0; // 0-1.0f

            /// <summary>
            /// 輝度
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
            /// Colorオブジェクトを作成する
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
                return Color.FromArgb((int)(R * 255), (int)(G * 255), (int)(B * 255));
            }

            /// <summary>
            /// アルファ付でColorオブジェクトを返す
            /// </summary>
            /// <param name="alpha">アルファ 0-1.0</param>
            /// <returns></returns>
            public Color ToColor(double alpha)
            {
                return Color.FromArgb((int)(alpha * 255), ToColor());
            }

            /// <summary>
            /// Colorオブジェクトからインスタンスを作る
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
                    ret.H += 360;
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
        /// アルファ成分を削除して不透明にする
        /// </summary>
        /// <param name="color">対象となる色</param>
        /// <returns>アルファが削除されたColorインスタンス</returns>
        public static Color DeleteAlpha(Color color)
        {
            return Color.FromArgb(color.R, color.G, color.B);
        }
    }
}
