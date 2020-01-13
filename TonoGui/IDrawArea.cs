// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.Gui
{
    /// <summary>
    /// drawing interface for TGuiView and TPane
    /// </summary>
    /// <remarks>
    /// NOTE: need to implement as thread safe for each properties.
    /// </remarks>
    public interface IDrawArea
    {
        /// <summary>
        /// Pane name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// horizontal offset distance of layout coodinate
        /// </summary>
        double ScrollX { get; set; }

        /// <summary>
        /// vertical offset distance of layout coodinate
        /// </summary>
        double ScrollY { get; set; }

        /// <summary>
        /// horizontal zoom value 1.0=original
        /// </summary>
        double ZoomX { get; set; }

        /// <summary>
        /// vertical zoom value 1.0=original
        /// </summary>
        double ZoomY { get; set; }

        /// <summary>
        /// draw area
        /// </summary>
        ScreenRect Rect { get; }
    }
}
