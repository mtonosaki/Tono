using System.Drawing;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Graphicsが指定できる uRichPaneBinder
    /// </summary>
    public class RichPaneCustomGraphicsBinder : RichPaneBinder
    {
        private Graphics _graphics = null;

        /// <summary>
        /// Graphicsを指定したインスタンスを構築
        /// </summary>
        /// <param name="value"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static RichPaneCustomGraphicsBinder CreateCopy(IRichPane value, Graphics g)
        {
            var ret = new RichPaneCustomGraphicsBinder
            {
                _parent = null,
                _zoom = value.Zoom,
                _scroll = value.Scroll,
                _idtext = value.IdText,
                _graphics = g
            };
            return ret;
        }


        /// <summary>
        /// 親Paneを強制的に指定する
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(IRichPane parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// カスタムGraphic
        /// </summary>
        public override Graphics Graphics => _graphics;
    }
}
