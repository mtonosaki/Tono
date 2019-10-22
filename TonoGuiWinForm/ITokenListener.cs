// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ITokenListener �̊T�v�̐����ł��B
    /// </summary>
    public interface ITokenListener
    {
        /// <summary>
        /// �g���K�[�ƂȂ�g�[�N����ID
        /// </summary>
        NamedId TokenTriggerID
        {
            get;
        }
    }

    /// <summary>
    /// �����g�[�N�����T�|�[�g����
    /// </summary>
    public interface IMultiTokenListener
    {
        NamedId[] MultiTokenTriggerID
        {
            get;
        }
    }
}
