// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tono;
using Tono.Logic;

namespace TestTonoLogic
{
    [TestClass]
    public class CumulativeSumTest
    {
        [TestMethod]
        public void Test001()
        {
            var dat = new[] {
                5.2,
                3.7,
                8.4,
                7.6,
                2.3,
            };
            var solver = new CumulativeSum
            {
                Data = dat,
            };
            Assert.AreEqual(solver.Get(0), 0);
            Assert.AreEqual(solver.Get(1), dat[0]);
            Assert.AreEqual(solver.Get(2), dat[0] + dat[1]);
            Assert.AreEqual(solver.Get(3), dat[0] + dat[1] + dat[2]);
            Assert.AreEqual(solver.Get(4), dat[0] + dat[1] + dat[2] + dat[3]);
            Assert.AreEqual(solver.Get(5), dat[0] + dat[1] + dat[2] + dat[3] + dat[4]);
        }

        [TestMethod]
        public void Test002()
        {
            var dat = new[] {
                5.2,
                3.7,
                8.4,
                7.6,
                2.3,
            };
            var solver = new CumulativeSum
            {
                Data = dat,
            };
            Assert.AreEqual(solver.Get(5), dat[0] + dat[1] + dat[2] + dat[3] + dat[4]);
            Assert.AreEqual(solver.Get(4), dat[0] + dat[1] + dat[2] + dat[3]);
            Assert.AreEqual(solver.Get(3), dat[0] + dat[1] + dat[2]);
            Assert.AreEqual(solver.Get(2), dat[0] + dat[1]);
            Assert.AreEqual(solver.Get(1), dat[0]);
            Assert.AreEqual(solver.Get(0), 0);
        }

        [TestMethod]
        public void Test003()
        {
            var dat = new[] {
                5.2,
                3.7,
                8.4,
                7.6,
                2.3,
            };
            var solver = new CumulativeSum
            {
                Data = dat,
            }.Prepare();

            Assert.AreEqual(solver.Get(0), 0);
            Assert.AreEqual(solver.Get(1), dat[0]);
            Assert.AreEqual(solver.Get(2), dat[0] + dat[1]);
            Assert.AreEqual(solver.Get(3), dat[0] + dat[1] + dat[2]);
            Assert.AreEqual(solver.Get(4), dat[0] + dat[1] + dat[2] + dat[3]);
            Assert.AreEqual(solver.Get(5), dat[0] + dat[1] + dat[2] + dat[3] + dat[4]);
        }
        [TestMethod]
        public void Test004()
        {
            var dat = new[] {
                5.2,
                3.7,
                8.4,
                7.6,
                2.3,
            };
            var solver = new CumulativeSum
            {
                Data = dat,
            }.Prepare();

            Assert.AreEqual(solver.SectionTotal(0, 0), 0);
            Assert.AreEqual(solver.SectionTotal(0, 1), dat[0]);
            Assert.AreEqual(Math.Round(solver.SectionTotal(0, 5), 5), Math.Round(dat[0] + dat[1] + dat[2] + dat[3] + dat[4], 5));
            Assert.AreEqual(Math.Round(solver.SectionTotal(1, 5), 5), Math.Round(dat[1] + dat[2] + dat[3] + dat[4], 5));
            Assert.AreEqual(Math.Round(solver.SectionTotal(1, 4), 5), Math.Round(dat[1] + dat[2] + dat[3], 5));
            Assert.AreEqual(Math.Round(solver.SectionTotal(2, 3), 5), dat[2]);
        }
    }
}
