// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
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
        public NamedId TargetLayer { get; set; } = NamedId.Nothing;

        public NamedId[] TargetLayers { get; set; }


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
            DataHot.IsPartsMoving[this] = false;
            var selcheck = Parts.GetParts(po.Position, Pane.Target, TargetLayers, p => p is IMovableParts).ToArray();
            foreach (var pt in selcheck)
            {
                if (pt.IsSelected)
                {
                    DataHot.IsPartsMoving[this] = true;
                    break;
                }
            }
            if (DataHot.IsPartsMoving.GetValueOrDefault(this))
            {
                var pts = Parts.GetParts(TargetLayers, p =>
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
            if (DataHot.IsPartsMoving.GetValueOrDefault(this) == false || moving is null)
            {
                return;
            }

            if (isTrigger(po) == false)
            {
                OnPointerReleased(po);
                return;
            }
            var tokenParts = new List<IMovableParts>();
            foreach (var pt in moving)
            {
                if (po.PositionDelta.Width != 0 || po.PositionDelta.Height != 0)
                {
                    pt.Move(Pane.Target, po.PositionDelta);
                    tokenParts.Add(pt);
                }
            }
            if (tokenParts.Count > 0)
            {
                Token.Link(po, new EventTokenPartsMovingTrigger
                {
                    TokenID = TokensGeneral.PartsMoving,
                    PartsSet = tokenParts,
                    Sender = this,
                    Remarks = "PartsMoving@FeatureMoveSelectedParts",
                });
                Redraw();
            }
        }

        public void OnPointerReleased(PointerState po)
        {
            DataHot.IsPartsMoving[this] = false;
            if (moving?.Count() > 0)
            {
                var moved = (from pt in moving where pt.IsMoved() select pt).ToArray();
                if (moved.Length > 0)
                {
                    Token.Link(po, new EventTokenPartsMovedTrigger
                    {
                        TokenID = TokensGeneral.PartsMoved,
                        PartsSet = moved,
                        Sender = this,
                        Remarks = "FinishedMoveParts@FeatureMoveSelectedParts",
                    });
                }
                moving = null;
            }
        }
    }
}
