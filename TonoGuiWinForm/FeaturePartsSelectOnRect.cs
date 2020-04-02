// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 指定矩形に入り込んだパーツを選択状態にする
    /// </summary>
    public class FeaturePartsSelectOnRect : FeatureControlBridgeBase, IMouseListener, IPartsIllusionable
#if DEBUG == false
, IAutoRemovable
#endif
    {
        private class dpMask : PartsRectangle, IPartsVisible
        {
            private static readonly Brush _maskBG = new SolidBrush(Color.FromArgb(48, 0, 255, 0));
            private static readonly Pen _maskPen = new Pen(Color.FromArgb(128, 0, 255, 0));

            /// <summary>
            /// 選択領域の矩形を描画する
            /// </summary>
            /// <param name="rp"></param>
            /// <returns></returns>
            public override bool Draw(IRichPane rp)
            {
                if (_isVisible)
                {
                    var spos = GetScRect(rp);
                    if (isInClip(rp, spos) == false)    // 描画不要であれば、なにもしない
                    {
                        return false;
                    }
                    rp.Graphics.FillRectangle(_maskBG, spos);
                    rp.Graphics.DrawRectangle(_maskPen, spos);
                }
                return true;
            }

            #region IPartsVisible メンバ
            private bool _isVisible = true;
            public bool Visible
            {
                get => _isVisible;
                set => _isVisible = value;
            }

            #endregion
        }

        [NonSerialized]
        private dpMask _mask = null;
        [NonSerialized]
        private IRichPane _tarPane = null;

        /// <summary>選択中のパーツ（共有変数）</summary>
        private PartsCollectionBase _selectedParts;

        /// <summary>
        /// コーダー
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private CodeRect _coder(LayoutRect rect, PartsBase target)
        {
            _tarPane.Convert(ScreenRect.FromLTRB(rect.LT.X, rect.LT.Y, rect.RB.X, rect.RB.Y));
            return CodeRect.FromLTRB(rect.LT.X, rect.LT.Y, rect.RB.X, rect.RB.Y);
        }

        /// <summary>
        /// ポジショナー
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private LayoutRect _positioner(CodeRect rect, PartsBase target)
        {
            var ret = _tarPane.Convert(ScreenRect.FromLTRB(rect.LT.X, rect.LT.Y, rect.RB.X, rect.RB.Y));
            return ret;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // ステータス同期
            _selectedParts = (PartsCollectionBase)Share.Get("SelectedParts", typeof(PartsCollection));
            _tarPane = Pane.GetPane("Resource");

            // 矩形描画用パーツを生成する
            _mask = new dpMask
            {
                Rect = CodeRect.FromLTWH(int.MinValue / 2, int.MinValue / 2, 0, 0),
                PartsPositionCorder = new PartsBase.PosCoderMethod(_coder),
                PartsPositioner = new PartsBase.PositionerMethod(_positioner),
                Visible = false
            };
            Parts.Add(_tarPane, _mask, Const.Layer.StaticLayers.MaskRect);
        }

        /// <summary>
        /// パーツ一覧をフィルターする（オーバーライドしないと全パーツとなる）
        /// </summary>
        /// <returns></returns>
        protected virtual ICollection<PartsBase> GetFilteredParts()
        {
            var parts = new List<PartsBase>();
            foreach (PartsCollection.PartsEntry pe in Parts)
            {
                if (pe.Pane.IdText == _tarPane.IdText)
                {
                    parts.Add(pe.Parts);
                }
            }
            return parts;
        }

        private ScreenPos _startPos = null;
        private readonly List<IPartsSelectable> _shiftAdd = new List<IPartsSelectable>();

        /// <summary>
        /// 全 BarTripの選択解除
        /// </summary>
        private void resetSelect(bool isInvaliadte)
        {
            _selectedParts.Clear();
            foreach (var pts in GetFilteredParts())
            {
                if (pts is IPartsSelectable part && part is PartsBase)
                {
                    if (part.IsSelected)
                    {
                        part.IsSelected = false;
                        if (isInvaliadte)
                        {
                            Parts.Invalidate((PartsBase)part, _tarPane);
                        }
                    }
                }
            }
        }


        #region IMouseListener メンバ

        public void OnMouseMove(MouseState e)
        {
            if (_mask.Visible)
            {
                if (e.Attr.IsButton == false)
                {
                    OnMouseUp(e);
                    return;
                }
                _mask.Rect = CodeRect.FromLTRB(_startPos.X, _startPos.Y, e.Pos.X, e.Pos.Y);
                _mask.Rect.Normalize();

                // 選択開始
                resetSelect(false);
                foreach (var shiftAdd in _shiftAdd)    // SHIFTキーで追加指定したパーツ
                {
                    shiftAdd.IsSelected = true;
                    _selectedParts.Add(_tarPane, (PartsBase)shiftAdd);
                }
                // 選択領域のパーツ
                foreach (var pts in GetFilteredParts())
                {
                    if (Parts.IsOverlapped(_tarPane, pts, _tarPane, _mask, true))
                    {
                        if (pts is IPartsSelectable selp)
                        {
                            var selected = _shiftAdd.Contains(selp);
                            var prevSel = selp.IsSelected;
                            selp.IsSelected = !selected;
                            if (selp.IsSelected)
                            {
                                _selectedParts.Add(_tarPane, (PartsBase)selp);
                            }
                            else
                            {
                                _selectedParts.Remove((PartsBase)selp);
                            }
                        }
                    }
                }
                Pane.Invalidate(null);
            }
        }

        public void OnMouseDown(MouseState e)
        {
            if (e.Attr.IsCtrl || e.Attr.IsButtonMiddle)
            {
                return;
            }
            if (ClickParts != null)
            {
                return; // パーツ外のドラッグのみ、選択開始できる。
            }
            if (e.Pos.Y >= _tarPane.GetPaneRect().RB.Y - 16)    // スクロールバー上では、選択開始できない
            {
                return;
            }
            if (e.Pos.X >= _tarPane.GetPaneRect().RB.X - 16)    // スクロールバー上では、選択開始できない
            {
                return;
            }

            _shiftAdd.Clear();
            if (e.Attr.IsShift)
            {
                foreach (PartsCollection.PartsEntry pe in _selectedParts)
                {
                    _shiftAdd.Add((IPartsSelectable)pe.Parts);
                }
            }
            resetSelect(true);
            _startPos = (ScreenPos)e.Pos.Clone();
            _mask.Rect = CodeRect.FromLTWH(_startPos.X, _startPos.Y, 0, 0);
            _mask.Visible = true;
        }

        public void OnMouseUp(MouseState e)
        {
            _mask.Visible = false;
            Pane.Invalidate(null);
        }

        public void OnMouseWheel(MouseState e)
        {
        }
        #endregion
    }
}
