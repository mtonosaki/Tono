// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uXy の概要の説明です。
    /// </summary>
    [Serializable]
    public class XyBase : ValueCouple
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>

        public XyBase()
        {
        }

        /// <summary>演算子のオーバーロード</summary>
        public static XyBase operator +(XyBase v1, ValueCouple v2) { return (XyBase)((ValueCouple)v1 + v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static XyBase operator +(XyBase v1, int v2) { return (XyBase)((ValueCouple)v1 + v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static XyBase operator -(XyBase v1, ValueCouple v2) { return (XyBase)((ValueCouple)v1 - v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static XyBase operator *(XyBase v1, ValueCouple v2) { return (XyBase)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static XyBase operator *(XyBase v1, int v2) { return (XyBase)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static XyBase operator /(XyBase v1, ValueCouple v2) { return (XyBase)((ValueCouple)v1 / v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static XyBase operator /(XyBase v1, int v2) { return (XyBase)((ValueCouple)v1 / v2); }

        /// <summary>
        /// インスタンスを変換する
        /// </summary>
        /// <param name="xy">変換対象</param>
        /// <returns>PointF型のインスタンス</returns>

        public static implicit operator PointF(XyBase xy)
        {
            return new PointF(xy.X, xy.Y);
        }

        /// <summary>
        /// 自動型変換
        /// </summary>
        public static implicit operator System.Drawing.Point(XyBase pos)
        {
            var ret = new System.Drawing.Point
            {
                X = pos.X,
                Y = pos.Y
            };
            return ret;
        }

        /// <summary>
        /// 値を指定してインスタンスを作る
        /// </summary>
        /// <param name="v1">値１</param>
        /// <param name="v2">値２</param>
        /// <returns>インスタンス</returns>

        public static new XyBase FromInt(int v1, int v2)
        {
            var ret = new XyBase
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }


        /// <summary>
        /// X座標
        /// </summary>
        public int X
        {

            get => _v1;

            set => _v1 = value;
        }

        /// <summary>
        /// Y座標
        /// </summary>
        public int Y
        {

            get => _v2;

            set => _v2 = value;
        }
    }
}
