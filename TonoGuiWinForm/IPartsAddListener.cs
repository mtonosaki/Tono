// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IPartsAddListener �̊T�v�̐����ł��B
    /// </summary>
    public interface IPartsAddListener
    {
        /// <summary>
        /// �p�[�c�ǉ��C�x���g
        /// </summary>
        /// <param name="removedPartsSet">�ǉ����ꂽ�p�[�c�̈ꗗ�idaPartsBase.PartsEntry�^�̃R���N�V�����j</param>
        void OnPartsAdded(ICollection addedPartsSet);
    }
}
