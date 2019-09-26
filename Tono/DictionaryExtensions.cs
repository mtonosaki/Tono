using System;
using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// Extension method of system Dictionary class
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Get value from key
        /// </summary>
        /// <typeparam name="TKEY"></typeparam>
        /// <typeparam name="TVAL"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <param name="isSetDefault">true=set return value into the key if not contains./param>
        /// <returns></returns>
        public static TVAL GetValueOrDefault<TKEY, TVAL>(this Dictionary<TKEY, TVAL> dic, TKEY key, TVAL def = default, bool isSetDefault = false)
        {
            if (dic.TryGetValue(key, out TVAL ret))
            {
                return ret;
            }
            else
            {
                if (isSetDefault)
                {
                    dic[key] = def;
                }
                return def;
            }
        }

        /// <summary>
        /// get value from key. default value makes deffered execution.
        /// </summary>
        /// <typeparam name="TKEY"></typeparam>
        /// <typeparam name="TVAL"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="deffunc"></param>
        /// <returns></returns>
        public static TVAL GetValueOrDefault<TKEY, TVAL>(this Dictionary<TKEY, TVAL> dic, TKEY key, bool isSetDefault, Func<TKEY, TVAL> deffunc)
        {
            if (dic.TryGetValue(key, out TVAL ret))
            {
                return ret;
            }
            else
            {
                ret = deffunc(key);
                if (isSetDefault)
                {
                    dic[key] = ret;
                }
                return ret;
            }
        }

        /// <summary>
        /// set value to key if not contains.
        /// </summary>
        /// <typeparam name="TKEY"></typeparam>
        /// <typeparam name="TVAL"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="deffunc"></param>
        /// <returns></returns>
        public static Dictionary<TKEY, TVAL> SetIfNoValue<TKEY, TVAL>(this Dictionary<TKEY, TVAL> dic, TKEY key, Func<TKEY, TVAL> deffunc)
        {
            if (dic.ContainsKey(key) == false)
            {
                dic[key] = deffunc.Invoke(key);
            }
            return dic;
        }


        /// <summary>
        /// get a value from dictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic">dictionary instance</param>
        /// <returns>a value</returns>
        public static TVAL GetAnObject<TKEY, TVAL>(this Dictionary<TKEY, TVAL> dic)
        {
            foreach (var val in dic.Values)
            {
                return val;
            }
            return default;
        }
    }
}
