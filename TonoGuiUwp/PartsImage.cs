// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// draw image general parts
    /// </summary>
    /// <typeparam name="TCL">left position / code coodinates</typeparam>
    /// <typeparam name="TCT">top position / code coodinates</typeparam>
    /// <typeparam name="TCR">right position / code coodinates</typeparam>
    /// <typeparam name="TCB">bottom position / code coodinates</typeparam>
    /// <remarks>
    /// Location property indicates Left Top position.
    /// </remarks>
    public class PartsImage<TCL, TCT, TCR, TCB> : PartsRectangleBase<TCL, TCT, TCR, TCB>
    {
        /// <summary>
        /// image data
        /// </summary>
        public CanvasBitmap Image { get; set; }

        public override void Draw(DrawProperty dp)
        {
            if (Image == null)
            {
                return;
            }

            var sr = GetScreenRect(dp.Pane);
            dp.Graphics.DrawImage(Image, _(sr));
        }
    }
}
