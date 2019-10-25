// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// パーツ選択中、カーソルをペーンの端にドラッグ状態ではみ出した時、自動的にスクロールする
    /// </summary>
    public class FeatureAutoEdgeScroll : FeatureControlBridgeBase, IMouseListener
    {
        /// <summary>パーツ位置管理オブジェクト</summary>
        protected PartsPositionManager _pos;
        private GuiTimer.Handle _timer = null;
        private IRichPane _tarPane;

        /// <summary>
        /// 初期化
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // 移動中のパーツ一覧
            _tarPane = Pane.GetPane("Resource");
        }

        /// <summary>
        /// オートスクロールする
        /// </summary>
        public void autoScroll()
        {
            try
            {
                var e = MouseState.Now;
                _timer = null;
                var pp = 0;
                if (e.Attr.IsButton)
                {
                    var pr = _tarPane.GetPaneRect();
                    e.Pos = ThreadSafe.PointToClient(_tarPane.Control, e.Pos);
                    if (e.Pos.Y > -24 && e.Pos.Y < pr.Height + 24)
                    {
                        if (e.Pos.X < 16)
                        {
                            pp = 20;
                        }
                        if (e.Pos.X > pr.Width - 32)
                        {
                            pp = -20;
                        }
                    }
                }
                if (pp != 0)
                {
                    Pane.Scroll = ScreenPos.FromInt(Pane.Scroll.X + pp, Pane.Scroll.Y);
                    Pane.Invalidate(null);
                    _timer = Timer.AddTrigger(100, new GuiTimer.Proc0(autoScroll));
                }
                else
                {
                    _timer = null;
                }
            }
            catch (Exception)
            {
            }
        }


        #region IMouseListener メンバ

        public void OnMouseMove(MouseState e)
        {
            if (e.Attr.IsButton && _timer == null && _pos.Count > 0)
            {
                autoScroll();
            }
        }

        public void OnMouseDown(MouseState e)
        {
        }

        public void OnMouseUp(MouseState e)
        {
        }

        public void OnMouseWheel(MouseState e)
        {
        }

        #endregion
    }
}
