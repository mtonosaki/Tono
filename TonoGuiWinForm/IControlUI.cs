// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ControlŒ^‚ÌUI‚ðŽæ‚èˆµ‚¤
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
