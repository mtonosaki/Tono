using System.Collections.Generic;
using System.Linq;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// キー2つの辞書
    /// </summary>
    /// <typeparam name="TKey1">1つめのキーの型</typeparam>
    /// <typeparam name="TKey2">2つめのキーの型</typeparam>
    /// <typeparam name="TVal">値の型</typeparam>
    public class Dictionary2d<TKey1, TKey2, TVal>
    {
        private readonly Dictionary<TKey1, Dictionary<TKey2, TVal>> _dat = new Dictionary<TKey1, Dictionary<TKey2, TVal>>();

        /// <summary>
        /// クリア
        /// </summary>
        public void Clear()
        {
            _dat.Clear();
        }

        /// <summary>
        /// キーが2つあるディクショナリ
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        public TVal this[TKey1 key1, TKey2 key2]
        {
            get => _dat[key1][key2];
            set
            {
                if (_dat.TryGetValue(key1, out var dic) == false)
                {
                    _dat[key1] = dic = new Dictionary<TKey2, TVal>();
                }
                dic[key2] = value;
            }
        }

        /// <summary>
        /// キーが無い時にデフォルト値を返す
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public TVal this[TKey1 key1, TKey2 key2, TVal def] => this[key1, key2, def, false];

        /// <summary>
        /// キーが無い時にデフォルト値を返す
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="def"></param>
        /// <param name="isRegisterDefault">true=キーが無い時にdefをコレクションに追加する</param>
        /// <returns></returns>
        public TVal this[TKey1 key1, TKey2 key2, TVal def, bool isRegisterDefault]
        {
            get
            {
                if (_dat.TryGetValue(key1, out var dic) == false)
                {
                    if (isRegisterDefault)
                    {
                        this[key1, key2] = def;
                    }
                    return def;
                }
                if (dic.TryGetValue(key2, out var ret))
                {
                    return ret;
                }
                else
                {
                    if (isRegisterDefault)
                    {
                        this[key1, key2] = def;
                    }
                    return def;
                }
            }
        }

        /// <summary>
        /// キーの存在を確認する
        /// </summary>
        /// <param name="key1"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey1 key1)
        {
            return _dat.ContainsKey(key1);
        }

        /// <summary>
        /// キーの存在を確認する
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        public bool ContainKeys(TKey1 key1, TKey2 key2)
        {
            if (_dat.TryGetValue(key1, out var dic) == false)
            {
                return false;
            }
            return dic.ContainsKey(key2);
        }

        /// <summary>
        /// 一つめのキー一覧
        /// </summary>
        public IEnumerable<TKey1> Key1s()
        {
            return _dat.Keys;
        }

        /// <summary>
        /// 2つめのキー一覧を返す
        /// </summary>
        /// <param name="key1"></param>
        /// <returns></returns>
        public IEnumerable<TKey2> Key2s(TKey1 key1)
        {
            if (_dat.TryGetValue(key1, out var dic) == false)
            {
                return new List<TKey2>();
            }
            else
            {
                return dic.Keys;
            }
        }

        /// <summary>
        /// すべての値を返す
        /// </summary>
        public IEnumerable<TVal> Values()
        {
            var ret =
                from dic in _dat.Values
                from val in dic.Values
                select val;
            return ret;
        }

        /// <summary>
        /// キーに属する値を返す
        /// </summary>
        /// <param name="key1"></param>
        /// <returns></returns>
        public IEnumerable<TVal> Values(TKey1 key1)
        {
            if (_dat.TryGetValue(key1, out var dic) == false)
            {
                return new List<TVal>();
            }
            else
            {
                return dic.Values;
            }
        }

        /// <summary>
        /// 全値の件数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return Values().Count();
        }

        /// <summary>
        /// キーに属するカウント
        /// </summary>
        /// <param name="key1"></param>
        /// <returns></returns>
        public int Count(TKey1 key1)
        {
            return Values(key1).Count();
        }
    }
}
