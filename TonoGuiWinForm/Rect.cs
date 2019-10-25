// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ��`���W�̎擾
    /// </summary>
    [Serializable]
    public class Rect : ISpace, ICloneable
    {
        #region �����i�V���A���C�Y�L��j

        ///<summary>left��top�̍��W�l</summary>
        protected XyBase _lt;
        ///<summary>right��bottom�̍��W�l</summary>
        protected XyBase _rb;

        #endregion

        #region Data tips for debugging
#if DEBUG
        public string _ => ToString();
#endif
        #endregion

        public static Rect Empty => Rect.FromLTRB(0, 0, 0, 0);

        /// <summary>
        /// LT��RB�������l���ǂ����𒲂ׂ�
        /// </summary>
        public bool IsEmpty => _lt.X == _rb.X && _lt.Y == _rb.Y;

        public virtual XyBase LT
        {

            get => _lt;

            set => _lt = value;
        }

        public virtual XyBase RB
        {

            get => _rb;

            set => _rb = value;
        }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>

        public Rect()
        {
            _lt = new XyBase();
            _rb = new XyBase();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is Rect)
            {
                return _lt.Equals(((Rect)obj)._lt) && _rb.Equals(((Rect)obj)._rb);
            }
            return false;
        }

        /// <summary>
        /// LT����ARB�E���ɂȂ�悤�ɍ��W����������
        /// </summary>
        public void Normalize()
        {
            if (_lt.X > _rb.X)
            {
                var tmp = _lt.X;
                _lt.X = _rb.X;
                _rb.X = tmp;
            }
            if (_lt.Y > _rb.Y)
            {
                var tmp = _lt.Y;
                _lt.Y = _rb.Y;
                _rb.Y = tmp;
            }
        }

        /// <summary>
        /// �C���X�^���X�̌^��RectangleF�^�ɕϊ�����
        /// </summary>
        /// <param name="r">�ϊ��Ώ�</param>
        /// <returns>�V�����^�̃C���X�^���X</returns>

        public static implicit operator RectangleF(Rect r)
        {
            return new RectangleF(r.LT.X, r.LT.Y, r.Width, r.Height);
        }

        /// <summary>
        /// �C���X�^���X�̌^��RectangleF�^�ɕϊ�����
        /// </summary>
        /// <param name="r">�ϊ��Ώ�</param>
        /// <returns>�V�����^�̃C���X�^���X</returns>

        public static implicit operator Rectangle(Rect r)
        {
            return new Rectangle(r.LT.X, r.LT.Y, r.Width, r.Height);
        }

        /// <summary>
        /// �����l����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x">������WX</param>
        /// <param name="y">������WY</param>
        /// <param name="width">��</param>
        /// <param name="height">����</param>
        /// <returns></returns>

        public static Rect FromLTWH(int x, int y, int width, int height)
        {
            var ret = new Rect();
            ret.LT.X = x;
            ret.LT.Y = y;
            ret.RB.X = x + width - 1;
            ret.RB.Y = y + height - 1;
            return ret;
        }

        /// <summary>
        /// ���W���w�肵�ĐV�����C���X�^���X���\�z����
        /// </summary>
        /// <param name="x0">�����X���W</param>
        /// <param name="y0">�����Y���W</param>
        /// <param name="x1">�E����X���W</param>
        /// <param name="y1">�E����Y���W</param>
        /// <returns>�\�z�����C���X�^���X</returns>

        public static Rect FromLTRB(int x0, int y0, int x1, int y1)
        {
            var ret = new Rect();
            ret.LT.X = x0;
            ret.LT.Y = y0;
            ret.RB.X = x1;
            ret.RB.Y = y1;
            return ret;
        }

        /// <summary>
        /// ���̌v�Z
        /// </summary>
        public int Width => RB.X - LT.X + 1;

        /// <summary>
        /// �����̌v�Z
        /// </summary>
        public int Height => RB.Y - LT.Y + 1;

        /// <summary>
        /// ���S�_���擾����
        /// </summary>
        /// <returns></returns>
        public XyBase GetCenter()
        {
            return XyBase.FromInt((LT.X + RB.X) / 2, (LT.Y + RB.Y) / 2);
        }

        public XyBase LB => XyBase.FromInt(LT.X, RB.Y);

        public XyBase RT => XyBase.FromInt(RB.X, LT.Y);

        public RangeXx LR => RangeXx.FromInt(LT.X, RB.X);

        public RangeYy TB => RangeYy.FromInt(LT.Y, RB.Y);

        /// <summary>
        /// �T�C�Y���{�P�����C���X�^���X��Ԃ�
        /// </summary>
        /// <returns>�T�C�Y�{�P�����C���X�^���X</returns>

        public Rect GetPpSize()
        {
            var ret = (Rect)Clone();
            ret.RB.X++;
            ret.RB.Y++;
            return ret;
        }

        /// <summary>
        /// ��������`���ǂ����i���̑傫���ɂȂĂȂ����j���ׂ�
        /// </summary>
        /// <returns>true = �������̈� / false = ���̗̈�</returns>
        //		 
        public bool IsNormalizedRect()
        {
            if (LT.X > RB.X)
            {
                return false;
            }
            if (LT.Y > RB.Y)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// �̈��AND�����
        /// </summary>
        /// <param name="r">�̈�P</param>
        /// <param name="v">�̈�Q</param>
        /// <returns>AND��̐V�����C���X�^���X</returns>

        public static Rect operator &(Rect r1, Rect r2)
        {
            if (r1 == null || r2 == null)
            {
                return null;
            }
            var ret = (Rect)r1.Clone();
            ret.LT.X = r1.LT.X >= r2.LT.X ? r1.LT.X : r2.LT.X;
            ret.LT.Y = r1.LT.Y >= r2.LT.Y ? r1.LT.Y : r2.LT.Y;
            ret.RB.X = r1.RB.X <= r2.RB.X ? r1.RB.X : r2.RB.X;
            ret.RB.Y = r1.RB.Y <= r2.RB.Y ? r1.RB.Y : r2.RB.Y;
            if (ret.IsNormalizedRect() == false)
            {
                return null;
            }
            return ret;
        }

        /// <summary>
        /// �̈��OR�����B���ꂽ�ʒu�ɂ����`���m�ł��e�̈�̍ŏ��E�ő���Ƃ����ЂƂ̋�`��Ԃ��B
        /// </summary>
        /// <param name="r">�̈�P</param>
        /// <param name="v">�̈�Q</param>
        /// <returns>OR��̐V�����C���X�^���X</returns>
        //		
        public static Rect operator |(Rect r1, Rect r2)
        {
            if (r1 == null || r2 == null)
            {
                return null;
            }
            var ret = (Rect)r1.Clone();
            ret.LT.X = r1.LT.X < r2.LT.X ? r1.LT.X : r2.LT.X;
            ret.LT.Y = r1.LT.Y < r2.LT.Y ? r1.LT.Y : r2.LT.Y;
            ret.RB.X = r1.RB.X > r2.RB.X ? r1.RB.X : r2.RB.X;
            ret.RB.Y = r1.RB.Y > r2.RB.Y ? r1.RB.Y : r2.RB.Y;
            if (ret.IsNormalizedRect() == false)
            {
                return null;
            }
            return ret;
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        /// <param name="r">���̃I�u�W�F�N�g</param>
        /// <param name="v">���Z�l</param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>

        public static Rect operator +(Rect r, XyBase v)
        {
            var ret = (Rect)r.Clone();
            ret.Transfer(v);
            return ret;
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        /// <param name="r">���̃I�u�W�F�N�g</param>
        /// <param name="v">���Z�l</param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>

        public static Rect operator -(Rect r, XyBase v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X -= v.X;
            ret.LT.Y -= v.Y;
            ret.RB.X -= v.X;
            ret.RB.Y -= v.Y;
            return ret;
        }

        /// <summary>
        /// ��Z���Z�q
        /// </summary>
        /// <param name="r">���̃I�u�W�F�N�g</param>
        /// <param name="v">���Z�l</param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>

        public static Rect operator *(Rect r, XyBase v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X = r.LT.X * v.X;
            ret.LT.Y = r.LT.Y * v.Y;
            ret.RB.X = r.RB.X * v.X;
            ret.RB.Y = r.RB.Y * v.Y;
            return ret;
        }

        /// <summary>
        /// ��Z���Z�q
        /// </summary>
        /// <param name="r">���̃I�u�W�F�N�g</param>
        /// <param name="v">���Z�l</param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>

        public static Rect operator *(Rect r, int v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X = r.LT.X * v;
            ret.LT.Y = r.LT.Y * v;
            ret.RB.X = r.RB.X * v;
            ret.RB.Y = r.RB.Y * v;
            return ret;
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        /// <param name="r">���̃I�u�W�F�N�g</param>
        /// <param name="v">���Z�l</param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>

        public static Rect operator /(Rect r, int v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X = r.LT.X / v;
            ret.LT.Y = r.LT.Y / v;
            ret.RB.X = r.RB.X / v;
            ret.RB.Y = r.RB.Y / v;
            return ret;
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        /// <param name="r">���̃I�u�W�F�N�g</param>
        /// <param name="v">���Z�l</param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>

        public static Rect operator /(Rect r, XyBase v)
        {
            var ret = (Rect)r.Clone();
            ret.LT.X = r.LT.X / v.X;
            ret.LT.Y = r.LT.Y / v.Y;
            ret.RB.X = r.RB.X / v.X;
            ret.RB.Y = r.RB.Y / v.Y;
            return ret;
        }

        #region ISpace �����o

        /// <summary>
        /// �w�肵���|�C���g���I�u�W�F�N�g�̈���ɂ��邩�ǂ������肷��
        /// </summary>
        /// <param name="o1">uXy�^�̎w��|�C���g</param>
        /// <returns>true:�̈�� / false:�̈�O</returns>
        public bool IsIn(object value)
        {
            if (value is XyBase xy)
            {
                if (LT.X <= xy.X && RB.X >= xy.X && LT.Y <= xy.Y && RB.Y >= xy.Y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (value is Rect tar)
            {
                if (LT.X <= tar.RB.X && RB.X >= tar.LT.X && LT.Y <= tar.RB.Y && RB.Y >= tar.LT.Y)
                {
                    return true;
                }
                if (LT.X >= tar.LT.X && RB.X <= tar.RB.X && LT.Y >= tar.LT.Y && RB.Y <= tar.RB.Y)
                {
                    return true;
                }
                if (tar.LT.X >= LT.X && tar.RB.X <= RB.X && tar.LT.Y >= LT.Y && tar.RB.Y <= RB.Y)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// �I�u�W�F�N�g�̈ړ�
        /// </summary>
        /// <param name="value">uXy�^�̈ړ��l (X,Y)</param>

        public void Transfer(object value)
        {
            System.Diagnostics.Debug.Assert(value is XyBase, "Transfer��uXy�^�����T�|�[�g���Ă��܂�");
            var pt = (XyBase)value;

            //�I�u�W�F�N�g�̈ړ�
            LT.X = LT.X + pt.X;
            LT.Y = LT.Y + pt.Y;
            RB.X = RB.X + pt.X;
            RB.Y = RB.Y + pt.Y;
        }

        /// <summary>
        /// �I�u�W�F�N�g�̊g��
        /// </summary>
        /// <param name="value">uXy�^�̏k���l (X,Y)</param>

        public void Inflate(object value)
        {
            if (value is double vald)
            {
                //�g��
                LT.X = (int)(LT.X - vald);
                LT.Y = (int)(LT.Y - vald);
                RB.X = (int)(RB.X + vald);
                RB.Y = (int)(RB.Y + vald);
            }
            else
            if (value is int vali)
            {
                //�g��
                LT.X = LT.X - vali;
                LT.Y = LT.Y - vali;
                RB.X = RB.X + vali;
                RB.Y = RB.Y + vali;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is XyBase, "Inflate��uXy�^�����T�|�[�g���Ă��܂�");

                var pt = (XyBase)value;

                //�g��
                LT.X = LT.X - pt.X;
                LT.Y = LT.Y - pt.Y;
                RB.X = RB.X + pt.X;
                RB.Y = RB.Y + pt.Y;
            }
        }

        /// <summary>
        /// �I�u�W�F�N�g�̏k��
        /// </summary>
        /// <param name="value">uXy�̊g��l (X,Y)</param>

        public void Deflate(object value)
        {
            if (value is double vald)
            {
                //�k��
                LT.X = (int)(LT.X + vald);
                LT.Y = (int)(LT.Y + vald);
                RB.X = (int)(RB.X - vald);
                RB.Y = (int)(RB.Y - vald);
            }
            else
            if (value is int vali)
            {
                //�k��
                LT.X = LT.X + vali;
                LT.Y = LT.Y + vali;
                RB.X = RB.X - vali;
                RB.Y = RB.Y - vali;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is XyBase, "Deflate��uXy�^�����T�|�[�g���Ă��܂�");

                var pt = (XyBase)value;

                //�k��
                LT.X = LT.X + pt.X;
                LT.Y = LT.Y + pt.Y;
                RB.X = RB.X - pt.X;
                RB.Y = RB.Y - pt.Y;
            }

        }

        #endregion

        #region ICloneable �����o


        public object Clone()
        {
            var r = (Rect)Activator.CreateInstance(GetType());
            r.LT.X = LT.X;
            r.LT.Y = LT.Y;
            r.RB.X = RB.X;
            r.RB.Y = RB.Y;
            return r;
        }

        #endregion

        /// <summary>
        /// �C���X�^���X��\�����镶������쐬����i�\�����@�͕ς��̂ŁA���o�ړI�ɂ̂ݎg�p���邱�Ɓj
        /// </summary>
        /// <returns>������</returns>

        public override string ToString()
        {
            return LT.ToString() + "-" + RB.ToString();
        }
    }
}
