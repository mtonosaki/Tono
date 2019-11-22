// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Tono;
using Tono.Jit;

namespace UnitTestProject1
{
    [TestClass]
    public class TonoJit_Process
    {
        [TestMethod]
        public void Test012_CiSwitchNextLink()
        {
            var st = new JitStage();

            JitProcess A, B, X, Y, SINK;
            st.Procs.Add(SINK = new JitProcess
            {
                Name = "SINK",
            });
            st.Procs.Add(X = new JitProcess
            {
                Name = "X",
                InCommands = new JitProcess.InCommandCollection
                {
                    new CiDelay
                    {
                        Delay = TimeSpan.FromMinutes(3),
                    },
                },
                NextLinks = new JitProcess.Destinations
                {
                    () => SINK,
                },
            });
            st.Procs.Add(Y = new JitProcess
            {
                Name = "Y",
                NextLinks = new JitProcess.Destinations
                {
                    () => SINK,
                },
            });
            st.Procs.Add(B = new JitProcess // 分岐元
            {
                Name = "B",
                NextLinks = new JitProcess.Destinations
                {
                    () => X, () => Y,
                },
            });
            st.Procs.Add(A = new JitProcess  // 前工程
            {
                Name = "A",
                NextLinks = new JitProcess.Destinations
                {
                    () => B,
                },
            });
        }

        [TestMethod]
        public void Test011_get_y1_and_z1_from_w1_assy_with_CoJoinFrom()
        {
            JitKanban.ResetIDCounter();
            var st = new JitStage();

            JitProcess A, B, C, SINK, Y, Z, D;
            st.Procs.Add(A = new JitProcess  // 前工程
            {
                Name = "A",
            });
            st.Procs.Add(B = new JitProcess  // 次工程（合流工程）
            {
                Name = "B",
            });
            st.Procs.Add(C = new JitProcess  // 分岐工程
            {
                Name = "C",
            });
            st.Procs.Add(Y = new JitProcess  // 横工程Y
            {
                Name = "Y",
            });
            st.Procs.Add(Z = new JitProcess  // 横工程Z
            {
                Name = "Z",
            });
            st.Procs.Add(SINK = new JitProcess // 消滅工程
            {
                Name = "SINK",
            });
            st.Procs.Add(D = new JitProcess // 分岐工程
            {
                Name = "D",
            });


            // 工程間リンク
            A.NextLinks.Add(() => B);     // A→B Push。但し、B.Co.JoinFromでJoinできるまで待つ
            B.NextLinks.Add(() => C);     // B→C Push。分岐工程への移動
            C.NextLinks.Add(() => SINK);  // B→SINK Push
            D.NextLinks.Add(() => SINK);  // D→SINK Push

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
                PullFrom = () => Y,
                WaitSpan = TimeSpan.FromMinutes(10),
            });
            CoJoinFrom JFZ;
            B.Constraints.Add(JFZ = new CoJoinFrom
            {
                PullFrom = () => Z,
                WaitSpan = TimeSpan.FromMinutes(10),
            });
            C.InCommands.Add(new CiPickTo(st)  // C工程で Dに分岐
            {
                Destination = () => D,
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


            //----------------------------------------------------
            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MSを０にする
            JitWork w1, y1, z1;
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, w1 = new JitWork
            {
                Name = $"w1",
                NextProcess = A,
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, y1 = new JitWork
            {
                Name = $"y1",
                NextProcess = Y,
                Classes = JitVariable.ClassList.From(":iOS:Sumaho"),    // :Workに、クラス「追加」
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 2), EventTypes.Out, z1 = new JitWork
            {
                Name = $"z1",
                NextProcess = Z,
                Classes = JitVariable.ClassList.From(":Android:Sumaho"),    // :Workに、クラス「追加」
            });
            var k = 0;

            // 初期状態は時間順に並んでる
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y")); // 工程Yは NextProcessが無いので、Eventキューに乗らない

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y")); // 工程Yは NextProcessが無いので、Eventキューに乗らない

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y"));  // 工程Yは NextProcessが無いので、Eventキューに乗らない
            // Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:32", "Z")); // 工程Zは NextProcessが無いので、Eventキューに乗らない
            Assert.AreEqual(dat.Count, 1);

            // 9:10に、w1@Aは、YとZから部品PULLを試みるが、部品が9:30まで届かないので、待つ
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:20", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y"));  // 工程Yは NextProcessが無いので、Eventキューに乗らない
            // Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:32", "Z")); // 工程Zは NextProcessが無いので、Eventキューに乗らない

            // 9:20に、w1@Aは、YとZから部品PULLを試みるが、部品が9:30まで届かないので、待つ
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:30", "A"));

            // 9:30に、w1@Aは、YとZから部品PULLを試みる。y1@Yが取得できる。 z1@Zは9:32まで到着しないので、w1は10分待機
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:40", "A"));
            Assert.AreEqual(w1.ChildWorks[JFY.ChildPartName], y1);

            // 9:40は、z1も到着しているので、w1が次の工程にIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:40", "A"));
            Assert.AreEqual(w1.ChildWorks[JFY.ChildPartName], y1);
            Assert.AreEqual(w1.ChildWorks[JFZ.ChildPartName], z1);

