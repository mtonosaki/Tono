// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uPtRect �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class LayoutRect : Rect
    {

        public LayoutRect()
        {
            _lt = new LayoutPos();
            _rb = new LayoutPos();
        }

        public new LayoutPos LT
        {

            get => (LayoutPos)_lt;

            set => _lt = value;
        }

        public new LayoutPos RB
        {

            get => (LayoutPos)_rb;

            set => _rb = value;
        }

        /// <summary>
        /// �����l����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x">������WX</param>
        /// <param name="y">������WY</param>
        /// <param name="width">��</param>
        /// <param name="height">����</param>
        /// <returns></returns>

        public static new LayoutRect FromLTWH(int x, int y, int width, int height)
        {
            var ret = new LayoutRect();
            ret.LT.X = x;
            ret.LT.Y = y;
            ret.RB.X = x + width - 1;
            ret.RB.Y = y + height - 1;
            return ret;
        }

        /// <summary>
        /// ���S���擾����
        /// </summary>
        /// <returns></returns>
        public new LayoutPos GetCenter()
        {
            var xy = base.GetCenter();
            return LayoutPos.FromInt(xy.X, xy.Y);
        }


        /// <summary>
        /// �����l����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x">������WX</param>
        /// <param name="y">������WY</param>
        /// <param name="width">��</param>
        /// <param name="height">����</param>
        /// <returns></returns>

        public static new LayoutRect FromLTRB(int l, int t, int r, int b)
        {
            var ret = new LayoutRect();
            ret.LT.X = l;
            ret.LT.Y = t;
            ret.RB.X = r;
            ret.RB.Y = b;
            return ret;
        }

        /// <summary>
        /// ���Z�q�I�[�o�[���[�h
        /// </summary>
        public static LayoutRect operator &(LayoutRect r1, Rect r2) { return (LayoutRect)((Rect)r1 & r2); }
        public static LayoutRect operator |(LayoutRect r1, Rect r2) { return (LayoutRect)((Rect)r1 | r2); }
        public static LayoutRect operator +(LayoutRect r1, XyBase r2) { return (LayoutRect)((Rect)r1 + r2); }
        public static LayoutRect operator -(LayoutRect r1, XyBase r2) { return (LayoutRect)((Rect)r1 - r2); }
        public static LayoutRect operator *(LayoutRect r1, XyBase r2) { return (LayoutRect)((Rect)r1 * r2); }
        public static LayoutRect operator /(LayoutRect r1, XyBase r2) { return (LayoutRect)((Rect)r1 / r2); }
    }
}
