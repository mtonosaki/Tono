// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �J�����w�b�_�[�̊g���N���X
    /// </summary>
    public class TColumnHeader : System.Windows.Forms.ColumnHeader
    {
        #region	����(�V���A���C�Y����)
        /// <summary>���J�����Ɋi�[�\�ȃf�[�^�̌^</summary>
        private Type _datType = typeof(object);
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        /// <summary>���ёւ��̌`���t���O</summary>
        public int SortType = 0;
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public TColumnHeader() : base()
        {
        }

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        /// <param name="str">�w�b�_�[�e�L�X�g</param>
        /// <param name="width">�J�����̕�</param>
        /// <param name="textAlign">�e�L�X�g�̕\���ʒu</param>
        /// <param name="type">�i�[�\�f�[�^�̌^</param>
        public TColumnHeader(string str, int width, HorizontalAlignment textAlign, Type type)
        {
            base.Text = str;
            base.Width = width;
            base.TextAlign = textAlign;
            _datType = type;
        }

        /// <summary>
        /// �i�[�f�[�^�̃^�C�v�̎擾/�ݒ�
        /// </summary>
        public Type DataType
        {
            get => _datType;
            set => _datType = value;
        }
    }
}
