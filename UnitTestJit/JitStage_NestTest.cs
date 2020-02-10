// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Tono;
using Tono.Jit;

namespace UnitTestJit
{
    [TestClass]
    public class JitStage_NestTest
    {
        [TestMethod]
        public void Test001()
        {
            // Unit testing to confirm a process instance used two stage can treat indipendently.
            // �P��Process�C���X�^���X���A�Q��Stage�ɂ܂�����ꍇ�ł��A���ꂼ���Stage�ŏ������ł��鎖�B

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

            var A = new JitProcess  // �O�H��
            {
                Name = "A",
            };
            var B = new JitProcess  // ���H���i�����H���j
            {
                Name = "B",
            };
            var C = new JitProcess  // ����H��
            {
                Name = "C",
            };
            var Y = new JitProcess  // ���H��Y
            {
                Name = "Y",
            };
            var Z = new JitProcess  // ���H��Z
            {
                Name = "Z",
            };
            var SINK = new JitProcess // ���ōH��
            {
                Name = "SINK",
            };
            var D = new JitProcess // ����H��
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
            C.InCommands.Add(new CiPickTo  // C�H���� D�ɕ���
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
                st.Subset.ChildProcesses.Add(A);
                st.Subset.ChildProcesses.Add(B);
                st.Subset.ChildProcesses.Add(C);
                st.Subset.ChildProcesses.Add(Y);
                st.Subset.ChildProcesses.Add(Z);
                st.Subset.ChildProcesses.Add(SINK);
                st.Subset.ChildProcesses.Add(D);

                // �H���ԃ����N
                st.Subset.AddProcessLink(A, B); // A��B Push�B�A���AB.Co.JoinFrom��Join�ł���܂ő҂�
                st.Subset.AddProcessLink(B, C); // B��C Push�B����H���ւ̈ړ�
                st.Subset.AddProcessLink(C, SINK); // B��SINK Push
                st.Subset.AddProcessLink(D, SINK); // D��SINK Push

                var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS���O�ɂ���
                st.Subset.Engine().Events.Enqueue(TimeUtil.Set(today, hour: tp[st], minute: 0), EventTypes.Out, tw[(st, "w1")] = new JitWork
                {
                    Subset = st.Subset,
                    Name = $"w1",
                    Next = (st.Subset, A),
                });
                Assert.IsTrue(tw[(st, "w1")].Is(":Work"));

                st.Subset.Engine().Events.Enqueue(TimeUtil.Set(today, hour: tp[st], minute: 0), EventTypes.Out, tw[(st, "y1")] = new JitWork
                {
                    Subset = st.Subset,
                    Name = $"y1",
                    Next = (st.Subset, Y),
                    Classes = JitVariable.ClassList.From(":iOS:Sumaho"),    // :Work�ɁA�N���X�u�ǉ��v
                });

                st.Subset.Engine().Events.Enqueue(TimeUtil.Set(today, hour: tp[st], minute: 2), EventTypes.Out, tw[(st, "z1")] = new JitWork
                {
                    Subset = st.Subset,
                    Name = $"z1",
                    Next = (st.Subset, Z),
                    Classes = JitVariable.ClassList.From(":Android:Sumaho"),    // :Work�ɁA�N���X�u�ǉ��v
                });
            }
            var k = 0;

            // ������Ԃ͎��ԏ��ɕ���ł�
            foreach (var st in new[] { st1, st2 })
            {
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, $"{t8[st]}:00"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:10", "A"));

                st.Subset.Engine().DoNext();
                dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t8[st]}:02"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:10", "A"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, $"{t8[st]}:02"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:10", "A"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:10", "A"));
                Assert.AreEqual(dat.Count, 1);
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:20", "A"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:30", "A"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t8[st]}:40", "A"));
                Assert.AreEqual(tw[(st, "w1")].ChildWorks[JFY.ChildWorkKey], tw[(st, "y1")]);
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, $"{t8[st]}:40", "A"));
                Assert.AreEqual(tw[(st, "w1")].ChildWorks[JFY.ChildWorkKey], tw[(st, "y1")]);
                Assert.AreEqual(tw[(st, "w1")].ChildWorks[JFZ.ChildWorkKey], tw[(st, "z1")]);
            }
            // B��InOK�BB.Ci.Delay=20
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:00", "B"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, $"{t9[st]}:00", "B"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, $"{t9[st]}:01"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:04"));    // D.Co.Span�����3�b���Z���ꂽ
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:04"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t9[st]}:51", "D"));   // in���ꂽ
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, $"{t9[st]}:04"));
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t9[st]}:51", "D"));
            }
            foreach (var st in new[] { st1, st2 })
            {
                st.Subset.Engine().DoNext();
                var dat = st.Subset.Engine().Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, $"{t9[st]}:08", "C"));
                Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, $"{t9[st]}:51", "D"));
                Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, $"{t9[st]}:54", "D"));
            }
        }
        [TestMethod]
        public void Test002()
        {
            // Test case come from Jit Model Class Design.pptx
            var X = new JitSubset();
            X.AddProcessLink("A", "B");

            JitProcess A, B;
            X.ChildProcesses.Add(A = new JitProcess  // First Process
            {
                Name = "A",
            });
            X.ChildProcesses.Add(B = new JitProcess  // 2nd Process
            {
                Name = "B",
            });
            A.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromSeconds(20),
            });
            B.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromSeconds(15),
            });

            var st = new JitStage();

            JitWork w1;
            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS���O�ɂ���
            st.Subset.Engine().Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, w1 = new JitWork
            {
                Subset = st.Subset,
                Name = $"w1",
                Next = (st.Subset, A),
            });
#if false

            var k = 0;

            // ������Ԃ͎��ԏ��ɕ���ł�
            var dat = st.Model.Engine().Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));

            st.Model.Engine().DoNext();
            dat = st.Model.Engine().Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));

            st.Model.Engine().DoNext();
            dat = st.Model.Engine().Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:20", "A"));

            st.Model.Engine().DoNext();
            dat = st.Model.Engine().Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:20", "A"));

            st.Model.Engine().DoNext();
            dat = st.Model.Engine().Events.Peeks(99).ToList(); k = 0;
            Assert.AreEqual(dat.Count, 0);
#endif
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
                    ret &= (JitWork.GetProcess(work.Current)?.Name ?? null) == procName;
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
