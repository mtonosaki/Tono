// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �c���A�����̍��W��ϊ�����x�����s���N���X
    /// </summary>
    public class CodeConverterRowCol
    {
        /// <summary>�������� �������p�[�c���W�ϊ��f���Q�[�g�^</summary>
        public delegate int ColCodeToPtPosFunction(int x);
        /// <summary>�������� �p�[�c���W�������ϊ��f���Q�[�g�^</summary>
        public delegate int ColPtPosToCodeFunction(int x);
        /// <summary>�������� �������p�[�c���W�ϊ��f���Q�[�g�^</summary>
        public delegate int RowCodeToPtPosFunction(int y);
        /// <summary>�������� �p�[�c���W�������ϊ��f���Q�[�g�^</summary>
        public delegate int RowPtPosToCodeFunction(int y);

        /// <summary>�������� �������p�[�c���W�ϊ�</summary>
        public ColCodeToPtPosFunction ColCodeToPtPos = null;
        /// <summary>�������� �p�[�c���W�������ϊ�</summary>
        public ColPtPosToCodeFunction ColPtPosToCode = null;
        /// <summary>�������� �������p�[�c���W�ϊ�</summary>
        public RowCodeToPtPosFunction RowCodeToPtPos = null;
        /// <summary>�������� �p�[�c���W�������ϊ�</summary>
        public RowPtPosToCodeFunction RowPtPosToCode = null;

        /// <summary>
        /// ���������ꂽ�l���p�[�c���W�ɕϊ�����
        /// </summary>
        /// <param name="codeX">���������ꂽX�̒l</param>
        /// <returns>�p�[�cX���W</returns>
        public int GetPtX(int codeX)
        {
            if (ColCodeToPtPos != null)
            {
                return ColCodeToPtPos(codeX);
            }
            return codeX;
        }

        /// <summary>
        /// �p�[�c���W�𕄍������ꂽ�l�ɕϊ�����
        /// </summary>
        /// <param name="partsX">�p�[�cX���W</param>
        /// <returns>���������ꂽX�̒l</returns>
        public int GetCodeX(int partsX)
        {
            if (ColPtPosToCode != null)
            {
                return ColPtPosToCode(partsX);
            }
            return partsX;
        }

        /// <summary>
        /// ���������ꂽ�l���p�[�c���W�ɕϊ�����
        /// </summary>
        /// <param name="codeY">���������ꂽY�̒l</param>
        /// <returns>�p�[�cY���W</returns>
        public int GetPtY(int codeY)
        {
            if (RowCodeToPtPos != null)
            {
                return RowCodeToPtPos(codeY);
            }
            return codeY;
        }

        /// <summary>
        /// �p�[�c���W�𕄍������ꂽ�l�ɕϊ�����
        /// </summary>
        /// <param name="partsY">�p�[�cY���W</param>
        /// <returns>���������ꂽY�̒l</returns>
        public int GetCodeY(int partsY)
        {
            if (RowPtPosToCode != null)
            {
                return RowPtPosToCode(partsY);
            }
            return partsY;
        }
    }
}
