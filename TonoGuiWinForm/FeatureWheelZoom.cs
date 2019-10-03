namespace Tono.GuiWinForm
{
    /// <summary>
    /// Ctrl+マウスホイールでズームをサポートする機能
    /// </summary>
    public class FeatureWheelZoom : Tono.GuiWinForm.FeatureBase, IMouseListener
    {
        #region		属性(シリアライズする)
        /** <summary>ズームを開始するトリガ</summary> */
        private MouseState.Buttons _trigger;
        #endregion
        #region		属性(シリアライズしない)
        /// <summary>
        /// ズーム限界なので自動視点移動しない
        /// </summary>
        private DataSharingManager.Boolean _noscrollmove = null;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeatureWheelZoom()
        {
            // デフォルトでドラッグスクロールするためのキーを設定する
            _trigger = new MouseState.Buttons
            {
                IsButton = false,
                IsButtonMiddle = false,
                IsCtrl = false,
                IsShift = false
            };
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
        /// トリガ（実行識別キー）を変更する
        /// </summary>
        /// <param name="value">新しいトリガー</param>
        public void SetTrigger(MouseState.Buttons value)
        {
            _trigger = value;
        }

        /// <summary>
        /// 文字解析
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);
            if (param != "")
            {
                var s = param.ToUpper();
                if (s.IndexOf('X') < 0)
                {
                    _isX = false;
                }
                if (s.IndexOf('Y') < 0)
                {
                    _isY = false;
                }
            }
        }

        private bool _isX = true;
        private bool _isY = true;


        private void log(string name, object o)
        {
            System.Diagnostics.Debug.WriteLine(name + " : " + o.ToString());
        }

        #region IMouseListener メンバ

        /// <summary>
        /// マウスMoveイベント
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseMove(MouseState e)
        {
            // 未使用
        }

        /// <summary>
        /// ボタンDownイベント
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseDown(MouseState e)
        {
            // 未使用
        }

        /// <summary>
        /// ボタンUpイベント
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseUp(MouseState e)
        {
            // 未使用
        }

        /// <summary>
        /// マウスWheelイベント
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseWheel(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                // 選択ペーンの描画領域の中心をズームのセンターの設定
                var _posDown = new ScreenPos
                {
                    X = e.Pane.GetPaneRect().LT.X + e.Pos.X - e.Pane.GetPaneRect().LT.X,
                    Y = e.Pane.GetPaneRect().LT.Y + e.Pos.Y - e.Pane.GetPaneRect().LT.Y
                };
                var _scrollDown = (ScreenPos)Pane.Scroll.Clone();
                var _zoomDown = (XyBase)Pane.Zoom.Clone();
                var vol = (int)(GeoEu.Length(Pane.Zoom.X, Pane.Zoom.Y) / 1000 * e.Delta.Y * 0.1);

                // 画面の拡大/縮小

                XyBase zoomNow;
                if (_isX && !_isY)
                {
                    zoomNow = Pane.Zoom + XyBase.FromInt(vol, 0);          // ズーム値の算出
                }
                else if (!_isX && _isY)
                {
                    zoomNow = Pane.Zoom + XyBase.FromInt(0, vol);          // ズーム値の算出
                }
                else
                {
                    zoomNow = Pane.Zoom + vol;                          // ズーム値の算出
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

                var beforeDownPos = _posDown - _scrollDown - e.Pane.GetPaneRect().LT;    // 
                var afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

                if (_noscrollmove.value == false)
                {
                    Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);
                }
                else
                {
                    _noscrollmove.value = false;
                }
                Pane.Invalidate(null);
            }
            else
            {
                OnMouseUp(e);
            }
        }

        #endregion
    }
}
