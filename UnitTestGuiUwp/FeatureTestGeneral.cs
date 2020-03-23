using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;

namespace UnitTestGuiUwp
{
    public class FeatureTestGeneral : FeatureBase
    {
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();

            Pane.Target = Pane.Main;

            for (var a = Angle.Zero; a.Deg < 360; a += Angle.FromDeg(360 / 12))
            {
                var parts = new PartsTestGeneral
                {
                    Location = CodePos<Distance, Angle>.From(Distance.FromMeter(300), a),
                    PositionerX = MyPositionerX,
                    PositionerY = MyPositionerY,
                    CoderX = MyCoderX,
                    CoderY = MyCoderY,
                };
                if (a.Deg < 180)
                {
                    Parts.Add(Pane.Target, parts, LAYER.SampleLayer1);
                }
                else
                {
                    Parts.Add(Pane.Target, parts, LAYER.SampleLayer2);
                }
            }
        }

        private LayoutX MyPositionerX(CodeX<Distance> x, CodeY<Angle> y)
        {
            return new LayoutX
            {
                Lx = Math.Cos(y.Cy.Rad) * x.Cx.m,
            };
        }

        private LayoutY MyPositionerY(CodeX<Distance> x, CodeY<Angle> y)
        {
            return new LayoutY
            {
                Ly = -Math.Sin(y.Cy.Rad) * x.Cx.m,
            };
        }

        private CodeX<Distance> MyCoderX(LayoutX x, LayoutY y)
        {
            return new CodeX<Distance>
            {
                Cx = Distance.FromMeter(GeoEu.Length(x.Lx, y.Ly)),
            };
        }
        private CodeY<Angle> MyCoderY(LayoutX x, LayoutY y)
        {
            return new CodeY<Angle>
            {
                Cy = GeoEu.Angle(0, 0, x.Lx, y.Ly),
            };
        }
    }
}
