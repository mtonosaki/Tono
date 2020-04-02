﻿// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureMousePosLabel の概要の説明です。
    /// 指定したコントロールに、マウスの座標を表示するフィーチャークラス
    /// これを使用することで、リアルタイムにマウス座標を知ることができる
    /// </summary>
    public class FeatureMousePosLabel : FeatureBase, IMouseListener
    {
        private Control _mousePosLabel;

        /// <summary>
        /// コントロールを取得、設定する
        /// </summary>
        public Control TargetControl
        {
            get => _mousePosLabel;
            set => _mousePosLabel = value;
        }

        /// <summary>
        /// マウス移動時のイベント
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            _mousePosLabel.Text = e.Pos.Y + "," + e.Pos.X;
        }

        #region IMouseListener メンバ
        public void OnMouseDown(MouseState e)
        {
            // 未使用
        }

        public void OnMouseUp(MouseState e)
        {
            // 未使用
        }

        public void OnMouseWheel(MouseState e)
        {
            // 未使用
        }


        #endregion
    }
}
