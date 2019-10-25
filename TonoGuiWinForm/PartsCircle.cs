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
        #region		����(�V���A���C�Y����)
        /** <summary>���̐F</summary> */
        private Color _color = Color.Blue;
        #endregion

        /// <summary>
        /// �h��Ԃ��̐F
        /// </summary>
        public Color BackColor
        {
            get => _color;
            set => _color = value;
        }

        /// <summary>
        /// �`�揈��
        /// </summary>
        public override bool Draw(IRichPane rp)
        {
            var spos = GetScRect(rp);
            spos.Normalize();

            // �~�ɂ���
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

            if (isInClip(rp, spos) == false)    // �`��s�v�ł���΁A�Ȃɂ����Ȃ�
            {
                return false;
            }
            using (Brush brush = new SolidBrush(_color))
            {
                rp.Graphics.FillEllipse(brush, spos.LT.X, spos.LT.Y, spos.Width, spos.Height); // �ȉ~��`��
            }
            return true;
        }
    }
}
