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
        /// 自分だけズームする
        /// </summary>
        XyBase ZoomMute
        {
            get;
            set;
        }

        /// <summary>
        /// 自分だけスクロールする
        /// </summary>
        ScreenPos ScrollMute
        {
            get;
            set;
        }
    }
}
