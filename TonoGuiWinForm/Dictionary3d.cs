// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    public class Dictionary3d<TKey1, TKey2, TKey3, TVAL>
    {
        private readonly Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, TVAL>>> dat = new Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, TVAL>>>();

        /// <summary>
        /// クリア
        /// </summary>
        public void Clear()
        {
            dat.Clear();
        }

        /// <summary>
        /// キー（１次元目）
        /// </summary>
        public IEnumerable<TKey1> GetKey1s()
        {
            return dat.Keys;
        }

        public IEnumerable<TKey2> GetKey2s(TKey1 key1)
        {
            if (dat.TryGetValue(key1, out var d2))
            {
                return d2.Keys;
            }
            else
            {
                return System.Array.Empty<TKey2>();
            }
        }

        public IEnumerable<TKey3> GetKey3s(TKey1 key1, TKey2 key2)
        {
            if (dat.TryGetValue(key1, out var d2))
            {
                if (d2.TryGetValue(key2, out var d3))
                {
                    return d3.Keys;
                }
            }
            return System.Array.Empty<TKey3>();
        }

        /// <summary>
        /// 指定キーが登録されているかチェック
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="key3"></param>
        /// <returns></returns>
        public bool ContainKeys(TKey1 key1, TKey2 key2, TKey3 key3)
        {
            if (dat.TryGetValue(key1, out var d2))
            {
                if (d2.TryGetValue(key2, out var d3))
                {
                    return d3.ContainsKey(key3);
                }
            }
            return false;
        }

        /// <summary>
        /// 二次元テーブルのデータアクセス
        /// </summary>
        public TVAL this[TKey1 key1, TKey2 key2, TKey3 key3]
        {
            get
            {
                if (dat.TryGetValue(key1, out var d2))
                {
                    if (d2.TryGetValue(key2, out var d3))
                    {
                        if (d3.TryGetValue(key3, out var ret))
                        {
                            return ret;
                        }
                    }
                }
                return default;
            }
            set
            {
                if (dat.TryGetValue(key1, out var d2) == false)
                {
                    dat[key1] = d2 = new Dictionary<TKey2, Dictionary<TKey3, TVAL>>();
                }
                if (d2.TryGetValue(key2, out var d3) == false)
                {
                    d2[key2] = d3 = new Dictionary<TKey3, TVAL>();
                }
                d3[key3] = value;
            }
        }
    }
}
