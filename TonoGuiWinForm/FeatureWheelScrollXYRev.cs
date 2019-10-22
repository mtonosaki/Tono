// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �}�E�X�z�C�[���ŃX�N�[�����T�|�[�g����@�\
    /// </summary>
    public class FeatureWheelScrollXYRev : FeatureBase, IMouseListener
    {
        #region �����i�V���A���C�Y����j
        /// <summary>�C�x���g�̎��s�L�[</summary>
        private MouseState.Buttons _trigger = null;
        #endregion
        #region �����i�V���A���C�Y���Ȃ��j
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public FeatureWheelScrollXYRev()
        {
            // �f�t�H���g�Ńh���b�O�X�N���[�����邽�߂̃L�[��ݒ肷��
            _trigger = new MouseState.Buttons();
        }

        /// <summary>
        /// �p�����[�^�[�̏�����
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            var coms = param.Split(new char[] { ';' });
            foreach (var com in coms)
            {
                var od = com.Split(new char[] { '=' });
                if (od.Length == 2)
                {
                    if (od[0].ToLower() == "attr")
                    {
                        _trigger = new MouseState.Buttons();

                        var ts = od[1].Split(new char[] { '+' });
                        foreach (var t in ts)
                        {
                            if (t.ToLower() == "ctrl")
                            {
                                _trigger.IsCtrl = true;
                            }
                            if (t.ToLower() == "shift")
                            {
                                _trigger.IsShift = true;
                            }
                        }
                    }
                }
            }
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
                var p = XyBase.FromInt(e.Delta.Y, e.Delta.X);
                Pane.Scroll += p / 5;
                Pane.Invalidate(null);
            }
        }
        #endregion
    }
}
