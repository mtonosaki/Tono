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
        /// 中心点と半径を指定して正方形の座標を作成する
        /// </summary>
        /// <param name="x">中心点（X)</param>
        /// <param name="y">中心点（Y）</param>
        /// <param name="r">半径</param>
        /// <returns>新しいインスタンス</returns>
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
        /// 整数値からインスタンスを構築する
        /// </summary>
        /// <param name="x">左上座標X</param>
        /// <param name="y">左上座標Y</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
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
        /// 整数値からインスタンスを構築する
        /// </summary>
        /// <param name="x">左上座標X</param>
        /// <param name="y">左上座標Y</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns></returns>

        public static CodeRect FromLTWH(double x, double y, double width, double height)
        {
            return FromLTWH((int)x, (int)y, (int)width, (int)height);
        }

        /// <summary>
        /// 整数値からインスタンスを構築する
        /// </summary>
        /// <param name="x">左上座標X</param>
        /// <param name="y">左上座標Y</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
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
        /// 演算子オーバーロード
        /// </summary>
        public static CodeRect operator &(CodeRect r1, Rect r2) { return (CodeRect)((Rect)r1 & r2); }
        public static CodeRect operator +(CodeRect r1, XyBase r2) { return (CodeRect)((Rect)r1 + r2); }
        public static CodeRect operator -(CodeRect r1, XyBase r2) { return (CodeRect)((Rect)r1 - r2); }
        public static CodeRect operator *(CodeRect r1, XyBase r2) { return (CodeRect)((Rect)r1 * r2); }
        public static CodeRect operator *(CodeRect r1, int r2) { return (CodeRect)((Rect)r1 * r2); }
        public static CodeRect operator /(CodeRect r1, XyBase r2) { return (CodeRect)((Rect)r1 / r2); }
    }
}
