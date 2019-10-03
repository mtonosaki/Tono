using System;
using System.Diagnostics;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uYy �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class RangeYy : ValueCouple, ISpace
    {
        /// <summary>
        /// �l���w�肵�ăC���X�^���X�����
        /// </summary>
        /// <param name="v1">�l1</param>
        /// <param name="v2">�l2</param>
        /// <returns>�C���X�^���X</returns>
        
        public static new RangeYy FromInt(int v1, int v2)
        {
            var ret = new RangeYy
            {
                Y0 = v1,
                Y1 = v2
            };
            return ret;
        }

        /// <summary>
        /// uRect��LT(=Y0)/RB(=Y1)��Y��p���ăC���X�^���X�𐶐�����
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static RangeYy FromRect(Rect rect)
        {
            return RangeYy.FromInt(rect.LT.Y, rect.RB.Y);
        }

        /// <summary>
        /// ���_
        /// </summary>
        public int Middle => (_v1 + _v2) / 2;

        /// <summary>
        /// ����(�Q�_�̕�)
        /// </summary>
        public int Height => _v2 - _v1;

        /// <summary>
        /// �P�ڂ�Y���W
        /// </summary>
        public int Y0
        {
            
            get => _v1;
            
            set => _v1 = value;
        }

        /// <summary>
        /// �Q�ڂ�Y���W
        /// </summary>
        public int Y1
        {
            
            get => _v2;
            
            set => _v2 = value;
        }
        #region ISpace �����o

        /// <summary>
        ///  �w�肵���|�C���g���I�u�W�F�N�g�̈���ɂ��邩�ǂ������肷��
        /// </summary>
        /// <param name="value">uYy�^�̎w��|�C���g / int�^�̎w��|�C���g</param>
        /// <returns>true:�̈�� / false:�̈�O</returns>
        
        public bool IsIn(object value)
        {
            if (value is int)
            {
                var pt = (int)value;
                if (Y0 <= pt && Y1 >= pt)
                {
                    return true;
                }
                return false;
            }
            if (value is RangeYy)
            {
                var pt = (RangeYy)value;
                if (Y0 <= pt.Y0 && Y1 >= pt.Y1)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// �I�u�W�F�N�g�̈ړ�
        /// </summary>
        /// <param name="value">uYy�^�̈ړ��l (Y0,Y1)</param>
        
        public void Transfer(object value)
        {
            System.Diagnostics.Debug.Assert(value is int, "Transfer��int�^�����T�|�[�g���܂�");
            var pt = (int)value;

            Y0 += pt;
            Y1 += pt;
        }

        /// <summary>
        /// �I�u�W�F�N�g�̊g��
        /// </summary>
        /// <param name="value">uYy�^�̈ړ��l (Y0,Y1)</param>
        
        public void Inflate(object value)
        {
            if (value is int)
            {
                Y0 -= (int)value;
                Y1 += (int)value;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is RangeYy, "Inflate��uYy�^�����T�|�[�g���܂�");
                var pt = (RangeYy)value;

                Y0 -= pt.Y0;
                Y1 += pt.Y1;
            }
        }

        /// <summary>
        /// �I�u�W�F�N�g�̏k��
        /// </summary>
        /// <param name="value">uYy�^�̈ړ��l (Y0,Y1)</param>
        
        public void Deflate(object value)
        {
            if (value is int)
            {
                Y0 += (int)value;
                Y1 -= (int)value;
            }
            else
            {
                System.Diagnostics.Debug.Assert(value is RangeYy, "Inflate��uYy�^�����T�|�[�g���܂�");
                var pt = (RangeYy)value;

                Y0 += pt.Y0;
                Y1 -= pt.Y1;
            }
        }

        #endregion
    }
}
