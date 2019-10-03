using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// CoupleObject �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class CoupleObject
    {
        public object V1 = null;
        public object V2 = null;

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        /// <param name="value1">�Q�ƒl�P</param>
        /// <param name="value2">�Q�ƒl�Q</param>
        public CoupleObject(object value1, object value2)
        {
            V1 = value1;
            V2 = value2;
        }

        /// <summary>
        /// �C���X�^���X���\�z����
        /// </summary>
        /// <param name="value1">�Q�ƒl�P</param>
        /// <param name="value2">�Q�ƒl�Q</param>
        /// <returns>�V�����C���X�^���X</returns>
        public static CoupleObject FromObjects(object value1, object value2)
        {
            return new CoupleObject(value1, value2);
        }
    }

    /// <summary>
    /// �^����肵���Q�l�O���[�s���O
    /// </summary>
    /// <typeparam name="T1">�P�Ԗڂ̒l�̌^</typeparam>
    /// <typeparam name="T2">�Q�Ԗڂ̒l�̌^</typeparam>
    public class uCouple<T1, T2>
    {
        public T1 V1;
        public T2 V2;

        /// <summary>
        /// �\�z�q
        /// </summary>
        /// <param name="value1">�Q�ƒl�P</param>
        /// <param name="value2">�Q�ƒl�Q</param>
        public uCouple(T1 value1, T2 value2)
        {
            V1 = value1;
            V2 = value2;
        }

        public override int GetHashCode()
        {
            var h1 = V1.GetHashCode();
            var h2 = V2.GetHashCode();
            h2 <<= 16;
            return h1 ^ h2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is uCouple<T1, T2> tar))
            {
                return false;
            }
            if (V1 == null)
            {
                return tar.V1 == null;
            }
            if (V2 == null)
            {
                return tar.V2 == null;
            }
            return V1.Equals(tar.V1) && V2.Equals(tar.V2);
        }

        public override string ToString()
        {
            return string.Format("{{{0},{1}}}", V1, V2);
        }
    }
}
