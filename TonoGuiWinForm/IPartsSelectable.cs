// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IPartsSelectable �̊T�v�̐����ł��B
    /// </summary>
    public interface IPartsSelectable
    {
        bool IsSelected { get; set; }
    }
}
