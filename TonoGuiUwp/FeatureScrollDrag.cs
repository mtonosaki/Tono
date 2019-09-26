namespace Tono.Gui.Uwp
{
    /// <summary>
    /// screen scroll support with mouse drag operation
    /// screen scroll support with swipe screen operation
    /// </summary>
    [FeatureDescription(En = "Scroll Swipe or mouse drag", Jp = "マウスドラッグ or スワイプで画面をスクロール")]
    public class FeatureScrollDrag : FeatureBase, IPointerListener
    {
        /// <summary>
        /// non operation margin size at bottm edge
        /// </summary>
        public double MarginBottom { get; set; } = 0;

        private ScreenPos _sPosDown;
        private ScreenPos _sZoomDown;
        private ScreenPos _lScrollDown;

        /// <summary>
        /// operating status
        /// </summary>
        protected virtual bool isScrolling { get; set; } = false;

        public override void OnInitialInstance()
        {
            Pane.Target = Pane.Main;
        }

        /// <summary>
        /// trigger key combination
        /// </summary>
        /// <param name="po"></param>
        /// <returns></returns>
        protected virtual bool isTrigger(PointerState po)
        {
            if (Pane.Target == null)
            {
                return false;
            }
            switch (po.DeviceType)
            {
                case PointerState.DeviceTypes.Mouse:
                    return po.IsInContact && po.IsKeyControl && po.IsKeyShift;      // Mouse : [CTRL]+[SHIFT] + Drag
                case PointerState.DeviceTypes.Touch:
                    return po.IsInContact && po.Scale == 1 && po.Rotation.IsZero(); // Swipe when NOT rotated and NOT pinched
                default:
                    return false;
            }
        }

        public void OnPointerPressed(PointerState po)
        {
            if (isTrigger(po))
            {
                if (po.Position.Y < Pane.Target.Rect.RB.Y - MarginBottom)
                {
                    _sPosDown = po.Position;
                    _lScrollDown = ScreenPos.From(Pane.Target.ScrollX, Pane.Target.ScrollY);
                    _sZoomDown = ScreenPos.From(Pane.Target.ZoomX, Pane.Target.ZoomY);
                    isScrolling = true;
                }
            }
        }
        public void OnPointerHold(PointerState po)
        {
        }

        protected virtual void onScrolled()
        {
        }

        public void OnPointerMoved(PointerState po)
        {
            if (isScrolling)
            {
                if (isTrigger(po))
                {
                    var sval = (po.Position - _sPosDown) / _sZoomDown;
                    if (sval.Height != 0 || sval.Width != 0)
                    {
                        var newpos = _lScrollDown + sval;
                        Pane.Target.ScrollX = newpos.X.Sx;
                        Pane.Target.ScrollY = newpos.Y.Sy;
                        Token.Link(po, new EventTokenPaneChanged
                        {
                            TokenID = TokensGeneral.Scrolled,
                            Sender = this,
                            Remarks = "fcDragScrollでのスクロール",
                            TargetPane = Pane.Target,
                        });
                        onScrolled();
                        Redraw();
                    }
                }
                else
                {
                    OnPointerReleased(po);
                }
            }
        }

        public void OnPointerReleased(PointerState po)
        {
            if (isScrolling)
            {
                isScrolling = false;
                Token.Link(po, new EventTokenPaneChanged
                {
                    TokenID = TokensGeneral.Scrolled,
                    Sender = this,
                    Remarks = "Finish Scroll",
                    TargetPane = Pane.Target,
                });
                onScrolled();
                Redraw();
            }
        }
    }
}
