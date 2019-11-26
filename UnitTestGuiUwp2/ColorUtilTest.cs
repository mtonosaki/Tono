
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tono.Gui.Uwp;
using Windows.UI;

namespace UnitTestGuiUwp2
{
    [TestClass]
    public class ColorUtilTest
    {
        [TestMethod]
        public void Test001()
        {
            var c = ColorUtil.From("Red");
            Assert.AreEqual(c, Colors.Red);
            c = ColorUtil.From("0xffff0000");
            Assert.AreEqual(c, Colors.Red);
            c = ColorUtil.From("#ffff0000");
            Assert.AreEqual(c, Colors.Red);
            c = ColorUtil.From("#FFFF0000");
            Assert.AreEqual(c, Colors.Red);
            c = ColorUtil.ChangeAlpha(c, 0.5f);
            Assert.AreEqual(c.A, 127);
        }
    }
}
