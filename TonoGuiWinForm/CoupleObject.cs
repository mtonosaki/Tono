using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// CoupleObject の概要の説明です。
    /// </summary>
    [Serializable]
    public class CoupleObject
    {
        public object V1 = null;
        public object V2 = null;

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="value1">参照値１</param>
        /// <param name="value2">参照値２</param>
        public CoupleObject(object value1, object value2)
        {
            V1 = value1;
            V2 = value2;
        }

        /// <summary>
        /// インスタンスを構築する
        /// </summary>
        /// <param name="value1">参照値１</param>
        /// <param name="value2">参照値２</param>
        /// <returns>新しいインスタンス</returns>
        public static CoupleObject FromObjects(object value1, object value2)
        {
            return new CoupleObject(value1, value2);
        }
    }

    /// <summary>
    /// 型を特定した２値グルーピング
    /// </summary>
    /// <typeparam name="T1">１番目の値の型</typeparam>
    /// <typeparam name="T2">２番目の値の型</typeparam>
    public class uCouple<T1, T2>
    {
        public T1 V1;
        public T2 V2;

        /// <summary>
        /// 構築子
        /// </summary>
        /// <param name="value1">参照値１</param>
        /// <param name="value2">参照値２</param>
        public uCouple(T1 value1, T2 value2)
        {
            V1 = value1;
            V2 = value2;
        }

        public override int GetHashCode()
        {
            var h1 = V1.GetHashCode();
            var h2 = V2.GetHashCode();
            h2 <<= 16;
            return h1 ^ h2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is uCouple<T1, T2> tar))
            {
                return false;
            }
            if (V1 == null)
            {
                return tar.V1 == null;
            }
            if (V2 == null)
            {
                return tar.V2 == null;
            }
            return V1.Equals(tar.V1) && V2.Equals(tar.V2);
        }

        public override string ToString()
        {
            return string.Format("{{{0},{1}}}", V1, V2);
        }
    }
}
