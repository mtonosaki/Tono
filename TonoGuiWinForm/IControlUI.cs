// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Control型のUIを取り扱う
    /// </summary>
    public interface IControlUI
    {
        System.Windows.Forms.Cursor Cursor
        {
            get;
            set;
        }
    }
}
