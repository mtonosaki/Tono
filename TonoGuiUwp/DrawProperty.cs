// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// graphics drawing property
    /// </summary>
    public class DrawProperty
    {
        /// <summary>
        /// target TPane or TGuiView
        /// </summary>
        public IDrawArea Pane { get; set; }

        /// <summary>
        /// target canvas control
        /// </summary>
        public CanvasControl Canvas { get; set; }

        /// <summary>
        /// canvas drawing session such as drawing handler
        /// </summary>
        public virtual CanvasDrawingSession Graphics { get; set; }

        /// <summary>
        /// canvas size and position
        /// </summary>
        public ScreenRect PaneRect { get; set; }

        /// <summary>
        /// layer number (when it have group layer, top layer number will be returned)
        /// </summary>
        public NamedId Layer { get; set; }
    }

    /// <summary>
    /// double buffer draw property
    /// </summary>
    public class DrawPropertyDoubleBuffer : DrawProperty
    {
        public enum Statuses
        {
            Empty,
            Creating,
            Created,
        }

        /// <summary>
        /// render target object
        /// </summary>
        public CanvasRenderTarget RenderTarget { get; set; } = null;

        /// <summary>
        /// target canvas drawing session
        /// </summary>
        private CanvasDrawingSession _ds = null;

        /// <summary>
        /// buffer status
        /// </summary>
        public Statuses Status { get; set; } = Statuses.Empty;

        /// <summary>
        /// returns target DrawingSession
        /// </summary>
        public override CanvasDrawingSession Graphics
        {
            get
            {
                if (_ds == null)
                {
                    _ds = RenderTarget.CreateDrawingSession();
                    //ds.DrawImage(offscreen);
                }
                return _ds;
            }
            set => throw new NotSupportedException();
        }

        public void ClearDrawingSession()
        {
            _ds?.Dispose();
            _ds = null;
        }

        /// <summary>
        /// Request to rebuild buffer image
        /// </summary>
        public void ClearBuffer(bool isSizeChanged)
        {
            Status = Statuses.Empty;
            if (isSizeChanged)
            {
                RenderTarget = null;
            }
        }
    }
}
