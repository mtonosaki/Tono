using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono;

namespace UnitTestGuiUwp
{
    public static class LAYER
    {
        public static readonly NamedId SampleLayer1 = NamedId.From("SampleLayer1", 100);
        public static readonly NamedId SampleLayer2 = NamedId.From("SampleLayer2", 101);

        public static readonly NamedId[] SampleLayers = new[] { SampleLayer1, SampleLayer2 };

        public static readonly NamedId SampleSelectMaskLayer = NamedId.From("SampleSelectMaskLayer", 110);
    }
}
