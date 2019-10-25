// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureKeyZoom の概要の説明です。
    /// </summary>
    public class FeatureKeyZoom : FeatureBase, IKeyListener, IMultiTokenListener
    {
        private IRichPane _tarPane = null;
        private bool _isTokenOnly = false;

        /// <summary>
        /// 初期化
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
            _tarPane = Pane.GetPane("Resource");
        }

        /// <summary>
        /// 起動パラメータ
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);
            if (string.IsNullOrEmpty(param) == false)
            {
                foreach (var com in param.Split(';'))
                {
                    if (com.Trim().ToUpper() == "TOKENSTARTONLY")
                    {
                        _isTokenOnly = true;
                        continue;
                    }
                    var ss = com.Split(new char[] { ',' });
                    System.Diagnostics.Debug.Assert(ss.Length == 2, "FeatureKeyZoomのパラメータは、\"Xの値,Yの値\"と書いてください");
                    _x = int.Parse(ss[0]);
                    _y = int.Parse(ss[1]);
                }
            }
        }

        /// <summary>
        /// ズーム量X
        /// </summary>
        private int _x = 100;

        /// <summary>
        /// ズーム量Y
        /// </summary>
        private int _y = 100;

        /// <summary>
        /// Xズームするか？
        /// </summary>
        private bool _isX => _x != 0;

        /// <summary>
        /// Yズームするか？
        /// </summary>
        private bool _isY => _y != 0;

        /// <summary>
        /// 指定量のズームを行う
        /// </summary>
        /// <param name="value"></param>
        private void zoom(int x, int y)
        {
            // 選択ペーンの描画領域の中心をズームのセンターの設定
            var _posDown = new ScreenPos
            {
                X = _tarPane.GetPaneRect().LT.X + _tarPane.GetPaneRect().Width / 2 - _tarPane.GetPaneRect().LT.X,
                Y = _tarPane.GetPaneRect().LT.Y + _tarPane.GetPaneRect().Height / 2 - _tarPane.GetPaneRect().LT.Y
            };
            var _scrollDown = (ScreenPos)Pane.Scroll.Clone();
            var _zoomDown = (XyBase)Pane.Zoom.Clone();

            // 画面の拡大/縮小
            XyBase zoomNow;
            if (_isX && !_isY)
            {
                zoomNow = Pane.Zoom + XyBase.FromInt(x, 0);            // ズーム値の算出
            }
            else if (!_isX && _isY)
            {
                zoomNow = Pane.Zoom + XyBase.FromInt(0, y);            // ズーム値の算出
            }
            else
            {
                zoomNow = Pane.Zoom + x;                            // ズーム値の算出
            }
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

            Pane.Zoom = (XyBase)zoomNow.Clone();           // ズーム値の反映

            // クリックした位置を基準にしてズームするように画面をスクロールする。
            var ZoomRatioX = (double)zoomNow.X / _zoomDown.X;    // X方向のズーム率の算出
            var ZoomRatioY = (double)zoomNow.Y / _zoomDown.Y;    // Y方向のズーム率の算出

            var beforeDownPos = _posDown - _scrollDown - _tarPane.GetPaneRect().LT;  // 
            var afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

            Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);
            Pane.Invalidate(null);
        }

        /// <summary>
        /// キーアップイベント時の処理
        /// </summary>
        public void OnKeyUp(KeyState e)
        {
            if (_isTokenOnly == false)
            {
                // ズームアップキー
                if (e.IsControl && e.Key == Keys.Add)
                {
                    zoom(_x, _y);
                }

                // ズームダウンキー
                if (e.IsControl && e.Key == Keys.Subtract)
                {
                    zoom(-_x, -_y);
                }
            }
        }

        /// <summary>
        /// キーダウンイベント時の処理
        /// </summary>
        public void OnKeyDown(KeyState e)
        {
        }

        /// <summary>
        /// トークン起動をサポート
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            base.Start(who);
            if (KeyZoomUp.Equals(who))
            {
                zoom(_x, _y);
            }
            if (KeyZoomDown.Equals(who))
            {
                zoom(-_x, -_y);
            }
        }

        #region IMultiTokenListener メンバ

        public static readonly NamedId KeyZoomUp = NamedId.FromName("FeatureKeyZoom.Up");
        public static readonly NamedId KeyZoomDown = NamedId.FromName("FeatureKeyZoom.Down");
        private static readonly NamedId[] _tokenTrigger = new NamedId[] { KeyZoomUp, KeyZoomDown };

        public NamedId[] MultiTokenTriggerID => _tokenTrigger;

        #endregion
    }
}
