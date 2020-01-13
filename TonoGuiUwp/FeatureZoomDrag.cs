// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Diagnostics;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// mouse drag zoom support (*1)
    /// </summary>
    /// <remarks>
    /// *1
    /// An implementation of JP2007264807A
    /// </remarks>
    /// <seealso cref="https://patents.google.com/patent/JP2007264807A/en?oq=JP2007264807"/>
    [FeatureDescription(En = "Zoom mouse drag", Jp = "マウスドラッグ or スワイプで画面をズーム")]
    public class FeatureZoomDrag : FeatureBase, IPointerListener
    {
        /// <summary>
        /// horizontal zoom speed
        /// </summary>
        public double SpeedX { get; set; } = 1.0;

        /// <summary>
        /// vertical zoom speed
        /// </summary>
        public double SpeedY { get; set; } = 1.0;

        public override void OnInitialInstance()
        {
            Pane.Target = Pane.Main;
        }

        /// <summary>
        /// zoom start key set
        /// </summary>
        /// <param name="po"></param>
        /// <returns></returns>
        private PointerState.DeviceTypes getTrigger(PointerState po)
        {
            switch (po.DeviceType)
            {
                case PointerState.DeviceTypes.Mouse:
                    return po.IsInContact && po.IsKeyControl && po.IsKeyShift == false ? PointerState.DeviceTypes.Mouse : PointerState.DeviceTypes.None;
                case PointerState.DeviceTypes.Touch:
                    return po.Scale != 1 ? PointerState.DeviceTypes.Touch : PointerState.DeviceTypes.None;
                default:
                    return PointerState.DeviceTypes.None;
            }
        }

        private ScreenPos _sZoomDown;  // Zoomをスクリーン座標にしているのは、スクリーン座標の移動量がズーム倍率になるため
        private ScreenPos _sPosDown;
        private LayoutPos _lPosDown;
        private LayoutPos _lScrollDown;

        protected virtual bool isZooming { get; set; } = false;

        public void OnPointerPressed(PointerState po)
        {
            //Debug.WriteLine($"★OnPointerPressed {po.Position} finger={po.FingerCount} Scale={po.Scale}");
            if (getTrigger(po) == PointerState.DeviceTypes.Mouse)
            {
                if (isZooming == false)
                {
                    _sPosDown = po.Position;
                    _lPosDown = LayoutPos.From(Pane.Target, po.Position);
                    _sZoomDown = ScreenPos.From(Pane.Target.ZoomX, Pane.Target.ZoomY);
                    _lScrollDown = LayoutPos.From(Pane.Target.ScrollX, Pane.Target.ScrollY);
                    isZooming = true;
                }
            }
        }
        public void OnPointerHold(PointerState po)
        {
        }

        protected virtual void onZoomed()
        {
        }

        public void OnPointerMoved(PointerState po)
        {
            //Debug.WriteLine($"★OnPointerMoved {po.Position} finger={po.FingerCount} Scale={po.Scale}");
            if (po.Scale != 1.0f || isZooming)
            {
                if (isZooming == false)
                {
                    _sPosDown = po.Position;
                    _lPosDown = LayoutPos.From(Pane.Target, po.Position);
                    _sZoomDown = ScreenPos.From(Pane.Target.ZoomX, Pane.Target.ZoomY);
                    _lScrollDown = LayoutPos.From(Pane.Target.ScrollX, Pane.Target.ScrollY);
                    isZooming = true;
                }
                ScreenPos sZoom;
                var lScroll = _lScrollDown;
                var sMove = po.Position - _sPosDown;    // pointer move distance
                switch (getTrigger(po))
                {
                    case PointerState.DeviceTypes.Mouse:
                        sZoom = _sZoomDown + sMove * (0.003 * SpeedX, 0.003 * SpeedY); // ズーム値の算出  速度変更(高解像度に伴い) 2016.11.15 Tono 2→3
                        break;

                    case PointerState.DeviceTypes.Touch:
                        sZoom = _sZoomDown * po.Scale;
                        break;
                    default:
                        OnPointerReleased(po);
                        return;
                }

                // sZoom.TrimMinimum(ScreenX.From(500), ScreenY.From(500));
                sZoom.TrimMinimum(ScreenX.From(0.2), ScreenY.From(0.2));  // 小さい値に対応
                sZoom.TrimMaximum(ScreenX.From(100000), ScreenY.From(100000));
                var isChanged = false;

                if (Pane.Target.ZoomX != sZoom.X || Pane.Target.ZoomY != sZoom.Y)
                {
                    Pane.Target.ZoomX = sZoom.X;
                    Pane.Target.ZoomY = sZoom.Y;
                    Token.Link(po, new EventTokenPaneChanged
                    {
                        TokenID = TokensGeneral.Zoomed,
                        Sender = this,
                        Remarks = "drag zoom",
                        TargetPane = Pane.Target,
                    });
                    isChanged = true;
                }
                //if (Pane.Target.ScrollX != lScroll.X.Lx || Pane.Target.ScrollY != lScroll.Y.Ly)
                //{
                //    Pane.Target.ScrollX = lScroll.X.Lx;
                //    Pane.Target.ScrollY = lScroll.Y.Ly;
                //    Token.Link(po, new EventTokenPaneChanged
                //    {
                //        TokenID = TokensGeneral.Scrolled,
                //        Sender = this,
                //        Remarks = "auto scroll when drag zoom",
                //        TargetPane = Pane.Target,
                //    });
                //    isChanged = true;
                //}
                if (isChanged)
                {
                    onZoomed();
                    Redraw();
                }
            }
        }
        public void OnPointerReleased(PointerState po)
        {
            //Debug.WriteLine($"★OnPointerReleased {po.Position} finger={po.FingerCount} Scale={po.Scale}");
            isZooming = false;
        }
    }
}
