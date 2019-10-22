// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uPtPos �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class LayoutPos : XyBase
    {
        /// <summary>
        /// �l���w�肵�ăC���X�^���X�����
        /// </summary>
        /// <param name="v1">�l�P</param>
        /// <param name="v2">�l�Q</param>
        /// <returns>�C���X�^���X</returns>

        public static new LayoutPos FromInt(int v1, int v2)
        {
            var ret = new LayoutPos
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }

        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static LayoutPos operator +(LayoutPos v1, ValueCouple v2) { return (LayoutPos)((ValueCouple)v1 + v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static LayoutPos operator +(LayoutPos v1, int v2) { return (LayoutPos)((ValueCouple)v1 + v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static LayoutPos operator -(LayoutPos v1, ValueCouple v2) { return (LayoutPos)((ValueCouple)v1 - v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static LayoutPos operator *(LayoutPos v1, ValueCouple v2) { return (LayoutPos)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static LayoutPos operator *(LayoutPos v1, int v2) { return (LayoutPos)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static LayoutPos operator /(LayoutPos v1, ValueCouple v2) { return (LayoutPos)((ValueCouple)v1 / v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static LayoutPos operator /(LayoutPos v1, int v2) { return (LayoutPos)((ValueCouple)v1 / v2); }
    }
}
