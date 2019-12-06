using Microsoft.Graphics.Canvas.Brushes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    public class FeaturePartsSelectOnRect : FeatureBase, IPointerListener, IKeyListener
    {
        private class PartsMask : PartsRectangleBase<ScreenX, ScreenY, ScreenX, ScreenY>
        {
            /// <summary>
            /// Visible Flag
            /// </summary>
            public bool Visible { get; set; }

            /// <summary>
            /// Masking rectangle color
            /// </summary>
            public Color MaskBG { get; set; } = Color.FromArgb(32, 0, 255, 0);

            /// <summary>
            /// Masking rectangle border color
            /// </summary>
            public Color MaskPen { get; set; } = Color.FromArgb(128, 0, 255, 0);

            /// <summary>
            /// The constructor
            /// </summary>
            public PartsMask()
            {
                Location = CodePos<ScreenX, ScreenY>.From(ScreenX.From(0), ScreenY.From(0));
                RB = CodePos<ScreenX, ScreenY>.From(ScreenX.From(0), ScreenY.From(0));
            }

            public override void Draw(DrawProperty dp)
            {
                if (Visible)
                {
                    var sr = ScreenRect.FromLTRB(Left.Cx, Top.Cy, Right.Cx, Bottom.Cy);
                    dp.Graphics.FillRectangle(_(sr), MaskBG);
                    dp.Graphics.FillRectangle(_(sr), MaskPen);
                }
            }
        }

        /// <summary>
        /// target pane name
        /// </summary>
        public string TargetPaneName { get; set; } = null;

        /// <summary>
        /// Mask visual drawing layer
        /// </summary>
        public NamedId MaskLayer { get; set; } = NamedId.From("MaskLayerDefault", Id.From(99999));

        /// <summary>
        /// Select target parts layer (Mandatory)
        /// </summary>
        public NamedId TargetLayer { get; set; } = NamedId.Nothing;

        /// <summary>
        /// Lazy evaluation to select parts instance in TargetLayer
        /// </summary>
        public Func<ISelectableParts, bool> PartsFilter { get; set; } = (a => true);

        public IEnumerable<KeyListenSetting> KeyListenSettings => _keys;

        private static readonly KeyListenSetting[] _keys = new KeyListenSetting[]
        {
            new KeyListenSetting // [0]
			{
                Name = "ShiftModeON",
                KeyStates = new[]
                {
                    (VirtualKey.Shift, KeyListenSetting.States.Down),
                },
            },
            new KeyListenSetting // [1]
			{
                Name = "ShiftModeOFF",
                KeyStates = new[]
                {
                    (VirtualKey.Shift, KeyListenSetting.States.Up),
                },
            },
        };

        private PartsMask Mask = null;

        public override void OnInitialInstance()
        {
            base.OnInitialInstance();

            Status["IsEnableSelectingBox"].AddBooleanValues(true);  // Prepare runtime feature enable control

            Pane.Target = TargetPaneName == null ? Pane.Main : Pane[TargetPaneName];

            Mask = new PartsMask
            {
                CoderL = CodeLayout.CoderSx,
                CoderR = CodeLayout.CoderSx,
                CoderT = CodeLayout.CoderSy,
                CoderB = CodeLayout.CoderSy,
                PositionerL = CodeLayout.PositionerSx,
                PositionerR = CodeLayout.PositionerSx,
                PositionerT = CodeLayout.PositionerSy,
                PositionerB = CodeLayout.PositionerSy,
                Visible = false,
            };
            Parts.Add(Pane.Target, Mask, MaskLayer);
        }

        private Dictionary<ISelectableParts, bool> FirstState = new Dictionary<ISelectableParts, bool>();
        private bool IsSelectingBox = false;

        private bool IsTrigger(PointerState po)
        {
            return po.IsInContact
                && !po.IsKeyControl
                && !po.IsKeyMenu
                && !po.IsKeyWindows;
        }

        public void OnPointerPressed(PointerState po)
        {
            if (IsTrigger(po) == false) return;
            if (Status["IsEnableSelectingBox"].ValueB == false) return;

            FirstState.Clear();
            foreach (var pt in Parts.GetParts(TargetLayer, PartsFilter))
            {
                if (pt.SelectingScore(Pane.Target, po.Position) <= 1.0f)
                {
                    Mask.Visible = false;
                    return;
                }
                FirstState[pt] = pt.IsSelected;
            }
            IsSelectingBox = true;
        }

        public void OnPointerHold(PointerState po)
        {
        }

        PointerState lastPo = null;
        public void OnPointerMoved(PointerState po)
        {
            lastPo = po.Clone();
            if (IsSelectingBox)
            {
                if (IsTrigger(po) == false)
                {
                    OnPointerReleased(null);
                    return;
                }
                var sr = ScreenRect.FromLTRB(po.PositionOrigin, po.Position);
                sr.Normalize();
                Mask.Left = CodeX<ScreenX>.From(sr.L);
                Mask.Top = CodeY<ScreenY>.From(sr.T);
                Mask.Right = CodeX<ScreenX>.From(sr.R);
                Mask.Bottom = CodeY<ScreenY>.From(sr.B);
                Mask.Visible = true;

                foreach (var pt in Parts.GetParts(TargetLayer, PartsFilter))
                {
                    var sw = pt.IsIn(Pane.Target, sr);
                    if (po.IsKeyShift == false)
                    {
                        pt.IsSelected = sw;
                    }
                    else
                    {
                        if (sw)
                        {
                            pt.IsSelected = !FirstState[pt];
                        }
                        else
                        {
                            pt.IsSelected = FirstState[pt];
                        }
                    }
                }
                Redraw();
            }
        }
        public void OnKey(KeyEventToken kt)
        {
            lastPo.IsKeyShift = kt.Setting.Name.EndsWith("ON");
            OnPointerMoved(lastPo);
        }


        public void OnPointerReleased(PointerState po)
        {
            Mask.Visible = false;
            IsSelectingBox = false;
            Redraw();
        }
    }
}
