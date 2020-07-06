// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class GeoEuTest
    {
        [TestMethod]
        public void Test001()
        {
            var k = 1.0 / Math.Sqrt(2);
            foreach (var dxy in new (double Deg, double X, double Y)[]
            {
                (45 + 360 * 20, 1, 1),
                (-45 - 360 * 19, 1, -1 ),
                (-45, 1, -1 ),
                (0, 1, 0),
                (45, 1, 1),
                (90, 0, 1),
                (135, -1, 1),
                (180, -1, 0),
                (225, -1, -1),
                (270, 0, -1),
            })
            {
                var pos = GeoEu.GetLocationOfInscribedSquareInCircle(Angle.FromDeg(dxy.Deg));
                Assert.IsTrue(Math.Abs(pos.X - dxy.X * k) < 0.001);
                Assert.IsTrue(Math.Abs(pos.Y - dxy.Y * k) < 0.001);
            }
        }
    }
}
