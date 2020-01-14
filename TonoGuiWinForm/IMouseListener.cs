// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// マウス・キーボードのリッチクライアント入力をサポートするインターフェース
    /// </summary>
    public interface IMouseListener
    {
        /// <summary>
        /// マウス移動イベントを転送する
        /// </summary>
        /// <param name="e">マウス状態</param>
        void OnMouseMove(MouseState e);

        /// <summary>
        /// ボタンダウンイベントを転送する
        /// </summary>
        /// <param name="e">マウス状態</param>
        void OnMouseDown(MouseState e);

        /// <summary>
        /// ボタンアップイベントを転送する
        /// </summary>
        /// <param name="e">マウス状態</param>
        void OnMouseUp(MouseState e);

        /// <summary>
        /// マウスホイールのイベントを転送する
        /// </summary>
        /// <param name="e">マウス状態</param>
        void OnMouseWheel(MouseState e);

    }
}
