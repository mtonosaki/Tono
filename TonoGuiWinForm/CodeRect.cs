// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// CodeRect
    /// </summary>
    [Serializable]
    public class CodeRect : Rect
    {

        public CodeRect()
        {
            _lt = new CodePos();
            _rb = new CodePos();
        }

        public new CodePos LT
        {

            get => (CodePos)_lt;

            set => _lt = value;
        }

        public new CodePos RB
        {

            get => (CodePos)_rb;

            set => _rb = value;
        }

        /// <summary>
        /// ���S�_�Ɣ��a���w�肵�Đ����`�̍��W���쐬����
        /// </summary>
        /// <param name="x">���S�_�iX)</param>
        /// <param name="y">���S�_�iY�j</param>
        /// <param name="r">���a</param>
        /// <returns>�V�����C���X�^���X</returns>
        public static CodeRect FromXYR(int x, int y, int r)
        {
            var ret = new CodeRect();
            ret.LT.X = x - r;
            ret.LT.Y = y - r;
            ret.RB.X = ret.LT.X + r * 2;
            ret.RB.Y = ret.LT.Y + r * 2;
            return ret;
        }

        /// <summary>
        /// �����l����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x">������WX</param>
        /// <param name="y">������WY</param>
        /// <param name="width">��</param>
        /// <param name="height">����</param>
        /// <returns></returns>

        public static new CodeRect FromLTWH(int x, int y, int width, int height)
        {
            var ret = new CodeRect();
            ret.LT.X = x;
            ret.LT.Y = y;
            ret.RB.X = x + width - 1;
            ret.RB.Y = y + height - 1;
            return ret;
        }

        /// <summary>
        /// �����l����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x">������WX</param>
        /// <param name="y">������WY</param>
        /// <param name="width">��</param>
        /// <param name="height">����</param>
        /// <returns></returns>

        public static CodeRect FromLTWH(double x, double y, double width, double height)
        {
            return FromLTWH((int)x, (int)y, (int)width, (int)height);
        }

        /// <summary>
        /// �����l����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x">������WX</param>
        /// <param name="y">������WY</param>
        /// <param name="width">��</param>
        /// <param name="height">����</param>
        /// <returns></returns>

        public static new CodeRect FromLTRB(int l, int t, int r, int b)
        {
            var ret = new CodeRect();
            ret.LT.X = l;
            ret.LT.Y = t;
            ret.RB.X = r;
            ret.RB.Y = b;
            return ret;
        }

        /// <summary>
        /// ���Z�q�I�[�o�[���[�h
        /// </summary>
        public static CodeRect operator &(CodeRect r1, Rect r2) { return (CodeRect)((Rect)r1 & r2); }
        public static CodeRect operator +(CodeRect r1, XyBase r2) { return (CodeRect)((Rect)r1 + r2); }
        public static CodeRect operator -(CodeRect r1, XyBase r2) { return (CodeRect)((Rect)r1 - r2); }
        public static CodeRect operator *(CodeRect r1, XyBase r2) { return (CodeRect)((Rect)r1 * r2); }
        public static CodeRect operator *(CodeRect r1, int r2) { return (CodeRect)((Rect)r1 * r2); }
        public static CodeRect operator /(CodeRect r1, XyBase r2) { return (CodeRect)((Rect)r1 / r2); }
    }
}
