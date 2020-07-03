// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Tono;

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

        private readonly Encrypt encrypt = new Encrypt
        {
            //IsCompression = true,  // TODO: TONO
        };

        /// <summary>
        /// create instance
        /// </summary>
        /// <param name="target">target controller</param>
        /// <returns></returns>
        public static ControllerUtils From(Controller target)
        {
            return new ControllerUtils
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
                return encrypt.Decode(cipher);
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
            Controller.Response.Cookies.Append(key, encrypt.Encode(value), new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires,
            });
        }

        /// <summary>
        /// Set/Get Model's value to/from cookie
        /// </summary>
        /// <typeparam name="TModel">type of MVC model</typeparam>
        /// <param name="propertyName">property name that contains in the model</param>
        /// <param name="model">model instance</param>
        /// <param name="def">Default value. When model value is same with this default, this method tries to get value from cookie.</param>
        public void PersistInput<TModel>(string propertyName, TModel model, string def)
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
