// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IKeyListener �̊T�v�̐����ł��B
    /// </summary>
    public interface IKeyListener
    {
        /// <summary>
        /// �L�[�_�E���C�x���g
        /// </summary>
        /// <param name="e">�L�[���</param>
        void OnKeyDown(KeyState e);

        /// <summary>
        /// �L�[�A�b�v�C�x���g
        /// </summary>
        /// <param name="e">�L�[���</param>
        void OnKeyUp(KeyState e);

    }
}
