// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class AngleTest
    {
        [TestMethod]
        public void Test001()
        {
            var a1 = Angle.FromDeg(180);
            Assert.AreEqual(a1.Rad, Math.PI);

            var a2 = Angle.FromRad(Angle.PI);
            Assert.AreEqual(a1, a2);

            Assert.AreEqual(Angle.From((0, 0), (+1, +0)), Angle.FromDeg(0));
            Assert.AreEqual(Angle.From((0, 0), (+1, -1)), Angle.FromDeg(45));
            Assert.AreEqual(Angle.From((0, 0), (+0, -1)), Angle.FromDeg(90));
            Assert.AreEqual(Angle.From((0, 0), (-1, -1)), Angle.FromDeg(135));
            Assert.AreEqual(Angle.From((0, 0), (-1, +0)), Angle.FromDeg(180));
            Assert.AreEqual(Angle.From((0, 0), (-1, +1)), Angle.FromDeg(225));
            Assert.AreEqual(Angle.From((0, 0), (+0, +1)), Angle.FromDeg(270));
            Assert.AreEqual(Angle.From((0, 0), (+1, +1)), Angle.FromDeg(315));
        }
    }
}
