// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �e�L�X�g�ŃR�}���h���s����d�g�݂��T�|�[�g����
    /// </summary>
    public interface ITextCommand
    {
        string[] tcTarget();
        void tcPlay(CommandBase tc);
    }
}
