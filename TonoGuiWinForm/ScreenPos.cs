// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uScPos �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class ScreenPos : XyBase
    {
        /// <summary>
        /// �l���w�肵�ăC���X�^���X�����
        /// </summary>
        /// <param name="v1">�l�P</param>
        /// <param name="v2">�l�Q</param>
        /// <returns>�C���X�^���X</returns>

        public static new ScreenPos FromInt(int v1, int v2)
        {
            var ret = new ScreenPos
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }

        /// <summary>
        /// �����^�ϊ�
        /// </summary>
        public static implicit operator ScreenPos(System.Drawing.Point pos)
        {
            var ret = new ScreenPos
            {
                X = pos.X,
                Y = pos.Y
            };
            return ret;
        }

        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static ScreenPos operator +(ScreenPos v1, ValueCouple v2) { return (ScreenPos)((ValueCouple)v1 + v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static ScreenPos operator +(ScreenPos v1, int v2) { return (ScreenPos)((ValueCouple)v1 + v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static ScreenPos operator -(ScreenPos v1, ValueCouple v2) { return (ScreenPos)((ValueCouple)v1 - v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static ScreenPos operator *(ScreenPos v1, ValueCouple v2) { return (ScreenPos)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static ScreenPos operator *(ScreenPos v1, int v2) { return (ScreenPos)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static ScreenPos operator *(ScreenPos v1, double v2) { return (ScreenPos)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static ScreenPos operator /(ScreenPos v1, ValueCouple v2) { return (ScreenPos)((ValueCouple)v1 / v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static ScreenPos operator /(ScreenPos v1, int v2) { return (ScreenPos)((ValueCouple)v1 / v2); }
    }
}
