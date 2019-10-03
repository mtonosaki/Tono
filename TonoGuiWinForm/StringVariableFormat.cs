#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ������ŁA�ϐ��ƒ萔�̗������\���ł���N���X
    /// �ϐ��́AToString���ɕ]�������
    /// </summary>
    public class StringVariableFormat
    {
        /// <summary>
        /// �ϐ��̏ꍇ�̕����񐶐�
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public delegate string VariableText(string key);

        private readonly string _key;
        private readonly VariableText _val;

        /// <summary>
        /// �萔�ŏ�����
        /// </summary>
        /// <param name="fix"></param>
        public StringVariableFormat(string fix)
        {
            _key = fix;
            _val = null;
        }

        /// <summary>
        /// �ϐ��ŏ�����
        /// </summary>
        /// <param name="key">�ϐ��𐶐����邽�߂̃L�[</param>
        /// <param name="val">�ϐ������񐶐��p�̊֐�</param>
        public StringVariableFormat(string key, VariableText val)
        {
            _key = key;
            _val = val;
        }

        /// <summary>
        /// �萔���ϐ��ŕ������Ԃ�
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_val == null)
            {
                return _key;
            }
            else
            {
                return _val(_key);
            }
        }
    }
}
