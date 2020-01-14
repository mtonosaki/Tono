// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 縦軸、横軸の座標を変換する支援を行うクラス
    /// </summary>
    public class CodeConverterRowCol
    {
        /// <summary>水平方向 符号→パーツ座標変換デリゲート型</summary>
        public delegate int ColCodeToPtPosFunction(int x);
        /// <summary>水平方向 パーツ座標→符号変換デリゲート型</summary>
        public delegate int ColPtPosToCodeFunction(int x);
        /// <summary>垂直方向 符号→パーツ座標変換デリゲート型</summary>
        public delegate int RowCodeToPtPosFunction(int y);
        /// <summary>垂直方向 パーツ座標→符号変換デリゲート型</summary>
        public delegate int RowPtPosToCodeFunction(int y);

        /// <summary>水平方向 符号→パーツ座標変換</summary>
        public ColCodeToPtPosFunction ColCodeToPtPos = null;
        /// <summary>水平方向 パーツ座標→符号変換</summary>
        public ColPtPosToCodeFunction ColPtPosToCode = null;
        /// <summary>垂直方向 符号→パーツ座標変換</summary>
        public RowCodeToPtPosFunction RowCodeToPtPos = null;
        /// <summary>垂直方向 パーツ座標→符号変換</summary>
        public RowPtPosToCodeFunction RowPtPosToCode = null;

        /// <summary>
        /// 符号化された値をパーツ座標に変換する
        /// </summary>
        /// <param name="codeX">符号化されたXの値</param>
        /// <returns>パーツX座標</returns>
        public int GetPtX(int codeX)
        {
            if (ColCodeToPtPos != null)
            {
                return ColCodeToPtPos(codeX);
            }
            return codeX;
        }

        /// <summary>
        /// パーツ座標を符号化された値に変換する
        /// </summary>
        /// <param name="partsX">パーツX座標</param>
        /// <returns>符号化されたXの値</returns>
        public int GetCodeX(int partsX)
        {
            if (ColPtPosToCode != null)
            {
                return ColPtPosToCode(partsX);
            }
            return partsX;
        }

        /// <summary>
        /// 符号化された値をパーツ座標に変換する
        /// </summary>
        /// <param name="codeY">符号化されたYの値</param>
        /// <returns>パーツY座標</returns>
        public int GetPtY(int codeY)
        {
            if (RowCodeToPtPos != null)
            {
                return RowCodeToPtPos(codeY);
            }
            return codeY;
        }

        /// <summary>
        /// パーツ座標を符号化された値に変換する
        /// </summary>
        /// <param name="partsY">パーツY座標</param>
        /// <returns>符号化されたYの値</returns>
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
