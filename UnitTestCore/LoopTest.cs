// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class LoopTest
    {
        [TestMethod]
        public void Test001()
        {
            var col = new List<int>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            };

            var st = new StringBuilder();
            Loop<int>
                .Build(col)
                .ForEach(a =>
                {
                    st.Append($"{a}, ");
                })
                .When(a => a == 4, a =>
                {
                    st.Append($"W{a}, ");
                })
                .Skip(a =>
                {
                    return a % 2 == 1;
                })
                .Skip(a =>
                {
                    return a == 10;
                })
                .Break(a =>
               {
                   return a == 18;
               })
                .Initialize(a =>
                {
                    st.Append($"I{a}, ");
                })
                .Finalize(a =>
               {
                   st.Append($"F{a}, ");
               })
                .Start();
            var ss = st.ToString();
            Assert.AreEqual(ss, "I2, 2, W4, 4, 6, 8, 12, 14, 16, F16, ");
        }

        [TestMethod]
        public void Test001R()
        {
            var col = new List<int>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            };

            var st = new StringBuilder();
            Loop<int>
                .Build(col)
                .Skip(a =>
                {
                    return a % 2 == 1;
                })
                .Skip(a =>
                {
                    return a == 10;
                })
                .When(a => a == 4, a =>
                {
                    st.Append($"W{a}, ");
                })
                .Finalize(a =>
                {
                    st.Append($"F{a}, ");
                })
                .ForEach(a =>
                {
                    st.Append($"{a}, ");
                })
                .Break(a =>
                {
                    return a == 18;
                })
                .Initialize(a =>
                {
                    st.Append($"I{a}, ");
                })
                .Start();
            var ss = st.ToString();
            Assert.AreEqual("I2, 2, W4, 4, 6, 8, 12, 14, 16, F16, ", ss);
        }

        [TestMethod]
        public void Test002()
        {
            var col = new List<int>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            };

            var st = new StringBuilder();
            Loop<int>
                .Build(col)
                .Switch(a => a % 3 == 0, a =>
                {
                    st.Append($"A{a}, ");
                })
                .Switch(a => a % 3 == 1, a =>
                {
                    st.Append($"B{a}, ");
                })
                .SwitchDefault(a =>
                {
                    st.Append($"C{a}, ");
                })
                .Start();
            var ss = st.ToString();
            Assert.AreEqual("B1, C2, A3, B4, C5, A6, B7, C8, A9, B10, ", ss);
        }

        [TestMethod]
        public void Test003()
        {
            var col = new List<int>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            };

            var st = new StringBuilder();
            Loop<int>
                .Build(col)
                .Switch(a => a % 3 == 0, a =>
                {
                    st.Append($"A{a}, ");
                })
                .Switch(a => a % 3 == 1, a =>
                {
                    st.Append($"B{a}, ");
                })
                .SwitchDefault(a =>
                {
                    st.Append($"C{a}, ");
                })
                .Initialize(a =>
               {
                   st.Append($"I{a}, ");
               })
                .Finalize(a =>
                {
                    st.Append($"F{a}, ");
                })
                .ForEach(a =>
                {
                    st.Append($"{a}, ");
                })
                .Skip(a => a < 2)
                .Start();
            var ss = st.ToString();
            Assert.AreEqual("I2, 2, C2, 3, A3, 4, B4, 5, C5, 6, A6, 7, B7, 8, C8, 9, A9, 10, B10, F10, ", ss);
        }

        [TestMethod]
        public void Test004()
        {
            var col = new List<int>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            };

            var st = new StringBuilder();
            Loop<int>
                .Build(col)
                .Skip(a => a < 2)
                .Break(a => a > 9)
                .ForEach((a, idx) =>
                {
                    Assert.AreEqual(a, col[idx]);
                })
                .ForEach((a, idx) =>
                {
                    Assert.AreEqual(a, idx + 1);
                })
                .Finalize((a, idx) =>
                {
                    Assert.AreEqual(8, idx);
                })
                .Initialize((a, idx) =>
                {
                    Assert.AreEqual(1, idx);
                })
                .Initialize((a, idx) =>
                {
                    Assert.AreEqual(2, a);
                })
                .Finalize((a, idx) =>
                {
                    Assert.AreEqual(9, a);
                })
                .Start();
            var ss = st.ToString();
        }

        [TestMethod]
        public void Test101()
        {
            var data = new List<int>
            {
                0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 
            };
            foreach (var item in LoopUtil<int>.From(data, out var cu))
            {
                Assert.AreEqual(item / 10, cu.CurrentIndex );
            }
        }

        private IEnumerable<string> test102enum()
        {
            yield return "I-0";
            yield return "I-1";
            yield return "I-2";
            yield return "I-3";
            yield return "I-4";
        }

        [TestMethod]
        public void Test102()
        {
            string lastitem = "agro;jhn";
            foreach (var item in LoopUtil<string>.From(test102enum(), out var cu))
            {
                lastitem = item;
                Assert.AreEqual($"I-{cu.CurrentIndex}", item);
            }
            Assert.AreEqual("I-4", lastitem);
        }

        [TestMethod]
        public void Test103()
        {
            var dat = new Dictionary<int, string>
            {
                [10] = "S-10",
                [20] = "S-20",
                [30] = "S-30",
                [40] = "S-40",
                [50] = "S-50",
                [60] = "S-60",
                [70] = "S-70",
                [80] = "S-80",
                [90] = "S-90",
            };
            var c1 = 0;
            var c2 = 0;
            var c3 = 0;
            var c4 = 0;
            foreach (var item in LoopUtil<KeyValuePair<int,string>>.From(dat, out var cu))
            {
                cu.DoFirstTime(() =>
                {
                    Assert.AreEqual($"S-10", item.Value);
                    c1++;
                });
                cu.DoFirstTime(() =>
                {
                    Assert.AreEqual($"S-10", item.Value);   // Can do twice
                });
                cu.DoSecondTimesAndSubsequent(() =>
                {
                    switch(c3++)
                    {
                        case 0:
                            Assert.AreEqual($"S-20", item.Value);
                            break;
                        case 1:
                            Assert.AreEqual($"S-30", item.Value);
                            break;
                        case 2:
                            Assert.AreEqual($"S-40", item.Value);
                            break;
                    }
                });
                cu.DoSecondTimesAndSubsequent(() => // Can do twice
                {
                    switch (c4++)
                    {
                        case 0:
                            Assert.AreEqual($"S-20", item.Value);
                            break;
                        case 1:
                            Assert.AreEqual($"S-30", item.Value);
                            break;
                        case 2:
                            Assert.AreEqual($"S-40", item.Value);
                            break;
                    }
                });
                cu.DoLastOneTime(() =>
                {
                    Assert.AreEqual($"S-90", item.Value);
                    c2++;
                });
                Assert.AreEqual($"S-{item.Key}", item.Value);
            }
            Assert.AreEqual(1, c1);
            Assert.AreEqual(1, c2);
            Assert.IsTrue(c3 > 2);
            Assert.IsTrue(c4 > 2);
        }
        public void Test103m()
        {
            var dat = new Dictionary<int, string>
            {
                [10] = "S-10",
                [20] = "S-20",
                [30] = "S-30",
                [40] = "S-40",
                [50] = "S-50",
                [60] = "S-60",
                [70] = "S-70",
                [80] = "S-80",
                [90] = "S-90",
            };
            var c1 = 0;
            var c2 = 0;
            foreach (var item in LoopUtil<KeyValuePair<int, string>>.From(dat, out var cu))
            {
                cu.DoFirstTime(() =>
                {
                    Assert.AreEqual($"S-10", item.Value);
                    c1++;
                });
                cu.DoFirstTime(() =>
                {
                    Assert.Fail("EXPECTING NOT EXEC Because of not 'first'");
                    c1++;
                });
                cu.DoLastOneTime(() =>
                {
                    Assert.Fail("EXPECTING NOT EXEC Because of not 'last'");
                    c2++;
                });
                cu.DoLastOneTime(() =>
                {
                    Assert.AreEqual($"S-90", item.Value);   // Override last action
                    c2++;
                });
                Assert.AreEqual($"S-{item.Key}", item.Value);
            }
            Assert.AreEqual(1, c1); // Expecting One Time
            Assert.AreEqual(1, c2); // Expecting One Time
        }
    }
}
