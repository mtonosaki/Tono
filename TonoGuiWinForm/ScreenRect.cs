using System;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uScRect の概要の説明です。
    /// </summary>
    [Serializable]
    public class ScreenRect : Rect
    {
        public static new ScreenRect Empty => ScreenRect.FromLTRB(0, 0, 0, 0);

        /// <summary>
        /// デフォルトコンストラクタ
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
        /// コントロールからインスタンスを生成する
        /// </summary>
        /// <param name="c">コントロール</param>
        /// <returns>新しいインスタンス</returns>

        public static ScreenRect FromControl(Control c)
        {
            var ret = ScreenRect.FromLTWH(c.Left, c.Top, c.Width, c.Height);
            return ret;
        }

        /// <summary>
        /// 整数値からインスタンスを構築する
        /// </summary>
        /// <param name="x">左上座標X</param>
        /// <param name="y">左上座標Y</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>新しいインスタンス</returns>

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
        /// 中心を取得する
        /// </summary>
        /// <returns></returns>
        public new ScreenPos GetCenter()
        {
            var xy = base.GetCenter();
            return ScreenPos.FromInt(xy.X, xy.Y);
        }

        /// <summary>
        /// 座標を指定して新しいインスタンスを構築する
        /// </summary>
        /// <param name="x0">左上のX座標</param>
        /// <param name="y0">左上のY座標</param>
        /// <param name="x1">右下のX座標</param>
        /// <param name="y1">右下のY座標</param>
        /// <returns>構築したインスタンス</returns>

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
        /// Rectangleからインスタンスを構築する
        /// </summary>
        /// <param name="r">Rectangle型の元の値</param>
        /// <returns>新しいインスタンス</returns>
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
        /// 演算子オーバーロード
        /// </summary>
        public static ScreenRect operator &(ScreenRect r1, Rect r2) { return (ScreenRect)((Rect)r1 & r2); }
        public static ScreenRect operator +(ScreenRect r1, XyBase r2) { return (ScreenRect)((Rect)r1 + r2); }
        public static ScreenRect operator -(ScreenRect r1, XyBase r2) { return (ScreenRect)((Rect)r1 - r2); }
        public static ScreenRect operator *(ScreenRect r1, XyBase r2) { return (ScreenRect)((Rect)r1 * r2); }
        public static ScreenRect operator /(ScreenRect r1, XyBase r2) { return (ScreenRect)((Rect)r1 / r2); }
    }
}
