// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class HuffmanTreeTest
    {
        public class Leaf : HuffmanTree.INode
        {
            public string Name { get; set; }
            public double Cost { get; set; }

            public override string ToString()
            {
                return $"{Name} x {Cost}";
            }
        }

        [TestMethod]
        public void Test001()
        {
            var t = new HuffmanTree
            {
                new Leaf { Name = "A", Cost = 2 },
                new Leaf { Name = "B", Cost = 5 },
                new Leaf { Name = "C", Cost = 3 },
                new Leaf { Name = "D", Cost = 1 },
                new Leaf { Name = "E", Cost = 1 },
            }.Build();

            var A = t.GetBitResult(t[0]);
            var B = t.GetBitResult(t[1]);
            var C = t.GetBitResult(t[2]);
            var D = t.GetBitResult(t[3]);
            var E = t.GetBitResult(t[4]);

            Assert.AreEqual(3, A.Count);
            Assert.AreEqual(1, B.Count);
            Assert.AreEqual(2, C.Count);
            Assert.AreEqual(4, D.Count);
            Assert.AreEqual(4, E.Count);
        }
        [TestMethod]
        public void Test002()
        {
            var t = new HuffmanTree
            {
                new Leaf { Name = "A128", Cost = 128 },
                new Leaf { Name = "B64", Cost = 64 },
                new Leaf { Name = "C64", Cost = 64 },
                new Leaf { Name = "d", Cost = 1 },
                new Leaf { Name = "e", Cost = 1 },
                new Leaf { Name = "f", Cost = 1 },
                new Leaf { Name = "g", Cost = 1 },
                new Leaf { Name = "h", Cost = 1 },
                new Leaf { Name = "i", Cost = 1 },
                new Leaf { Name = "j", Cost = 1 },
                new Leaf { Name = "k", Cost = 1 },
                new Leaf { Name = "l", Cost = 1 },
                new Leaf { Name = "m", Cost = 1 },
                new Leaf { Name = "n", Cost = 1 },
                new Leaf { Name = "o", Cost = 1 },
                new Leaf { Name = "p", Cost = 1 },
                new Leaf { Name = "q", Cost = 1 },
                new Leaf { Name = "r", Cost = 1 },
                new Leaf { Name = "s", Cost = 1 },
            }.Build();

            var A = t.GetBitResult(t[0]);
            var B = t.GetBitResult(t[1]);
            var C = t.GetBitResult(t[2]);
            var d = t.GetBitResult(t[3]);
            var e = t.GetBitResult(t[4]);
            Assert.AreEqual(1, A.Count);
            Assert.AreEqual(2, B.Count);
            Assert.AreEqual(3, C.Count);
            Assert.AreEqual(7, d.Count);
            Assert.AreEqual(7, e.Count);
        }
        [TestMethod]
        public void Test003()
        {
            var t = new HuffmanTree
            {
                new Leaf { Name = "A", Cost = 14 },
                new Leaf { Name = "B", Cost = 12 },
                new Leaf { Name = "C", Cost = 7 },
                new Leaf { Name = "d", Cost = 1 },
                new Leaf { Name = "e", Cost = 1 },
                new Leaf { Name = "f", Cost = 1 },
                new Leaf { Name = "g", Cost = 1 },
                new Leaf { Name = "h", Cost = 1 },
                new Leaf { Name = "i", Cost = 1 },
                new Leaf { Name = "j", Cost = 1 },
                new Leaf { Name = "k", Cost = 1 },
                new Leaf { Name = "l", Cost = 1 },
                new Leaf { Name = "m", Cost = 1 },
                new Leaf { Name = "n", Cost = 1 },
                new Leaf { Name = "o", Cost = 1 },
                new Leaf { Name = "p", Cost = 1 },
                new Leaf { Name = "q", Cost = 1 },
                new Leaf { Name = "r", Cost = 1 },
                new Leaf { Name = "s", Cost = 1 },
            }.Build();

            var A = t.GetBitResult(t[0]);
            var B = t.GetBitResult(t[1]);
            var C = t.GetBitResult(t[2]);
            var d = t.GetBitResult(t[3]);
            var e = t.GetBitResult(t[4]);
            Assert.AreEqual(2, A.Count);
            Assert.AreEqual(2, B.Count);
            Assert.AreEqual(3, C.Count);
            Assert.AreEqual(6, d.Count);
            Assert.AreEqual(6, e.Count);
        }
        [TestMethod]
        public void Test004()
        {
            var t = new HuffmanTree
            {
                new Leaf { Name = "A", Cost = 15 },
                new Leaf { Name = "B", Cost = 13 },
                new Leaf { Name = "C", Cost = 11 },
                new Leaf { Name = "d", Cost = 8 },
                new Leaf { Name = "e", Cost = 6 },
                new Leaf { Name = "f", Cost = 5 },
                new Leaf { Name = "g", Cost = 4 },
                new Leaf { Name = "h", Cost = 3 },
                new Leaf { Name = "i", Cost = 2 },
                new Leaf { Name = "j", Cost = 1 },
            }.Build();
            var cnts = new[] { 2, 2, 3, 3, 4, 4, 4, 5, 6, 6 };
            for (var i = 0; i < t.Count; i++)
            {
                Assert.AreEqual(cnts[i], t.GetBitResult(t[i]).Count);
            }
        }
        [TestMethod]
        public void Test005()
        {
            var t = new HuffmanTree
            {
                new Leaf { Name = "A", Cost = 1 },
                new Leaf { Name = "B", Cost = 1 },
                new Leaf { Name = "C", Cost = 1 },
                new Leaf { Name = "d", Cost = 1 },
                new Leaf { Name = "e", Cost = 1 },
                new Leaf { Name = "f", Cost = 1 },
                new Leaf { Name = "g", Cost = 1 },
                new Leaf { Name = "h", Cost = 1 },
                new Leaf { Name = "i", Cost = 1 },
                new Leaf { Name = "j", Cost = 1 },
            }.Build();
            for (var i = 0; i < t.Count; i++)
            {
                Assert.IsTrue(t.GetBitResult(t[i]).Count == 4 || t.GetBitResult(t[i]).Count == 3);
            }
        }
    }
}
