using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;

namespace UnitTestGuiUwp
{
    public class PartsTestGeneral : PartsBase<Distance, Angle>, ISelectableParts, IMovableParts
    {
        public bool IsSelected { get; set; }
        private float DotR = 20;

        public override void Draw(DrawProperty dp)
        {
            var s0 = GetScreenPos(dp.Pane, CodePos<Distance, Angle>.From(Distance.Zero, Angle.Zero));
            var s1 = GetScreenPos(dp.Pane);
            dp.Graphics.DrawLine(s0 - ScreenX.From(-10000), s0 + ScreenX.From(10000), Colors.Gray);
            dp.Graphics.DrawLine(s0 - ScreenY.From(-10000), s0 + ScreenY.From(10000), Colors.Gray);
            dp.Graphics.DrawLine(s0, s1, Colors.Red);
            dp.Graphics.FillCircle(s1, DotR, Colors.Magenta);
            if (IsSelected)
            {
                dp.Graphics.DrawCircle(s1, DotR, Colors.Blue, 5.0f);
            }
        }

        /// <summary>
        /// selection score
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public float SelectingScore(IDrawArea pane, ScreenPos pos)
        {
            var s1 = GetScreenPos(pane);
            return (float)GeoEu.Length(s1.X.Sx, s1.Y.Sy, pos.X.Sx, pos.Y.Sy) / DotR;
        }

        private CodePos<Distance, Angle> PositionBackup { get; set; }

        public void SaveLocationAsOrigin()
        {
            PositionBackup = Location;
            LOG.WriteLine(LLV.INF, $"Move Start from {Location}");
        }

        public void Move(IDrawArea pane, ScreenSize offset)
        {
            var s0 = GetScreenPos(pane, PositionBackup);
            var s1 = s0 + offset;
            var l1 = LayoutPos.From(pane, s1);
            var cl = CoderX(l1.X, l1.Y);
            var ca = CoderY(l1.X, l1.Y);
            Location = CodePos<Distance, Angle>.From(cl, ca);
        }

        public bool IsMoved()
        {
            return !PositionBackup.Equals(Location);
        }
    }
}
