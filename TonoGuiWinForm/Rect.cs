// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 矩形座標の取得
    /// </summary>
    [Serializable]
    public class Rect : ISpace, ICloneable
    {
        #region 属性（シリアライズ有り）

        ///<summary>leftとtopの座標値</summary>
        protected XyBase _lt;
        ///<summary>rightとbottomの座標値</summary>
        protected XyBase _rb;

        #endregion

        #region Data tips for debugging
#if DEBUG
        public string _ => ToString();
#endif
        #endregion

        public static Rect Empty => Rect.FromLTRB(0, 0, 0, 0);

        /// <summary>
        /// LTとRBが同じ値かどうかを調べる
        /// </summary>
        public bool IsEmpty => _lt.X == _rb.X && _lt.Y == _rb.Y;

        public virtual XyBase LT
        {

            get => _lt;

            set => _lt = value;
        }

        public virtual XyBase RB
        {

            get => _rb;

            set => _rb = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>

        public Rect()
        {
            _lt = new XyBase();
            _rb = new XyBase();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is Rect)
            {
                return _lt.Equals(((Rect)obj)._lt) && _rb.Equals(((Rect)obj)._rb);
            }
            return false;
        }

        /// <summary>
        /// LT左上、RB右下になるように座標を交換する
        /// </summary>
        public void Normalize()
        {
            if (_lt.X > _rb.X)
            {
                var tmp = _lt.X;
                _lt.X = _rb.X;
                _rb.X = tmp;
            }
            if (_lt.Y > _rb.Y)
            {
                var tmp = _lt.Y;
                _lt.Y = _rb.Y;
                _rb.Y = tmp;
            }
        }

        /// <summary>
        /// インスタンスの型をRectangleF型に変換する
        /// </summary>
        /// <param name="r">変換対象</param>
        /// <returns>新しい型のインスタンス</returns>

        public static implicit operator RectangleF(Rect r)
        {
            return new RectangleF(r.LT.X, r.LT.Y, r.Width, r.Height);
        }

        /// <summary>
        /// インスタンスの型をRectangleF型に変換する
        /// </summary>
        /// <param name="r">変換対象</param>
        /// <returns>新しい型のインスタンス</returns>

        public static implicit operator Rectangle(Rect r)
        {
            return new Rectangle(r.LT.X, r.LT.Y, r.Width, r.Height);
        }

        /// <summary>
        /// 整数値からインスタンスを構築する
        /// </summary>
        /// <param name="x">左上座標X</param>
        /// <param name="y">左上座標Y</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns></returns>

        public static Rect FromLTWH(int x, int y, int width, int height)
        {
            var ret = new Rect();
            ret.LT.X = x;
            ret.LT.Y = y;
            ret.RB.X = x + width - 1;
            ret.RB.Y = y + height - 1;
            return ret;
        }

        /// <summary>
        /// 座標を指定して新しいインスタンスを構築する
        /// </summary>
        /// <param name="x0">左上のX座標</param>
        /// <param name="y0">左上のY座標</param>
        /// <param name="x1">右下のX座標</param>
        /// <param name="y1">右下のY座標</param>
        /// <returns>構築したインスタンス</returns>

        public static Rect FromLTRB(int x0, int y0, int x1, int y1)
        {
            var ret = new Rect();
            ret.LT.X = x0;
            ret.LT.Y = y0;
            ret.RB.X = x1;
            ret.RB.Y = y1;
            return ret;
        }

        /// <summary>
        /// 幅の計算
        /// </summary>
        public int Width => RB.X - LT.X + 1;

        /// <summary>
        /// 高さの計算
        /// </summary>
        public int Height => RB.Y - LT.Y + 1;

        /// <summary>
        /// 中心点を取得する
        /// </summary>
        /// <returns></returns>
        public XyBase GetCenter()
        {
            return XyBase.FromInt((LT.X + RB.X) / 2, (LT.Y + RB.Y) / 2);
        }

        public XyBase LB => XyBase.FromInt(LT.X, RB.Y);

        public XyBase RT => XyBase.FromInt(RB.X, LT.Y);

        public RangeXx LR => RangeXx.FromInt(LT.X, RB.X);

        public RangeYy TB => RangeYy.FromInt(LT.Y, RB.Y);

        /// <summary>
        /// サイズを＋１したインスタンスを返す
        /// </summary>
        /// <returns>サイズ＋１したインスタンス</returns>

        public Rect GetPpSize()
        {
            var ret = (Rect)Clone();
            ret.RB.X++;
            ret.RB.Y++;
            return ret;
        }

        /// <summary>
        /// 正しい矩形かどうか（負の大きさになてないか）調べる
        /// </summary>
        /// <returns>true = 正しい領域 / false = 負の領域</returns>
        //		 
        public bool IsNormalizedRect()
        {
            if (LT.X > RB.X)
            {
                return false;
            }
            if (LT.Y > RB.Y)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 領域のANDを取る
        /// </summary>
        /// <param name="r">領域１</param>
        /// <param name="v">領域２</param>
        /// <returns>AND後の新しいインスタンス</returns>

        public static Rect operator &(Rect r1, Rect r2)
        {
            if (r1 == null || r2 == null)
            {
                return null;
            }
            var ret = (Rect)r1.Clone();
            ret.LT.X = r1.LT.X >= r2.LT.X ? r1.LT.X : r2.LT.X;
            ret.LT.Y = r1.LT.Y >= r2.LT.Y ? r1.LT.Y : r2.LT.Y;
            ret.RB.X = r1.RB.X <= r2.RB.X ? r1.RB.X : r2.RB.X;
            ret.RB.Y = r1.RB.Y <= r2.RB.Y ? r1.RB.Y : r2.RB.Y;
            if (ret.IsNormalizedRect() == false)
            {
                return null;
            }
            return ret;
        }

        /// <summary>
        /// 領域のORを取る。離れた位置にある矩形同士でも各領域の最小・最大をとったひとつの矩形を返す。
        /// </summary>
        /// <param name="r">領域１</param>
        /// <param name="v">領域２</param>
        /// <returns>OR後の新しいインスタンス</returns>
        //		
        public static Rect operator |(Rect r1, Rect r2)
        {
            if (r1 == null || r2 == null)
            {
                return null;
            }
            var ret = (Rect)r1.Clone();
            ret.LT.X = r1.LT.X < r2.LT.X ? r1.LT.X : r2.LT.X;
            ret.LT.Y = r1.LT.Y < r2.LT.Y ? r1.LT.Y : r2.LT.Y;
            ret.RB.X = r1.RB.X > r2.RB.X ? r1.RB.X : r2.RB.X;
            ret.RB.Y = r1.RB.Y > r2.RB.Y ? r1.RB.Y : r2.RB.Y;
            if (ret.IsNormalizedRect() == false)
            {
                return null;
            }
            return ret;
        }

        /// <summary>
        /// 加算演算子
        /// </summary>
        /// <param name="r">元のオブジェクト</param>
        /// <param name="v">加算値</param>
        /// <returns>加算後の新しいインスタンス</returns>

        public static Rect operator +(Rect r, XyBase v)
        {
            var ret = (Rect)r.Clone();
            ret.Transfer(v);
            return ret;
        }

        /// <summary>
        /// 減算演算子
        /// </summary>
        /// <param name="r">元のオブジェクト</param>
        /// <param name="v">加算値</param>
        /// <returns>加算後の新しいインスタンス</returns>

        public static Rect operator -(Rect r, XyBase v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X -= v.X;
            ret.LT.Y -= v.Y;
            ret.RB.X -= v.X;
            ret.RB.Y -= v.Y;
            return ret;
        }

        /// <summary>
        /// 乗算演算子
        /// </summary>
        /// <param name="r">元のオブジェクト</param>
        /// <param name="v">加算値</param>
        /// <returns>加算後の新しいインスタンス</returns>

        public static Rect operator *(Rect r, XyBase v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X = r.LT.X * v.X;
            ret.LT.Y = r.LT.Y * v.Y;
            ret.RB.X = r.RB.X * v.X;
            ret.RB.Y = r.RB.Y * v.Y;
            return ret;
        }

        /// <summary>
        /// 乗算演算子
        /// </summary>
        /// <param name="r">元のオブジェクト</param>
        /// <param name="v">加算値</param>
        /// <returns>加算後の新しいインスタンス</returns>

        public static Rect operator *(Rect r, int v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X = r.LT.X * v;
            ret.LT.Y = r.LT.Y * v;
            ret.RB.X = r.RB.X * v;
            ret.RB.Y = r.RB.Y * v;
            return ret;
        }

        /// <summary>
        /// 除算演算子
        /// </summary>
        /// <param name="r">元のオブジェクト</param>
        /// <param name="v">加算値</param>
        /// <returns>加算後の新しいインスタンス</returns>

        public static Rect operator /(Rect r, int v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X = r.LT.X / v;
            ret.LT.Y = r.LT.Y / v;
            ret.RB.X = r.RB.X / v;
            ret.RB.Y = r.RB.Y / v;
            return ret;
        }

        /// <summary>
        /// 除算演算子
        /// </summary>
        /// <param name="r">元のオブジェクト</param>
        /// <param name="v">加算値</param>
        /// <returns>加算後の新しいインスタンス</returns>

        public static Rect operator /(Rect r, XyBase v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X = r.LT.X / v.X;
            ret.LT.Y = r.LT.Y / v.Y;
            ret.RB.X = r.RB.X / v.X;
            ret.RB.Y = r.RB.Y / v.Y;
            return ret;
        }

        #region ISpace メンバ

        /// <summary>
        /// 指定したポイントがオブジェクト領域内にあるかどうか判定する
        /// </summary>
        /// <param name="o1">uXy型の指定ポイント</param>
        /// <returns>true:領域内 / false:領域外</returns>
        public bool IsIn(object value)
        {
            if (value is XyBase xy)
            {
                if (LT.X <= xy.X && RB.X >= xy.X && LT.Y <= xy.Y && RB.Y >= xy.Y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (value is Rect tar)
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
        /// オブジェクトの移動
        /// </summary>
        /// <param name="value">uXy型の移動値 (X,Y)</param>

        public void Transfer(object value)
        {
            System.Diagnostics.Debug.Assert(value is XyBase, "TransferはuXy型だけサポートしています");
            var pt = (XyBase)value;

            //オブジェクトの移動
            LT.X = LT.X + pt.X;
            LT.Y = LT.Y + pt.Y;
            RB.X = RB.X + pt.X;
            RB.Y = RB.Y + pt.Y;
        }

        /// <summary>
        /// オブジェクトの拡大
        /// </summary>
        /// <param name="value">uXy型の縮小値 (X,Y)</param>

        public void Inflate(object value)
        {
            if (value is double vald)
            {
                //拡大
                LT.X = (int)(LT.X - vald);
                LT.Y = (int)(LT.Y - vald);
                RB.X = (int)(RB.X + vald);
                RB.Y = (int)(RB.Y + vald);
            }
            else
            if (value is int vali)
            {
                //拡大
                LT.X = LT.X - vali;
                LT.Y = LT.Y - vali;
                RB.X = RB.X + vali;
                RB.Y = RB.Y + vali;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is XyBase, "InflateはuXy型だけサポートしています");

                var pt = (XyBase)value;

                //拡大
                LT.X = LT.X - pt.X;
                LT.Y = LT.Y - pt.Y;
                RB.X = RB.X + pt.X;
                RB.Y = RB.Y + pt.Y;
            }
        }

        /// <summary>
        /// オブジェクトの縮小
        /// </summary>
        /// <param name="value">uXyの拡大値 (X,Y)</param>

        public void Deflate(object value)
        {
            if (value is double vald)
            {
                //縮小
                LT.X = (int)(LT.X + vald);
                LT.Y = (int)(LT.Y + vald);
                RB.X = (int)(RB.X - vald);
                RB.Y = (int)(RB.Y - vald);
            }
            else
            if (value is int vali)
            {
                //縮小
                LT.X = LT.X + vali;
                LT.Y = LT.Y + vali;
                RB.X = RB.X - vali;
                RB.Y = RB.Y - vali;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is XyBase, "DeflateはuXy型だけサポートしています");

                var pt = (XyBase)value;

                //縮小
                LT.X = LT.X + pt.X;
                LT.Y = LT.Y + pt.Y;
                RB.X = RB.X - pt.X;
                RB.Y = RB.Y - pt.Y;
            }

        }

        #endregion

        #region ICloneable メンバ


        public object Clone()
        {
            var r = (Rect)Activator.CreateInstance(GetType());
            r.LT.X = LT.X;
            r.LT.Y = LT.Y;
            r.RB.X = RB.X;
            r.RB.Y = RB.Y;
            return r;
        }

        #endregion

        /// <summary>
        /// インスタンスを表現する文字列を作成する（表示方法は変わるので、視覚目的にのみ使用すること）
        /// </summary>
        /// <returns>文字列</returns>

        public override string ToString()
        {
            return LT.ToString() + "-" + RB.ToString();
        }
    }
}
