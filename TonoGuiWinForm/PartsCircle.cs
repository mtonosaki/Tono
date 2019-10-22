// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PartsCircle : PartsBase
    {
        #region		属性(シリアライズする)
        /** <summary>線の色</summary> */
        private Color _color = Color.Blue;
        #endregion

        /// <summary>
        /// 塗りつぶしの色
        /// </summary>
        public Color BackColor
        {
            get => _color;
            set => _color = value;
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        public override bool Draw(IRichPane rp)
        {
            var spos = GetScRect(rp);
            spos.Normalize();

            // 円にする
            if (spos.Width < spos.Height)
            {
                var d = (spos.Height - spos.Width) / 2;
                spos.LT.Y += d;
                spos.RB.Y -= d;
            }
            else
            {
                var d = (spos.Width - spos.Height) / 2;
                spos.LT.X += d;
                spos.RB.X -= d;
            }

            if (isInClip(rp, spos) == false)    // 描画不要であれば、なにもしない
            {
                return false;
            }
            using (Brush brush = new SolidBrush(_color))
            {
                rp.Graphics.FillEllipse(brush, spos.LT.X, spos.LT.Y, spos.Width, spos.Height); // 楕円を描画
            }
            return true;
        }
    }
}
