// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRichPaneSync
    {
        /// <summary>
        /// ���������Y�[������
        /// </summary>
        XyBase ZoomMute
        {
            get;
            set;
        }

        /// <summary>
        /// ���������X�N���[������
        /// </summary>
        ScreenPos ScrollMute
        {
            get;
            set;
        }
    }
}
