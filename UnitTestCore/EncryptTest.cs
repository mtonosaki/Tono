// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class EncryptTest
    {
        [TestMethod]
        public void Test001()
        {
            var enc = new Encrypt();
            var secret = enc.Encode("");
            var decode = enc.Decode(secret);
            Assert.AreEqual("", decode);
        }
        [TestMethod]
        public void Test002()
        {
            var enc = new Encrypt();
            var secret = enc.Encode(null);
            var decode = enc.Decode(secret);
            Assert.AreEqual(null, decode);
        }
        [TestMethod]
        public void Test003()
        {
            var enc = new Encrypt();
            for( int cnt = 0; cnt < 100; cnt++)
            {
                var planeBuild = new StringBuilder();
                for (var i = MathUtil.Rand(300, 500); i >= 0; i--)
                {
                    planeBuild.Append((char)MathUtil.Rand(' ', '~'));
                }
                var plane = planeBuild.ToString();
                var secret = enc.Encode(plane);
                Assert.IsTrue(secret?.Length > 0);
                var decode = enc.Decode(secret);
                Assert.AreEqual(plane, decode);
            }
        }
        [TestMethod]
        public void Test004()
        {
            var enc = new Encrypt();
            for (int cnt = 0; cnt < 3; cnt++)
            {
                var planeBuild = new StringBuilder();
                for (var i = MathUtil.Rand(10000, 11000); i >= 0; i--)
                {
                    planeBuild.Append((char)MathUtil.Rand(' ', '~'));
                }
                var plane = planeBuild.ToString();
                var secret = enc.Encode(plane);
                Assert.IsTrue(secret?.Length > 0);
                var decode = enc.Decode(secret);
                Assert.AreEqual(plane, decode);
            }
        }

        [TestMethod]
        public void Test005()
        {
            var enc = new Encrypt();
            var plane = "祇園精舎の鐘の声＋諸行無常の響き有り-沙羅双樹の花の色_盛者必衰の理を現す|奢れる人も久からず-ただ春の夜の夢のごとし:猛き者も遂には滅びぬ--ひとへに風の前の塵に同じ+";
            var secret = enc.Encode(plane);
            var decode = enc.Decode(secret);
            Assert.AreEqual(plane, decode);
        }
    }
}
