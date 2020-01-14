// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �����\���p�Ƀe�L�X�g���}�X�N����
    /// ����̃L�[�̓��͂��󂯕t���Ȃ��e�L�X�g�{�b�N�X
    /// </summary>
    public class TTextBoxTimeMask : TextBox
    {
        #region	����(�V���A���C�Y����)
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        /// <summary>�\�����鎞��</summary>
        private DateTimeEx _time = new DateTimeEx();
        /// <summary>�R�s�[���J�n�����e�L�X�g�ʒu</summary>
        private static int _CopyStartIndex = int.MaxValue;
        /// <summary>�R�s�[����������</summary>
        private static string _CopyText = "";
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public TTextBoxTimeMask() : base()
        {
        }

        /// <summary>
        /// �L�[�_�E���̃C�x���g
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {   // BackSpace��������Delete(Ctrl+BackSpace)�͏������Ȃ��B
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// �L�[�v���X�C�x���g(�L�[�_�E���C�x���g����ɔ�������)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // BackSpace��������Delete(Ctrl+BackSpace)�͏������Ȃ��B
            if (e.KeyChar == 0x8 || e.KeyChar == 0x7f)
            {
                e.Handled = true;
            }

            switch ((int)e.KeyChar)
            {   // char�^��int�^�ɃL���X�g�����ASCII�����R�[�h�ɕϊ��ł���
                case 0x30:          // 0
                case 0x31:          // 1
                case 0x32:          // 2
                case 0x33:          // 3
                case 0x34:          // 4
                case 0x35:          // 5
                case 0x36:          // 6
                case 0x37:          // 7
                case 0x38:          // 8
                case 0x39:          // 9
                                    // �����A�I��͈͂�":"���܂܂�Ă�����A������͏��������Ȃ��B
                    if (SelectedText.IndexOf(":", 0, SelectedText.Length) >= 0)
                    {
                        e.Handled = true;
                    }

                    var Sindex = 0;
                    var s = Text.Split(new char[] { ':' });
                    var text = Text + ":";
                    var count = 0;
                    if (SelectionStart > 7)
                    {   // �S�̂łV���ȏ�̕\���͂��肦�Ȃ�
                        e.Handled = true;
                    }
                    else
                    {
                        while (text.IndexOf(":", Sindex) >= 0)
                        {
                            var Eindex = text.IndexOf(":", Sindex);
                            if (Sindex <= SelectionStart && SelectionStart <= Eindex)
                            {
                                var t = s[count];
                                if (SelectionLength == 0)
                                {
                                    if (t.Length >= 2)
                                    {   // �e�P�ʂłR���ȏ�̕\���͂��Ȃ�
                                        e.Handled = true;
                                    }
                                }
                                if (e.Handled == false)
                                {   // ���͒l���e�P�ʂ̏��(��:23 ���A�b:59)�𒴂��Ă����ꍇ�A���͂𖳌��ɂ���
                                    var after = Text.Remove(SelectionStart, SelectionLength);
                                    after = after.Insert(SelectionStart, e.KeyChar.ToString());
                                    var tmp = after.Split(new char[] { ':' });
                                    if (count == 0 && int.Parse(tmp[count]) > 23)
                                    {
                                        e.Handled = true;
                                    }

                                    if (count > 0 && int.Parse(tmp[count]) > 59)
                                    {
                                        e.Handled = true;
                                    }
                                }
                            }
                            Sindex = Eindex + 1;
                            count++;
                        }
                    }
                    break;
                case 0x3:   // Ctrl + C �R�s�[�͋�����
                    if (SelectionLength == 0)
                    {
                        e.Handled = true;
                        _CopyStartIndex = int.MaxValue;
                        _CopyText = "";
                        break;
                    }
                    _CopyStartIndex = SelectionStart;
                    _CopyText = SelectedText;
                    break;
                case 0x16:  // Ctrl + V �y�[�X�g�͋�����
                    var str = (string)System.Windows.Forms.Clipboard.GetDataObject().GetData(typeof(string));
                    if (SelectionLength == 0 ||
                        (SelectionStart != _CopyStartIndex || SelectionLength != _CopyText.Length || _CopyText != str))
                    {   // ���̃A�v��(��������G�N�Z��)����̃R�s�[�͎󂯕t���Ȃ��悤�ɂ���
                        e.Handled = true;
                    }
                    break;
                default: e.Handled = true; break;   // ����ȊO
            }

            base.OnKeyPress(e);
        }

        /// <summary>
        /// �e�L�X�g���ύX���ꂽ���ɌĂяo����܂�
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(EventArgs e)
        {
            var s = Text.Split(new char[] { ':' });

            if (s.Length != 3)
            {
                return;
            }

            _time = DateTimeEx.FromDHMS(0, int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]));
            base.OnTextChanged(e);
        }

        /// <summary>
        /// ���Ԃ̎擾
        /// </summary>
        public DateTimeEx GetTime()
        {
            return DateTimeEx.FromDHMS(0, _time.Hour, _time.Minute, _time.Second);
        }
        /// <summary>
        /// �\�������̐ݒ�
        /// </summary>
        /// <param name="value">�\�����鎞��</param>
        public void SetTime(DateTimeEx value)
        {
            // ���͐؂�̂ĂŁA�����݂̂���������
            _time = DateTimeEx.FromDHMS(0, value.Hour, value.Minute, value.Second);
            Text = _time.Hour.ToString("00") + ":" + _time.Minute.ToString("00") + ":" + _time.Second.ToString("00");
        }
    }
}
