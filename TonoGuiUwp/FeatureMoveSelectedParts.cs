// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Feature parts move support
    /// </summary>
    [FeatureDescription(En = "Move selected parts", Jp = "選択されているパーツを移動")]
    public class FeatureMoveSelectedParts : FeatureBase, IPointerListener
    {
        /// <summary>
        /// target parts layer
        /// </summary>
        public NamedId TargetLayer { get; set; }

        /// <summary>
        /// target pane name
        /// </summary>
        public string TargetPaneName { get; set; } = null;

        /// <summary>
        /// initialize feature
        /// </summary>
        /// <returns></returns>
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
                    return po.IsInContact && po.IsKeyControl == false && po.IsKeyShift == false;
                case PointerState.DeviceTypes.Touch:
                    return po.IsInContact && po.Scale == 1 && po.Rotation.IsZero();
                default:
                    return false;
            }
        }

        public void OnPointerHold(PointerState po)
        {
        }

        private IEnumerable<IMovableParts> moving = null;

        public void OnPointerPressed(PointerState po)
        {
            if (isTrigger(po) == false)
            {
                return;
            }
            DataHot.IsPartsMoving = false;
            var selcheck = Parts.GetParts(po.Position, Pane.Target, TargetLayer, p => p is IMovableParts).ToArray();
            foreach (var pt in selcheck)
            {
                if (pt.IsSelected)
                {
                    DataHot.IsPartsMoving = true;
                    break;
                }
            }
            if (DataHot.IsPartsMoving)
            {
                var pts = Parts.GetParts(TargetLayer, p =>
                {
                    if (p is IMovableParts)
                    {
                        if (p is ISelectableParts sp)
                        {
                            if (sp.IsSelected)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                });
                var mos = new List<IMovableParts>();
                foreach (IMovableParts pt in pts)
                {
                    pt.SaveLocationAsOrigin();
                    mos.Add(pt);
                }
                moving = mos;
            }
        }

        public void OnPointerMoved(PointerState po)
        {
            if (DataHot.IsPartsMoving == false)
            {
                return;
            }

            if (isTrigger(po) == false)
            {
                OnPointerReleased(po);
                return;
            }
            var nMoved = 0;
            foreach (var pt in moving)
            {
                if (po.PositionDelta.Width != 0 || po.PositionDelta.Height != 0)
                {
                    pt.Move(Pane.Target, po.PositionDelta);
                    nMoved++;
                }
            }
            if (nMoved > 0)
            {
                Redraw();
            }
        }

        public void OnPointerReleased(PointerState po)
        {
            DataHot.IsPartsMoving = false;
            if (moving?.Count() > 0)
            {
                var moved = (from pt in moving where pt.IsMoved() select pt).ToArray();
                if (moved.Length > 0)
                {
                    Token.Link(po, new EventTokenPartsMovedTrigger
                    {
                        TokenID = TokensGeneral.PartsMoved,
                        Parts = moved,
                        Sender = this,
                        Remarks = "FinishMoveParts@fcMoveSelectedParts",
                    });
                }
                moving = null;
            }
        }
    }
}
