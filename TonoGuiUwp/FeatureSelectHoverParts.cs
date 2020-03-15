// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Parts select support
    /// </summary>
    [FeatureDescription(En = "Select parts with mouse click or touch", Jp = "クリック/タッチによるパーツ選択")]
    public class FeatureSelectHoverParts : FeatureBase, IPointerListener
    {
        /// <summary>
        /// Token send mode : true = at every mouse event, false = at state changed
        /// </summary>
        public bool ForceTokenWhenOn { get; set; } = false;

        /// <summary>
        /// target parts layere
        /// </summary>
        public NamedId TargetLayer { get; set; }

        /// <summary>
        /// target pane name
        /// </summary>
        public string TargetPaneName { get; set; } = null;

        public override void OnInitialInstance()
        {
            if (TargetPaneName != null)
            {
                Pane.Target = Pane[TargetPaneName];
            }
            else
            {
                Pane.Target = Pane.Main;
            }
        }

        protected virtual bool isTrigger(PointerState po)
        {
            switch (po.DeviceType)
            {
                case PointerState.DeviceTypes.Mouse:
                    return po.IsKeyControl == false && po.IsKeyShift == false;
                case PointerState.DeviceTypes.Touch:
                    return false;
                default:
                    return false;
            }
        }

        public void OnPointerPressed(PointerState po)
        {
        }

        public void OnPointerHold(PointerState po)
        {
        }

        public void OnPointerMoved(PointerState po)
        {
            if (isTrigger(po))
            {
                var pts =
                    from sp in Parts.GetParts<ISelectableParts>(TargetLayer)
                    let score = sp.SelectingScore(Pane.Target, po.Position)
                    where score >= 0.0f
                    orderby score
                    select sp;

                foreach (var sp in LoopUtil<ISelectableParts>.From(pts, out var lu))
                {
                    lu.DoFirstTime(() =>
                    {
                        var score = sp.SelectingScore(Pane.Target, po.Position);
                        sp.IsSelected = score <= 1.0f;
                    });
                    lu.DoSecondTimesAndSubsequent(() =>
                    {
                        sp.IsSelected = false;
                    });
                }
                Redraw();
            }
        }

        public void OnPointerReleased(PointerState po)
        {
        }
    }
}
