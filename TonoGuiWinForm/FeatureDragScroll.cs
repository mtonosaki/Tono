// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �}�E�X���h���b�O���ĉ�ʃX�N���[������@�\
    /// </summary>
    public class FeatureDragScroll : Tono.GuiWinForm.FeatureBase, IMouseListener
    {
        #region �����i�V���A���C�Y����j
        /// <summary>�C�x���g�����s����L�[�ƂȂ�}�E�X�̏��</summary>
        protected MouseState.Buttons _trigger;
        #endregion
        #region �����i�V���A���C�Y���Ȃ��j
        /// <summary>�}�E�X���N���b�N�������_�ł̃}�E�X���W</summary>
        protected ScreenPos _posDown = null;
        /// <summary>�}�E�X���N���b�N�������_�ł̃X�N���[����</summary>
        protected ScreenPos _scrollDown;
        /// <summary>���O�ƃL�[�̏�Ԃ��L��</summary>
        protected MouseState _prev = new MouseState();
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public FeatureDragScroll()
        {
            // �f�t�H���g�Ńh���b�O�X�N���[�����邽�߂̃L�[��ݒ肷��
            _trigger = new MouseState.Buttons
            {
                IsButton = false,
                IsButtonMiddle = true,
                IsDoubleClick = false,
                IsCtrl = false,
                IsShift = false
            };
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
                    if (od[0].ToLower() == "trigger")
                    {
                        _trigger = new MouseState.Buttons();

                        var ts = od[1].Split(new char[] { '+' });
                        foreach (var t in ts)
                        {
                            if (t.ToLower() == "middle")
                            {
                                _trigger.IsButtonMiddle = true;
                            }
                            if (t.ToLower() == "button" || t.ToLower() == "left")
                            {
                                _trigger.IsButton = true;
                            }
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

        #region IMouseListener �����o

        /// <summary>
        /// �}�E�X�ړ��C�x���g����
        /// </summary>
        public virtual void OnMouseMove(MouseState e)
        {
            if (_posDown != null)
            {
                if (e.Attr.Equals(_trigger))
                {
                    var spos = _scrollDown + (e.Pos - _posDown);
                    Pane.Scroll = spos;
                    Pane.Invalidate(null);
                }
                else
                {
                    OnMouseUp(e);
                }
            }
        }

        /// <summary>
        /// �}�E�X�_�E���C�x���g����
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseDown(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                _posDown = e.Pos;
                _scrollDown = Pane.Scroll;

            }
        }

        /// <summary>
        /// �}�E�X�A�b�v�C�x���g����
        /// </summary>
        public virtual void OnMouseUp(MouseState e)
        {
            _posDown = null;
            //System.Diagnostics.Debug.WriteLine("Scroll = " + Pane.Scroll.ToString());
        }

        public virtual void OnMouseWheel(MouseState e)
        {
            // ���g�p
        }
        #endregion
    }
}
