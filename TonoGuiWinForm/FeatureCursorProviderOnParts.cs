// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureCursorProviderOnParts の概要の説明です。
    /// </summary>
    public class FeatureCursorProviderOnParts : FeatureBase, IKeyListener, IMouseListener
    {
        private bool _isTop = true;
        private bool _isRight = true;
        private bool _isLeft = true;
        private bool _isBottom = true;
        private bool _isInside = true;
        private bool _isMove = true;
        private IRichPane _filterPane = null;
        private int _filterLayer = int.MinValue;

        #region 属性（シリアライズしない）

        /// <summary>通常カーソルを記憶する</summary>
        private Cursor _normalCursor;

        /// <summary>現在のキーの状態を記憶</summary>
        private MouseState _ms = new MouseState();

        /// <summary>要求するカーソル（後で設定するもの）</summary>
        private Cursor _requestedCursor = null;

        /// <summary>カーソル表示状態（共有変数）</summary>
        private DataSharingManager.Int _state;

        /// <summary>パーツ位置管理オブジェクト</summary>
        protected PartsPositionManager _pos;

        /// <summary>高速化（遅延処理のタイマーハンドル）</summary>
        private GuiTimer.Handle _th = null;

        #endregion

        #region 解析
        /// <summary>
        /// パラメータ解析
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);

            var coms = param.Split(new char[] { ';' });
            foreach (var com in coms)
            {
                var od = com.Split(new char[] { '=' });
                if (od.Length < 2)
                {
                    continue;
                }

                if (od[0].ToLower() == "pane")
                {
                    _filterPane = Pane.GetPane(od[1]);
                }
                if (od[0].ToLower() == "layer")
                {
                    _filterLayer = int.Parse(od[1]);
                }
                if (od[0].ToLower() == "avoid")
                {
                    var ts = od[1].Split(new char[] { ',' });
                    foreach (var tt in ts)
                    {
                        var t = tt.Trim().ToLower();
                        switch (t)
                        {
                            case "top":
                                _isTop = false;
                                break;
                            case "left":
                                _isLeft = false;
                                break;
                            case "right":
                                _isRight = false;
                                break;
                            case "bottom":
                                _isBottom = false;
                                break;
                            case "inside":
                                _isInside = false;
                                break;
                            case "move":
                                _isMove = false;
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 初期化処理
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            if (Pane is IControlUI)
            {
                _normalCursor = ((IControlUI)Pane).Cursor;
            }
            else
            {
                throw new NotSupportedException("FeatureCursorProviderは、IControlUIを実装しているPaneにのみ使用できます");
            }

            // ステータス同期
            _state = (DataSharingManager.Int)Share.Get("CursorProviderStatus", typeof(DataSharingManager.Int));
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // 移動中のパーツ一覧
        }

        /// <summary>
        /// カーソルをセットする（ファイナライズ）
        /// </summary>
        private delegate void SetCursorCallback();

        private void onCursorSet()
        {
            if (_requestedCursor != null)
            {
                if (Pane.Control.InvokeRequired)
                {
                    var d = new SetCursorCallback(onCursorSet);
                    Pane.Control.Invoke(d);
                }
                else
                {
                    Pane.Control.Cursor = _requestedCursor;
                }
            }
        }

        /// <summary>マウスカーソル変更遅延処理(高速化)</summary>
        /// <param name="param">マウス座標</param>
        private void proc(object param)
        {
            var e = (MouseState)param;
            _ms = e;
            IRichPane tarPane;
            PartsBase parts;
            if (_filterPane == null)
            {
                parts = Parts.GetPartsAt(_ms.Pos, true, out tarPane);
            }
            else
            {
                parts = Parts.GetPartsAt(_ms.Pos, _filterPane, _filterLayer, true);
                tarPane = _filterPane;
            }
            _requestedCursor = _isMove ? _normalCursor : null;
            if (parts != null)
            {
                // 境界線のチェック対象を絞る
                var check = PartsBase.PointType.Inside | PartsBase.PointType.Outside;
                if (_isLeft)
                {
                    check |= PartsBase.PointType.OnLeft;
                }

                if (_isRight)
                {
                    check |= PartsBase.PointType.OnRight;
                }

                if (_isTop)
                {
                    check |= PartsBase.PointType.OnTop;
                }

                if (_isBottom)
                {
                    check |= PartsBase.PointType.OnBottom;
                }

                // 境界線上などをチェック
                switch (parts.IsOn(_ms.Pos, tarPane, check))
                {
                    case PartsBase.PointType.Inside:
                        _requestedCursor = _isInside ? Cursors.Hand : _requestedCursor;
                        break;
                    case PartsBase.PointType.OnLeft:
                        _requestedCursor = _isLeft ? Cursors.VSplit : _requestedCursor;
                        break;
                    case PartsBase.PointType.OnRight:
                        _requestedCursor = _isRight ? Cursors.VSplit : _requestedCursor;
                        break;
                    case PartsBase.PointType.OnTop:
                        _requestedCursor = _isTop ? Cursors.HSplit : _requestedCursor;
                        break;
                    case PartsBase.PointType.OnBottom:
                        _requestedCursor = _isBottom ? Cursors.HSplit : _requestedCursor;
                        break;
                }
            }
            onCursorSet();
        }

        #region IKeyListener メンバ

        public void OnKeyDown(KeyState e)
        {
            _ms.Attr.SetKeyFrags(e);
        }

        public void OnKeyUp(KeyState e)
        {
            _ms.Attr.SetKeyFrags(e);
        }

        #endregion

        #region IMouseListener メンバ

        public void OnMouseMove(MouseState e)
        {
            Timer.Stop(_th);
            if (_state.value == 0)  // FeatureCursorProviderKey でカーソル変更していない状態の時のみ、処理を行いたい
            {
                if (_pos.Count == 0)
                {
                    _th = Timer.AddTrigger(e, 50, new GuiTimer.Proc1(proc));
                }
            }
        }

        public void OnMouseDown(MouseState e)
        {
            _ms = e;
        }

        public void OnMouseUp(MouseState e)
        {
            if (e != null)
            {
                _ms.Attr.ResetKeyFlags(e.Attr);
                _ms.Attr.SetKeyFrags(e.Attr);
            }
        }

        public void OnMouseWheel(MouseState e)
        {
        }

        #endregion
    }
}
