// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    public class PartsImage : PartsBase
    {
        private Image _img = null;

        public Image Image
        {
            get => _img;
            set => _img = value;
        }

        public override bool Draw(IRichPane rp)
        {
            if (_img != null)
            {
                var spos = GetScRect(rp);
                rp.Graphics.DrawImage(_img, spos.LT.X, spos.LT.Y, spos.Width, spos.Height);
            }
            return true;
        }
    }
}
