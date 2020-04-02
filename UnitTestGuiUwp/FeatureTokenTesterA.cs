using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono.Gui;
using Tono.Gui.Uwp;

namespace UnitTestGuiUwp
{
    public class FeatureTokenTesterA : FeatureBase, IPointerListener
    {
        public void OnPointerHold(PointerState po)
        {
        }

        public void OnPointerMoved(PointerState po)
        {
        }

        public void OnPointerPressed(PointerState po)
        {
            Token.Link(po, new MyToken
            {
                Sender = this,
                TokenID = "MyToken",
                Name = "NameB",
                Remarks = "Expectiong Name filter A",
            });
        }

        public void OnPointerReleased(PointerState po)
        {
        }
    }
    public class MyToken : EventTokenTrigger, ITokenNameKey
    {
        public string Name { get; set; }
    }
}
