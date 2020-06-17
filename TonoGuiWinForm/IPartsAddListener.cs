﻿// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IPartsAddListener の概要の説明です。
    /// </summary>
    public interface IPartsAddListener
    {
        /// <summary>
        /// パーツ追加イベント
        /// </summary>
        /// <param name="removedPartsSet">追加されたパーツの一覧</param>
        void OnPartsAdded(ICollection addedPartsSet);
    }
}
