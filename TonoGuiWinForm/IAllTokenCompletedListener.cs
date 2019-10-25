// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IAllTokenCompleted の概要の説明です。
    /// すべてのトークン処理が終わったときに受けるイベント。
    /// OnAllTokenCompletedでファイナライザ、トークンを投げても、処理されない。
    /// </summary>
    public interface IAllTokenCompletedListener
    {
        void OnAllTokenCompleted();
    }
}
