// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ドラッグ＆ドロップでの入出力をサポートするインターフェース
    /// </summary>
    public interface IDragDropListener
    {
        /// <summary>
        /// アイテムがドロップされた時のイベントを転送します
        /// </summary>
        /// <param name="e">ドラッグ＆ドロップの状態</param>
        void OnDragDrop(DragState e);
    }
}
