using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono.Gui;
using Tono.Gui.Uwp;

namespace UnitTestGuiUwp
{
    public class FeatureTokenTesterB : FeatureBase
    {
        [EventCatch(TokenID = "MyToken", Name = "NameB")]
        public void NameB(MyToken token)
        {
            LOG.WriteLine(LLV.INF, "NameB OK.");
        }
    }
}
