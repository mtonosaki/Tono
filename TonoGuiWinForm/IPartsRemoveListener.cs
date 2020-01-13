// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IPartsRemoveListener の概要の説明です。
    /// </summary>
    public interface IPartsRemoveListener
    {
        /// <summary>
        /// パーツ削除イベント
        /// </summary>
        /// <param name="removedPartsSet">削除されたパーツの一覧</param>
        void OnPartsRemoved(ICollection removedPartsSet);
    }
}
