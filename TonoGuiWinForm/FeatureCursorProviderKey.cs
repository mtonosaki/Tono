// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �J�[�\���Z�b�g��S������
    /// �L�[�����ɂ��}�E�X�J�[�\���̕ύX
    /// </summary>
    public class FeatureCursorProviderKey : Tono.GuiWinForm.FeatureBase, IKeyListener, IMouseListener
    {
        /// <summary>
        /// �J�[�\����ύX��������A�J�[�\���ύX�\��I�u�W�F�N�g
        /// </summary>
        public class Reserve
        {
            #region �����i�V���A���C�Y����j

            /// <summary>�J�[�\���ύX�������</summary>
            public MouseState.Buttons Buttons;

            /// <summary>�����ɒB�����ۂɎg�p����J�[�\��</summary>
            public Cursor Cursor;

            /// <summary>�����ɑ΂��郁�b�Z�[�W</summary>
            public string Message = string.Empty;

            /// <summary>
            /// 
            /// </summary>
            public MouseState.Buttons PreviousButtons = null;

            #endregion

            /// <summary>
            /// �������R���X�g���N�^
            /// </summary>
            /// <param name="buttons">�\�������ƂȂ�{�^���̏��</param>
            /// <param name="cursor">���̏����ŕ\���������J�[�\��</param>
            /// <param name="mes">�J�[�\���ւƓ������b�Z�[�W</param>
            public Reserve(MouseState.Buttons buttons, Cursor cursor, string mes)
            {
                Buttons = buttons;
                Cursor = cursor;
                Message = mes;
            }

            /// <summary>
            /// �������R���X�g���N�^
            /// </summary>
            /// <param name="previous">�J�[�\����L���Ƃ��钼�O�̏�Ԃ���肷��B����ȊO�̃{�^�����炱�̏�ԂɂȂ��Ă��J�[�\����ύX���Ȃ�</param>
            /// <param name="buttons">�\�������ƂȂ�{�^���̏��</param>
            /// <param name="cursor">���̏����ŕ\���������J�[�\��</param>
            /// <param name="mes">�J�[�\���ւƓ������b�Z�[�W</param>
            public Reserve(MouseState.Buttons previous, MouseState.Buttons buttons, Cursor cursor, string mes)
            {
                Buttons = buttons;
                Cursor = cursor;
                Message = mes;
                PreviousButtons = previous;

            }
        }


        #region �����i�V���A���C�Y����j

        /// <summary>�ʏ�J�[�\�����L������</summary>
        private readonly Hashtable /*<uMouseState.Buttons, Reserve>*/ _resData = new Hashtable();

        #endregion
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>�ʏ�J�[�\�����L������</summary>
        private Cursor _normalCursor;

        /// <summary>���݂̃L�[�̏�Ԃ��L��</summary>
        private MouseState _ms = new MouseState();

        private MouseState _prev = new MouseState();

        /// <summary>�v������J�[�\���i��Őݒ肷����́j</summary>
        private Cursor _requestedCursor = null;

        /// <summary>�J�[�\���\����ԁi���L�ϐ��j</summary>
        private DataSharingManager.Int _state;

        /// <summary>�J�[�\���Z�b�g�󋵂�ۑ����邽�߂�ID</summary>
        private NamedId _cursorSet;

        #endregion


        /// <summary>
        /// ����������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            _cursorSet = NamedId.FromName("CursorSet");

            if (Pane is IControlUI)
            {
                _normalCursor = ((IControlUI)Pane).Cursor;
            }
            else
            {
                throw new NotSupportedException("FeatureCursorProvider�́AIControlUI���������Ă���Pane�ɂ̂ݎg�p�ł��܂�");
            }

            // �X�e�[�^�X����
            _state = (DataSharingManager.Int)Share.Get("CursorProviderStatus", typeof(DataSharingManager.Int));
        }

        /// <summary>
        /// �J�[�\�����U�[�u��o�^����i�\�������Ƃ��̃J�[�\���̃Z�b�g�����U�[�u�j
        /// </summary>
        /// <param name="value">�o�^���郊�U�[�u�̃C���X�^���X</param>
        public void Add(Reserve value)
        {
            _resData.Add(value.Buttons, value);
        }

        /// <summary>
        /// �������f���ăJ�[�\����ύX����
        /// </summary>
        /// <param name="ms">���݂̃L�[���</param>
        private void proc(MouseState ms)
        {
            // ���̂܂ܕ]������
            for (var de = _resData.GetEnumerator(); de.MoveNext();)
            {
                if (((MouseState.Buttons)de.Key).Equals(ms.Attr))
                {
                    var res = ((Reserve)de.Value);

                    if (res.PreviousButtons != null)
                    {
                        if (res.PreviousButtons.Equals(_prev.Attr) == false)
                        {
                            continue;
                        }
                    }

                    _requestedCursor = res.Cursor;
                    Finalizers.Add(_cursorSet, new FinalizeManager.Finalize(onCursorSet));
                    _state.value = 1;
                    _prev = _ms;
                    return;
                }
            }
            // �J�[�\����߂����̏���
            if (Finalizers.Contains(_cursorSet) == false)
            {
                _requestedCursor = _normalCursor;
                Finalizers.Add(_cursorSet, new FinalizeManager.Finalize(onCursorSet));
                _state.value = 0;
            }
            _prev = _ms;
        }

        /// <summary>
        /// �J�[�\�����Z�b�g����i�t�@�C�i���C�Y�j
        /// </summary>
        private void onCursorSet()
        {
            if (_requestedCursor != null)
            {
                ((IControlUI)Pane).Cursor = _requestedCursor;
            }
        }

        //int aa = 0;

        #region IKeyListener �����o

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnKeyDown(KeyState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnKeyDown " + aa++);
            _ms.Attr.SetKeyFrags(e);
            proc(_ms);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnKeyUp(KeyState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnKeyUp " + aa++);
            _ms.Attr.SetKeyFrags(e);
            proc(_ms);
        }

        #endregion

        #region IMouseListener �����o

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseMove(MouseState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnMouseMove " + aa++);
            _ms = e;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseDown(MouseState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnMouseDown " + aa++);
            _ms = e;
            proc(_ms);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseUp(MouseState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnMouseUp " + aa++);
            _ms.Attr.ResetKeyFlags(e.Attr);
            _ms.Attr.SetKeyFrags(e.Attr);
            proc(_ms);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseWheel(MouseState e)
        {
        }

        #endregion
    }
}
