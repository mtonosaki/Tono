using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// UWP Graphic handling utility
    /// </summary>
    public class GraphicUtil
    {
        /// <summary>
        /// check font size
        /// </summary>
        /// <param name="resourceCreator">canvas</param>
        /// <param name="text">measure text</param>
        /// <param name="textFormat">the text format</param>
        /// <param name="limitedToWidth"></param>
        /// <param name="limitedToHeight"></param>
        /// <returns></returns>
        public static ScreenSize MeasureString(ICanvasResourceCreator resourceCreator, string text, CanvasTextFormat textFormat, float limitedToWidth = 0.0f, float limitedToHeight = 0.0f)
        {
            var layout = new CanvasTextLayout(resourceCreator, text, textFormat, limitedToWidth, limitedToHeight);
            return ScreenSize.From(layout.DrawBounds.Height, layout.DrawBounds.Width);
        }
    }
}
