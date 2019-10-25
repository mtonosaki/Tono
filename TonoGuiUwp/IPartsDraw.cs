// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// parts interface of draw properties (set to PartsBase)
    /// </summary>
    public interface IPartsDraw
    {
        /// <summary>
        /// Draw event
        /// </summary>
        /// <param name="pane"></param>
        void Draw(DrawProperty pane);

        /// <summary>
        /// Shared asset manager
        /// </summary>
        GuiAssets Assets { get; set; }

        /// <summary>
        /// canvas priority
        /// </summary>
        uint ZIndex { get; set; }
    }
}
