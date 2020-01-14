// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Diagnostics;
using System.IO;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ログテキストをノートパッドで開く
    /// </summary>
    public class FeatureLogOpenAsText : FeatureBase, ITokenListener
    {
        private static readonly NamedId TOKEN = NamedId.FromName("OpenLogAsText");

        /// <summary>
        /// 起動
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            var fname = Path.GetTempFileName();
            File.WriteAllText(fname, LOG.GetCurrent());
            Process.Start("notepad.exe", fname);
        }

        #region ITokenListener メンバ

        public NamedId TokenTriggerID => TOKEN;

        #endregion
    }
}
