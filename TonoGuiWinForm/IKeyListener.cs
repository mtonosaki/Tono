// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IKeyListener の概要の説明です。
    /// </summary>
    public interface IKeyListener
    {
        /// <summary>
        /// キーダウンイベント
        /// </summary>
        /// <param name="e">キー状態</param>
        void OnKeyDown(KeyState e);

        /// <summary>
        /// キーアップイベント
        /// </summary>
        /// <param name="e">キー状態</param>
        void OnKeyUp(KeyState e);

    }
}
