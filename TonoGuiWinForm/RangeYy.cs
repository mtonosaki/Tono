using System;
using System.Diagnostics;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uYy の概要の説明です。
    /// </summary>
    [Serializable]
    public class RangeYy : ValueCouple, ISpace
    {
        /// <summary>
        /// 値を指定してインスタンスを作る
        /// </summary>
        /// <param name="v1">値1</param>
        /// <param name="v2">値2</param>
        /// <returns>インスタンス</returns>
        
        public static new RangeYy FromInt(int v1, int v2)
        {
            var ret = new RangeYy
            {
                Y0 = v1,
                Y1 = v2
            };
            return ret;
        }

        /// <summary>
        /// uRectのLT(=Y0)/RB(=Y1)のYを用いてインスタンスを生成する
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static RangeYy FromRect(Rect rect)
        {
            return RangeYy.FromInt(rect.LT.Y, rect.RB.Y);
        }

        /// <summary>
        /// 中点
        /// </summary>
        public int Middle => (_v1 + _v2) / 2;

        /// <summary>
        /// 高さ(２点の幅)
        /// </summary>
        public int Height => _v2 - _v1;

        /// <summary>
        /// １つ目のY座標
        /// </summary>
        public int Y0
        {
            
            get => _v1;
            
            set => _v1 = value;
        }

        /// <summary>
        /// ２つ目のY座標
        /// </summary>
        public int Y1
        {
            
            get => _v2;
            
            set => _v2 = value;
        }
        #region ISpace メンバ

        /// <summary>
        ///  指定したポイントがオブジェクト領域内にあるかどうか判定する
        /// </summary>
        /// <param name="value">uYy型の指定ポイント / int型の指定ポイント</param>
        /// <returns>true:領域内 / false:領域外</returns>
        
        public bool IsIn(object value)
        {
            if (value is int)
            {
                var pt = (int)value;
                if (Y0 <= pt && Y1 >= pt)
                {
                    return true;
                }
                return false;
            }
            if (value is RangeYy)
            {
                var pt = (RangeYy)value;
                if (Y0 <= pt.Y0 && Y1 >= pt.Y1)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// オブジェクトの移動
        /// </summary>
        /// <param name="value">uYy型の移動値 (Y0,Y1)</param>
        
        public void Transfer(object value)
        {
            System.Diagnostics.Debug.Assert(value is int, "Transferはint型だけサポートします");
            var pt = (int)value;

            Y0 += pt;
            Y1 += pt;
        }

        /// <summary>
        /// オブジェクトの拡大
        /// </summary>
        /// <param name="value">uYy型の移動値 (Y0,Y1)</param>
        
        public void Inflate(object value)
        {
            if (value is int)
            {
                Y0 -= (int)value;
                Y1 += (int)value;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is RangeYy, "InflateはuYy型だけサポートします");
                var pt = (RangeYy)value;

                Y0 -= pt.Y0;
                Y1 += pt.Y1;
            }
        }

        /// <summary>
        /// オブジェクトの縮小
        /// </summary>
        /// <param name="value">uYy型の移動値 (Y0,Y1)</param>
        
        public void Deflate(object value)
        {
            if (value is int)
            {
                Y0 += (int)value;
                Y1 -= (int)value;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is RangeYy, "InflateはuYy型だけサポートします");
                var pt = (RangeYy)value;

                Y0 += pt.Y0;
                Y1 -= pt.Y1;
            }
        }

        #endregion
    }
}
