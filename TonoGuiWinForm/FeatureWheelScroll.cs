// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// マウスホイールでスクールをサポートする機能
    /// </summary>
    public class FeatureWheelScroll : Tono.GuiWinForm.FeatureBase, IMouseListener
    {
        #region 属性（シリアライズする）
        /// <summary>イベントの実行キー</summary>
        private MouseState.Buttons _trigger;
        #endregion
        #region 属性（シリアライズしない）
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeatureWheelScroll()
        {
            // デフォルトでドラッグスクロールするためのキーを設定する
            _trigger = new MouseState.Buttons
            {
                IsButton = false,
                IsButtonMiddle = false,
                IsCtrl = true,
                IsShift = false
            };
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
                Pane.Scroll += e.Delta / 5;
                Pane.Invalidate(null);
            }
        }
        #endregion
    }
}
