// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tono;

namespace TestTonoCore
{
    [TestClass]
    public class AccelerationTest
    {
        [TestMethod]
        public void Test001()
        {
            var a1 = Acceleration.FromMeterPerScondPerScond(12.3);
            var a2 = Acceleration.Parse("12.3m/s/s");
            Assert.AreEqual(a1, a2);

            var d = 12.3;
            Assert.AreEqual(a1.To_m_per_s_per_s, d);

            var a3 = Acceleration.FromMeterPerScondPerScond(12.2);
            Assert.IsTrue(a3 < a1);
            Assert.IsTrue(a1 > a3);
        }
    }
}
