
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tono;
using Tono.Gui.Uwp;
using Windows.UI;

namespace UnitTestGuiUwp2
{
    [TestClass]
    public class PartsCollectionTest
    {
        [TestMethod]
        public void Test001()
        {
            var view = new TGuiView();
            view.Parts.Add(view, new TestParts001
            {
                TestID = 1,
            }, NamedId.From("LAYER1", 100));
            view.Parts.Add(view, new TestParts001
            {
                TestID = 2,
            }, NamedId.From("LAYER1", 100));
            view.Parts.Add(view, new TestParts002
            {
                TestID = 3,
            }, NamedId.From("LAYER1", 100));

            var pts = view.Parts.GetParts<TestParts002>(NamedId.From("LAYER1", 100));
            Assert.IsNotNull(pts);
            Assert.AreEqual(pts.Count(), 1);
            var pt = pts.FirstOrDefault();
            Assert.AreEqual(pt.TestID, 3);
        }

        public class TestParts001 : PartsBase<Distance, DateTime>
        {
            public int TestID { get; set; }
            public override void Draw(DrawProperty dp)
            {
            }
        }

        public class TestParts002 : PartsBase<Distance, DateTime>
        {
            public int TestID { get; set; }
            public override void Draw(DrawProperty dp)
            {
            }
        }
    }
}
