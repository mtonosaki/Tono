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
    public class GcmDistributerTest
    {
        [TestMethod]
        public void Test001()
        {
            var ret = new GcmDistributer
            {
                ["A"] = 6,
                ["B"] = 3,
                ["C"] = 1,
            }.ToArray();

            Assert.AreEqual(ret.Length, 6 + 3 + 1);
            var s = string.Join("", ret);
            Assert.AreEqual(s, "ABAABACABA");
        }
        [TestMethod]
        public void Test002()
        {
            var gcm = new GcmDistributer
            {
                ["A"] = 3,
                ["B"] = 3,
                ["C"] = 1,
            };
            gcm.Add("A", 2);
            gcm.Add("A", 1);
            var ret = gcm.ToArray();

            Assert.AreEqual(ret.Length, (3 + 2 + 1) + 3 + 1);
            var s = string.Join("", ret);
            Assert.AreEqual(s, "ABAABACABA");
        }
    }
}
