using System;
using System.Diagnostics;
using System.Runtime.Serialization;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �ύX���ɃC�x���g�����s�ł��镶����
    /// </summary>
    [Serializable]
    public class StringListener : ISerializable
    {
        /// <summary>
        /// �e�L�X�g�ύX���O�̃C�x���g
        /// </summary>
        public event EventHandler TextChanging;
        /// <summary>
        /// �e�L�X�g�ύX����̃C�x���g
        /// </summary>
        public event EventHandler TextChanged;

        /// <summary>
        /// �V���A���C�Y����Ώۂ̕�����
        /// </summary>
        private string _str = "";

        public override string ToString()
        {
            return _str;
        }

        public override int GetHashCode()
        {
            return _str.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _str.Equals(obj);
        }

        public static bool operator ==(StringListener s0, StringListener s1)
        {
            if (s0 == null && s1 == null)
            {
                return true;
            }
            if (s0 != null && s1 != null)
            {
                return s0.Equals(s1);
            }
            return false;
        }

        public static bool operator !=(StringListener s0, StringListener s1)
        {
            return !(s0 == s1);
        }

        /// <summary>
        /// string�^�ւ̎����ϊ�
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static implicit operator string(StringListener from)
        {
            return from._str;
        }

        /// <summary>
        /// �����񂩂�L���X�g����ƁA�C�x���g��������̂ŋ����Ȃ�
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static explicit operator StringListener(string from)
        {
            Debug.Assert(false, "Program error : 'StrUtilinge = string' is not accepted. use StrUtilinge.Value property");
            return new StringListener(from);
        }

        /// <summary>
        /// �C�x���g���l�������l
        /// </summary>
        public string Text
        {
            get => _str;
            set
            {
                if (_str != value)
                {
                    var e = new EventArgs();
                    TextChanging?.Invoke(this, e);
                    _str = value;
                    TextChanged?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        public StringListener()
        {
        }

        /// <summary>
        /// ����R���X�g���N�^
        /// </summary>
        /// <param name="val"></param>
        public StringListener(string val)
        {
            _str = val;
        }

        /// <summary>
        /// �R�s�[�R���X�g���N�^
        /// </summary>
        /// <param name="val"></param>
        public StringListener(StringListener val)
        {
            _str = val._str;
            TextChanged = val.TextChanged;
            TextChanging = val.TextChanging;
        }

        #region ISerializable �����o

        /// <summary>
        /// �V���A���C�Y�\�z�i�C�x���g�͕��A���Ȃ��j
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected StringListener(SerializationInfo info, StreamingContext context)
        {
            _str = info.GetString("_str");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_str", _str);
        }

        #endregion
    }
}
