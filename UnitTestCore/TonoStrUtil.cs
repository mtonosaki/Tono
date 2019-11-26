// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using Tono;

namespace UnitTestProject2
{
    [TestClass]
    public class TonoStrUtil
    {
        [TestMethod]
        public void Test001()
        {
            var t1 = StrUtil.SplitConsideringDoubleQuatation("aaa bbb  ccc", new[] { ' ', '=' }, true, false);
            Assert.AreEqual(t1.Length, 4);
            Assert.AreEqual(t1[0].Block, "aaa"); Assert.AreEqual(t1[0].Separator, "");
            Assert.AreEqual(t1[1].Block, "bbb"); Assert.AreEqual(t1[1].Separator, " ");
            Assert.AreEqual(t1[2].Block, ""); Assert.AreEqual(t1[2].Separator, " ");
            Assert.AreEqual(t1[3].Block, "ccc"); Assert.AreEqual(t1[3].Separator, " ");

            t1 = StrUtil.SplitConsideringDoubleQuatation("  aaa bbb  ccc  ", new[] { ' ', '=' }, true, true);
            Assert.AreEqual(t1.Length, 3);
            Assert.AreEqual(t1[0].Block, "aaa"); Assert.AreEqual(t1[0].Separator, " ");
            Assert.AreEqual(t1[1].Block, "bbb"); Assert.AreEqual(t1[1].Separator, " ");
            Assert.AreEqual(t1[2].Block, "ccc"); Assert.AreEqual(t1[2].Separator, " ");

            t1 = StrUtil.SplitConsideringDoubleQuatation("  aaa=bbb : ccc = ddd =: eee  ", new[] { '=', ':', ' ' }, true, true);
            Assert.AreEqual(t1.Length, 5);
            Assert.AreEqual(t1[0].Block, "aaa"); Assert.AreEqual(t1[0].Separator, " ");
            Assert.AreEqual(t1[1].Block, "bbb"); Assert.AreEqual(t1[1].Separator, "=");
            Assert.AreEqual(t1[2].Block, "ccc"); Assert.AreEqual(t1[2].Separator, ":");
            Assert.AreEqual(t1[3].Block, "ddd"); Assert.AreEqual(t1[3].Separator, "=");
            Assert.AreEqual(t1[4].Block, "eee"); Assert.AreEqual(t1[4].Separator, "=");
        }

        [TestMethod]
        public void Test002()
        {
            var t1 = StrUtil.SplitConsideringQuatationContainsSeparator(" aaa=bbb : ccc = ddd =: eee  ", new[] { '=', ':', ' ' }, true, true);
            Assert.AreEqual(t1.Length, 10);
            Assert.AreEqual(t1[0], "aaa"); 
            Assert.AreEqual(t1[1], "=");
            Assert.AreEqual(t1[2], "bbb");
            Assert.AreEqual(t1[3], ":");
            Assert.AreEqual(t1[4], "ccc");
            Assert.AreEqual(t1[5], "=");
            Assert.AreEqual(t1[6], "ddd");
            Assert.AreEqual(t1[7], "=");
            Assert.AreEqual(t1[8], ":");
            Assert.AreEqual(t1[9], "eee");
        }
        [TestMethod]
        public void Test003()        
        {
            var t1 = StrUtil.SplitConsideringQuatationContainsSeparator(" aaa=bbb : ccc = ddd =: eee  ", new[] { '=', ':', ' ' }, true, false);
            Assert.AreEqual(t1.Length, 19);
            var expected = new[] { "", "aaa", "=", "bbb", "", ":", "", "ccc", "", "=", "", "ddd", "", "=", ":", "", "eee", "", "" };
            var i = 0;
            foreach (var ex in expected)
            {
                Assert.AreEqual(t1[i++], ex);
            }
        }
        [TestMethod]
        public void Test004()
        {
            var t1 = StrUtil.SplitConsideringQuatationContainsSeparator(" aaa=bbb : ccc = ddd =: eee  ", new[] { '=', ':', ' ' }, false, false);
            Assert.AreEqual(t1.Length, 19);
            var expected = new[] { " ", "aaa", "=", "bbb", " ", ":", " ", "ccc", " ", "=", " ", "ddd", " ", "=", ":", " ", "eee", " ", " " };
            var i = 0;
            foreach (var ex in expected)
            {
                Assert.AreEqual(t1[i++], ex);
            }
        }
    }
}
