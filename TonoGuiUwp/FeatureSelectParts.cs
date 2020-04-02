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
    public class FeatureSelectParts : FeatureBase, IPointerListener
    {
        /// <summary>
        /// Token send mode : true = at every mouse event, false = at state changed
        /// </summary>
        public bool ForceTokenWhenOn { get; set; } = false;

        /// <summary>
        /// target parts layere
        /// </summary>
        public NamedId TargetLayer { get; set; } = NamedId.Nothing;

        public NamedId[] TargetLayers { get; set; }

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

            // Make TargetLayers
            var ls = new Dictionary<NamedId, NamedId>
            {
                [TargetLayer] = TargetLayer
            };
            foreach (var l in TargetLayers ?? Array.Empty<NamedId>())
            {
                ls[l] = l;
            }
            ls.Remove(NamedId.Nothing);
            TargetLayers = ls.Keys.ToArray();
            TargetLayer = null;

        }

        protected virtual bool isTrigger(PointerState po)
        {
            switch (po.DeviceType)
            {
                case PointerState.DeviceTypes.Mouse:
                    return po.IsInContact && po.IsKeyControl == false;
                case PointerState.DeviceTypes.Touch:
                    return po.IsInContact && po.Scale == 1 && po.Rotation.IsZero();
                default:
                    return false;
            }
        }

        private bool isTriggered = false;

        public void OnPointerPressed(PointerState po)
        {
            isTriggered = isTrigger(po);
        }

        public void OnPointerHold(PointerState po)
        {
        }

        public void OnPointerMoved(PointerState po)
        {
            if (isTriggered && po.PositionDelta.Length > 4)
            {
                isTriggered = false;
            }
        }

        public void OnPointerReleased(PointerState po)
        {
            if (isTriggered == false) { return; }

            isTriggered = false;
            IEnumerable<(ISelectableParts parts, bool sw)> selchanges = new (ISelectableParts, bool)[] { };
            var pts = Parts.GetParts(po.Position, Pane.Target, TargetLayers);

            if (po.DeviceType != PointerState.DeviceTypes.Mouse || (po.DeviceType == PointerState.DeviceTypes.Mouse && po.IsKeyShift == false))
            {
                var offs =
                    from pt in Parts.GetParts(TargetLayers)
                    let sp = pt as ISelectableParts
                    where sp != null
                    where sp.IsSelected
                    where Collection<ISelectableParts>.Contains(pts, sp) == false
                    select (sp, false);
                selchanges = selchanges.Concat(offs);
            }

            var pt1 = pts.FirstOrDefault();
            if (pt1 != null)
            {
                if ((po.DeviceType == PointerState.DeviceTypes.Mouse && po.IsKeyShift == false))
                {
                    // When not in "multi selecting mode", not support toggle state (set SELECT only)
                    // マルチ選択モードでない時は、何度パーツを選んでも「選択＝ON」になる
                    var sels =
                        from p in pts
                        where ForceTokenWhenOn || p.IsSelected == false
                        select (p, true);
                    selchanges = selchanges.Concat(sels);
                }
                else
                {
                    // When "multi select mode" ([Shift]+[Click]), toggle selecting state
                    // マルチ選択モードの時は、選択状態を反転させる
                    var sels =
                        from p in pts
                        where ForceTokenWhenOn || p.IsSelected == pt1.IsSelected
                        select (p, !pt1.IsSelected);
                    selchanges = selchanges.Concat(sels);
                }
            }
            var selchangesA = selchanges.ToArray();

            foreach (var (parts, sw) in selchangesA)
            {
                parts.IsSelected = sw;
            }
            if (selchangesA.Length > 0)
            {
                Token.Link(po, new EventTokenPartsSelectChangedTrigger
                {
                    TokenID = TokensGeneral.PartsSelectChanged,
                    PartStates = selchangesA,
                    Sender = this,
                    Remarks = "parts select state changed",
                });
                Redraw();
            }
        }
    }
}
