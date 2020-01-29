// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// MVFP Architecture : View
    /// </summary>
    public partial class TGuiView : RelativePanel, IDrawArea
    {
        /// <summary>
        /// feature collection
        /// </summary>
        public FeatureCollection Features { get; private set; }

        /// <summary>
        /// the shared data COLD mode
        /// </summary>
        public DataColdBase DataCold { get; set; }

        /// <summary>
        /// the shared data HOT mode
        /// </summary>
        public DataHotBase DataHot { get; set; }

        /// <summary>
        /// parts collection
        /// </summary>
        public PartsCollection Parts { get; set; } = new PartsCollection();

        /// <summary>
        /// horizontal offset position of layout coodinates
        /// </summary>
        public double ScrollX { get; set; } = 0.0;

        /// <summary>
        /// vertical offset position of layout coodinates
        /// </summary>
        public double ScrollY { get; set; } = 0.0;

        /// <summary>
        /// horizontal zoom volume (1.0 = normal)
        /// </summary>
        public double ZoomX { get; set; } = 1.0;

        /// <summary>
        /// vertical zoom volume (1.0 = normal)
        /// </summary>
        public double ZoomY { get; set; } = 1.0;

        /// <summary>
        /// the canvas control to draw parts
        /// </summary>
        public CanvasControl Canvas { get; private set; } = null;

        /// <summary>
        /// remove requested features
        /// </summary>
        private readonly List<(FeatureBase, Exception)> _kills = new List<(FeatureBase, Exception)>();

        /// <summary>
        /// Z-Index of view
        /// </summary>
        private static readonly int _zindex_canvas = 1000;

        /// <summary>
        /// the constructor of this class
        /// </summary>
        public TGuiView()
        {
            DataContext = this;

            Features = new FeatureCollection(this);
            Features.CollectionChanged += Features_CollectionChanged;

            // Win2D canvas
            Canvas = new CanvasControl
            {
                Width = Width,
                Height = Height,
                Margin = new Thickness { Left = 0, Top = 0, Right = 0, Bottom = 0, },
                ClearColor = Colors.Transparent,
            };
            Canvas.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
            Canvas.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
            Canvas.SetValue(RelativePanel.AlignRightWithPanelProperty, true);
            Canvas.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
            Canvas.SetValue(Windows.UI.Xaml.Controls.Canvas.ZIndexProperty, _zindex_canvas);
            Canvas.CreateResources += onCanvasCreateResources;
            Canvas.Draw += onCanvasDraw;
            Children.Add(Canvas);

            SizeChanged += onSizeChanged;
            Loaded += onLoaded;
        }

        private ScreenRect _rect = ScreenRect.FromLTWH(0, 0, 0, 0);  // スレッドセーフなコントロール矩形

        /// <summary>
        /// view rectangle
        /// </summary>
        public ScreenRect Rect => _rect;

        /// <summary>
        /// initial process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onLoaded(object sender, RoutedEventArgs e)
        {
            // consider Z-Index because if transparent control is top, you cannot operate lower control
            var top = Window.Current.Content;
            var allControls = ControlUtil.DescendantsAndSelf(top);
            var cs = allControls.OfType<DependencyObject>();
            foreach (var c in cs)
            {
                if (c is TPane == false && c is TGuiView == false && c is Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl == false)
                {
                    var zi = (int)c.GetValue(Windows.UI.Xaml.Controls.Canvas.ZIndexProperty);
                    c.SetValue(Windows.UI.Xaml.Controls.Canvas.ZIndexProperty, _zindex_canvas + 1 + zi);
                }
            }

            // Poka-yoke
            DelayUtil.Start(TimeSpan.FromMilliseconds(3789), () =>
            {
                Canvas.Invalidate();
            });
        }
        private readonly Dictionary<string, IDrawArea> _panes = new Dictionary<string, IDrawArea>();

        /// <summary>
        /// additional assets folders
        /// </summary>
        public List<string> AdditionalAssetFolders { get; set; } = new List<string>();

        /// <summary>
        /// initial task executed before OnInitialInstance
        /// </summary>
        private void onPreInitialFeatures()
        {
            Parts.Assets = new GuiAssets(Canvas, AdditionalAssetFolders.ToArray());

            var allControls = ControlUtil.DescendantsAndSelf(this);
            var panes =
                from c in allControls.OfType<IDrawArea>()
                let c0 = c as FrameworkElement
                where c0 != null
                select (FrameworkElement)c;

            foreach (var pane in panes)
            {
                var da = (IDrawArea)pane;
                if (da.Name == null)
                {
                    da.Name = pane.Name;
                }
                if (pane.Name != null)
                {
                    _panes[pane.Name] = (IDrawArea)pane;
                }
            }
        }

        #region getFeatures --- Some type of the thread safe collection cache

        private IEnumerable<FeatureBase> featuresCopy1 = null;
        private IEnumerable<FeatureBase> featuresCopy2 = null;
        private IEnumerable<FeatureBase> featuresCopy3 = null;

        private void Features_CollectionChanged(object sender, EventArgs e)
        {
            featuresCopy1 = null;
            featuresCopy2 = null;
            featuresCopy3 = null;
        }

        /// <summary>
        /// feature collection cache for multi thread safe
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FeatureBase> getFeatures()
        {
            if (featuresCopy1 == null)
            {
                lock (Features)
                {
                    var fcs =
                        from fc in Features
                        select fc;
                    featuresCopy1 = fcs.ToArray();
                }
            }
            return featuresCopy1;
        }

        /// <summary>
        /// feature that have IPointerListener interface for multi thread safe
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FeatureBase> getPointerListenerFeatures()
        {
            if (featuresCopy2 == null)
            {
                lock (Features)
                {
                    var fcs =
                        from fc in Features
                        where fc is IPointerListener
                        select fc;
                    featuresCopy2 = fcs.ToArray();
                }
                PrepareEventCatchFilter();
            }
            return featuresCopy2;
        }

        /// <summary>
        /// feature that have IPointerWheelListener interface for multi thread safe
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FeatureBase> getPointerWheelListenerFeatures()
        {
            if (featuresCopy3 == null)
            {
                lock (Features)
                {
                    var fcs =
                        from fc in Features
                        where fc is IPointerWheelListener
                        select fc;
                    featuresCopy3 = fcs.ToArray();
                }
                PrepareEventCatchFilter();
            }
            return featuresCopy3;
        }
        #endregion

        private string _name = null;
        /// <summary>
        /// view control name cache for thread safe
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

        /// <summary>
        /// initialize event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void onCanvasCreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            if (ControlUtil.IsDesignMode())
            {
                return;
            }
            onPreInitialFeatures();

            Features.Findable = true;

            foreach (var fc in getFeatures())
            {
                fc.OnInitialInstance();
            }

            Features.Findable = false;

            InitPointer();  // TGuiView.Pointer.cs
            InitKeyboard(); // TGuiView.Keyboard.cs

            foreach (var fc in (from fc in getFeatures() where fc is IKeyListener select fc))
            {
                InitKeyboard(fc, (IKeyListener)fc);
            }

            initToken();    // TGuiView.Token.cs
        }

        /// <summary>
        /// feature remove request
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="ex"></param>
        /// <remarks>
        /// use this method when you cannot use IAutoRemovable because of another thread
        /// </remarks>
        public void Kill(FeatureBase feature, Exception ex)
        {
            lock (_kills)
            {
                _kills.Add((feature, ex));
            }
        }

        private void killproc()
        {

            List<(FeatureBase, Exception)> killnow;
            lock (_kills)
            {
                killnow = _kills.ToList();
                _kills.Clear();
            }
            if (killnow.Count > 0)
            {
                lock (Features)
                {
                    foreach (var (fc, ex) in killnow)
                    {
                        Features.Remove(fc);
                        LOG.AddException(ex);
                        LOG.WriteLine(LLV.ERR, $"Feature {fc.GetType().Name}[{(fc?.Name ?? "noname")}] ID={fc.ID.Value} is removed automatically.");
                    }
                }
            }
        }

        /// <summary>
        /// find pane by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDrawArea GetPane(string name)
        {
            if (_panes.TryGetValue(name, out var pane))
            {
                return pane;
            }
            throw new ArgumentOutOfRangeException($"TPane Name={name} は TGuiView Name={Name} から見つかりませんでした。");
        }

        /// <summary>
        /// background color
        /// </summary>
        public new Brush Background
        {
            get => base.Background;
            set
            {
                base.Background = value;
                if (value is SolidColorBrush sb)
                {
                    Canvas.ClearColor = sb.Color;
                }
            }
        }

        /// <summary>
        /// size change event task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DelayUtil.Start(TimeSpan.FromMilliseconds(300), () =>
            {
                var size = ScreenSize.From(ActualWidth, ActualHeight);
                _rect = ScreenRect.From(0, 0, size);
                Canvas.InvalidateArrange();
                Canvas.InvalidateMeasure();
                Canvas.Invalidate();

                AddToken(new SizeChangedEventTokenTrigger
                {
                    TokenID = TokensGeneral.SizeChanged,
                    Sender = this,
                    WindowSize = size,
                    Remarks = "Windows size changed event bridge",
                });
            });
        }

        /// <summary>
        /// pane collection order by z-index
        /// </summary>
        private IEnumerable<IDrawArea> _sortedPanes = null;
        private bool _isDrawing = false;

        /// <summary>
        /// canvas draw task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void onCanvasDraw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (_isDrawing)
            {
                return;
            }

            _isDrawing = true;
            if (_sortedPanes == null)
            {
                _sortedPanes = ControlUtil.FindDrawAreas(this);
            }
            foreach (var pane in _sortedPanes)
            {
                Parts.ProvideDraw(pane, sender, args.DrawingSession);
            }
            _isDrawing = false;
        }
    }
}
