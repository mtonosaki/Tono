// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TonoAspNetCore
{
    /// <summary>
    /// Utility Controller
    /// </summary>
    public class ControllerUtils
    {
        /// <summary>
        /// Target controller
        /// </summary>
        public Controller Controller { get; set; }

        /// <summary>
        /// usable 64 character for output/input
        /// </summary>
        /// <remarks>
        /// for your original security, shuffle this strings as you like.
        /// </remarks>
        public const string TEXTSET64 = "wK0cskEpvVlBUXitL+byQIT5W89xRmdZAjJMe62HYSO/3u7raPCNhogzDfG1nq4F";

        /// <summary>
        /// Rijndael security key
        /// </summary>
        /// <remarks>16 characters</remarks>
        public const string KEY = "F4iK4KH4PY9eBxWA";

        /// <summary>
        /// Scramble text
        /// </summary>
        /// <remarks>
        /// For example, if you want make different security key by user account.
        /// </remarks>
        public const string MASK = "D6B7763D-43D5-492F-BAEA-0F4A1062D7AE";

        /// <summary>
        /// create instance
        /// </summary>
        /// <param name="target">target controller</param>
        /// <returns></returns>
        public static TonoControllerUtils From(Controller target)
        {
            return new TonoControllerUtils
            {
                Controller = target,
            };
        }

        /// <summary>
        /// Get Value from scrambled cookie
        /// </summary>
        /// <param name="key">Cookie key</param>
        /// <param name="def">default value if there is no key</param>
        /// <returns>value as plane text</returns>
        public string GetCookieSecure(string key, string def = "")
        {
            var cipher = Controller.Request.Cookies[key];
            if (string.IsNullOrEmpty(cipher))
            {
                return def;
            }
            else
            {
                return Decode(cipher);
            }
        }

        /// <summary>
        /// Set scrabmled value to cookie
        /// </summary>
        /// <param name="key">Cookie key</param>
        /// <param name="value">plane text</param>
        protected void SetCookieSecure(string key, string value)
        {
            SetCookieSecure(key, value, DateTimeOffset.UtcNow + TimeSpan.FromDays(14));
        }

        /// <summary>
        /// Set scrabmled value to cookie
        /// </summary>
        /// <param name="key">Cookie key</param>
        /// <param name="value">plane text</param>
        /// <param name="expires">Cookie expires</param>
        protected void SetCookieSecure(string key, string value, DateTimeOffset expires)
        {
            Controller.Response.Cookies.Append(key, Encode(value), new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires,
            });
        }

        private static readonly Random RND = new Random(DateTime.Now.Ticks.GetHashCode());

        /// <summary>
        /// Make Fusion string
        /// </summary>
        /// <param name="basestr"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private string FusionString(string basestr, string filter)
        {
            var nums = new[] { 79, 47, 37, 2, 7, 223, 269, 229, 211, 59, 89, 263, 149, 13, 179, 83, 281, 127, 199, 227, 31, 131, 73, 157, 5, 19, 139, 197, 167, 193, 53, 151, 29, 137, 97, 107, 241, 239, 163, 113, 103, 11, 257, 109, 23, 3, 41, 61, 233, 277, };
            var ret = new StringBuilder(basestr);
            var nB = basestr.Length;
            var nF = filter.Length;
            var offset = 0;
            for (var i = Math.Max(nB, nF) - 1; i >= 0; i--)
            {
                ret[i % nB] = TEXTSET64[(TEXTSET64.IndexOf(ret[i % nB]) + (int)filter[i % nF] + nums[(i + offset) % nums.Length]) % TEXTSET64.Length];
                offset++;
            }
            return ret.ToString();
        }

        /// <summary>
        /// RIjndael Encoding
        /// </summary>
        /// <param name="planeText"></param>
        /// <returns></returns>
        private string Encode(string planeText)
        {
            var iv = new StringBuilder();
            int ivN = 0;
            for (int ivi = 0; ivi < ivN + 16; ivi++)
            {
                iv.Append(TEXTSET64[RND.Next(TEXTSET64.Length - 1)]);
            }
            using (var ri = new RijndaelManaged
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Encoding.ASCII.GetBytes(iv.ToString()),
                Key = Encoding.ASCII.GetBytes(FusionString(KEY, MASK)),
            })
            {
                var enc = ri.CreateEncryptor(ri.Key, ri.IV);
                byte[] buf;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(planeText);
                        }
                        buf = ms.ToArray();
                    }
                }
                return ($"{TEXTSET64[ivN]}{iv}{Convert.ToBase64String(buf)}");
            }
        }

        /// <summary>
        /// Rijndael Decoding
        /// </summary>
        /// <param name="secText"></param>
        /// <returns></returns>
        private string Decode(string secText)
        {
            int ivN = TEXTSET64.IndexOf(secText[0]);
            string iv = secText.Substring(1, ivN + 16);
            string sec = secText.Substring(ivN + iv.Length + 1);

            using (var rijndael = new RijndaelManaged
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Encoding.ASCII.GetBytes(iv),
                Key = Encoding.ASCII.GetBytes(FusionString(KEY, MASK)),
            })
            {
                var de = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
                using (var ms = new MemoryStream(Convert.FromBase64String(sec)))
                {
                    using (var cs = new CryptoStream(ms, de, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set/Get Model's value to/from cookie
        /// </summary>
        /// <typeparam name="TModel">type of MVC model</typeparam>
        /// <param name="propertyName">property name that contains in the model</param>
        /// <param name="model">model instance</param>
        /// <param name="def">Default value. When model value is same with this default, this method tries to get value from cookie.</param>
        protected void PersistInput<TModel>(string propertyName, TModel model, string def)
        {
            var pp = model.GetType().GetProperty(propertyName);
            if (pp == null)
            {
                throw new KeyNotFoundException($"Cannot find the property name '{propertyName}' in the model type '{model.GetType().Name}'");
            }
            var inval = pp.GetValue(model)?.ToString() ?? "";

            if (string.IsNullOrEmpty(inval.Trim()) || inval.Equals(def))
            {
                var ck = GetCookieSecure($"{model.GetType().Name}_{propertyName}", def);
                pp.SetValue(model, ck);
            }
            else
            {
                SetCookieSecure($"{model.GetType().Name}_{propertyName}", inval);
            }
        }
    }
}
