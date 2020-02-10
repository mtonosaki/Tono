// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Tono;
using Tono.Logic;

namespace TestTonoLogic
{
    [TestClass]
    public class IntegerDistributerTest
    {
        [TestMethod]
        public void Test001()
        {
            var ret = new IntegerDistributer(27, 8);
            Assert.AreEqual(ret.Count(), 8);
            Assert.AreEqual(ret.Sum(), 27);
            foreach( var val in ret)
            {
                Assert.AreEqual(val?.GetType(), typeof(int));
            }
        }

        [TestMethod]
        public void Test002()
        {
            var ret = new IntegerDistributer
            {
                1.6,
                1.6,
                2.6,
                1.8,
            };
            ret.Add(1.4); // 5 items. Total = 9.0
            var col = ret.ToArray();
            Assert.AreEqual(col[0], 2);
            Assert.AreEqual(col[1], 2);
            Assert.AreEqual(col[2], 2);
            Assert.AreEqual(col[3], 2);
            Assert.AreEqual(col[4], 1);
        }

        [TestMethod]
        public void Test003()
        {
            var id = new IntegerDistributer(2, 24)
                .SetFirstPriorityMode() // First weight mode
                .Select(a => a.ToString())
                .ToArray();
            Assert.AreEqual(string.Join("", id), "100000000000100000000000");
        }

        [TestMethod]
        public void Test004()
        {
            var id = new IntegerDistributer(2, 24)
                .SetMiddlePriorityMode()    // middle weight mode
                .Select(a => a.ToString())
                .ToArray();
            Assert.AreEqual(string.Join("", id), "000000100000000001000000");
        }
    }
}
