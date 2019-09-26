using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// View area separator control to design using UWP designer
    /// </summary>
    public class TPane : RelativePanel, IDrawArea
    {
        /// <summary>
        /// horizontal scroll value(layout coodinates)
        /// </summary>
        public double ScrollX { get; set; } = 0.0;

        /// <summary>
        /// vertical scroll value(layout coodinates)
        /// </summary>
        public double ScrollY { get; set; } = 0.0;

        /// <summary>
        /// horizontal zoom value (1.0=normal)
        /// </summary>
        public double ZoomX { get; set; } = 1.0;

        /// <summary>
        /// vertical zoom value (1.0=normal)
        /// </summary>
        public double ZoomY { get; set; } = 1.0;

        /// <summary>
        /// pane area rectangle
        /// </summary>
        private ScreenRect _rect;

        /// <summary>
        /// pane rectangle position of parent view
        /// </summary>
        public ScreenRect Rect => _rect.Clone();

        /// <summary>
        /// the constructor of this class
        /// </summary>
        public TPane()
        {
            SizeChanged += onSizeChanged;
            LayoutUpdated += onLayoutUpdated;
        }

        private void onLayoutUpdated(object sender, object e)
        {
            var r = ControlUtil.GetElementRect(this);
            _rect = _(r);
        }

        private void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var r = ControlUtil.GetElementRect(this);
            _rect = _(r);
        }

        private string _name = null;

        /// <summary>
        /// name property to use as thread safe
        /// </summary>
        public new string Name
        {
            get => _name;
            set
            {
                _name = value;
                base.Name = value;
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} Name={Name} Rect={Rect}";
        }
    }
}
