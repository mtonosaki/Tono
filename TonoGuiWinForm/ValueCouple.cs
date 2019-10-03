using System;
using System.Diagnostics;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// u2 �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class ValueCouple : ICloneable
    {
        ///<summary>��ڂ̒l</summary>
        protected int _v1;

        ///<summary>��ڂ̒l</summary>
        protected int _v2;

        #region Data tips for debugging
#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        public string _ => ToString();
#endif
        #endregion

        /// <summary>
        /// ��ڂ̒l
        /// </summary>
        public int Value1
        {
            
            get => _v1;
        }

        /// <summary>
        /// ��ڂ̒l
        /// </summary>
        public int Value2
        {
            
            get => _v2;
        }

        /// <summary>
        /// �l���w�肵�ăC���X�^���X�����
        /// </summary>
        /// <param name="v1">�l�P</param>
        /// <param name="v2">�l�Q</param>
        /// <returns>�C���X�^���X</returns>
        
        public static ValueCouple FromInt(int v1, int v2)
        {
            var ret = new ValueCouple
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ValueCouple item)
            {
                return _v1 == item._v1 && _v2 == item._v2;
            }
            return false;
        }


        /// <summary>
        /// ���W��(0,0)�̔�����s��
        /// </summary>
        /// <returns></returns>
        
        public bool IsZero()
        {
            return (_v1 & _v2) == 0 ? true : false;
        }

        /// <summary>
        /// ���Z���Z�q(u2�^��u2�^�̉��Z)
        /// </summary>
        /// <param name="v1">�l�P</param>
        /// <param name="v2">�l�Q</param>
        /// <returns></returns>
        
        public static ValueCouple operator +(ValueCouple v1, ValueCouple v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 + v2._v1;
            ret._v2 = v1._v2 + v2._v2;

            return ret;
        }

        /// <summary>
        /// ���Z���Z�q(_v1��_v2�̗����ɒl�Q�����Z����)
        /// </summary>
        /// <param name="v1">�l�P</param>
        /// <param name="v2">�l�Q</param>
        /// <returns></returns>
        
        public static ValueCouple operator +(ValueCouple v1, int v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 + v2;
            ret._v2 = v1._v2 + v2;

            return ret;
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator -(ValueCouple v1, int v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 - v2;
            ret._v2 = v1._v2 - v2;

            return ret;
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator -(ValueCouple v1, ValueCouple v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 - v2._v1;
            ret._v2 = v1._v2 - v2._v2;

            return ret;
        }

        /// <summary>
        /// ��Z���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator *(ValueCouple v1, ValueCouple v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 * v2._v1;
            ret._v2 = v1._v2 * v2._v2;

            return ret;
        }

        /// <summary>
        /// ��Z���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator *(ValueCouple v1, double v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = (int)(v2 * v1._v1);
            ret._v2 = (int)(v2 * v1._v2);

            return ret;
        }

        /// <summary>
        /// ��Z���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator *(ValueCouple v1, int v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 * v2;
            ret._v2 = v1._v2 * v2;

            return ret;
        }

        /// <summary>
        /// ���Z���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator /(ValueCouple v1, int v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 / v2;
            ret._v2 = v1._v2 / v2;

            return ret;
        }

        /// <summary>
        /// ��Z���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static ValueCouple operator /(ValueCouple v1, ValueCouple v2)
        {
            var ret = (ValueCouple)v1.Clone();

            ret._v1 = v1._v1 / v2._v1;
            ret._v2 = v1._v2 / v2._v2;

            return ret;
        }

        /// <summary>
        /// ��r���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static bool operator <(ValueCouple v1, ValueCouple v2)
        {
            if (v1._v1 < v2._v1 && v1._v2 < v2._v2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// ��r���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static bool operator >(ValueCouple v1, ValueCouple v2)
        {
            if (v1._v1 > v2._v1 && v1._v2 > v2._v2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ��r���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static bool operator <=(ValueCouple v1, ValueCouple v2)
        {
            if (v1._v1 <= v2._v1 && v1._v2 <= v2._v2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ��r���Z�q
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        
        public static bool operator >=(ValueCouple v1, ValueCouple v2)
        {
            if (v1._v1 >= v2._v1 && v1._v2 >= v2._v2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// �n�b�V���R�[�h�𐶐�����
        /// </summary>
        /// <returns>�n�b�V���R�[�h</returns>
        public override int GetHashCode()
        {
            // _v1 �ɑ΂��A_v2 ��8bits�P�ʂŔ��]�i�G���f�B�A�����]�j�������̂� XOR�����l���n�b�V���R�[�h�Ƃ���
            unchecked
            {
                var ret = (uint)_v1;
                uint mask = 0x00000fff;
                var speed = 12;     // mask�̃r�b�g��
                var s = 0;
                var pp = 32 - speed;

                while (mask != 0)
                {
                    if ((_v2 & mask) != 0)
                    {

                        ret ^= (((((uint)_v2) & mask) >> s) << pp);
                    }
                    mask <<= speed;
                    s += speed;
                    pp -= speed;
                }
                return (int)ret;
            }
        }


        #region ICloneable �����o

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        
        public object Clone()
        {
            var r = (ValueCouple)Activator.CreateInstance(GetType());
            r._v1 = _v1;
            r._v2 = _v2;
            return r;
        }

        #endregion

        /// <summary>
        /// �C���X�^���X��\�����镶������쐬����i�\�����@�͕ς��̂ŁA���o�ړI�ɂ̂ݎg�p���邱�Ɓj
        /// </summary>
        /// <returns>������</returns>
        
        public override string ToString()
        {
            return "(" + _v1 + ", " + _v2 + ")";
        }
    }
}
