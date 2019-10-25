// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �h���b�O���h���b�v�C�x���g�̑�����\������N���X
    /// </summary>
    public class DragState : Tono.GuiWinForm.MouseState
    {
        #region	����(�V���A���C�Y����)
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        /// <summary>�h���b�v���ꂽ�t�@�C���p�X</summary>
        public string[] filepath;
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public DragState()
        {
        }

        /// <summary>
        /// �}�E�X�C�x���g��������C���X�^���X�𐶐�����
        /// </summary>
        /// <param name="e">�}�E�X�C�x���g����</param>
        /// <returns>�V�����C���X�^���X</returns>
        public static DragState FromDragEventArgs(System.Windows.Forms.DragEventArgs e, IRichPane posPane)
        {
            var ret = new DragState();
            ret.Pos.X = e.X;
            ret.Pos.Y = e.Y;
            ret.Delta.X = 0;
            ret.Delta.Y = 0;
            ret._paneAtPos = posPane;
            ret.Attr.IsButton = (e.KeyState == 1);
            ret.Attr.IsButtonMiddle = (e.KeyState == 16);
            ret.Attr.IsCtrl = (e.KeyState == 8);
            ret.Attr.IsShift = (e.KeyState == 4);
            ret.filepath = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            return ret;
        }

    }
}
