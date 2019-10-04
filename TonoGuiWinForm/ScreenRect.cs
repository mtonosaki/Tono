using System;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uScRect �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class ScreenRect : Rect
    {
        public static new ScreenRect Empty => ScreenRect.FromLTRB(0, 0, 0, 0);

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>

        public ScreenRect()
        {
            _lt = new ScreenPos();
            _rb = new ScreenPos();
        }


        public ScreenRect(Rect r)
        {
            LT.X = r.LT.X;
            LT.Y = r.LT.Y;
            RB.X = r.RB.X;
            RB.Y = r.RB.Y;
        }

        public new ScreenPos LT
        {

            get => (ScreenPos)_lt;

            set => _lt = value;
        }

        public new ScreenPos RT => ScreenPos.FromInt(_rb.X, _lt.Y);

        public new ScreenPos LB => ScreenPos.FromInt(_lt.X, _rb.Y);

        public new ScreenPos RB
        {

            get => (ScreenPos)_rb;

            set => _rb = value;
        }

        /// <summary>
        /// �R���g���[������C���X�^���X�𐶐�����
        /// </summary>
        /// <param name="c">�R���g���[��</param>
        /// <returns>�V�����C���X�^���X</returns>

        public static ScreenRect FromControl(Control c)
        {
            var ret = ScreenRect.FromLTWH(c.Left, c.Top, c.Width, c.Height);
            return ret;
        }

        /// <summary>
        /// �����l����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x">������WX</param>
        /// <param name="y">������WY</param>
        /// <param name="width">��</param>
        /// <param name="height">����</param>
        /// <returns>�V�����C���X�^���X</returns>

        public static new ScreenRect FromLTWH(int x, int y, int width, int height)
        {
            var ret = new ScreenRect();
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
        public new ScreenPos GetCenter()
        {
            var xy = base.GetCenter();
            return ScreenPos.FromInt(xy.X, xy.Y);
        }

        /// <summary>
        /// ���W���w�肵�ĐV�����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x0">�����X���W</param>
        /// <param name="y0">�����Y���W</param>
        /// <param name="x1">�E����X���W</param>
        /// <param name="y1">�E����Y���W</param>
        /// <returns>�\�z�����C���X�^���X</returns>

        public static new ScreenRect FromLTRB(int x0, int y0, int x1, int y1)
        {
            var ret = new ScreenRect();
            ret.LT.X = x0;
            ret.LT.Y = y0;
            ret.RB.X = x1;
            ret.RB.Y = y1;
            return ret;
        }


        /// <summary>
        /// Rectangle����C���X�^���X���\�z����
        /// </summary>
        /// <param name="r">Rectangle�^�̌��̒l</param>
        /// <returns>�V�����C���X�^���X</returns>
        //		 
        public static ScreenRect FromRectangle(System.Drawing.Rectangle r)
        {
            var ret = new ScreenRect();
            ret.LT.X = r.Left;
            ret.LT.Y = r.Top;
            ret.RB.X = r.Right;
            ret.RB.Y = r.Bottom;
            return ret;
        }

        /// <summary>
        /// ���Z�q�I�[�o�[���[�h
        /// </summary>
        public static ScreenRect operator &(ScreenRect r1, Rect r2) { return (ScreenRect)((Rect)r1 & r2); }
        public static ScreenRect operator +(ScreenRect r1, XyBase r2) { return (ScreenRect)((Rect)r1 + r2); }
        public static ScreenRect operator -(ScreenRect r1, XyBase r2) { return (ScreenRect)((Rect)r1 - r2); }
        public static ScreenRect operator *(ScreenRect r1, XyBase r2) { return (ScreenRect)((Rect)r1 * r2); }
        public static ScreenRect operator /(ScreenRect r1, XyBase r2) { return (ScreenRect)((Rect)r1 / r2); }
    }
}
