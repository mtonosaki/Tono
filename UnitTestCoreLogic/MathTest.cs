// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Tono;

namespace TestTonoCore
{
    [TestClass]
    public class MathTest
    {
        [TestMethod]
        public void Test001()
        {
            var checkers = new List<(int Number, bool Expected)> {
                (0,false),
                (1, false),
                (2,true),
                (3,true),
                (4,false),
                (5,true),
                (6,false),
                (7,true),
                (8,false),
                (9,false),
                (10,false),
                (11,true),

                (-1,false),
                (-1000,false),
                (941,true),
            };
            foreach( var chk in checkers)
            {
                Assert.AreEqual(chk.Expected, MathUtil.IsPrimeNumber(chk.Number), $"IsPrimeNumber({chk.Number}) should be {chk.Expected}" );
            }
        }
#if HEAVYTEST  // over 650ms with Intel i7-4720HQ
        [TestMethod]
        public void Test001_2() 
        {
            Assert.IsFalse(MathUtil.IsPrimeNumber(2166136261));
            Assert.IsTrue(MathUtil.IsPrimeNumber(2147483647));
            Assert.IsTrue(MathUtil.IsPrimeNumber(92709568269121));
            Assert.IsTrue(MathUtil.IsPrimeNumber(16777619));    
            Assert.IsTrue(MathUtil.IsPrimeNumber(9007199254740997));
        }
#endif
        [TestMethod]
        public void Test002()
        {
            //f(x) = 5x ^ 4 + 4x ^ 3 + 3x ^ 2 + 2x + 1
            Assert.AreEqual( MathUtil.HornersRule(1, new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 4), 15.0);
            Assert.AreEqual(MathUtil.HornersRule(2, new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 4), 129.0);
            Assert.AreEqual(MathUtil.HornersRule(3, new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 4), 547.0);
            Assert.AreEqual(MathUtil.HornersRule(4, new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 4), 1593.0);
            Assert.AreEqual(MathUtil.HornersRule(5, new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 4), 3711.0);
        }
    }
}
