// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uConst の概要の説明です。
    /// </summary>
    public static class Const
    {
        /// <summary>長さゼロの配列</summary>
        public static readonly ICollection ZeroCollection = Array.Empty<object>();

        /// <summary>
        /// 長さゼロのリスト
        /// </summary>
        public static readonly IList ZeroList = new ArrayList(0);

        /// <summary>
        /// 赤道半径[mm]
        /// </summary>
        public static readonly double EarthRadiusX = 6378137000;

        /// <summary>
        /// 極半径[mm]
        /// </summary>
        public static readonly double EarthRadiusY = 6356752000;

        /// <summary>
        /// 文字列のフォーマット
        /// </summary>
        public static class Formatter
        {
            public static string Size(long size)
            {
                if (size < 1000)
                {
                    return size.ToString();
                }
                if (size < 1024 * 1024)
                {
                    return (size / 1024).ToString() + "k";
                }
                if (size < 1024 * 1024 * 1024)
                {
                    return (size / 1024.0 / 1024.0).ToString("0.0") + "M";
                }
                return (size / 1024.0 / 1024.0 / 1024.0).ToString("0.00") + "G";
            }
        }

        /// <summary>
        /// レイヤー
        /// </summary>
        public static class Layer
        {
            /// <summary>デバイスプレイヤーが使用するフリーレイヤー</summary>
            public const int DevicePlayer = 60001;

            /// <summary>ツールチップ</summary>
            public const int Tooltip = 79003;

            /// <summary>
            /// 特殊なパーツ（Clearしたくないもの）専用のレイヤー
            /// </summary>
            public static class StaticLayers
            {
                // ログ表示用パネル描画
                public const int LogPanel = 79008;

                // 選択領域(FeaturePartsSelectOnRect)
                public const int MaskRect = 79007;

                // スクロールバー(FeatureScrollBarHorz/FeatureScrollBarVert)
                public const int ScrollBarH = 79001;
                public const int ScrollBarV = 79002;

                /// <summary>
				/// 特殊なパーツ（Clearしたくないもの）かどうか？
				/// </summary>
				/// <param name="layer">検査対象</param>
				/// <returns></returns>
				public static bool IsStaticLayers(int layer)
                {
                    switch (layer)
                    {
                        case LogPanel:
                        case MaskRect:
                            return true;
                        default:
                            return false;
                    }
                }
            }
        }

        /// <summary>
        /// 文字列がTrueを示しているかどうかを調べる
        /// </summary>
        /// <param name="s">文字列</param>
        /// <returns>結果</returns>
        /// <remarks>!IsTrue() != IsFalse()であることに注意する</remarks>
        public static bool IsTrue(string s)
        {
            var ss = s.ToLower();
            if (ss == "1" || ss == "true" || ss == "on" || ss == "入" || ss == "真" || ss == "ok" || ss == "yes" || ss == "y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 浮動小数から安全に trueを判断する
        /// </summary>
        /// <param name="val">浮動小数 / 0 に近ければ false、そうでなければtrue</param>
        /// <returns>true / false</returns>
        public static bool IsTrue(double val)
        {
            if (Math.Abs(val) < 0.01)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 文字列がFalseを示しているかどうかを調べる
        /// </summary>
        /// <param name="s">文字列</param>
        /// <returns>結果</returns>
        /// <remarks>!IsFalse() != IsTrue()であることに注意する</remarks>
        public static bool IsFalse(string s)
        {
            var ss = s.ToLower();
            if (ss == "0" || ss == "false" || ss == "off" || ss == "切" || ss == "偽" || ss == "ng" || ss == "no" || ss == "n")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 浮動小数から安全に trueを判断する
        /// </summary>
        /// <param name="val">浮動小数 / 0 に近ければ false、そうでなければtrue</param>
        /// <returns>true / false</returns>
        public static bool IsFalse(double val)
        {
            return !IsTrue(val);
        }

        /// <summary>改行コード</summary>
        public const string CR = "\r\n";

        /// <summary>
        /// 指定コレクションからひとつ取得する
        /// </summary>
        /// <param name="col">コレクション</param>
        /// <returns>コレクション中のひとつ目</returns>
        public static object GetOne(ICollection col)
        {
            var e = col.GetEnumerator();
            if (e.MoveNext())
            {
                return e.Current;
            }
            return null;
        }
    }
}
