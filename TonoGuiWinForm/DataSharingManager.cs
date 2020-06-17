// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 変数を共有する
    /// </summary>
    public class DataSharingManager
    {
        /// <summary>
        /// 使用禁止
        /// 共有に使える文字列型
        /// </summary>
        public class String
        {
            public string Value = "";
        }

        /// <summary>
        /// 共有に使える文字列型
        /// </summary>
        public class Object
        {
            public object value;
        }

        /// <summary>
        /// 共有に使えるブーリアン型
        /// </summary>
        public class Boolean
        {
            public bool value;
        }

        /// <summary>
        /// 共有に使える整数型
        /// </summary>
        public class Int
        {
            public int value = 0;
#if DEBUG
            public string _ => value.ToString();
#endif
            public static Int operator ++(Int tar)
            {
                tar.value++;
                return tar;
            }
            public static Int operator --(Int tar)
            {
                tar.value--;
                return tar;
            }

            /// <summary>
            /// int型へのキャストサポート
            /// </summary>
            public static implicit operator int(Int from)
            {
                return from.value;
            }

            public override int GetHashCode()
            {
                return value;
            }

            public override bool Equals(object obj)
            {
                return int.Equals(value, obj);
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        #region 属性（シリアライズする）

        /// <summary>インスタンスを構成するデータ</summary>
        private readonly IDictionary _dat = new HybridDictionary();

        #endregion

        /// <summary>
        /// 登録キーの件数
        /// </summary>
        public int Count => _dat.Count;

        /// <summary>
        /// 共有変数を取得する
        /// </summary>
        /// <param name="name">共有変数名</param>
        /// <param name="valueType">共有変数の値の型</param>
        /// <returns>共有変数のインスタンス</returns>
        public object Get(string name, Type valueType)
        {
            var ret = _dat[name];
            if (ret == null)
            {
                System.Diagnostics.Debug.Assert(valueType.IsSubclassOf(typeof(System.ValueType)) == false, "共有変数ffShareに使用できるのは参照型だけです。" + name + " という名前の共有変数に使用している型を変更してください");
                if (valueType.Equals(typeof(string)))
                {
                    ret = "";
                }
                else
                {
                    ret = Activator.CreateInstance(valueType, true);
                }
                _dat.Add(name, ret);
            }
            return ret;
        }

        /// <summary>
        /// 共有変数を取得する(存在しなかったら、指定したインスタンスを割り当てる)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object Get(string name, object instance)
        {
            var ret = _dat[name];
            if (ret == null)
            {
                System.Diagnostics.Debug.Assert(instance.GetType().IsSubclassOf(typeof(System.ValueType)) == false, "共有変数ffShareに使用できるのは参照型だけです。" + name + " という名前の共有変数に使用している型を変更してください");
                ret = instance;
                _dat.Add(name, ret);
            }
            return ret;
        }
    }
}
