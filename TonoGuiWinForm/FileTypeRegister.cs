// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ファイルタイプを登録する
    /// </summary>
    public class FileUtilTypeRegister
    {
        public enum FileType
        {
            XmlDocument,
            Binary,
            AscII,
            UTF8,
        }
        /// <summary>
        /// 登録する
        /// </summary>
        /// <param name="tar"></param>
        /// <param name="ext"></param>
        public FileUtilTypeRegister(Assembly tar, string ext, FileType ft, int iconNo)
        {
            Debug.Assert(ext.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0);

            var isJp = CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja";

            if (ext.StartsWith(".") == false)
            {
                ext = "." + ext;
            }
            // 拡張子との連絡キー
            var key = Registry.ClassesRoot.CreateSubKey(ext);
            var ftname = tar.GetName().Name + ".Document";
            key.SetValue(null, ftname);
            switch (ft)
            {
                case FileType.XmlDocument:
                    key.SetValue("Content Type", "text/xml");
                    key.SetValue("PerceivedType", "text");
                    break;
                case FileType.UTF8:
                case FileType.AscII:
                    key.SetValue("Content Type", "text/plain");
                    key.SetValue("PerceivedType", "text");
                    break;
                case FileType.Binary:
                    break;
            }
            key.Close();

            // ファイルタイプに対する起動情報
            var kft = Registry.ClassesRoot.CreateSubKey(ftname);
            string title = "", desc;
            var ats = tar.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
            if (ats.Length == 1)
            {
                title = ((AssemblyTitleAttribute)ats[0]).Title;
            }
            if (string.IsNullOrEmpty(title))
            {
                title = tar.GetName().Name;
            }
            ats = tar.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true);
            if (ats.Length == 1)
            {
                desc = ((AssemblyDescriptionAttribute)ats[0]).Description;
            }
            else
            {
                desc = "";
            }
            var s = title + (string.IsNullOrEmpty(desc) ? "" : " : " + desc);
            kft.SetValue(null, s);

            kft.SetValue("BrowserFlags", 8, RegistryValueKind.DWord);

            var icon = kft.CreateSubKey("DefaultIcon");
            icon.SetValue(null, string.Format("{0},{1}", tar.ManifestModule.FullyQualifiedName, iconNo), RegistryValueKind.ExpandString);
            icon.Close();
            var shell = kft.CreateSubKey("shell");
            shell.SetValue(null, "open");
            var open = shell.CreateSubKey("open");
            if (isJp)
            {
                open.SetValue(null, tar.GetName().Name + "で開く(&O)");
            }
            else
            {
                open.SetValue(null, "&Open with " + tar.GetName().Name);
            }
            var command = open.CreateSubKey("command");
            command.SetValue(null, "\"" + tar.ManifestModule.FullyQualifiedName + "\" \"%L\"", RegistryValueKind.ExpandString);
            command.Close();
            open.Close();
            shell.Close();
            kft.Close();
        }
    }
}
