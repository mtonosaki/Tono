using System;
using System.Diagnostics;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// u2 の概要の説明です。
    /// </summary>
    [Serializable]
    public class ValueCouple : ICloneable
    {
        ///<summary>一つ目の値</summary>
        protected int _v1;

        ///<summary>二つ目の値</summary>
        protected int _v2;

        #region Data tips for debugging
#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        public string _ => ToString();
#endif
        #endregion

        /// <summary>
        /// 一つ目の値
        /// </summary>
        public int Value1
        {
            
            get => _v1;
        }

        /// <summary>
        /// 二つ目の値
        /// </summary>
        public int Value2
        {
            
            get => _v2;
        }

        /// <summary>
        /// 値を指定してインスタンスを作る
        /// </summary>
        /// <param name="v1">値１</param>
        /// <param name="v2">値２</param>
        /// <returns>インスタンス</returns>
        
        public static ValueCouple FromInt(int v1, int v2)
        {
            var ret = new ValueCouple
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ValueCouple item)
            {
                return _v1 == item._v1 && _v2 == item._v2;
            }
            return false;
        }


        /// <summary>
        /// 座標が(0,0)の判定を行う
        /// </summary>
        /// <returns></returns>
        
        public bool IsZero()
        {
            return (_v1 & _v2) == 0 ? true : false;
        }

        /// <summary>
        /// 加算演算子(u2型とu2型の加算)
        /// </summary>
        /// <param name="v1">値１</param>
        /// <param name="v2">値２</param>
        /// <returns></returns>
        
        public static ValueCouple operator +(ValueCouple v1, ValueCouple v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 + v2._v1;
            ret._v2 = v1._v2 + v2._v2;

            return ret;
        }

        /// <summary>
        /// 加算演算子(_v1と_v2の両方に値２を加算する)
        /// </summary>
        /// <param name="v1">値１</param>
        /// <param name="v2">値２</param>
        /// <returns></returns>
        
        public static ValueCouple operator +(ValueCouple v1, int v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 + v2;
            ret._v2 = v1._v2 + v2;

            return ret;
        }

        /// <summary>
        /// 減算演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator -(ValueCouple v1, int v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 - v2;
            ret._v2 = v1._v2 - v2;

            return ret;
        }

        /// <summary>
        /// 減算演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator -(ValueCouple v1, ValueCouple v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 - v2._v1;
            ret._v2 = v1._v2 - v2._v2;

            return ret;
        }

        /// <summary>
        /// 乗算演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator *(ValueCouple v1, ValueCouple v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 * v2._v1;
            ret._v2 = v1._v2 * v2._v2;

            return ret;
        }

        /// <summary>
        /// 乗算演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator *(ValueCouple v1, double v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = (int)(v2 * v1._v1);
            ret._v2 = (int)(v2 * v1._v2);

            return ret;
        }

        /// <summary>
        /// 乗算演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator *(ValueCouple v1, int v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 * v2;
            ret._v2 = v1._v2 * v2;

            return ret;
        }

        /// <summary>
        /// 除算演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator /(ValueCouple v1, int v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 / v2;
            ret._v2 = v1._v2 / v2;

            return ret;
        }

        /// <summary>
        /// 乗算演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator /(ValueCouple v1, ValueCouple v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 / v2._v1;
            ret._v2 = v1._v2 / v2._v2;

            return ret;
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static bool operator <(ValueCouple v1, ValueCouple v2)
        {
            if (v1._v1 < v2._v1 && v1._v2 < v2._v2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static bool operator >(ValueCouple v1, ValueCouple v2)
        {
            if (v1._v1 > v2._v1 && v1._v2 > v2._v2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static bool operator <=(ValueCouple v1, ValueCouple v2)
        {
            if (v1._v1 <= v2._v1 && v1._v2 <= v2._v2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static bool operator >=(ValueCouple v1, ValueCouple v2)
        {
            if (v1._v1 >= v2._v1 && v1._v2 >= v2._v2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ハッシュコードを生成する
        /// </summary>
        /// <returns>ハッシュコード</returns>
        public override int GetHashCode()
        {
            // _v1 に対し、_v2 を8bits単位で反転（エンディアン反転）したものを XORした値をハッシュコードとする
            unchecked
            {
                var ret = (uint)_v1;
                uint mask = 0x00000fff;
                var speed = 12;     // maskのビット数
                var s = 0;
                var pp = 32 - speed;

                while (mask != 0)
                {
                    if ((_v2 & mask) != 0)
                    {

                        ret ^= (((((uint)_v2) & mask) >> s) << pp);
                    }
                    mask <<= speed;
                    s += speed;
                    pp -= speed;
                }
                return (int)ret;
            }
        }


        #region ICloneable メンバ

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        
        public object Clone()
        {
            var r = (ValueCouple)Activator.CreateInstance(GetType());
            r._v1 = _v1;
            r._v2 = _v2;
            return r;
        }

        #endregion

        /// <summary>
        /// インスタンスを表現する文字列を作成する（表示方法は変わるので、視覚目的にのみ使用すること）
        /// </summary>
        /// <returns>文字列</returns>
        
        public override string ToString()
        {
            return "(" + _v1 + ", " + _v2 + ")";
        }
    }
}
