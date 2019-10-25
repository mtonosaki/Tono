// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uXx �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class RangeXx : ValueCouple, ISpace
    {
        public static RangeXx operator +(RangeXx v1, ValueCouple v2) { return (RangeXx)((ValueCouple)v1 + v2); }
        public static RangeXx operator +(RangeXx v1, int v2) { return (RangeXx)((ValueCouple)v1 + v2); }
        public static RangeXx operator -(RangeXx v1, ValueCouple v2) { return (RangeXx)((ValueCouple)v1 - v2); }
        public static RangeXx operator -(RangeXx v1, int v2) { return (RangeXx)((ValueCouple)v1 - v2); }
        public static RangeXx operator *(RangeXx v1, ValueCouple v2) { return (RangeXx)((ValueCouple)v1 * v2); }
        public static RangeXx operator *(RangeXx v1, int v2) { return (RangeXx)((ValueCouple)v1 * v2); }
        public static RangeXx operator /(RangeXx v1, ValueCouple v2) { return (RangeXx)((ValueCouple)v1 / v2); }
        public static RangeXx operator /(RangeXx v1, int v2) { return (RangeXx)((ValueCouple)v1 / v2); }

        /// <summary>
        /// ���S
        /// </summary>
        public int Middle => (_v1 + _v2) / 2;

        /// <summary>
        /// �l���w�肵�ăC���X�^���X�����
        /// </summary>
        /// <param name="v1">�l�P</param>
        /// <param name="v2">�l�Q</param>
        /// <returns>�C���X�^���X</returns>

        public static new RangeXx FromInt(int v1, int v2)
        {
            var ret = new RangeXx
            {
                X0 = v1,
                X1 = v2
            };
            return ret;
        }

        /// <summary>
        /// X0 = int.Max / X1 = int.Min �ŃC���X�^���X�𐶐�����
        /// </summary>
        /// <returns></returns>
        public static RangeXx FromNegativeSpan()
        {
            return RangeXx.FromInt(int.MaxValue, int.MinValue);
        }

        /// <summary>
        /// ��`����X���W�݂̂𒊏o���ăC���X�^���X�𐶐�����
        /// </summary>
        /// <param name="value">��`</param>
        /// <returns>�V�����C���X�^���X</returns>

        public static RangeXx FromRect(Rect value)
        {
            var ret = new RangeXx
            {
                X0 = value.LT.X,
                X1 = value.RB.X
            };
            return ret;
        }

        /// <summary>
        /// 1�ڂ�X���W
        /// </summary>
        public int X0
        {

            get => _v1;

            set => _v1 = value;
        }

        /// <summary>
        /// �Q�ڂ�X���W
        /// </summary>
        public int X1
        {

            get => _v2;

            set => _v2 = value;
        }
        #region ISpace �����o

        /// <summary>
        /// �w�肵���|�C���g���I�u�W�F�N�g�̈���ɂ��邩�ǂ������肷��
        /// </summary>
        /// <param name="value">uXx�^�̎w��|�C���g</param>
        /// <returns>true:�̈�� / false:�̈�O</returns>
        // 
        public bool IsIn(object value)
        {
            if (value is int)
            {
                var pt = (int)value;
                if (X0 <= pt && X1 >= pt)
                {
                    return true;
                }
                return false;
            }
            if (value is RangeXx)
            {
                var pt = (RangeXx)value;
                if (X0 <= pt.X0 && pt.X0 <= X1 ||
                    X0 <= pt.X1 && pt.X1 <= X1 ||
                    pt.X0 <= X0 && pt.X1 >= X1)
                {
                    return true;
                }
            }
            if (value is XyBase)
            {
                return IsIn(((XyBase)value).X);
            }
            if (value is Rect)
            {
                var r = value as Rect;
                if (r is CodeRect || r.Height > 0)
                {
                    if (X0 <= r.LT.X && r.LT.X <= X1 ||
                        X0 <= r.RB.X && r.RB.X <= X1 ||
                        r.LT.X <= X0 && r.RB.X >= X1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// �I�u�W�F�N�g�̈ړ�
        /// </summary>
        /// <param name="value">uXx�^�̈ړ��l (X0,X1)</param>

        public void Transfer(object value)
        {
            System.Diagnostics.Debug.Assert(value is int, "Transfer��int�^�����T�|�[�g���Ă��܂�");
            var pt = (int)value;

            X0 += pt;
            X1 += pt;
        }

        /// <summary>
        /// �I�u�W�F�N�g�̊g��
        /// </summary>
        /// <param name="value">uXx�^�̈ړ��l (X0,X1)</param>

        public void Inflate(object value)
        {
            if (value is int p)
            {
                X0 -= p;
                X1 += p;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is RangeXx, "Inflate��uXx�^�����T�|�[�g���Ă��܂�");
                var pt = (RangeXx)value;

                X0 -= pt.X0;
                X1 += pt.X1;
            }
        }

        /// <summary>
        /// �I�u�W�F�N�g�̏k��
        /// </summary>
        /// <param name="value">uXx�^�̈ړ��l (X0,X1)</param>

        public void Deflate(object value)
        {
            if (value is int)
            {
                X0 += (int)value;
                X1 -= (int)value;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is RangeXx, "Inflate��uXx�^�����T�|�[�g���Ă��܂�");
                var pt = (RangeXx)value;

                X0 += pt.X0;
                X1 -= pt.X1;
            }
        }

        #endregion
    }
}
