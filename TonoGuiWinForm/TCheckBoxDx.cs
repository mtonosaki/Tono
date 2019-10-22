// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
#pragma warning disable 1591, 1572, 1573

    /// <summary>
    /// 
    /// </summary>
    public class TCheckBoxDx : CheckBox
    {
        private Image _onImage = null;
        private Image _orgImage = null;

        /// <summary>
        /// ONéûÇÃÉCÉÅÅ[ÉW
        /// </summary>
		public Image ImageON
        {
            get => _onImage;
            set => _onImage = value;
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            if (_onImage != null)
            {
                if (_orgImage == null)
                {
                    _orgImage = Image;
                }
                Image = Checked ? _onImage : _orgImage;
            }
        }

    }
}
