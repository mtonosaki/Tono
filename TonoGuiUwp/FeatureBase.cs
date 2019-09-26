using System;
using System.Diagnostics;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// MVFP : FEATURE base class
    /// </summary>
    public abstract partial class FeatureBase
    {
        /// <summary>
        /// the constructor of the all features
        /// </summary>
        public FeatureBase()
        {
            Pane = new PaneTray(this);
            Token = new TokenTray(this);
            Status = new StatusTray(this);
        }

        /// <summary>
        /// target pane
        /// </summary>
        public PaneTray Pane { get; private set; }

        /// <summary>
        /// event token tray to throw your original token
        /// </summary>
        public TokenTray Token { get; private set; }

        /// <summary>
        /// shared status manager
        /// </summary>
        public StatusTray Status { get; private set; }

        /// <summary>
        /// feature id (auto numbering)
        /// </summary>
        public Id ID { get; set; } = Id.Nothing;

        /// <summary>
        /// true=runable by event trigger
        /// </summary>
        public virtual bool IsEnabled { get; set; } = true;

        /// <summary>
        /// feature name for human
        /// </summary>
        public virtual string Name { get; set; } = $"(n/a)";

        /// <summary>
        /// UWP control names(comma separation) for auto trigger (EventTokenButton)
        /// </summary>
        public virtual string ListeningButtonNames { get; set; } = string.Empty;

        /// <summary>
        /// owner TGuiView
        /// </summary>
        public TGuiView View { get; internal set; }

        /// <summary>
        /// sharing data system (hot data)
        /// </summary>
        public DataHotBase DataHot => View.DataHot;

        /// <summary>
        /// sharing data system (cold data)
        /// </summary>
        public DataColdBase DataCold => View.DataCold;

        /// <summary>
        /// horizontal scroll value
        /// </summary>
        public LayoutX ScrollX => new LayoutX { Lx = (float)View.ScrollX };

        /// <summary>
        /// vertical scroll value
        /// </summary>
        public LayoutY ScrollY => new LayoutY { Ly = (float)View.ScrollY };

        /// <summary>
        /// horizontal zoom volume (1.0 = normal)
        /// </summary>
        public float ZoomX { get => (float)View.ZoomX; set => View.ZoomX = value; }

        /// <summary>
        /// vertical zoom volume (1.0 = normal)
        /// </summary>
        public float ZoomY { get => (float)View.ZoomY; set => View.ZoomY = value; }

        /// <summary>
        /// parts sharing system
        /// </summary>
        public PartsCollection Parts => View.Parts;

        public override bool Equals(object obj)
        {
            if (obj is FeatureBase fc)
            {
                Debug.Assert(ID.IsNothing() == false);
                return ID.Equals(fc.ID);
            }
            return false;
        }

        public override int GetHashCode()
        {
            Debug.Assert(ID.IsNothing() == false);
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name}, {ID}, Name={Name}";
        }

        /// <summary>
        /// feature initialize event (auto execute from TGuiView)
        /// </summary>
        public virtual void OnInitialInstance()
        {
        }

        /// <summary>
        /// run action on main thread
        /// </summary>
        /// <param name="func"></param>
        /// <remarks>
        /// TS(async () => 
        ///	{
        ///		await ...
        ///	});
        /// </remarks>
        protected void TS(Action func)
        {
            var ra = View.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => func());
        }

        /// <summary>
        /// remove self feature request
        /// </summary>
        /// <param name="ex"></param>
        /// <remarks>
        /// IAutoRemovableが効かない、TS（スレッドセーフ）でのディスパッチ処理内で使用する
        /// </remarks>
        protected void Kill(Exception ex)
        {
            View.Kill(this, ex);
        }

        /// <summary>
        /// redraw request
        /// </summary>
        public void Redraw(bool forceSw = false)
        {
            DataHot.IsRedrawRequested = true;
            if (forceSw)
            {
                View.Canvas.Invalidate();
            }
        }
    }
}
