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
        /// <param name="limitedToWidth">0=auto</param>
        /// <param name="limitedToHeight">0=auto</param>
        /// <returns></returns>
        public static ScreenSize MeasureString(ICanvasResourceCreator resourceCreator, string text, CanvasTextFormat textFormat, float limitedToWidth = 0.0f, float limitedToHeight = 0.0f)
        {
            var tmp = textFormat.WordWrapping;
            if (limitedToHeight < 0.001f && limitedToWidth < 0.001f)
            {
                textFormat.WordWrapping = CanvasWordWrapping.NoWrap;
            }
            var layout = new CanvasTextLayout(resourceCreator, text, textFormat, limitedToWidth, limitedToHeight);
            textFormat.WordWrapping = tmp;
            return ScreenSize.From(layout.DrawBounds.Width, layout.DrawBounds.Height);
        }
    }
}
