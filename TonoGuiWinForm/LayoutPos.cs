// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uPtPos の概要の説明です。
    /// </summary>
    [Serializable]
    public class LayoutPos : XyBase
    {
        /// <summary>
        /// 値を指定してインスタンスを作る
        /// </summary>
        /// <param name="v1">値１</param>
        /// <param name="v2">値２</param>
        /// <returns>インスタンス</returns>

        public static new LayoutPos FromInt(int v1, int v2)
        {
            var ret = new LayoutPos
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }

        /// <summary>演算子のオーバーロード</summary>
        public static LayoutPos operator +(LayoutPos v1, ValueCouple v2) { return (LayoutPos)((ValueCouple)v1 + v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static LayoutPos operator +(LayoutPos v1, int v2) { return (LayoutPos)((ValueCouple)v1 + v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static LayoutPos operator -(LayoutPos v1, ValueCouple v2) { return (LayoutPos)((ValueCouple)v1 - v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static LayoutPos operator *(LayoutPos v1, ValueCouple v2) { return (LayoutPos)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static LayoutPos operator *(LayoutPos v1, int v2) { return (LayoutPos)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static LayoutPos operator /(LayoutPos v1, ValueCouple v2) { return (LayoutPos)((ValueCouple)v1 / v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static LayoutPos operator /(LayoutPos v1, int v2) { return (LayoutPos)((ValueCouple)v1 / v2); }
    }
}
