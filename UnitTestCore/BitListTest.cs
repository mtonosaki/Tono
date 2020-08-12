// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class BitListTest
    {
        [TestMethod]
        public void Test001()
        {
            var bs = new BitList();
            Assert.AreEqual(0x00, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0x01, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0x03, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0x07, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0x0f, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0x1f, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0x3f, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0x7f, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0xff, bs.GetByte(0));
            bs.Add(true);
            Assert.AreEqual(0xff, bs.GetByte(0));
            Assert.AreEqual(0x01, bs.GetByte(1));
            Assert.AreEqual(9, bs.Count);

            bs.Clear();
            Assert.AreEqual(0, bs.Count);

            foreach (var _ in bs)
            {
                Assert.Fail();
            }
            foreach (var _ in bs.ToBytes())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Test002()
        {
            var bs = new BitList();
            foreach (var _ in bs.ToByteArray())
            {
                Assert.Fail();
            }
            foreach (var _ in Collection.Seq(32))
            {
                bs.Add(true);
            }
            for (var i = 0; i < 32; i++)
            {
                Assert.AreEqual(true, bs.Check(i));
            }
            Assert.AreEqual(0b11111111_11111111_11111111_11111111u, BitConverter.ToUInt32(bs.ToByteArray(), 0));

            bs[10] = false;
            Assert.AreEqual(0b11111111_11111111_11111011_11111111u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
            bs[20] = false;
            Assert.AreEqual(0b11111111_11101111_11111011_11111111u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
            bs[30] = false;
            Assert.AreEqual(0b10111111_11101111_11111011_11111111u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
            bs[31] = false;
            Assert.AreEqual(0b00111111_11101111_11111011_11111111u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
            bs.Set(0, false);
            Assert.AreEqual(0b00111111_11101111_11111011_11111110u, BitConverter.ToUInt32(bs.ToByteArray(), 0));

            bs[10] = true;
            Assert.AreEqual(0b00111111_11101111_11111111_11111110u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
            bs[20] = true;
            Assert.AreEqual(0b00111111_11111111_11111111_11111110u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
            bs[30] = true;
            Assert.AreEqual(0b01111111_11111111_11111111_11111110u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
            bs[31] = true;
            Assert.AreEqual(0b11111111_11111111_11111111_11111110u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
            bs.Set(0, true);
            Assert.AreEqual(0b11111111_11111111_11111111_11111111u, BitConverter.ToUInt32(bs.ToByteArray(), 0));
        }

        [TestMethod]
        public void Test003()
        {
            var bs = new BitList();
            var ret = new StringBuilder();
            for (var i = 0; i < 12; i++)
            {
                var item = (i % 2) != 0;
                bs.Add(item);
                ret.Insert(0, item ? "1" : "0");
            }
            Assert.AreEqual(ret.ToString(), string.Join("", bs.Reverse().Select(a => a ? "1" : "0")));
        }
        [TestMethod]
        public void Test004()
        {
            var bs = new BitList();
            var ret = new StringBuilder();
            for (var i = 0; i < 10000; i++)
            {
                var item = (i % 2) != 0;
                bs.Add(item);
                ret.Insert(0, item ? "1" : "0");
            }
            Assert.AreEqual(ret.ToString(), string.Join("", bs.Reverse().Select(a => a ? "1" : "0")));
        }
        [TestMethod]
        public void Test005()
        {
            var bs = new BitList();
            for (var i = 0; i < 8000000; i++)
            {
                bs.Add(i % 2 != 0); // Index# Even=false, Odd=true
            }
            Assert.AreEqual(false, bs[0]);
            Assert.AreEqual(false, bs[1000000]);
            Assert.AreEqual(true, bs[4999999]);
            Assert.AreEqual(true, bs[7999999]);
        }

        [TestMethod]
        public void Test006()
        {
            var bs1 = new BitList
            {
                true,
                false,
                true
            };

            var bs2 = new BitList
            {
                true,
                false,
                true
            };

            var bs3 = BitList.Join(bs1, bs2);
            Assert.AreEqual("101101", string.Join("", bs3.Reverse().Select(a => a ? "1" : "0")));
        }
        [TestMethod]
        public void Test007()
        {
            var bs1 = new BitList
            {
                true,
                false,
                true
            };

            var bs2 = new BitList();

            var bs3 = BitList.Join(bs1, bs2);
            Assert.AreEqual("101", string.Join("", bs3.Reverse().Select(a => a ? "1" : "0")));
        }
        [TestMethod]
        public void Test008()
        {
            var bs1 = new BitList
            {
                true,
                false,
                true
            };

            var bs3 = BitList.Join(bs1, new[] { true, false, true });
            Assert.AreEqual("101101", string.Join("", bs3.Reverse().Select(a => a ? "1" : "0")));
        }
        [TestMethod]
        public void Test009()
        {
            var bs1 = new BitList
            {
                true,
                false,
                true
            };

            var bs2 = new BitArray(new[] { true, false, true, true, });
            bs1.Add(bs2);
            Assert.AreEqual("1101101", string.Join("", bs1.Reverse().Select(a => a ? "1" : "0")));
        }

        [TestMethod]
        public void Test010()
        {
            var bs = new BitList
            {
                BitList.From(BitConverter.GetBytes((ushort)1))
            };
            Assert.AreEqual("0000000000000001", string.Join("", bs.Reverse().Select(a => a ? "1" : "0")));
        }
        [TestMethod]
        public void Test011()
        {
            var dat = (ushort)0b11110000_11110000;
            var bs = BitList.From(dat);
            Assert.AreEqual("1111000011110000", string.Join("", bs.Reverse().Select(a => a ? "1" : "0")));
        }

        [TestMethod]
        public void Test012()
        {
            var d = '@';
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToChar(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test012u()
        {
            var d = (byte)233;
            var bs = BitList.From(d);
            Assert.AreEqual(d, bs.ToByteArray()[0]);
        }
        [TestMethod]
        public void Test013()
        {
            var d = (short)32760;
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToInt16(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test013u()
        {
            var d = (ushort)65534;
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToUInt16(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test014()
        {
            var d = 327603838;
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToInt32(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test014u()
        {
            var d = (uint)655342323;
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToUInt32(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test015()
        {
            var d = 3276038382821;
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToInt64(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test015u()
        {
            var d = (ulong)65534232338730;
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToUInt64(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test016()
        {
            var d = 2.6568e23f;
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToSingle(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test017()
        {
            var d = 2.65846823321e64;
            var bs = BitList.From(d);
            Assert.AreEqual(d, BitConverter.ToDouble(bs.ToByteArray()));
        }
        [TestMethod]
        public void Test018()
        {
            var bs = new BitList();
            bs.AddPad();
            Assert.AreEqual(0, bs.Count);
        }
        [TestMethod]
        public void Test019()
        {
            var bs = new BitList
            {
                true,
            };
            bs.AddPad();
            Assert.AreEqual(8, bs.Count);
            Assert.AreEqual(1, bs.ToByteArray()[0]);
        }
        [TestMethod]
        public void Test020()
        {
            var bs = new BitList
            {
                true, false, false, true,
                true, false, false,
            };
            bs.AddPad();
            Assert.AreEqual(8, bs.Count);
            Assert.AreEqual(0b0001_1001, bs.ToByteArray()[0]);
        }
        [TestMethod]
        public void Test021()
        {
            var bs = new BitList
            {
                true, false, false, true,
                true, false, false, true,
            };
            bs.AddPad();
            Assert.AreEqual(8, bs.Count);
            Assert.AreEqual(0b1001_1001, bs.ToByteArray()[0]);
        }
        [TestMethod]
        public void Test022()
        {
            var bs = new BitList
            {
                true, false, false, true,
                true, false, false, true,
                true,
            };
            bs.AddPad();
            Assert.AreEqual(16, bs.Count);
            Assert.AreEqual(0b0000_0001_1001_1001, BitConverter.ToUInt16(bs.ToByteArray()));
        }

        [TestMethod]
        public void Test023()
        {
            // 4, 6, 12, 24
            foreach (var noval in new[]
            {  
                /*  4 */ (5,0UL), (5,1UL), (5,2UL), (5,0b1111UL), 
                /*  6 */ (8,0b10000UL), (8,0b100000UL), (8,0b111111UL),
                /* 12 */ (15,0b1000000UL), (15,0b100000000000UL), (15,0b111111111111UL),
                /* 24 */ (28,0b1000000000000UL), (28,0b100000000000000000000000UL), (28,0b111111111111111111111111UL),
                /* 64 */ (68,0b1000000000000000000000000UL), (68,0b1111111111111111111111111111111111111111111111111111111111111111UL),
            })
            {
                var nBit = noval.Item1;
                var val = noval.Item2;
                var bits = BitList.MakeVariableBits(val);
                var val2 = BitList.GetNumberFromVariableBits(bits, out var n);
                Assert.AreEqual(nBit, bits.Count);
                Assert.AreEqual(nBit, n);
                Assert.AreEqual(val, val2);
            }
        }
        [TestMethod]
        public void Test024()
        {
            var b1 = new BitList(new[] { true, true, true, false, false, true });
            var b2 = new BitList(b1, 1, 12);
            Assert.AreEqual("000000010011", string.Join("", b2.Reverse().Select(a => a ? "1" : "0")));
        }
    }
}
