// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// テキストでコマンド実行する仕組みをサポートする
    /// </summary>
    public interface ITextCommand
    {
        string[] tcTarget();
        void tcPlay(CommandBase tc);
    }
}
