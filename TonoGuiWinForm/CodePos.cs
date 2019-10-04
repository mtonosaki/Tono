using System;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uPtPos の概要の説明です。
    /// </summary>
    [Serializable]
    public class CodePos : XyBase
    {
        /// <summary>
        /// 値を指定してインスタンスを作る
        /// </summary>
        /// <param name="v1">値１</param>
        /// <param name="v2">値２</param>
        /// <returns>インスタンス</returns>

        public static new CodePos FromInt(int v1, int v2)
        {
            var ret = new CodePos
            {
                _v1 = v1,
                _v2 = v2
            };
            return ret;
        }
        /// <summary>演算子のオーバーロード</summary>
        public static CodePos operator +(CodePos v1, ValueCouple v2) { return (CodePos)((ValueCouple)v1 + v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static CodePos operator -(CodePos v1, ValueCouple v2) { return (CodePos)((ValueCouple)v1 - v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static CodePos operator *(CodePos v1, ValueCouple v2) { return (CodePos)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static CodePos operator *(CodePos v1, int v2) { return (CodePos)((ValueCouple)v1 * v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static CodePos operator /(CodePos v1, ValueCouple v2) { return (CodePos)((ValueCouple)v1 / v2); }
        /// <summary>演算子のオーバーロード</summary>
        public static CodePos operator /(CodePos v1, int v2) { return (CodePos)((ValueCouple)v1 / v2); }
    }
}
