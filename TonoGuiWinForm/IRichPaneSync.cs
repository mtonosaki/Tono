// (c) 2019 Manabu Tonosaki
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
