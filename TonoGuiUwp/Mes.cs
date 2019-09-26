using System;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// runtime language change support
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/ja-jp/windows/uwp/app-resources/localize-strings-ui-manifest"/>
    public static class Mes
    {
        public static readonly string Jp = "jp";
        public static readonly string En = "en";
        public static readonly string Zh = "zh-Hans-CN";

#pragma warning disable CS0618 // 型またはメンバーが古い形式です
        private static readonly ResourceLoader loader = null;
        private static readonly ResourceLoader loaderApp = null;

        static Mes()
        {
            try
            {
                loader = new ResourceLoader("TonoGuiUwp/Resources");
            }
            catch (Exception)
            {
                loader = null;
            }
            try
            {
                loaderApp = new ResourceLoader("Resources");
            }
            catch (Exception)
            {
                loaderApp = null;
            }
        }

        /// <summary>
        /// get message string by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }
            var t1 = loaderApp?.GetString(key);
            if (string.IsNullOrEmpty(t1) == false && string.IsNullOrWhiteSpace(t1) == false)
            {
                return t1;
            }
            var s = loader?.GetString(key) ?? string.Empty;
            return string.IsNullOrEmpty(s) == false ? s : key;
        }

        /// <summary>
        /// get message formatted string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Get(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        /// <summary>
        /// current language code
        /// </summary>
        public static string Langcode
        {
            get
            {
                if (_langcodeNow == "")
                {
                    _langcodeNow = ApplicationLanguages.PrimaryLanguageOverride;
                    if (_langcodeNow == "")
                    {
                        ChangeLanguage("jp");
                    }
                }
                return _langcodeNow;
            }
        }


        private static string _langcodeNow = "";

        /// <summary>
        /// change language code at runtime
        /// </summary>
        /// <param name="langcode">language code such as jp, en</param>
        public static void ChangeLanguage(string langcode)
        {
            _langcodeNow = langcode;
            ApplicationLanguages.PrimaryLanguageOverride = langcode;
        }
    }
}
