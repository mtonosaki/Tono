// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ドラッグでのズームをサポート
    /// </summary>
    /// <remarks>
    /// 特許公開番号：特開2007-264807
    /// </remarks>
    public class FeatureDragZoom : Tono.GuiWinForm.FeatureBase, IMouseListener, IKeyListener
    {
        #region		属性(シリアライズする)
        /** <summary>ズームを開始するトリガ</summary> */
        protected MouseState.Buttons _trigger;
        protected bool _isSameXY;
        protected bool _isCenterLock;
        #endregion
        #region		属性(シリアライズしない)
        /// <summary>マウスをクリックした時点でのマウス座標</summary>
        protected ScreenPos _posDown = null;
        /// <summary>マウスをクリックした時点でのスクロール量</summary>
        protected ScreenPos _scrollDown;
        /// <summary>マウスをクリックした時のズーム値</summary>
        protected XyBase _zoomDown;
        /// <summary>マウスをクリックしたときのペーン</summary>
        protected IRichPane _paneDown;
        /// <summary>イベントによって変更するカーソルのリスト</summary>
        protected Hashtable _CursorList = new Hashtable();
        /// <summary>直前のマウスカーソルの状況</summary>
        protected MouseState.Buttons _prev = new MouseState.Buttons();
        /// <summary>カーソルを</summary>
        protected NamedId _tokenListenID = NamedId.FromName("CursorSetJob");
        /// <summary>
        /// ズーム限界なので自動視点移動しない
        /// </summary>
        protected DataSharingManager.Boolean _noscrollmove = null;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeatureDragZoom()
        {
            // デフォルトでドラッグスクロールするためのキーを設定する
            _trigger = new MouseState.Buttons
            {
                IsButton = true,
                IsButtonMiddle = false,
                IsCtrl = true,
                IsShift = false
            };
            _isSameXY = false;
            _isCenterLock = false;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
            _noscrollmove = (DataSharingManager.Boolean)Share.Get("NoScrollMoveFlag", typeof(DataSharingManager.Boolean));
        }

        /// <summary>
        /// パラメーターの初期化
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            string[] coms = param.Split(new char[] { ';' });
            foreach (string com in coms)
            {
                string[] od = com.Split(new char[] { '=' });
                if (od.Length < 2)
                {
                    continue;
                }

                if (od[0].ToLower() == "centerlock")
                {
                    _isCenterLock = Const.IsTrue(od[1]);
                }
                if (od[0].ToLower() == "samexy")
                {
                    _isSameXY = Const.IsTrue(od[1]);
                }
                if (od[0].ToLower() == "trigger")
                {
                    _trigger = new MouseState.Buttons();

                    string[] ts = od[1].Split(new char[] { '+' });
                    foreach (string t in ts)
                    {
                        if (t.ToLower() == "middle")
                        {
                            _trigger.IsButtonMiddle = true;
                        }
                        if (t.ToLower() == "button" || t.ToLower() == "left")
                        {
                            _trigger.IsButton = true;
                        }
                        if (t.ToLower() == "ctrl")
                        {
                            _trigger.IsCtrl = true;
                        }
                        if (t.ToLower() == "shift")
                        {
                            _trigger.IsShift = true;
                        }
                    }
                }
                else if (od[0].ToLower() == "cursor")
                {
                    if (od.Length == 3)
                    {
                        string[] ts = od[2].Split(new char[] { '.' });
                        string[] trs = od[1].Split(new char[] { '+' });
                        MouseState.Buttons trg = new Tono.GuiWinForm.MouseState.Buttons();
                        foreach (string t in trs)
                        {
                            if (t.ToLower() == "middle")
                            {
                                trg.IsButtonMiddle = true;
                            }

                            if (t.ToLower() == "button" || t.ToLower() == "left")
                            {
                                trg.IsButton = true;
                            }

                            if (t.ToLower() == "ctrl")
                            {
                                trg.IsCtrl = true;
                            }

                            if (t.ToLower() == "shift")
                            {
                                trg.IsShift = true;
                            }
                        }
                        if (od[2].ToLower().IndexOf("cursors") != -1)
                        {
                            Type t = typeof(System.Windows.Forms.Cursors);
                            System.Reflection.PropertyInfo pi = t.GetProperty(ts[ts.Length - 1].ToString());
                            if (pi != null)
                            {
                                _EventCursor = (Cursor)pi.GetValue(null, Array.Empty<object>());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// トリガ（実行識別キー）を変更する
        /// </summary>
        /// <param name="value">新しいトリガー</param>
        public void SetTrigger(MouseState.Buttons value)
        {
            _trigger = value;
        }

        #region IMouseListener メンバ
        /// <summary>
        /// ボタンDownイベント
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseDown(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                _posDown = (ScreenPos)e.Pos.Clone();
                _paneDown = Pane;
                _zoomDown = (XyBase)Pane.Zoom.Clone();
                _scrollDown = (ScreenPos)Pane.Scroll.Clone();
            }
        }

        /// <summary>
        /// マウスMoveイベント
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseMove(MouseState e)
        {
            if (_posDown == null || _zoomDown == null || _scrollDown == null || e.Pane == null)
            {
                return;
            }

            if (e.Attr.Equals(_trigger))
            {
                // 画面の拡大/縮小
                ScreenPos movePos = e.Pos - _posDown;          // カーソルの移動量の計算
                ScreenPos pdBak = (ScreenPos)_posDown.Clone();
                if (_isCenterLock)
                {
                    _posDown.X = e.Pane.GetPaneRect().LT.X + (e.Pane.GetPaneRect().RB.X - e.Pane.GetPaneRect().LT.X) / 2;
                    _posDown.Y = e.Pane.GetPaneRect().LT.Y + (e.Pane.GetPaneRect().RB.Y - e.Pane.GetPaneRect().LT.Y) / 2;
                }

                XyBase zoomNow = _zoomDown + movePos * 3;      // ズーム値の算出  速度変更(高解像度に伴い) 2016.11.15 Tono 2→3

                // ズーム値を規定範囲内に収める
                if (zoomNow.X > 4000)
                {
                    zoomNow.X = 4000;
                }

                if (zoomNow.Y > 4000)
                {
                    zoomNow.Y = 4000;
                }

                if (zoomNow.X < 5)
                {
                    zoomNow.X = 5;
                }

                if (zoomNow.Y < 5)
                {
                    zoomNow.Y = 5;
                }

                if (_isSameXY)
                {
                    zoomNow.Y = zoomNow.X;
                }

                Pane.Zoom = (XyBase)zoomNow.Clone();           // ズーム値の反映

                // クリックした位置を基準にしてズームするように画面をスクロールする。
                double ZoomRatioX = (double)zoomNow.X / _zoomDown.X;    // X方向のズーム率の算出
                double ZoomRatioY = (double)zoomNow.Y / _zoomDown.Y;    // Y方向のズーム率の算出

                ScreenPos beforeDownPos = _posDown - _scrollDown - e.Pane.GetPaneRect().LT;    // 
                ScreenPos afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

                if (_noscrollmove.value == false)
                {
                    Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);
                }
                else
                {
                    _noscrollmove.value = false;
                }
                Pane.Invalidate(null);
                _posDown = pdBak;
            }
            else
            {
                OnMouseUp(e);
            }
        }

        /// <summary>
        /// ボタンUpイベント
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseUp(MouseState e)
        {
            _posDown = null;
            _zoomDown = null;
            _scrollDown = null;
        }

        /// <summary>
        /// マウスホイールイベント
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseWheel(MouseState e)
        {
            // 未使用
        }

        #endregion
        #region IKeyListener メンバ

        public virtual void OnKeyDown(KeyState e)
        {
            if (MouseState.NowButtons.IsButton == false && e.IsControl == true)
            {
                FeatureBase.Cursor = _EventCursor;
                Finalizers.Add(_tokenListenID, new FinalizeManager.Finalize(onCursorSeFinalizert));
            }
        }

        public virtual void OnKeyUp(KeyState e)
        {
            if (e.IsControl == true)
            {
                if (FeatureBase.Cursor == _EventCursor)
                {
                    FeatureBase.Cursor = Cursors.Default;
                    Finalizers.Add(_tokenListenID, new FinalizeManager.Finalize(onCursorSeFinalizert));
                }
            }
        }

        #endregion
    }
}
