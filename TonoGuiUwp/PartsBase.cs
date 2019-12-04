// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// MVFP model, PARTS base class
    /// </summary>
    public abstract class PartsBase<TCX, TCY> : IPartsDraw
    {
        /// <summary>
        /// function of code converter to layout coodinates
        /// コード座標をレイアウト座標に変換する関数
        /// </summary>
        public Func<CodeX<TCX>, CodeY<TCY>, LayoutX> PositionerX { get; set; }

        /// <summary>
        /// function of code converter to layout coodinates
        /// コード座標をレイアウト座標に変換する
        /// </summary>
        public Func<CodeX<TCX>, CodeY<TCY>, LayoutY> PositionerY { get; set; }

        /// <summary>
        /// function of layout position converter to code
        /// レイアウト座標をコード座標に変換する
        /// </summary>
        public Func<LayoutX, LayoutY, CodeX<TCX>> CoderX { get; set; }

        /// <summary>
        /// function of layout position converter to code
        /// レイアウト座標をコード座標に変換する
        /// </summary>
        public Func<LayoutX, LayoutY, CodeY<TCY>> CoderY { get; set; }

        /// <summary>
        /// parts location (code coodinates)
        /// </summary>
        public virtual CodePos<TCX, TCY> Location { get; set; }

        /// <summary>
        /// display priority (LOWER:0  --- 9999:HIGHER priority)
        /// </summary>
        public virtual uint ZIndex { get; set; } = 0;

        public override bool Equals(object obj)
        {
            return object.ReferenceEquals(this, obj);
        }

        /// <summary>
        /// hash code generator
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public override string ToString()
        {
            return $"{GetType().Name} L={Location}";
        }

        /// <summary>
        /// get screen position
        /// </summary>
        /// <param name="pane"></param>
        /// <returns></returns>
        public virtual ScreenPos GetScreenPos(IDrawArea pane, CodePos<TCX, TCY> codepos)
        {
            var lpos = new LayoutPos
            {
                X = PositionerX(codepos.X, codepos.Y),
                Y = PositionerY(codepos.X, codepos.Y),
            };
            return ScreenPos.From(pane, lpos);
        }

        /// <summary>
        /// caluclate the maximum screen rectangle of the all specified partsset
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="partsset"></param>
        /// <returns></returns>
        public static ScreenRect GetScreenPosArea(IDrawArea pane, IEnumerable<PartsBase<TCX, TCY>> partsset)
        {
            float l = float.PositiveInfinity, t = float.PositiveInfinity, r = float.NegativeInfinity, b = float.NegativeInfinity;
            foreach (var parts in partsset)
            {
                var spos = parts.GetScreenPos(pane);
                if (spos.X < l)
                {
                    l = spos.X;
                }

                if (spos.Y < t)
                {
                    t = spos.Y;
                }

                if (spos.X > r)
                {
                    r = spos.X;
                }

                if (spos.Y > b)
                {
                    b = spos.Y;
                }
            }
            if (float.IsPositiveInfinity(l))
            {
                return ScreenRect.FromLTRB(0, 0, 0, 0);
            }
            else
            {
                return ScreenRect.FromLTRB(l, t, r, b);
            }
        }

        /// <summary>
        /// Screen position of the pane
        /// </summary>
        /// <param name="pane"></param>
        /// <returns></returns>
        public ScreenPos GetScreenPos(IDrawArea pane)
        {
            return GetScreenPos(pane, Location);
        }

        /// <summary>
        /// To Support Rectangle overlap check
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="sr">check rectangle area</param>
        /// <returns>this parts is in sr</returns>
        /// <remarks>
        /// Using by FeaturePartsSelectOnRect
        /// </remarks>
        public virtual bool IsIn(IDrawArea pane, ScreenRect sr)
        {
            return false;
        }

        /// <summary>
        /// Get code position from screen position
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="spos">screen position</param>
        /// <returns></returns>
        public CodePos<TCX, TCY> GetCodePos(IDrawArea pane, ScreenPos spos)
        {
            var lpos = LayoutPos.From(pane, spos);
            return CodePos<TCX, TCY>.From(CoderX(lpos.X, lpos.Y), CoderY(lpos.X, lpos.Y));
        }

        /// <summary>
        /// Get code position X
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="spos"></param>
        /// <returns></returns>
        public CodeX<TCX> GetCodeX(IDrawArea pane, ScreenPos spos)
        {
            var lpos = LayoutPos.From(pane, spos);
            return CoderX(lpos.X, lpos.Y);
        }

        /// <summary>
        /// Get code position Y
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="spos"></param>
        /// <returns></returns>
        public CodeY<TCY> GetCodeY(IDrawArea pane, ScreenPos spos)
        {
            var lpos = LayoutPos.From(pane, spos);
            return CoderY(lpos.X, lpos.Y);
        }

        /// <summary>
        /// Gui shared asset reference
        /// </summary>
        public GuiAssets Assets { get; set; }

        /// <summary>
        /// draw evnt base method
        /// </summary>
        public abstract void Draw(DrawProperty dp);
    }
}
