// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uScPos の概要の説明です。
    /// </summary>
    [Serializable]
    public class ScreenPos : XyBase
    {
        /// <summary>
        /// 値を指定してインスタンスを作る
        /// </summary>
        /// <param name="v1">値１</param>
        /// <param name="v2">値２</param>
        /// <returns>インスタンス</returns>

        public static new ScreenPos FromInt(int v1, int v2)
        {
            var ret = new ScreenPos
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }

        /// <summary>
        /// 自動型変換
        /// </summary>
        public static implicit operator ScreenPos(System.Drawing.Point pos)
        {
            var ret = new ScreenPos
            {
                X = pos.X,
                Y = pos.Y
            };
            return ret;
        }

        /// <summary>演算子のオーバーロード</summary>
        public static ScreenPos operator +(ScreenPos v1, ValueCouple v2) { return (ScreenPos)((ValueCouple)v1 + v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static ScreenPos operator +(ScreenPos v1, int v2) { return (ScreenPos)((ValueCouple)v1 + v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static ScreenPos operator -(ScreenPos v1, ValueCouple v2) { return (ScreenPos)((ValueCouple)v1 - v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static ScreenPos operator *(ScreenPos v1, ValueCouple v2) { return (ScreenPos)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static ScreenPos operator *(ScreenPos v1, int v2) { return (ScreenPos)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static ScreenPos operator *(ScreenPos v1, double v2) { return (ScreenPos)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static ScreenPos operator /(ScreenPos v1, ValueCouple v2) { return (ScreenPos)((ValueCouple)v1 / v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static ScreenPos operator /(ScreenPos v1, int v2) { return (ScreenPos)((ValueCouple)v1 / v2); }
    }
}