            // BにInOK。B.Ci.Delay=20
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:00", "B"));

            // 分岐判断工程CにIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "10:00", "B"));

            // CにIn。CのCi.Delay＝8
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));

            // 分岐ワーク y1が、工程DにIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));

            // 分岐ワーク z1は、D.Co.Span=3制約で、３秒後回し
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:04"));    // D.Co.Span制約で3秒加算された
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));

            // 分岐ワーク y1が、DにIn。D.Ci.Delay=50
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:04"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "10:51", "D"));   // inされた

            // 分岐ワーク z1は、D.Co.Span=3を待ったので、In準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, "10:04"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "10:51", "D"));

            // 分岐ワーク z1は、DにIn。
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "10:51", "D"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:54", "D"));
        }

        [TestMethod]
        public void Test010_CoJoinFrom()
        {
            // 前工程Aで横工程YからJoinできたら、工程Bに移動

            JitKanban.ResetIDCounter();
            var st = new JitStage();

            JitProcess A, B, SINK, Y, Z;
            st.Procs.Add(A = new JitProcess  // 前工程
            {
                Name = "A",
            });
            st.Procs.Add(B = new JitProcess  // 次工程
            {
                Name = "B",
            });
            st.Procs.Add(Y = new JitProcess  // 横工程Y
            {
                Name = "Y",
            });
            st.Procs.Add(Z = new JitProcess  // 横工程Z
            {
                Name = "Z",
            });
            st.Procs.Add(SINK = new JitProcess // 消滅工程
            {
                Name = "SINK",
            });

            // 工程間リンク
            A.NextLinks.Add(() => B);     // A→B Push。但し、B.Co.JoinFromでJoinできるまで待つ
            B.NextLinks.Add(() => SINK);  // B→SINK Push

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
                PullFrom = () => Y,
                WaitSpan = TimeSpan.FromMinutes(10),
            });
            CoJoinFrom JFZ;
            B.Constraints.Add(JFZ = new CoJoinFrom
            {
                PullFrom = () => Z,
                WaitSpan = TimeSpan.FromMinutes(10),
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(30),
            });
            Z.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(30),
            });

            //----------------------------------------------------
            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MSを０にする
            JitWork w1, y1, z1;
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, w1 = new JitWork
            {
                Name = $"w1",
                NextProcess = A,
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, y1 = new JitWork
            {
                Name = $"y1",
                NextProcess = Y,
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 2), EventTypes.Out, z1 = new JitWork
            {
                Name = $"z1",
                NextProcess = Z,
            });
            var k = 0;

            // 初期状態は時間順に並んでる
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y")); // 工程Yは NextProcessが無いので、Eventキューに乗らない

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y")); // 工程Yは NextProcessが無いので、Eventキューに乗らない

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y"));  // 工程Yは NextProcessが無いので、Eventキューに乗らない
            // Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:32", "Z")); // 工程Zは NextProcessが無いので、Eventキューに乗らない
            Assert.AreEqual(dat.Count, 1);

            // 9:10に、w1@Aは、YとZから部品PULLを試みるが、部品が9:30まで届かないので、待つ
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:20", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y"));  // 工程Yは NextProcessが無いので、Eventキューに乗らない
            // Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:32", "Z")); // 工程Zは NextProcessが無いので、Eventキューに乗らない

            // 9:20に、w1@Aは、YとZから部品PULLを試みるが、部品が9:30まで届かないので、待つ
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:30", "A"));

            // 9:30に、w1@Aは、YとZから部品PULLを試みる。y1@Yが取得できる。 z1@Zは9:32まで到着しないので、w1は10分待機
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:40", "A"));
            Assert.AreEqual(w1.ChildWorks[JFY.ChildPartName], y1);

            // 9:40は、z1も到着しているので、w1が次の工程にIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:40", "A"));
            Assert.AreEqual(w1.ChildWorks[JFY.ChildPartName], y1);
            Assert.AreEqual(w1.ChildWorks[JFZ.ChildPartName], z1);

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:00", "B"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "10:00", "B"));
            Assert.AreEqual(SINK.Works.Count(), 0);

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.AreEqual(dat.Count, 0);
            Assert.AreEqual(SINK.Works.Count(), 1);
        }

        [TestMethod]
        public void Test009_CiKanbanReturn()
        {
            JitKanban.ResetIDCounter();
            var st = new JitStage();

            JitProcess X, Y, SINK;
            st.Procs.Add(X = new JitProcess
            {
                Name = "X",
            });
            st.Procs.Add(Y = new JitProcess
            {
                Name = "Y",
            });
            st.Procs.Add(SINK = new JitProcess
            {
                Name = "SINK",
            });

            // 工程間リンク
            Y.NextLinks.Add(() => SINK);

            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(10),
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(20),
            });

            // 工程に制約を付与
            Y.InCommands.Add(new CiKanbanReturn(st)
            {
                Delay = TimeSpan.FromMinutes(0),
                TargetKanbanClass = ":Dog",
            });


            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MSを０にする

            st.SendKanban(new JitKanban
            {
                PullFrom = () => X,
                PullTo = () => Y,
            }).Classes.Add(":Dog");

            st.SendKanban(new JitKanban
            {
                PullFrom = () => X,
                PullTo = () => Y,
            }).Classes.Add(":Cat");

            // テストワーク投入（Xに工程充足）
            for (var i = 0; i < 2; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: i + 1), EventTypes.Out, new JitWork
                {
                    Name = $"w{(i + 1):0}",
                    NextProcess = X,
                });
            }

            var k = 0;

            // 初期状態は時間順に並んでる
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "0:00"));
            Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "0:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:01"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "0:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:01"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:01"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:01"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:02"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:11", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:11", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:11", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:12", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:11", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:12", "X"));
            Assert.IsTrue(dat[0].Work.Kanbans.Count == 1);

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:11"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:12", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:31", "Y"));
            Assert.IsTrue(dat[2].Work.Kanbans.Count == 0);   // :Dogかんばんが自動で返却された

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:12", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:31", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:12", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:31", "Y"));
            Assert.IsTrue(dat[0].Work.Kanbans.Count == 1);

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            //Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "9:12")); // :Catは転送されない
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:31", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:32", "Y"));
            Assert.IsTrue(dat[1].Work.Kanbans.Count == 1);   // w2の:Catかんばんが付いたまま
        }

        [TestMethod]
        public void Test008_PULL_Kanban_reuse()
        {
            JitKanban.ResetIDCounter();
            var st = new JitStage();

            JitProcess X, Y, SINK;
            st.Procs.Add(X = new JitProcess
            {
                Name = "X",
            });
            st.Procs.Add(Y = new JitProcess
            {
                Name = "Y",
            });
            st.Procs.Add(SINK = new JitProcess
            {
                Name = "SINK",
            });

            // 工程間リンク
            // st.Links.SetPushLink(X, Y);  // 後工程引き取りの場合は、PushLinkは設定しない。
            Y.NextLinks.Add(() => SINK);

            // 工程に制約を付与
            X.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(5),
            });
            Y.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(10),
            });
            Y.InCommands.Add(new CiKanbanReturn(st) // かんばんを前工程に自動的に返却するモード（瞬時にかんばんが帰る）
            {
                Delay = TimeSpan.FromSeconds(15),
            });


            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MSを０にする

            st.SendKanban(new JitKanban
            {
                PullFrom = () => X,
                PullTo = () => Y,
            });

            // テストワーク投入（Xに工程充足）
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"w{(i + 1):0}",
                    NextProcess = X,
                });
            }

            var k = 0;

            // 初期状態は時間順に並んでる
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "0:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(dat.Count == 2);   // w2 9:08 はXに入ったが、かんばんが無いので、Eventキューには入らなかった

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(dat.Count == 2);   // w2 9:08 はXに入ったが、かんばんが無いので、Eventキューには入らなかった




            // w1がYに入る。w1についていた かんばんが、Xに自動で返却されるモードになっている
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:05"));
            Assert.IsTrue(dat[0].DT.Second == 15);
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            // w2 9:08 はXに入ったが、かんばんが無いので、Eventキューには入らなかった
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // 9:05:15 Kanban1がw2に付いた。w2は9:08までX内に滞在
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            // w3は、9:06にXに入ったが、かんばんが無いので、Eventキューから除外。w3は9:11までXで作業している
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3は、9:06にXに入ったが、かんばんが無いので、Eventキューから除外。w3は9:11までXで作業している
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3は、9:06にXに入ったが、かんばんが無いので、Eventキューから除外。w3は9:11までXで作業している
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:15", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3は、9:06にXに入ったが、かんばんが無いので、Eventキューから除外。w3は9:11までXで作業している
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));

            // w1はSINK
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3は、9:06にXに入ったが、かんばんが無いので、Eventキューから除外。w3は9:11までXで作業している
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3は、9:06にXに入ったが、かんばんが無いので、Eventキューから除外。w3は9:11までXで作業している
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:15", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3は、9:06にXに入ったが、かんばんが無いので、Eventキューから除外。w3は9:11までXで作業している
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:15"));
            Assert.IsTrue(dat[0].DT.Second == 15);
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:25", "Y"));

            // w3にKanban1がついて Xに流れる（9:08に作業が終わっているが）
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(dat[0].DT.Second == 15);   // Kanban1のリードタイムが加算されている
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:25", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:25", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:25", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:25", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));

            // w2がSINK
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:25"));
            Assert.IsTrue(dat[0].DT.Second == 15);   // Kanban1のリードタイムが加算されている
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:35", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:35", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:35", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(dat.Count == 0);
        }

        [TestMethod]
        public void Test007_PULL_Kanban_too_late()
        {
            JitKanban.ResetIDCounter();
            var st = new JitStage();

            JitProcess X, Y, SINK;
            st.Procs.Add(X = new JitProcess
            {
                Name = "X",
            });
            st.Procs.Add(Y = new JitProcess
            {
                Name = "Y",
            });
            st.Procs.Add(SINK = new JitProcess
            {
                Name = "SINK",
            });

            // 工程間リンク
            // st.Links.SetPushLink(X, Y);  // 後工程引き取りの場合は、PushLinkは設定しない。
            Y.NextLinks.Add(() => SINK);

            // 工程に制約を付与
            X.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(5),
            });
            Y.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(10),
            });

            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MSを０にする

            // テストワーク投入（Xに工程充足）
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"w{(i + 1):0}",
                    NextProcess = X,
                });
            }

            var k = 0;

            // 初期状態は時間順に並んでる
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:06"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(dat.Count == 0);

            // 初期かんばんを投入
            st.SendKanban(TimeUtil.Set(today, hour: 9, minute: 30), new JitKanban   // かんばん送るも、工程Xにはワークが無いので、なにもしない
            {
                PullFrom = () => X,
                PullTo = () => Y,
            });
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:30"));

            st.SendKanban(TimeUtil.Set(today, hour: 9, minute: 30), new JitKanban   // かんばん送るも、工程Xにはワークが無いので、なにもしない
            {
                PullFrom = () => X,
                PullTo = () => Y,
            });
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:30"));
            Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "9:30"));

            st.SendKanban(TimeUtil.Set(today, hour: 9, minute: 32), new JitKanban   // かんばん送るも、工程Xにはワークが無いので、なにもしない
            {
                PullFrom = () => X,
                PullTo = () => Y,
            });
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:30"));
            Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "9:30"));
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:32"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "9:30"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:30", "X"));
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:32"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:30", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:30", "X"));
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:32"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:30", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:30", "X"));
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:32"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:30", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:30", "X"));
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:32"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:30", "X"));
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:32"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:40", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:32"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:40", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:40", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:32", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:40", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:40", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:40", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:40", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:40", "Y"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:40", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:40", "X"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:40", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:40", "X"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:40", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:40", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:40", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:40", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:50", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:50", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:50", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:50", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:50", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:50", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:50", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:50", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:50", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "10:00", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "10:00", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(dat.Count == 0);


            // Yから、SINKにPULL要求してみる
            foreach (var nl in Collection.Rep(4))
            {
                st.SendKanban(TimeUtil.Set(today, hour: 12, minute: 00), new JitKanban   // かんばん送るも、工程Xにはワークが無いので、なにもしない
                {
                    PullFrom = () => SINK,
                    PullTo = () => Y,
                });
            }

            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban4", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "Kanban5", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "Kanban6", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "Kanban7", EventTypes.KanbanIn, "12:00"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban5", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "Kanban6", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "Kanban7", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:00", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban6", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "Kanban7", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:00", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban7", EventTypes.KanbanIn, "12:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:00", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:00", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "12:00", "SINK"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:00", "SINK"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:00", "SINK"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:00", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:10", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "12:10", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "12:10", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:10", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "12:10", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "SINK")); // Kanban7が、w1について、またSINK→Yが指示された

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "12:10", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:20", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:20", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:20", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "12:20", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:20", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "12:20", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "12:20", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:20", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "12:20", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:20", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(dat.Count == 2); // もうかんばんが無いので、w2は、SINKで消滅

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "12:20", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:20", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:20", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:30", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "12:30", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:30", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:30", "SINK"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "12:30", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "12:30", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:30", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:30", "SINK"));
            Assert.IsTrue(dat.Count == 1); // もうかんばんが無いので、w3は、SINKで消滅

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "12:30", "SINK"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:40", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "12:40", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(dat.Count == 0);
        }

        [TestMethod]
        public void Test006_Case_Pull_Kanban_JIT_or_not()
        {
            JitKanban.ResetIDCounter();
            var st = new JitStage();

            JitProcess X, Y, SINK;
            st.Procs.Add(X = new JitProcess
            {
                Name = "X",
            });
            st.Procs.Add(Y = new JitProcess
            {
                Name = "Y",
            });
            st.Procs.Add(SINK = new JitProcess
            {
                Name = "SINK",
            });

            // 工程間リンク
            // st.Links.SetPushLink(X, Y);  // 後工程引き取りの場合は、PushLinkは設定しない。
            Y.NextLinks.Add(() => SINK);

            // 工程に制約を付与
            X.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(5),
            });
            Y.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(10),
            });

            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MSを０にする

            // 初期かんばんを投入
            st.SendKanban(TimeUtil.Set(today, hour: 8, minute: 0), new JitKanban   // かんばん送るも、工程Xにはワークが無いので、なにもしない
            {
                PullFrom = () => X,
                PullTo = () => Y,
            });

            // テストワーク投入（Xに工程充足）
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"w{(i + 1):0}",
                    NextProcess = X,
                });
            }

            var k = 0;

            // 初期状態は時間順に並んでる
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "8:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));

            // かんばんが工程Xに投入される（工程Xにはワークが入っていないので何もせず投入待ち）
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));

            // w1がXにIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));

            // w2はX.Ci.Span=3でw1＋３分後に移動
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));

            // w3はX.Ci.Span=3でw1＋３分後に移動
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));

            // w1がXにINした時、Kanban1の目的地Yが付与される
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));

            // w2がXにIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));

            // w3はX.Ci.Span=3制約でw2+3分後に移動
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            // w2がXにInしたが、かんばんが無いので目的地が無い。その為 Eventキューから外された。
            // しかし、w2のOut時刻は、9:03+5で、9:08の状態。
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            // Kanban2を Xに投入依頼
            st.SendKanban(TimeUtil.Set(today, hour: 9, minute: 4), new JitKanban
            {
                PullFrom = () => X,
                PullTo = () => Y,
            });
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            // Kanban2がw2に付き、退出待ちになる。
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));

            // Kanban3を Xに投入依頼。w3のInに先立ち、入れとくテスト
            st.SendKanban(new JitKanban
            {
                PullFrom = () => X,
                PullTo = () => Y,
            });

            // Kanban3 In待ち
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban3", EventTypes.KanbanIn, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // Kanban3は、Xにワークが無いので、キューに入るだけ
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // w3が、XにIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // w3がXにInするときに、Kanban3が有るので、次工程Yがセットされる。Eventキューにも入る
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:11", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // w2は、Y.Ci.Max=1で入れないので後回し
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:11", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));

            // w3は、Y.Ci.Max=1で入れないので後回し
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));

            // w1が、SINKにIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:15", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));

            // w1がSINK
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:15", "X"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:25", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:25", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:25", "Y"));
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:25", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:35", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:35", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(dat.Count == 0);
        }

        [TestMethod]
        public void Test005_JointProcess_No3()
        {
            JitProcessPriorityJoint JP;
            var st = new JitStage();

            st.Procs.Add(JP = new JitProcessPriorityJoint
            {
                Name = "JP",
            });

            JitProcess X, Y, Z, SINK;
            X = new JitProcess
            {
                Name = "X",
            };
            Y = new JitProcess
            {
                Name = "Y",
            };
            JP.Add(() => X);
            JP.Add(() => Y);

            st.Procs.Add(JP);
            st.Procs.Add(Z = new JitProcess
            {
                Name = "Z",
            });
            st.Procs.Add(SINK = new JitProcess
            {
                Name = "SINK",
            });


            JP.NextLinks.Add(() => Z);
            Z.NextLinks.Add(() => SINK);
            // No need to add next link from X to JP because X is a child of JP(auto linked)
            // No need to add next link from Y to JP because Y is a child of JP(auto linked)

            X.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(1),
            });
            Y.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(2),
            });
            Z.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Z.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(10),
            });

            // テストワーク投入
            var today = TimeUtil.ClearTime(DateTime.Now);
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"x{(i + 1):0}",
                    NextProcess = X,
                });
            }
            for (var i = 0; i < 1; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 5), EventTypes.Out, new JitWork
                {
                    Name = $"y{(i + 1):0}",
                    NextProcess = Y,
                });
            }

            var k = 0;

            // arranged by scheduled time
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));

            // prepare x1 to enter
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));

            // x2 moved to Exit(x1)+3 because of X.Ci.Span = 3
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));

            // x3 moved to Exit(x1)+3 because of X.Ci.Span = 3
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));

            // x1が 工程XにIn（X.Co.Delay=1）
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:01", "X"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));

            // prepare x1 to enter to Y1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:01", "X"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));

            // x1 have entered to JP
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:01", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));

            // prepare x1 to enter to Z
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:01", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));

            // x1が、ZにIn
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            // prepare x2 to enter to X
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            // x3 have moved to Exit(x2)+3 because of X.Ci.Span=3
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            // x2は、XにIn(X.Co.Delay=1)
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:04", "X"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            // prepare x2 to enter to JP
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:04", "X"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            // x2 have entered to JP
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:04", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            // x2 have moved to later queue because x1 is in Z, Z.Ci.Max=1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // y1は、YへのIn準備
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // y1 have entered to Y. (Y.Delay=2)
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:07", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // prepare x3 to enter to X
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.In, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:07", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // x3 have entered to X (X.Co.Delay=1)
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:07", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:07", "X"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // prepare y1 to enter to JP
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:07", "X"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:07", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // prepare x3 to enter to JP
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:07", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.In, "9:07", "X"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // y1 have entered to JP. y1 moved to priority to x2 because of JP setting.
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.In, "9:07", "X"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));    // priority to x2
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // x3 have entered to JP. x3 moved to last priority. then arranged exit time to 9:11 (just consider queue sequence=priority)
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));

            // prepere x1 to enter to Z
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:11", "Z"));

            // y1 have rejected to move to Z from JP. because of Z.Ci.Max=1. moved to last of JP queue.
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));

            // x2 have rejected to move to Z from JP. because of Z.Ci.Max=1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // x3 have rejected to move to Z from JP. because of Z.Ci.Max=1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));

            // x1 have moved to SINK then Z is now free.
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));

            // prepare y1 to enter to Z
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:11", "JP"));

            // x2 cannot to enter to Z because y1 is going to enter to same process. So moved to later.
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            // x3 cannot to enter to Z because y1 is going to enter to same process. So moved to later.
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));

            // y1 have entered to Z (Z.Co.Delay=10)
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:21", "Z"));

            // x2 cannot to enter to Z because of Z.Ci.Max=1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:21", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));

            // x3 cannot to enter to Z because of Z.Ci.Max=1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:21", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:21", "JP"));

            // prepare y1 to enter to SINK
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:21", "Z"));

            // x2 cannot to enter to Z because of Z.Ci.Max=1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:21", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));

            // x3 cannot to enter to Z because of Z.Ci.Max=1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:21", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:21", "JP"));

            // y1 have moved to SINK then Z become free now.
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:21", "JP"));

            // prepare x2 to enter to Z
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:21", "JP"));

            // x3 cannot enter to Z
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:21", "JP"));

            // x2 have entered to Z(Z.Co.Delay=10)
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:31", "Z"));

            // x3 cannot enter to Z because of Z.Ci.Max=1
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:31", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:31", "JP"));

            // prepare x2 to enter to SINK
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:31", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:31", "Z"));

            // x3 cannot enter to Z
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:31", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:31", "JP"));

            // x2 have entered to SINK then Z is no constraint now.
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:31", "JP"));

            // prepare x3 to enter to Z
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.In, "9:31", "JP"));

            // x3 have entered to Z (Z.Co.Delay=10)
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.Out, "9:41", "Z"));

            // prepare x3 to enter to SINK
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x3", EventTypes.In, "9:41", "Z"));

            // x3 have entered to SINK then no work in event queue.
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(dat.Count == 0);
        }

        [TestMethod]
        public void Test004_JointProcess_No2()
        {
            JitProcessPriorityJoint JP;
            var st = new JitStage();

            st.Procs.Add(JP = new JitProcessPriorityJoint
            {
                Name = "JP",
            });

            JitProcess X, Y, Z, SINK;
            X = new JitProcess
            {
                Name = "X",
            };
            Y = new JitProcess
            {
                Name = "Y",
            };
            JP.Add(() => X);
            JP.Add(() => Y);    // Y is priority

            st.Procs.Add(JP);
            st.Procs.Add(Z = new JitProcess
            {
                Name = "Z",
            });
            st.Procs.Add(SINK = new JitProcess
            {
                Name = "SINK",
            });

            JP.NextLinks.Add(() => Z);
            Z.NextLinks.Add(() => SINK);

            X.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(1),
            });
            Y.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(2),
            });
            Z.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Z.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(10),
            });

            var today = TimeUtil.ClearTime(DateTime.Now);
            for (var i = 0; i < 2; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"x{(i + 1):0}",
                    NextProcess = X,
                });
            }
            for (var i = 0; i < 2; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"y{(i + 1):0}",
                    NextProcess = Y,
                });
            }

            var k = 0;
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:00"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));  // IN-Span constraint

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));  // y1 is not yet enter. so move to low priority
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));  // IN priority

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:01", "X")); // enter to X at 9:00
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:01", "X")); // enter to X at 9:00
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:02", "Y")); // enter to Y at 9:00
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:01", "X"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:02", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:02"));  // y2 exit time have set to 9:02 that is y1's exit time.
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:01", "X"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:02", "Y")); // enter to Y at 9:00
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:01", "JP")); // enter to JP at 9:01
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:02", "Y")); // enter to Y at 9:00
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:01", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:02", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:02", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:02", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:02", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:02", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:02", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:04", "X"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:04", "X"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:04", "Y"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:04"), "Y");
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:04"), "X");
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:04", "X"));
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.Out, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP")); // y1 styed at JP because Z still have x1
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:11", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x1", EventTypes.In, "9:11", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:11", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:11", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:21", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:11", "JP")); // JP-IN 9:04
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:21", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:21", "JP")); // JP-IN 9:04 let it waited because Z still have y1

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:21", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:21", "JP")); // JP-IN 9:04 let it waited because Z still have y1
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP")); // JP-IN 9:04 let it waited because Z still have y1

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:21", "JP")); // JP-IN 9:04 let it waited because Z still have y1
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP")); // JP-IN 9:04 let it waited because Z still have y1
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:21", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP")); // JP-IN 9:04 let it waited because Z still have y1
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:21", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:21", "JP")); // let it waited because Z still have y1

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:21", "Z"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:21", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));    // let it be waited because y2 is going to enter to Z

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:21", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:31", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:31", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:31", "JP"));    // let x2 waited because Z have y2

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:31", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:31", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:31", "Z"));
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:31", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:31", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:31", "JP"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.Out, "9:41", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "x2", EventTypes.In, "9:41", "Z"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(dat.Count == 0);
        }

        [TestMethod]

        public void Test003_JointProcess_No1()
        {
            JitProcessPriorityJoint JP;
            var st = new JitStage();

            st.Procs.Add(JP = new JitProcessPriorityJoint
            {
                Name = "JP",
            });

            JitProcess X, Y, Z;
            X = new JitProcess
            {
                Name = "X",
            };
            Y = new JitProcess
            {
                Name = "Y",
            };
            JP.Add(() => X);
            JP.Add(() => Y);

            st.Procs.Add(JP);
            st.Procs.Add(Z = new JitProcess
            {
                Name = "Z",
            });

            JP.NextLinks.Add(() => Z);

            X.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(1),
            });
            Y.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(4),
            });
            Z.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 2.0,
            });
            Z.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            Z.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(6),
            });

            var today = TimeUtil.ClearTime(DateTime.Now);
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"y{(i + 1):0}",
                    NextProcess = Y,
                });
            }

            var k = 0;
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:00"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:00"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:04", "Y"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:04", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:04", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:04", "Y"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:04", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:04", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:04", "JP"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:04", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:04"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "9:04", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:04", "JP"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:04", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:08", "Y"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:04", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:08", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:08"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:08", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:08"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:08"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:08", "Y"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:08", "Y"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:08"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:08"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:08", "JP"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.Out, "9:08", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.In, "9:08"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.In, "9:08"));
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:08", "JP"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y2", EventTypes.In, "9:08", "JP"));
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:12", "Y"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:12", "Y"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.In, "9:12", "Y"));

            st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:12", "JP"));

            for (int i = 0; i < 100; i++)
            {
                st.DoNext(); dat = st.Events.Peeks(99).ToList(); k = 0;
                Assert.IsTrue(CMP(dat[k++], "y3", EventTypes.Out, "9:12", "JP"));    // next process Z is a final process. y3 will no be able to enter forever because a Max==2 constraint
            }
        }

        [TestMethod]
        public void Test002_Works_same_timing()
        {
            var st = new JitStage();
            JitProcess X, Y, Z;
            st.Procs.Add(new[]
            {
                X = new JitProcess
                {
                    Name = "X",
                },
                Y = new JitProcess
                {
                    Name = "Y",
                },
                Z = new JitProcess
                {
                    Name = "Z",
                },
            });

            X.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(1),
            });
            Y.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(4),
            });

            X.NextLinks.Add(() => Y);
            Y.NextLinks.Add(() => Z);

            var today = TimeUtil.ClearTime(DateTime.Now);
            JitWork a, b, c;
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, a = new JitWork
            {
                Name = "a",
                NextProcess = X,
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, b = new JitWork
            {
                Name = "b",
                NextProcess = X,
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, c = new JitWork
            {
                Name = "c",
                NextProcess = X,
            });

            st.DoNext();
            var k = 0;
            var dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.In, "9:00"));


            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:03"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:03"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.Out, "9:01", "X"));  // X:a900   Y:    Z:
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:03"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.In, "9:01"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:03"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.Out, "9:05", "Y")); // X:   Y:a901    Z:

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.Out, "9:05"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:06"));   // Span constraint of X (9:03+3 == entering b)

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:04", "X"));   // X:b903   Y:a901    Z:
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:06"));   // Span constraint of X (9:03+3 == entering b)

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:04
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:05"));   // set 9:05 to a from Y.Max==1 constraint
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:06"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:05
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.In, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:06"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:05
            Assert.IsTrue(CMP(dat[k++], "a", EventTypes.In, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:05"));   // b have tried to get last exit time in Y. But a does not have exit time because of 'IN' status. So, just enqueue it now.
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:06"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:05
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:06"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:05   // X:b903   Y:    Z:a905
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.In, "9:05"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:06"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:05   // X:   Y:b905    Z:a905
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:09"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:05   // X:   Y:b905    Z:a905
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.In, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:09"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:06   // X:c906   Y:b905    Z:a905
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:07"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:09"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:07   // X:c906   Y:b905    Z:a905
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.Out, "9:09"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:09"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:09   // X:c906   Y:b905    Z:a905
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:09"));
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.In, "9:09"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:09   // X:c906   Y:b905    Z:a905
            Assert.IsTrue(CMP(dat[k++], "b", EventTypes.In, "9:09"));
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:09"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:09   // X:c906   Y:    Z:a905+b909
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:09"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:09   // X:c906   Y:    Z:a905+b909
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.In, "9:09"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:09   // X:   Y:c909    Z:a905+b909
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.Out, "9:13"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:13   // X:   Y:c909    Z:a905+b909
            Assert.IsTrue(CMP(dat[k++], "c", EventTypes.In, "9:13"));

            st.DoNext(); dat = st.Events.Peeks(3).ToList(); k = 0;  // 9:13   // X:   Y:    Z:a905+b909+c913
            Assert.IsTrue(dat.Count == 0);
        }

        [TestMethod]

        public void Test001_tmStage_Basic()
        {
            var st = new JitStage();

            JitProcess X, Y, Z;
            st.Procs.Add(new[]
            {
                X = new JitProcess
                {
                    Name = "X",
                },
                Y = new JitProcess
                {
                    Name = "Y",
                },
                Z = new JitProcess
                {
                    Name = "Z",
                },
            });

            X.Constraints.Add(new CoSpan
            {
                Span = TimeSpan.FromMinutes(3),
            });
            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(1),
            });
            Y.Constraints.Add(new CoMaxCost
            {
                ReferenceVarName = JitVariable.From("Count"),
                Value = 1.0,
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(4),
            });

            X.NextLinks.Add(() => Y);
            Y.NextLinks.Add(() => Z);

            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS = 00:00:00:000
            JitWork a, b, c;
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, a = new JitWork
            {
                Name = "a",
                NextProcess = X,
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 1), EventTypes.Out, b = new JitWork
            {
                Name = "b",
                NextProcess = X,
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 2), EventTypes.Out, c = new JitWork
            {
                Name = "c",
                NextProcess = X,
            });

            //  1 a out->in
            st.DoNext();
            var dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "a", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.Out, "9:01"));
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:02"));

            //  2 a in -> X
            st.DoNext();
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.Out, "9:01"));
            Assert.IsTrue(CMP(dat[1], "a", EventTypes.Out, "9:01")); Assert.IsTrue(dat[1].Work.CurrentProcess.Name == "X");
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:02"));
            Assert.IsTrue(st.Procs["X"].Works.Count() == 1);

            st.DoNext();    //  3
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "a", EventTypes.Out, "9:01"));
            Assert.IsTrue(CMP(dat[1], "c", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[2], "b", EventTypes.Out, "9:03"));

            st.DoNext();    //  4
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "a", EventTypes.In, "9:01"));
            Assert.IsTrue(CMP(dat[1], "c", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[2], "b", EventTypes.Out, "9:03"));

            st.DoNext();    //  5
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.Out, "9:02"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[2], "a", EventTypes.Out, "9:05")); Assert.IsTrue(dat[2].Work.CurrentProcess.Name == "Y");
            Assert.IsTrue(st.Procs["X"].Works.Count() == 0);
            Assert.IsTrue(st.Procs["Y"].Works.Count() == 1);

            st.DoNext();    //  6
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[1], "c", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[2], "a", EventTypes.Out, "9:05"));

            st.DoNext();    //  7
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[2], "a", EventTypes.Out, "9:05"));

            st.DoNext();    //  8
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[1], "a", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:06"));

            st.DoNext();    //  9
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.Out, "9:04")); Assert.IsTrue(dat[0].Work.CurrentProcess.Name == "X");
            Assert.IsTrue(CMP(dat[1], "a", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:06"));
            Assert.IsTrue(st.Procs["X"].Works.Count() == 1);


            st.DoNext();    // 10
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "a", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:06"));

            st.DoNext();    // 11
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[1], "a", EventTypes.In, "9:05"));
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:06"));

            st.DoNext();    // 12
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "a", EventTypes.In, "9:05"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:06"));

            st.DoNext();    // 13
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(dat.Count == 2, "a moved to Z. a have sunk because Z have not next process");
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[1], "c", EventTypes.Out, "9:06"));
            Assert.IsTrue(st.Procs["Y"].Works.Count() == 0);
            Assert.IsTrue(st.Procs["Z"].Works.Count() == 1);

            st.DoNext();    // 14
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.In, "9:05"));
            Assert.IsTrue(CMP(dat[1], "c", EventTypes.Out, "9:06"));

            st.DoNext();    // 15
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.Out, "9:09"));
            Assert.IsTrue(st.Procs["X"].Works.Count() == 0);
            Assert.IsTrue(st.Procs["Y"].Works.Count() == 1);
            Assert.IsTrue(st.Procs["Z"].Works.Count() == 1);

            st.DoNext();    // 16
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.In, "9:06"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.Out, "9:09"));

            st.DoNext();    // 17
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.Out, "9:07"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.Out, "9:09"));

            st.DoNext();    // 18
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.Out, "9:09"));
            Assert.IsTrue(CMP(dat[1], "c", EventTypes.Out, "9:09"));
            Assert.IsTrue(st.Procs["X"].Works.Count() == 1);
            Assert.IsTrue(st.Procs["Y"].Works.Count() == 1);
            Assert.IsTrue(st.Procs["Z"].Works.Count() == 1);

            st.DoNext();    // 19
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.Out, "9:09"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.In, "9:09"));

            st.DoNext();    // 20
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.In, "9:09"));
            Assert.IsTrue(CMP(dat[1], "c", EventTypes.Out, "9:09"));

            st.DoNext();    // 21
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(dat.Count == 1, "b moved to Z then b have sunk because Z have not next process");
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.Out, "9:09"));
            Assert.IsTrue(st.Procs["X"].Works.Count() == 1);
            Assert.IsTrue(st.Procs["Y"].Works.Count() == 0);
            Assert.IsTrue(st.Procs["Z"].Works.Count() == 2);

            st.DoNext();    // 22
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.In, "9:09"));

            st.DoNext();    // 23
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.Out, "9:13"));

            st.DoNext();    // 23
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.In, "9:13"));

            st.DoNext();    // 25
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(dat.Count == 0, "c moved to Z them c have sunk because Z have not next process");
            Assert.IsTrue(st.Procs["X"].Works.Count() == 0);
            Assert.IsTrue(st.Procs["Y"].Works.Count() == 0);
            Assert.IsTrue(st.Procs["Z"].Works.Count() == 3);
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
                ret &= name == ("Kanban" + ei.Kanban.ID.ToString());
            }
            if (procName != null)
            {
                if (ei.Work is JitWork work)
                {
                    ret &= (work.CurrentProcess?.Name ?? null) == procName;
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
