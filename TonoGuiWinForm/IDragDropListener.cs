// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �h���b�O���h���b�v�ł̓��o�͂��T�|�[�g����C���^�[�t�F�[�X
    /// </summary>
    public interface IDragDropListener
    {
        /// <summary>
        /// �A�C�e�����h���b�v���ꂽ���̃C�x���g��]�����܂�
        /// </summary>
        /// <param name="e">�h���b�O���h���b�v�̏��</param>
        void OnDragDrop(DragState e);
    }
}
