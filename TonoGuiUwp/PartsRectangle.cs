// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Standard parts : Rectangle base class
    /// </summary>
    /// <typeparam name="TCL">left position(code coodinates)</typeparam>
    /// <typeparam name="TCT">top position(code coodinates)</typeparam>
    /// <typeparam name="TCR">right position(code coodinates)</typeparam>
    /// <typeparam name="TCB">bottom position(code coodinates)</typeparam>
    /// <remarks>
    /// Location = Left-Top
    /// </remarks>
    public abstract class PartsRectangleBase<TCL, TCT, TCR, TCB> : PartsBase<TCL, TCT>
    {
        /// <summary>
        /// parts location Left-Top
        /// </summary>
        public virtual CodePos<TCL, TCT> LT { get => Location; set => Location = value; }

        /// <summary>
        /// parts locaiton Right-Bottom
        /// </summary>
        public virtual CodePos<TCR, TCB> RB { get; set; }

        /// <summary>
        /// left location
        /// </summary>
        public CodeX<TCL> Left { get => LT.X; set => LT.X = value; }

        /// <summary>
        /// top location
        /// </summary>
        public CodeY<TCT> Top { get => LT.Y; set => LT.Y = value; }

        /// <summary>
        /// right location
        /// </summary>
        public CodeX<TCR> Right { get => RB.X; set => RB.X = value; }

        /// <summary>
        /// bottom location
        /// </summary>
        public CodeY<TCB> Bottom { get => RB.Y; set => RB.Y = value; }

        /// <summary>
        /// coodinate converter from layout to code
        /// </summary>
        public Func<CodeX<TCL>, CodeY<TCT>, LayoutX> PositionerL { get => PositionerX; set => PositionerX = value; }

        /// <summary>
        /// coodinate converter from layout to code
        /// </summary>
        public Func<CodeX<TCL>, CodeY<TCT>, LayoutY> PositionerT { get => PositionerY; set => PositionerY = value; }

        /// <summary>
        /// coodinate converter from layout to code
        /// </summary>
        public Func<CodeX<TCR>, CodeY<TCB>, LayoutX> PositionerR { get; set; }

        /// <summary>
        /// coodinate converter from layout to code
        /// </summary>
        public Func<CodeX<TCR>, CodeY<TCB>, LayoutY> PositionerB { get; set; }

        /// <summary>
        /// coodinate converter from code to layout
        /// </summary>
        public Func<LayoutX, LayoutY, CodeX<TCL>> CoderL { get => CoderX; set => CoderX = value; }

        /// <summary>
        /// coodinate converter from code to layout
        /// </summary>
        public Func<LayoutX, LayoutY, CodeY<TCT>> CoderT { get => CoderY; set => CoderY = value; }

        /// <summary>
        /// coodinate converter from code to layout
        /// </summary>
        public Func<LayoutX, LayoutY, CodeX<TCL>> CoderR { get; set; }

        /// <summary>
        /// coodinate converter from code to layout
        /// </summary>
        public Func<LayoutX, LayoutY, CodeY<TCT>> CoderB { get; set; }

        /// <summary>
        /// get screen rectangle area of specific pane
        /// </summary>
        /// <param name="pane"></param>
        /// <returns></returns>
        public ScreenRect GetScreenRect(IDrawArea pane)
        {
            var ll = PositionerL(Left, Top);
            var lt = PositionerT(Left, Top);
            var lr = PositionerR(Right, Bottom);
            var lb = PositionerB(Right, Bottom);
            return new ScreenRect
            {
                LT = ScreenPos.From(pane, LayoutPos.From(ll, lt)),
                RB = ScreenPos.From(pane, LayoutPos.From(lr, lb)),
            };
        }
    }

    /// <summary>
    /// Rentangle parts
    /// </summary>
    /// <typeparam name="TCL">left position(code coodinates)</typeparam>
    /// <typeparam name="TCT">top position(code coodinates)</typeparam>
    /// <typeparam name="TCR">right position(code coodinates)</typeparam>
    /// <typeparam name="TCB">bottom position(code coodinates)</typeparam>
    public class PartsRectangle<TCL, TCT, TCR, TCB> : PartsRectangleBase<TCL, TCT, TCR, TCB>
    {
        /// <summary>
        /// sample drawing
        /// </summary>
        /// <param name="pane"></param>
        public override void Draw(DrawProperty pane)
        {
            var sr = GetScreenRect(pane.Pane);
            pane.Graphics.FillRectangle(_(sr), Colors.Blue);
            pane.Graphics.DrawRectangle(_(sr), Colors.Red);
        }
    }
}
