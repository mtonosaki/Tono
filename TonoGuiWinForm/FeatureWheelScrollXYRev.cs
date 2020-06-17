// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// マウスホイールでスクールをサポートする機能
    /// </summary>
    public class FeatureWheelScrollXYRev : FeatureBase, IMouseListener
    {
        #region 属性（シリアライズする）
        /// <summary>イベントの実行キー</summary>
        private MouseState.Buttons _trigger = null;
        #endregion
        #region 属性（シリアライズしない）
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeatureWheelScrollXYRev()
        {
            // デフォルトでドラッグスクロールするためのキーを設定する
            _trigger = new MouseState.Buttons();
        }

        /// <summary>
        /// パラメーターの初期化
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            var coms = param.Split(new char[] { ';' });
            foreach (var com in coms)
            {
                var od = com.Split(new char[] { '=' });
                if (od.Length == 2)
                {
                    if (od[0].ToLower() == "attr")
                    {
                        _trigger = new MouseState.Buttons();

                        var ts = od[1].Split(new char[] { '+' });
                        foreach (var t in ts)
                        {
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

        #region	IMoueListener メンバ
        /// <summary>
        /// マウス移動イベントを転送する
        /// </summary>
        /// <param name="e">マウス状態</param>
        public void OnMouseMove(MouseState e)
        {
            // 未使用
        }

        /// <summary>
        /// マウス移動イベントを転送する
        /// </summary>
        /// <param name="e">マウス状態</param>
        public void OnMouseDown(MouseState e)
        {
            // 未使用
        }

        /// <summary>
        /// マウス移動イベントを転送する
        /// </summary>
        /// <param name="e">マウス状態</param>
        public void OnMouseUp(MouseState e)
        {
            // 未使用
        }

        /// <summary>
        /// マウスホイールのイベントを転送する
        /// </summary>
        /// <param name="e">マウス状態</param>
        public void OnMouseWheel(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                var p = XyBase.FromInt(e.Delta.Y, e.Delta.X);
                Pane.Scroll += p / 5;
                Pane.Invalidate(null);
            }
        }
        #endregion
    }
}
