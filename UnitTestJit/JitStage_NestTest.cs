// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Tono;
using Tono.Jit;
using static Tono.Jit.Utils;

namespace UnitTestJit
{
    [TestClass]
    public class JitStage_NestTest
    {
        [TestMethod]
        public void Test001()
        {
            // Unit testing to confirm a process instance used two stage can treat indipendently.
            // ÇPÇ¬ÇÃProcessÉCÉìÉXÉ^ÉìÉXÇ™ÅAÇQÇ¬ÇÃStageÇ…Ç‹ÇΩÇ™ÇÈèÍçáÇ≈Ç‡ÅAÇªÇÍÇºÇÍÇÃStageÇ≈èàóùÇ™Ç≈Ç´ÇÈéñÅB

            var st1 = new JitStage();
            var st2 = new JitStage();
            var tp = new Dictionary<JitStage, int>
            {
                [st1] = 8,
                [st2] = 20, // start time for each stage
            };
            var t8 = new Dictionary<JitStage, string>
            {
                [st1] = "8",
                [st2] = "20",
            };
            var t9 = new Dictionary<JitStage, string>
            {
                [st1] = "9",
                [st2] = "21",
            };
            var tw = new Dictionary<(JitStage Stage, string Name), JitWork>();

            //----------------------------------------------

            var A = new JitProcess  // ëOçHíˆ
            {
                Name = "A",
            };
            var B = new JitProcess  // éüçHíˆÅiçáó¨çHíˆÅj
            {
                Name = "B",
            };
            var C = new JitProcess  // ï™äÚçHíˆ
            {
                Name = "C",
            };
            var Y = new JitProcess  // â°çHíˆY
            {
                Name = "Y",
            };
            var Z = new JitProcess  // â°çHíˆZ
            {
                Name = "Z",
            };
            var SINK = new JitProcess // è¡ñ≈çHíˆ
            {
                Name = "SINK",
            };
            var D = new JitProcess // ï™äÚçHíˆ
            {
                Name = "D",
            };
            A.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(10),
            });
            B.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(20),
            });
            B.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            CoJoinFrom JFY;
            B.Constraints.Add(JFY = new CoJoinFrom
            {
                PullFromProcessKey = Y.ID,
                PorlingSpan = TimeSpan.FromMinutes(10),
            });
            CoJoinFrom JFZ;
            B.Constraints.Add(JFZ = new CoJoinFrom
            {
                PullFromProcessKey = Z.ID,
                PorlingSpan = TimeSpan.FromMinutes(10),
            });
            C.InCommands.Add(new CiPickTo  // CçHíˆÇ≈ DÇ…ï™äÚ
            {
                DestProcessKey = "D",
                Delay = TimeSpan.FromMinutes(1),
                TargetWorkClass = ":Sumaho",
            });
            C.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(8),
            });
            D.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(50),
            });
            D.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(30),
            });
            Z.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(30),
            });

            foreach (var st in new[] { st1, st2 })
            {
                st.AddChildProcess(A);
                st.AddChildProcess(B);
                st.AddChildProcess(C);
                st.AddChildProcess(Y);
                st.AddChildProcess(Z);
                st.AddChildProcess(SINK);
                st.AddChildProcess(D);

                // çHíˆä‘ÉäÉìÉN
                st.AddProcessLink(A, B); // AÅ®B PushÅBíAÇµÅAB.Co.JoinFromÇ≈JoinÇ≈Ç´ÇÈÇ‹Ç≈ë“Ç¬
                st.AddProcessLink(B, C); // BÅ®C PushÅBï™äÚçHíˆÇ÷ÇÃà⁄ìÆ
                st.AddProcessLink(C, SINK); // BÅ®SINK Push
                st.AddProcessLink(D, SINK); // DÅ®SINK Push

                var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MSÇÇOÇ…Ç∑ÇÈ
                st.Events.Enqueue(TimeUtil.Set(today, hour: tp[st], minute: 0), EventTypes.Out, tw[(st, "w1")] = new JitWork
                {
                    Name = $"w1",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, A),
                });
                Assert.IsTrue(tw[(st, "w1")].Is(":Work"));

                st.Events.Enqueue(TimeUtil.Set(today, hour: tp[st], minute: 0), EventTypes.Out, tw[(st, "y1")] = new JitWork
                {
                    Name = $"y1",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, Y),
                    Classes = JitVariable.ClassList.From(":iOS:Sumaho"),    // :WorkÇ…ÅAÉNÉâÉXÅuí«â¡Åv
                });

                st.Events.Enqueue(TimeUtil.Set(today, hour: tp[st], minute: 2), EventTypes.Out, tw[(st, "z1")] = new JitWork
                {
                    Name = $"z1",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, Z),
                    Classes = JitVariable.ClassList.From(":Android:Sumaho"),    // :WorkÇ…ÅAÉNÉâÉXÅuí«â¡Åv
                });
            }
            var k = 0;

            // èâä˙èÛë‘ÇÕéûä‘èáÇ…ï¿ÇÒÇ≈ÇÈ
            foreach (var st in new[] { st1, st2 })
            {
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:10", "A"));

                st.DoNext();
                dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:10", "A"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, $"{t8[st]}:02"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:10", "A"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:10", "A"));
                Assert.AreEqual(dat.Count, 1);
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:20", "A"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:30", "A"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:40", "A"));
                Assert.AreEqual(tw[(st, "w1")].ChildWorks[JFY.ChildWorkKey], tw[(st, "y1")]);
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, $"{t8[st]}:40", "A"));
                Assert.AreEqual(tw[(st, "w1")].ChildWorks[JFY.ChildWorkKey], tw[(st, "y1")]);
                Assert.AreEqual(tw[(st, "w1")].ChildWorks[JFZ.ChildWorkKey], tw[(st, "z1")]);
            }
            // BÇ…InOKÅBB.Ci.Delay=20
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:00", "B"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, $"{t9[st]}:00", "B"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:04"));    // D.Co.SpanêßñÒÇ≈3ïbâ¡éZÇ≥ÇÍÇΩ
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:04"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t9[st]}:51", "D"));   // inÇ≥ÇÍÇΩ
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, $"{t9[st]}:04"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t9[st]}:51", "D"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.DoNext();
                var dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t9[st]}:51", "D"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:54", "D"));
            }
        }

        [TestMethod]
        public void Test002()
        {
            JitProcess a, b, c, d, x;
            JitSubset f, g, h;
            var st = new JitStage
            {
                Name = "st",
            };
            st.AddChildProcess(f = new JitSubset
            {
                Name = "f",
            });
            st.AddChildProcess(x = new JitProcess
            {
                Name = "x",
            });
            f.AddChildProcess(g = new JitSubset
            {
                Name = "g",
            });
            f.AddChildProcess(h = new JitSubset
            {
                Name = "h",
            });
            g.AddChildProcess(a = new JitProcess
            {
                Name = "a",
            });
            g.AddChildProcess(b = new JitProcess
            {
                Name = "b",
            });
            h.AddChildProcess(c = new JitProcess
            {
                Name = "c",
            });
            h.AddChildProcess(g);
            h.AddChildProcess(d = new JitProcess
            {
                Name = "d",
            });
            foreach (var proc in new[] { a, b, c, d, x })
            {
                proc.AddCio(new CiDelay
                {
                    Delay = TimeSpan.FromSeconds(60),
                });
            }
        }

        [TestMethod]
        public void Test003_JitLocationCompine()
        {
            Assert.AreEqual(JitLocation.CombinePath(""), "\\");
            Assert.AreEqual(JitLocation.CombinePath("aaa"), "aaa");
            Assert.AreEqual(JitLocation.CombinePath("aaa", "bbb"), "aaa\\bbb");
            Assert.AreEqual(JitLocation.CombinePath("\\"), "\\");
            Assert.AreEqual(JitLocation.CombinePath("\\aaa"), "\\aaa");
            Assert.AreEqual(JitLocation.CombinePath("\\aaa"), "\\aaa");
            Assert.AreEqual(JitLocation.CombinePath("\\aaa", "bbb"), "\\aaa\\bbb");
            Assert.AreEqual(JitLocation.CombinePath("\\aaa\\", "bbb"), "\\aaa\\bbb");
            Assert.AreEqual(JitLocation.CombinePath("\\aaa", "\\bbb"), "\\bbb");
            Assert.AreEqual(JitLocation.CombinePath("\\aaa\\bbb", "\\ccc"), "\\ccc");
            Assert.AreEqual(JitLocation.CombinePath("\\aaa\\bbb", "ccc"), "\\aaa\\bbb\\ccc");
        }

        [TestMethod]
        public void Test004_JitLocationNormalize()
        {
            Assert.AreEqual(JitLocation.Normalize(""), "");
            Assert.AreEqual(JitLocation.Normalize("\\"), "\\");
            Assert.AreEqual(JitLocation.Normalize(".\\"), "\\");
            Assert.AreEqual(JitLocation.Normalize(".\\aaa"), "aaa");
            Assert.AreEqual(JitLocation.Normalize(".\\.\\."), "\\");
            Assert.AreEqual(JitLocation.Normalize("\\aaa\\bbb\\ccc\\..\\ddd"), "\\aaa\\bbb\\ddd");
            Assert.AreEqual(JitLocation.Normalize("\\aaa\\bbb\\ccc\\..\\..\\ddd"), "\\aaa\\ddd");
            Assert.AreEqual(JitLocation.Normalize("\\aaa\\bbb\\ccc\\..\\..\\..\\ddd"), "\\ddd");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                JitLocation.Normalize("\\aaa\\bbb\\ccc\\..\\..\\..\\..\\ddd");
            });
            Assert.AreEqual(JitLocation.Normalize("aaa\\bbb\\ccc\\ddd"), "aaa\\bbb\\ccc\\ddd");
            Assert.AreEqual(JitLocation.Normalize("aaa\\bbb\\ccc\\..\\ddd"), "aaa\\bbb\\ddd");
            Assert.AreEqual(JitLocation.Normalize("aaa\\bbb\\ccc\\..\\..\\ddd"), "aaa\\ddd");
            Assert.AreEqual(JitLocation.Normalize("aaa\\bbb\\ccc\\..\\..\\..\\ddd"), "ddd");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                JitLocation.Normalize("aaa\\bbb\\ccc\\..\\..\\..\\..\\ddd");
            });
            Assert.AreEqual(JitLocation.Normalize("\\aaa\\bbb\\ccc\\..\\ddd\\eee\\..\\..\\..\\fff"), "\\aaa\\fff");
        }

        [TestMethod]
        public void Test005_JitLocationGetPath()
        {
            Assert.AreEqual(JitLocation.GetPath("\\aaa\\bbb"), "\\aaa");
            Assert.AreEqual(JitLocation.GetPath("\\aaa\\bbb\\ccc"), "\\aaa\\bbb");
            Assert.AreEqual(JitLocation.GetPath("\\aaa"), "\\");
            Assert.AreEqual(JitLocation.GetPath("aaa"), "aaa");
            Assert.AreEqual(JitLocation.GetPath("aaa\\bbb\\ccc"), "aaa\\bbb");
            Assert.AreEqual(JitLocation.GetPath("aaa\\bbb"), "aaa");
        }


        private bool CMP(JitStage.WorkEventQueue.Item ei, string name, EventTypes et, string time, string procName = null)
        {
            var ts = time.Split(':');
            var h = int.Parse(ts[0]);
            var m = int.Parse(ts[1]);
            var ret = ei.Type == et && ei.DT.Hour == h && ei.DT.Minute == m;
            if (ei.Work != null)
            {
                ret &= name == ei.Work.Name;
            }
            if (ei.Kanban != null)
            {
                ret &= name == ("Kanban" + ei.Kanban.TestID.ToString());
            }
            if (procName != null)
            {
                if (ei.Work is JitWork work)
                {
                    ret &= work.Current?.Process?.Name == procName;
                }
                else
                {
                    ret = false;
                }
            }
            return ret;
        }
    }
}
