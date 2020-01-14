// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureMenuZoom の概要の説明です。
    /// </summary>
    public class FeatureMenuZoom : FeatureBase
    {
        #region 属性（シリアライズする）
        #endregion
        #region 属性（シリアライズしない

        /// <summary>ズーム値を格納する</summary>
        private double zoom = 0.0;

        /// <summary>スクロール・ズームイベントを受信するペーン</summary>
        private IRichPane[] _tarRps = null;


        #endregion

        /// <summary>
        /// パラメータを取得する
        /// </summary>
        public override void ParseParameter(string param)
        {
            zoom = double.Parse(param);
        }

        public override void OnInitInstance()
        {
            _tarRps = new IRichPane[] { Pane.GetPane("Resource") };
        }

        /// <summary>
        /// 指定された値にズームする
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            // 選択ペーンの描画領域の中心をズームのセンターの設定
            var _posDown = new ScreenPos
            {
                X = _tarRps[0].GetPaneRect().LT.X + (_tarRps[0].GetPaneRect().RB.X - _tarRps[0].GetPaneRect().LT.X) / 2,       //ペーンのX座標の中心
                Y = _tarRps[0].GetPaneRect().LT.Y + (_tarRps[0].GetPaneRect().RB.Y - _tarRps[0].GetPaneRect().LT.Y) / 2       //ペーンのY座標の中心
            };
            var _scrollDown = (ScreenPos)Pane.Scroll.Clone();       //ズーム前のスクロール値
            var _zoomDown = (XyBase)Pane.Zoom.Clone();                 //ズーム前のズーム値
                                                                       // 画面の拡大/縮小
            var intZ = (int)(zoom * 100);                           //ズーム値の取得
            Pane.Zoom = XyBase.FromInt(1000, 1000);                    // ズーム値の初期化
            var zoomX = Pane.Zoom.X * intZ / 100;
            var zoomY = Pane.Zoom.Y * intZ / 100;
            var zoomNow = XyBase.FromInt(zoomX, zoomY);                // ズーム値の算出
            Pane.Zoom = zoomNow;                                    // ズーム値の反映

            // クリックした位置を基準にしてズームするように画面をスクロールする。
            var ZoomRatioX = (double)zoomNow.X / _zoomDown.X;    // X方向のズーム率の算出
            var ZoomRatioY = (double)zoomNow.Y / _zoomDown.Y;    // Y方向のズーム率の算出

            var beforeDownPos = _posDown - _scrollDown - _tarRps[0].GetPaneRect().LT;    // 
            var afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

            Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);     //スクロール値の反映

            Pane.Invalidate(null);
        }



    }
}
