// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Change screen zoom volume with UWP control event set at ListeningButtonNames property.
    /// </summary>
    /// <example>
    /// XAML
    /// ＜Button Name="ZOOMINBUTTON" /＞
    /// ＜tw:FeatureZoomButton ListeningButtonNames="ZOOMINBUTTON" ZoomTimes="1.25" /＞
    /// </example>
    [FeatureDescription(En = "Zoom screen by token", Jp = "画面ズーム（トークン起動）")]
    public class FeatureZoomButton : FeatureBase
    {
        /// <summary>
        /// zoom offset value
        /// </summary>
        public double ZoomPlus { get; set; } = 0;

        /// <summary>
        /// zoom rate value
        /// </summary>
        public double ZoomTimes { get; set; } = 1.0;

        /// <summary>
        /// when use this value, this feature set specific value to ZoomX
        /// </summary>
        public double ZoomXSet { get; set; } = double.NaN;

        /// <summary>
        /// when use this value, this feature set specific value to ZoomY
        /// </summary>
        public double ZoomYSet { get; set; } = double.NaN;

        public override void OnInitialInstance()
        {
            Pane.Target = Pane.Main;
        }

        /// <summary>
        /// UWP control click event set at ListeningButtonNames property
        /// </summary>
        /// <param name="token"></param>
        [EventCatch]
        public void Start(EventTokenButton token)
        {
            zoomWithProperty(token);
        }

        private void zoomWithProperty(EventTokenButton ta)
        {
            var zx = MathUtil.Trim(Pane.Target.ZoomX * ZoomTimes + ZoomPlus, 500, 100000);
            var zy = MathUtil.Trim(Pane.Target.ZoomY * ZoomTimes + ZoomPlus, 500, 100000);
            if (double.IsNaN(ZoomXSet) == false)
            {
                zx = MathUtil.Trim(ZoomXSet, 500, 100000);
            }
            if (double.IsNaN(ZoomYSet) == false)
            {
                zy = MathUtil.Trim(ZoomYSet, 500, 100000);
            }
            Pane.Target.ZoomX = zx;
            Pane.Target.ZoomY = zy;
            Token.Link(ta, new EventTokenPaneChanged
            {
                TokenID = TokensGeneral.Zoomed,
                Sender = this,
                Remarks = "button zoomed",
                TargetPane = Pane.Target,
            });
            Redraw();
        }
    }
}
