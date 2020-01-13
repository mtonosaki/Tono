// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IPartsRemoveListener �̊T�v�̐����ł��B
    /// </summary>
    public interface IPartsRemoveListener
    {
        /// <summary>
        /// �p�[�c�폜�C�x���g
        /// </summary>
        /// <param name="removedPartsSet">�폜���ꂽ�p�[�c�̈ꗗ</param>
        void OnPartsRemoved(ICollection removedPartsSet);
    }
}
