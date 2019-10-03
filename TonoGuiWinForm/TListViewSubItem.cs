#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ���X�g�r���[�̃T�u�A�C�e���̊g���N���X
    /// </summary>
    public class TListViewSubItem : System.Windows.Forms.ListViewItem.ListViewSubItem
    {
        #region	�����i�V���A���C�Y����j
        /// <summary>�e�L�X�g�̃f�[�^</summary>
        private object _obj = null;
        /// <summary>���l�^�̃f�[�^���������Ă��邩</summary>
        private bool _isInt = false;
        /// <summary>�����N��̃e�[�u��</summary>
        private string _tableName = "";
        /// <summary>�����N��̃J����</summary>
        private string _columnName = "";
        #endregion
        #region	�����i�V���A���C�Y���Ȃ��j
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public TListViewSubItem()
        {
            var s = "";
            Text = s.Clone();
        }

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        /// <param name="text">�\������e�L�X�g</param>
        public TListViewSubItem(object value)
        {
            Text = value;
        }

        /// <summary>
        /// int�^�̒l���擾
        /// </summary>
        public int intText
        {
            get
            {
                if (_obj is Id)
                {
                    return ((Id)_obj).Value;
                }

                if (_obj is int)
                {
                    return (int)_obj;
                }

                return 0;   // Id�^�ł�int�^�ł����������ꍇ
            }
        }

        /// <summary>
        /// ���l�^���f�[�^���������Ă��邩��������
        /// </summary>
        public bool IsInt
        {
            get => _isInt;
            set => _isInt = value;
        }

        /// <summary>
        /// �����N��̃e�[�u�������擾/�ݒ�
        /// </summary>
        public string TableName
        {
            get => _tableName;
            set => _tableName = value;
        }

        /// <summary>
        /// �����N��̗񖼂��擾/�ݒ�
        /// </summary>
        public string ColumnName
        {
            get => _columnName;
            set => _columnName = value;
        }

        /// <summary>
        /// �\���e�L�X�g�̎擾/�ݒ�
        /// </summary>
        public new object Text
        {
            get => _obj;
            set
            {
                if (value is Id)
                {
                    base.Text = ((Id)value).Value.ToString();
                    IsInt = true;
                }
                else if (value is DateTimeEx)
                {
                    base.Text = ((DateTimeEx)value).TotalSeconds.ToString();
                }
                else
                {
                    if (value is int)
                    {
                        IsInt = true;
                    }

                    base.Text = value.ToString();
                }
                _obj = value;
            }
        }
    }
}
