// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uXx の概要の説明です。
    /// </summary>
    [Serializable]
    public class RangeXx : ValueCouple, ISpace
    {
        public static RangeXx operator +(RangeXx v1, ValueCouple v2) { return (RangeXx)((ValueCouple)v1 + v2); }
        public static RangeXx operator +(RangeXx v1, int v2) { return (RangeXx)((ValueCouple)v1 + v2); }
        public static RangeXx operator -(RangeXx v1, ValueCouple v2) { return (RangeXx)((ValueCouple)v1 - v2); }
        public static RangeXx operator -(RangeXx v1, int v2) { return (RangeXx)((ValueCouple)v1 - v2); }
        public static RangeXx operator *(RangeXx v1, ValueCouple v2) { return (RangeXx)((ValueCouple)v1 * v2); }
        public static RangeXx operator *(RangeXx v1, int v2) { return (RangeXx)((ValueCouple)v1 * v2); }
        public static RangeXx operator /(RangeXx v1, ValueCouple v2) { return (RangeXx)((ValueCouple)v1 / v2); }
        public static RangeXx operator /(RangeXx v1, int v2) { return (RangeXx)((ValueCouple)v1 / v2); }

        /// <summary>
        /// 中心
        /// </summary>
        public int Middle => (_v1 + _v2) / 2;

        /// <summary>
        /// 値を指定してインスタンスを作る
        /// </summary>
        /// <param name="v1">値１</param>
        /// <param name="v2">値２</param>
        /// <returns>インスタンス</returns>

        public static new RangeXx FromInt(int v1, int v2)
        {
            var ret = new RangeXx
            {
                X0 = v1,
                X1 = v2
            };
            return ret;
        }

        /// <summary>
        /// X0 = int.Max / X1 = int.Min でインスタンスを生成する
        /// </summary>
        /// <returns></returns>
        public static RangeXx FromNegativeSpan()
        {
            return RangeXx.FromInt(int.MaxValue, int.MinValue);
        }

        /// <summary>
        /// 矩形からX座標のみを抽出してインスタンスを生成する
        /// </summary>
        /// <param name="value">矩形</param>
        /// <returns>新しいインスタンス</returns>

        public static RangeXx FromRect(Rect value)
        {
            var ret = new RangeXx
            {
                X0 = value.LT.X,
                X1 = value.RB.X
            };
            return ret;
        }

        /// <summary>
        /// 1つ目のX座標
        /// </summary>
        public int X0
        {

            get => _v1;

            set => _v1 = value;
        }

        /// <summary>
        /// ２つ目のX座標
        /// </summary>
        public int X1
        {

            get => _v2;

            set => _v2 = value;
        }
        #region ISpace メンバ

        /// <summary>
        /// 指定したポイントがオブジェクト領域内にあるかどうか判定する
        /// </summary>
        /// <param name="value">uXx型の指定ポイント</param>
        /// <returns>true:領域内 / false:領域外</returns>
        // 
        public bool IsIn(object value)
        {
            if (value is int)
            {
                var pt = (int)value;
                if (X0 <= pt && X1 >= pt)
                {
                    return true;
                }
                return false;
            }
            if (value is RangeXx)
            {
                var pt = (RangeXx)value;
                if (X0 <= pt.X0 && pt.X0 <= X1 ||
                    X0 <= pt.X1 && pt.X1 <= X1 ||
                    pt.X0 <= X0 && pt.X1 >= X1)
                {
                    return true;
                }
            }
            if (value is XyBase)
            {
                return IsIn(((XyBase)value).X);
            }
            if (value is Rect)
            {
                var r = value as Rect;
                if (r is CodeRect || r.Height > 0)
                {
                    if (X0 <= r.LT.X && r.LT.X <= X1 ||
                        X0 <= r.RB.X && r.RB.X <= X1 ||
                        r.LT.X <= X0 && r.RB.X >= X1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// オブジェクトの移動
        /// </summary>
        /// <param name="value">uXx型の移動値 (X0,X1)</param>

        public void Transfer(object value)
        {
            System.Diagnostics.Debug.Assert(value is int, "Transferはint型だけサポートしています");
            var pt = (int)value;

            X0 += pt;
            X1 += pt;
        }

        /// <summary>
        /// オブジェクトの拡大
        /// </summary>
        /// <param name="value">uXx型の移動値 (X0,X1)</param>

        public void Inflate(object value)
        {
            if (value is int p)
            {
                X0 -= p;
                X1 += p;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is RangeXx, "InflateはuXx型だけサポートしています");
                var pt = (RangeXx)value;

                X0 -= pt.X0;
                X1 += pt.X1;
            }
        }

        /// <summary>
        /// オブジェクトの縮小
        /// </summary>
        /// <param name="value">uXx型の移動値 (X0,X1)</param>

        public void Deflate(object value)
        {
            if (value is int)
            {
                X0 += (int)value;
                X1 -= (int)value;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is RangeXx, "InflateはuXx型だけサポートしています");
                var pt = (RangeXx)value;

                X0 += pt.X0;
                X1 -= pt.X1;
            }
        }

        #endregion
    }
}
