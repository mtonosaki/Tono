using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono.Gui;
using Tono.Gui.Uwp;

namespace UnitTestGuiUwp
{
    public class FeatureTokenTesterC : FeatureBase
    {
        [EventCatch(TokenID = "MyToken", Name = "NameC")]
        public void NameC(MyToken token)
        {
            LOG.WriteLine(LLV.ERR, "NameC is not expected token.");
        }
    }
}
