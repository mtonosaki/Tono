// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �L�[�C�x���g�̑�����\������N���X
    /// </summary>
    public class KeyState
    {
        #region �����i�V���A���C�Y���K�v�j
        private Keys _key;
        private bool _isControl;
        private bool _isShift;
        #endregion

        #region Data tips for debugging
#if DEBUG
        public string _ => ToString();
#endif
        #endregion

        public override string ToString()
        {
            return (_isControl ? "[CTRL]+" : "") + (_isShift ? "[SHIFT]+" : "") + _key.ToString();
        }


        /// <summary>
        /// �L�[�̏��
        /// </summary>
        public Keys Key => _key;

        /// <summary>
        /// �V�t�g�L�[�̏��
        /// </summary>
        public bool IsShift => _isShift;

        /// <summary>
        /// �R���g���[���L�[�̏��
        /// </summary>
        public bool IsControl => _isControl;

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>

        public KeyState()
        {
            _key = Keys.None;
        }

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>

        public KeyState(Keys value)
        {
            _key = value;
            _isControl = false;
            _isShift = false;
        }

        /// <summary>
        /// �L�[�C�x���g��������C���X�^���X�𐶐�����
        /// </summary>
        /// <returns>�V�����C���X�^���X</returns>
        // 
        public static KeyState FromKeyEventArgs(KeyEventArgs e)
        {
            var ret = new KeyState
            {
                _key = e.KeyCode,
                _isControl = e.Control,
                _isShift = e.Shift
            };
            return ret;
        }

        /// <summary>
        /// uMouseState�̈ꕔ��؂���C���X�^���X�𐶐�����
        /// </summary>
        /// <param name="value">�}�E�X���</param>
        /// <returns>�V�����C���X�^���X</returns>

        public static KeyState FromMouseStateButtons(MouseState.Buttons value)
        {
            var ret = new KeyState
            {
                _isControl = value.IsCtrl,
                _isShift = value.IsShift
            };
            ret._key = (ret._isControl ? Keys.Control : 0) | (ret._isShift ? Keys.Shift : 0);
            return ret;
        }

        /// <summary>
        /// �C���X�^���X�̓��e�����������ǂ������ׂ�
        /// </summary>
        /// <param name="obj">��r�Ώ�</param>
        /// <returns>true = �C���X�^���X�̓��e��������</returns>

        public override bool Equals(object obj)
        {
            if (obj is KeyState)
            {
                return GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        /// <summary>
        /// �C���X�^���X�̓��e�𐔒l������
        /// </summary>
        /// <returns>�n�b�V���R�[�h</returns>

        public override int GetHashCode()
        {
            var ret = (int)Key;
            ret |= IsControl ? (int)Keys.Control : 0;
            ret |= IsShift ? (int)Keys.Shift : 0;
            return ret;
        }
    }
}
