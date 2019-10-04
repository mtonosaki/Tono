using System;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uPtPos �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class CodePos : XyBase
    {
        /// <summary>
        /// �l���w�肵�ăC���X�^���X�����
        /// </summary>
        /// <param name="v1">�l�P</param>
        /// <param name="v2">�l�Q</param>
        /// <returns>�C���X�^���X</returns>

        public static new CodePos FromInt(int v1, int v2)
        {
            var ret = new CodePos
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static CodePos operator +(CodePos v1, ValueCouple v2) { return (CodePos)((ValueCouple)v1 + v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static CodePos operator -(CodePos v1, ValueCouple v2) { return (CodePos)((ValueCouple)v1 - v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static CodePos operator *(CodePos v1, ValueCouple v2) { return (CodePos)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static CodePos operator *(CodePos v1, int v2) { return (CodePos)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static CodePos operator /(CodePos v1, ValueCouple v2) { return (CodePos)((ValueCouple)v1 / v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static CodePos operator /(CodePos v1, int v2) { return (CodePos)((ValueCouple)v1 / v2); }
    }
}
