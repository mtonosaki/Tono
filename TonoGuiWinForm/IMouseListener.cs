// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �}�E�X�E�L�[�{�[�h�̃��b�`�N���C�A���g���͂��T�|�[�g����C���^�[�t�F�[�X
    /// </summary>
    public interface IMouseListener
    {
        /// <summary>
        /// �}�E�X�ړ��C�x���g��]������
        /// </summary>
        /// <param name="e">�}�E�X���</param>
        void OnMouseMove(MouseState e);

        /// <summary>
        /// �{�^���_�E���C�x���g��]������
        /// </summary>
        /// <param name="e">�}�E�X���</param>
        void OnMouseDown(MouseState e);

        /// <summary>
        /// �{�^���A�b�v�C�x���g��]������
        /// </summary>
        /// <param name="e">�}�E�X���</param>
        void OnMouseUp(MouseState e);

        /// <summary>
        /// �}�E�X�z�C�[���̃C�x���g��]������
        /// </summary>
        /// <param name="e">�}�E�X���</param>
        void OnMouseWheel(MouseState e);

    }
}
