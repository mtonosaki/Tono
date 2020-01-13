// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using Windows.Storage;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// application persistant value utility
    /// </summary>
    public static class ConfigUtil
    {
        private static readonly ApplicationDataContainer adc = ApplicationData.Current.RoamingSettings;

        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            return adc.Values[key] as string;
        }

        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static string Get(string key, string def)
        {
            //var ret = adc.Values[key] as string;
            if (adc.Values[key] is string ret)
            {
                return ret;
            }
            else
            {
                return def;
            }
        }

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, string value)
        {
            adc.Values[key] = value;
        }

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="v"></param>
        /// <param name="value"></param>
        public static void Set(string v, object value)
        {
            throw new NotImplementedException("The value should be set as string.");    // 文字列で代入してください
        }
    }
}
