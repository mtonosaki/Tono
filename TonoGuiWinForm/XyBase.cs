// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uXy �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class XyBase : ValueCouple
    {
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>

        public XyBase()
        {
        }

        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static XyBase operator +(XyBase v1, ValueCouple v2) { return (XyBase)((ValueCouple)v1 + v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static XyBase operator +(XyBase v1, int v2) { return (XyBase)((ValueCouple)v1 + v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static XyBase operator -(XyBase v1, ValueCouple v2) { return (XyBase)((ValueCouple)v1 - v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static XyBase operator *(XyBase v1, ValueCouple v2) { return (XyBase)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static XyBase operator *(XyBase v1, int v2) { return (XyBase)((ValueCouple)v1 * v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static XyBase operator /(XyBase v1, ValueCouple v2) { return (XyBase)((ValueCouple)v1 / v2); }
        /// <summary>���Z�q�̃I�[�o�[���[�h</summary>
        public static XyBase operator /(XyBase v1, int v2) { return (XyBase)((ValueCouple)v1 / v2); }

        /// <summary>
        /// �C���X�^���X��ϊ�����
        /// </summary>
        /// <param name="xy">�ϊ��Ώ�</param>
        /// <returns>PointF�^�̃C���X�^���X</returns>

        public static implicit operator PointF(XyBase xy)
        {
            return new PointF(xy.X, xy.Y);
        }

        /// <summary>
        /// �����^�ϊ�
        /// </summary>
        public static implicit operator System.Drawing.Point(XyBase pos)
        {
            var ret = new System.Drawing.Point
            {
                X = pos.X,
                Y = pos.Y
            };
            return ret;
        }

        /// <summary>
        /// �l���w�肵�ăC���X�^���X�����
        /// </summary>
        /// <param name="v1">�l�P</param>
        /// <param name="v2">�l�Q</param>
        /// <returns>�C���X�^���X</returns>

        public static new XyBase FromInt(int v1, int v2)
        {
            var ret = new XyBase
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }


        /// <summary>
        /// X���W
        /// </summary>
        public int X
        {

            get => _v1;

            set => _v1 = value;
        }

        /// <summary>
        /// Y���W
        /// </summary>
        public int Y
        {

            get => _v2;

            set => _v2 = value;
        }
    }
}
