// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �}�E�X�z�C�[���ŃX�N�[�����T�|�[�g����@�\
    /// </summary>
    public class FeatureWheelScroll : Tono.GuiWinForm.FeatureBase, IMouseListener
    {
        #region �����i�V���A���C�Y����j
        /// <summary>�C�x���g�̎��s�L�[</summary>
        private MouseState.Buttons _trigger;
        #endregion
        #region �����i�V���A���C�Y���Ȃ��j
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public FeatureWheelScroll()
        {
            // �f�t�H���g�Ńh���b�O�X�N���[�����邽�߂̃L�[��ݒ肷��
            _trigger = new MouseState.Buttons
            {
                IsButton = false,
                IsButtonMiddle = false,
                IsCtrl = true,
                IsShift = false
            };
        }

        /// <summary>
        /// �g���K�i���s���ʃL�[�j��ύX����
        /// </summary>
        /// <param name="value">�V�����g���K�[</param>
        public void SetTrigger(MouseState.Buttons value)
        {
            _trigger = value;
        }

        #region	IMoueListener �����o
        /// <summary>
        /// �}�E�X�ړ��C�x���g��]������
        /// </summary>
        /// <param name="e">�}�E�X���</param>
        public void OnMouseMove(MouseState e)
        {
            // ���g�p
        }

        /// <summary>
        /// �}�E�X�ړ��C�x���g��]������
        /// </summary>
        /// <param name="e">�}�E�X���</param>
        public void OnMouseDown(MouseState e)
        {
            // ���g�p
        }

        /// <summary>
        /// �}�E�X�ړ��C�x���g��]������
        /// </summary>
        /// <param name="e">�}�E�X���</param>
        public void OnMouseUp(MouseState e)
        {
            // ���g�p
        }

        /// <summary>
        /// �}�E�X�z�C�[���̃C�x���g��]������
        /// </summary>
        /// <param name="e">�}�E�X���</param>
        public void OnMouseWheel(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                Pane.Scroll += e.Delta / 5;
                Pane.Invalidate(null);
            }
        }
        #endregion
    }
}
