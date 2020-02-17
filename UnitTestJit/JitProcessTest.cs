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
    public class JitProcessTest
    {
        [TestMethod]
        public void Test012_CiSwitchNextLink()
        {
            var st = new JitStage();

            JitProcess A, B, X, Y, SINK;
            st.AddChildProcess(SINK = new JitProcess
            {
                Name = "SINK",
            });
            st.AddChildProcess(X = new JitProcess
            {
                Name = "X",
                InCommands = new List<CiBase>
                {
                    new CiDelay
                    {
                        Delay = TimeSpan.FromMinutes(3),
                    },
                },
            });
            st.AddProcessLink(X, SINK);

            st.AddChildProcess(Y = new JitProcess
            {
                Name = "Y",
            });
            st.AddProcessLink(Y, SINK);

            st.AddChildProcess(B = new JitProcess // ����
            {
                Name = "B",
            });
            st.AddProcessLink(B, X);
            st.AddProcessLink(B, Y);

            st.AddChildProcess(A = new JitProcess  // �O�H��
            {
                Name = "A",
            });
            st.AddProcessLink(A, B);
        }

        [TestMethod]
        public void Test011_get_y1_and_z1_from_w1_assy_with_CoJoinFrom()
        {
            var st = new JitStage();

            JitProcess A, B, C, SINK, Y, Z, D;
            st.AddChildProcess(A = new JitProcess  // �O�H��
            {
                Name = "A",
            });
            st.AddChildProcess(B = new JitProcess  // ���H���i�����H���j
            {
                Name = "B",
            });
            st.AddChildProcess(C = new JitProcess  // ����H��
            {
                Name = "C",
            });
            st.AddChildProcess(Y = new JitProcess  // ���H��Y
            {
                Name = "Y",
            });
            st.AddChildProcess(Z = new JitProcess  // ���H��Z
            {
                Name = "Z",
            });
            st.AddChildProcess(SINK = new JitProcess // ���ōH��
            {
                Name = "SINK",
            });
            st.AddChildProcess(D = new JitProcess // ����H��
            {
                Name = "D",
            });


            // �H���ԃ����N

            st.AddProcessLink(A, B); // A��B Push�B�A���AB.Co.JoinFrom��Join�ł���܂ő҂�
            st.AddProcessLink(B, C); // B��C Push�B����H���ւ̈ړ�
            st.AddProcessLink(C, SINK); // B��SINK Push
            st.AddProcessLink(D, SINK); // D��SINK Push

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


            //----------------------------------------------------
            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS���O�ɂ���
            JitWork w1, y1, z1;
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, w1 = new JitWork
            {
                Name = $"w1",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, A),
            });
            Assert.IsTrue(w1.Is(":Work"));
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, y1 = new JitWork
            {
                Name = $"y1",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, Y),
                Classes = JitVariable.ClassList.From(":iOS:Sumaho"),    // :Work�ɁA�N���X�u�ǉ��v
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 2), EventTypes.Out, z1 = new JitWork
            {
                Name = $"z1",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, Z),
                Classes = JitVariable.ClassList.From(":Android:Sumaho"),    // :Work�ɁA�N���X�u�ǉ��v
            });
            var k = 0;

            // ������Ԃ͎��ԏ��ɕ���ł�
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
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y")); // �H��Y�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y")); // �H��Y�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y"));  // �H��Y�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�
            // Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:32", "Z")); // �H��Z�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�
            Assert.AreEqual(dat.Count, 1);

            // 9:10�ɁAw1@A�́AY��Z���畔�iPULL�����݂邪�A���i��9:30�܂œ͂��Ȃ��̂ŁA�҂�
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:20", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y"));  // �H��Y�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�
            // Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:32", "Z")); // �H��Z�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�

            // 9:20�ɁAw1@A�́AY��Z���畔�iPULL�����݂邪�A���i��9:30�܂œ͂��Ȃ��̂ŁA�҂�
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:30", "A"));

            // 9:30�ɁAw1@A�́AY��Z���畔�iPULL�����݂�By1@Y���擾�ł���B z1@Z��9:32�܂œ������Ȃ��̂ŁAw1��10���ҋ@
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:40", "A"));
            Assert.AreEqual(w1.ChildWorks[JFY.ChildWorkKey], y1);

            // 9:40�́Az1���������Ă���̂ŁAw1�����̍H����In����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:40", "A"));
            Assert.AreEqual(w1.ChildWorks[JFY.ChildWorkKey], y1);
            Assert.AreEqual(w1.ChildWorks[JFZ.ChildWorkKey], z1);

            // B��InOK�BB.Ci.Delay=20
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:00", "B"));

            // ���򔻒f�H��C��In����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "10:00", "B"));

            // C��In�BC��Ci.Delay��8
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));

            // ���򃏁[�N y1���A�H��D��In����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));

            // ���򃏁[�N z1�́AD.Co.Span=3����ŁA�R�b���
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "10:01"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:04"));    // D.Co.Span�����3�b���Z���ꂽ
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));

            // ���򃏁[�N y1���AD��In�BD.Ci.Delay=50
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:04"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "10:51", "D"));   // in���ꂽ

            // ���򃏁[�N z1�́AD.Co.Span=3��҂����̂ŁAIn����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, "10:04"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "10:51", "D"));

            // ���򃏁[�N z1�́AD��In�B
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:08", "C"));
            Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.Out, "10:51", "D"));
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "10:54", "D"));
        }

        [TestMethod]
        public void Test010_CoJoinFrom()
        {
            // �O�H��A�ŉ��H��Y����Join�ł�����A�H��B�Ɉړ�

            var st = new JitStage();

            JitProcess A, B, SINK, Y, Z;
            st.AddChildProcess(A = new JitProcess  // �O�H��
            {
                Name = "A",
            });
            st.AddChildProcess(B = new JitProcess  // ���H��
            {
                Name = "B",
            });
            st.AddChildProcess(Y = new JitProcess  // ���H��Y
            {
                Name = "Y",
            });
            st.AddChildProcess(Z = new JitProcess  // ���H��Z
            {
                Name = "Z",
            });
            st.AddChildProcess(SINK = new JitProcess // ���ōH��
            {
                Name = "SINK",
            });

            // �H���ԃ����N
            st.AddProcessLink(A, B); // A��B Push�B�A���AB.Co.JoinFrom��Join�ł���܂ő҂�
            st.AddProcessLink(B, SINK); // B��SINK Push

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
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(30),
            });
            Z.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(30),
            });

            //----------------------------------------------------
            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS���O�ɂ���
            JitWork w1, y1, z1;
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, w1 = new JitWork
            {
                Name = $"w1",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, A),
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, y1 = new JitWork
            {
                Name = $"y1",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, Y),
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 2), EventTypes.Out, z1 = new JitWork
            {
                Name = $"z1",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, Z),
            });
            var k = 0;

            // ������Ԃ͎��ԏ��ɕ���ł�
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
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y")); // �H��Y�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.In, "9:02"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y")); // �H��Y�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:10", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y"));  // �H��Y�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�
            // Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:32", "Z")); // �H��Z�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�
            Assert.AreEqual(dat.Count, 1);

            // 9:10�ɁAw1@A�́AY��Z���畔�iPULL�����݂邪�A���i��9:30�܂œ͂��Ȃ��̂ŁA�҂�
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:20", "A"));
            // Assert.IsTrue(CMP(dat[k++], "y1", EventTypes.In, "9:30", "Y"));  // �H��Y�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�
            // Assert.IsTrue(CMP(dat[k++], "z1", EventTypes.Out, "9:32", "Z")); // �H��Z�� NextProcess�������̂ŁAEvent�L���[�ɏ��Ȃ�

            // 9:20�ɁAw1@A�́AY��Z���畔�iPULL�����݂邪�A���i��9:30�܂œ͂��Ȃ��̂ŁA�҂�
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:30", "A"));

            // 9:30�ɁAw1@A�́AY��Z���畔�iPULL�����݂�By1@Y���擾�ł���B z1@Z��9:32�܂œ������Ȃ��̂ŁAw1��10���ҋ@
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:40", "A"));
            Assert.AreEqual(w1.ChildWorks[JFY.ChildWorkKey], y1);

            // 9:40�́Az1���������Ă���̂ŁAw1�����̍H����In����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:40", "A"));
            Assert.AreEqual(w1.ChildWorks[JFY.ChildWorkKey], y1);
            Assert.AreEqual(w1.ChildWorks[JFZ.ChildWorkKey], z1);

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "10:00", "B"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "10:00", "B"));
            Assert.AreEqual(st.GetWorks("\\", SINK).Count(), 0);

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.AreEqual(dat.Count, 0);
            Assert.AreEqual(st.GetWorks("\\", SINK).Count(), 1);
        }

        [TestMethod]
        public void Test009_CiKanbanReturn()
        {
            var st = new JitStage();

            JitProcess X, Y, SINK;
            st.AddChildProcess(X = new JitProcess
            {
                Name = "X",
            });
            st.AddChildProcess(Y = new JitProcess
            {
                Name = "Y",
            });
            st.AddChildProcess(SINK = new JitProcess
            {
                Name = "SINK",
            });

            // �H���ԃ����N
            st.AddProcessLink(Y, SINK);

            X.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(10),
            });
            Y.InCommands.Add(new CiDelay
            {
                Delay = TimeSpan.FromMinutes(20),
            });

            // �H���ɐ����t�^
            Y.InCommands.Add(new CiKanbanReturn
            {
                Delay = TimeSpan.FromMinutes(0),
                TargetKanbanClass = ":Dog",
            });

            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS���O�ɂ���
            JitKanban ka;
            var testid = 0;
            st.SendKanban(ka = new JitKanban
            {
                Location = JitLocation.CreateRoot(st, null),
                PullFromProcessKey = X.ID,  // You can set ID here
                PullToProcessKey = Y.Name,  // You can also set Name here
                TestID = ++testid,
            }).Classes.Add(":Dog");
            Assert.IsTrue(ka.Is(":Kanban"));

            st.SendKanban(new JitKanban
            {
                Location = JitLocation.CreateRoot(st, null),
                PullFromProcessKey = "X",   // You can set Name here
                PullToProcessKey = "Y",
                TestID = ++testid,
            }).Classes.Add(":Cat");

            // �e�X�g���[�N�����iX�ɍH���[���j
            for (var i = 0; i < 2; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: i + 1), EventTypes.Out, new JitWork
                {
                    Name = $"w{(i + 1):0}",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, X),
                });
            }

            var k = 0;

            // ������Ԃ͎��ԏ��ɕ���ł�
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
            Assert.IsTrue(dat[2].Work.Kanbans.Count == 0);   // :Dog����΂񂪎����ŕԋp���ꂽ

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
            //Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "9:12")); // :Cat�͓]������Ȃ�
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:31", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:32", "Y"));
            Assert.IsTrue(dat[1].Work.Kanbans.Count == 1);   // w2��:Cat����΂񂪕t�����܂�
        }

        [TestMethod]
        public void Test008_PULL_Kanban_reuse()
        {
            var st = new JitStage();

            JitProcess X, Y, SINK;
            st.AddChildProcess(X = new JitProcess
            {
                Name = "X",
            });
            st.AddChildProcess(Y = new JitProcess
            {
                Name = "Y",
            });
            st.AddChildProcess(SINK = new JitProcess
            {
                Name = "SINK",
            });

            // �H���ԃ����N
            // st.Links.SetPushLink(X, Y);  // Not set Push link because of pull expected. ��H���������̏ꍇ�́APushLink�͐ݒ肵�Ȃ��B
            st.AddProcessLink(Y, SINK);

            // �H���ɐ����t�^
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
            Y.InCommands.Add(new CiKanbanReturn // ����΂��O�H���Ɏ����I�ɕԋp���郂�[�h�i�u���ɂ���΂񂪋A��j
            {
                Delay = TimeSpan.FromSeconds(15),
            });


            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS���O�ɂ���

            st.SendKanban(new JitKanban
            {
                Location = JitLocation.CreateRoot(st, null),
                PullFromProcessKey = "X",
                PullToProcessKey = "Y",
                TestID = 1,
            });

            // �e�X�g���[�N�����iX�ɍH���[���j
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"w{(i + 1):0}",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, X),
                });
            }

            var k = 0;

            // ������Ԃ͎��ԏ��ɕ���ł�
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
            Assert.IsTrue(dat.Count == 2);   // w2 9:08 ��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[�ɂ͓���Ȃ�����

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(dat.Count == 2);   // w2 9:08 ��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[�ɂ͓���Ȃ�����


            // w1��Y�ɓ���Bw1�ɂ��Ă��� ����΂񂪁AX�Ɏ����ŕԋp����郂�[�h�ɂȂ��Ă���
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:05"));
            Assert.IsTrue(dat[0].DT.Second == 15);
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            // w2 9:08 ��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[�ɂ͓���Ȃ�����
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // 9:05:15 Kanban1��w2�ɕt�����Bw2��9:08�܂�X���ɑ؍�
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
            // w3�́A9:06��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[���珜�O�Bw3��9:11�܂�X�ō�Ƃ��Ă���
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3�́A9:06��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[���珜�O�Bw3��9:11�܂�X�ō�Ƃ��Ă���
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3�́A9:06��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[���珜�O�Bw3��9:11�܂�X�ō�Ƃ��Ă���
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:15", "Y"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3�́A9:06��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[���珜�O�Bw3��9:11�܂�X�ō�Ƃ��Ă���
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));

            // w1��SINK
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3�́A9:06��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[���珜�O�Bw3��9:11�܂�X�ō�Ƃ��Ă���
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3�́A9:06��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[���珜�O�Bw3��9:11�܂�X�ō�Ƃ��Ă���
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:15", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            // w3�́A9:06��X�ɓ��������A����΂񂪖����̂ŁAEvent�L���[���珜�O�Bw3��9:11�܂�X�ō�Ƃ��Ă���
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:15"));
            Assert.IsTrue(dat[0].DT.Second == 15);
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:25", "Y"));

            // w3��Kanban1������ X�ɗ����i9:08�ɍ�Ƃ��I����Ă��邪�j
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(dat[0].DT.Second == 15);   // Kanban1�̃��[�h�^�C�������Z����Ă���
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

            // w2��SINK
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:25", "X"));

            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:25"));
            Assert.IsTrue(dat[0].DT.Second == 15);   // Kanban1�̃��[�h�^�C�������Z����Ă���
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
            var st = new JitStage();

            JitProcess X, Y, SINK;
            st.AddChildProcess(X = new JitProcess
            {
                Name = "X",
            });
            st.AddChildProcess(Y = new JitProcess
            {
                Name = "Y",
            });
            st.AddChildProcess(SINK = new JitProcess
            {
                Name = "SINK",
            });

            // �H���ԃ����N
            // st.Links.SetPushLink(X, Y);  // ��H���������̏ꍇ�́APushLink�͐ݒ肵�Ȃ��B
            st.AddProcessLink(Y, SINK);

            // �H���ɐ����t�^
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

            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS���O�ɂ���

            // �e�X�g���[�N�����iX�ɍH���[���j
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"w{(i + 1):0}",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, X),
                });
            }

            var k = 0;

            // ������Ԃ͎��ԏ��ɕ���ł�
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

            // ��������΂�𓊓�
            var testid = 0;
            st.SendKanban(TimeUtil.Set(today, hour: 9, minute: 30), new JitKanban   // ����΂񑗂���A�H��X�ɂ̓��[�N�������̂ŁA�Ȃɂ����Ȃ�
            {
                Location = JitLocation.CreateRoot(st, null),
                PullFromProcessKey = "X",
                PullToProcessKey = Y.ID,
                TestID = ++testid,
            });
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:30"));

            st.SendKanban(TimeUtil.Set(today, hour: 9, minute: 30), new JitKanban   // ����΂񑗂���A�H��X�ɂ̓��[�N�������̂ŁA�Ȃɂ����Ȃ�
            {
                Location = JitLocation.CreateRoot(st, null),
                PullFromProcessKey = X.Name,
                PullToProcessKey = "Y",
                TestID = ++testid,
            });
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "9:30"));
            Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "9:30"));

            st.SendKanban(TimeUtil.Set(today, hour: 9, minute: 32), new JitKanban   // ����΂񑗂���A�H��X�ɂ̓��[�N�������̂ŁA�Ȃɂ����Ȃ�
            {
                Location = JitLocation.CreateRoot(st, null),
                PullFromProcessKey = "X",
                PullToProcessKey = "Y",
                TestID = ++testid,
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


            // Y����ASINK��PULL�v�����Ă݂�
            foreach (var nl in Collection.Rep(4))
            {
                st.SendKanban(TimeUtil.Set(today, hour: 12, minute: 00), new JitKanban   // ����΂񑗂���A�H��X�ɂ̓��[�N�������̂ŁA�Ȃɂ����Ȃ�
                {
                    Location = JitLocation.CreateRoot(st, null),
                    PullFromProcessKey = "SINK",
                    PullToProcessKey = "Y",
                    TestID = ++testid,
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
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "12:10", "SINK")); // Kanban7���Aw1�ɂ��āA�܂�SINK��Y���w�����ꂽ

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
            Assert.IsTrue(dat.Count == 2); // ��������΂񂪖����̂ŁAw2�́ASINK�ŏ���

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
            Assert.IsTrue(dat.Count == 1); // ��������΂񂪖����̂ŁAw3�́ASINK�ŏ���

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
            var st = new JitStage();

            JitProcess X, Y, SINK;
            st.AddChildProcess(X = new JitProcess
            {
                Name = "X",
            });
            st.AddChildProcess(Y = new JitProcess
            {
                Name = "Y",
            });
            st.AddChildProcess(SINK = new JitProcess
            {
                Name = "SINK",
            });

            // �H���ԃ����N
            // st.Links.SetPushLink(X, Y);  // ��H���������̏ꍇ�́APushLink�͐ݒ肵�Ȃ��B
            st.AddProcessLink(Y, SINK);

            // �H���ɐ����t�^
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

            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS���O�ɂ���

            // ��������΂�𓊓�
            int testid = 0;
            st.SendKanban(TimeUtil.Set(today, hour: 8, minute: 0), new JitKanban   // ����΂񑗂���A�H��X�ɂ̓��[�N�������̂ŁA�Ȃɂ����Ȃ�
            {
                Location = JitLocation.CreateRoot(st, null),
                TestID = ++testid,
                PullFromProcessKey = "X",
                PullToProcessKey = "Y",
            });

            // �e�X�g���[�N�����iX�ɍH���[���j
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"w{(i + 1):0}",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, X),
                });
            }

            var k = 0;

            // ������Ԃ͎��ԏ��ɕ���ł�
            var dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban1", EventTypes.KanbanIn, "8:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));

            // ����΂񂪍H��X�ɓ��������i�H��X�ɂ̓��[�N�������Ă��Ȃ��̂ŉ������������҂��j
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));

            // w1��X��In����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));

            // w2��X.Ci.Span=3��w1�{�R����Ɉړ�
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));

            // w3��X.Ci.Span=3��w1�{�R����Ɉړ�
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.In, "9:00"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));

            // w1��X��IN�������AKanban1�̖ړI�nY���t�^�����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));

            // w2��X��In����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));

            // w3��X.Ci.Span=3�����w2+3����Ɉړ�
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.In, "9:03"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            // w2��X��In�������A����΂񂪖����̂ŖړI�n�������B���̈� Event�L���[����O���ꂽ�B
            // �������Aw2��Out�����́A9:03+5�ŁA9:08�̏�ԁB
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            // Kanban2�� X�ɓ����˗�
            st.SendKanban(TimeUtil.Set(today, hour: 9, minute: 4), new JitKanban
            {
                Location = JitLocation.CreateRoot(st, null),
                TestID = ++testid,
                PullFromProcessKey = "X",
                PullToProcessKey = "Y",
            });
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "Kanban2", EventTypes.KanbanIn, "9:04"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:05", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));

            // Kanban2��w2�ɕt���A�ޏo�҂��ɂȂ�B
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

            // Kanban3�� X�ɓ����˗��Bw3��In�ɐ旧���A����Ƃ��e�X�g
            st.SendKanban(new JitKanban
            {
                Location = JitLocation.CreateRoot(st, null),
                TestID = ++testid,
                PullFromProcessKey = "X",
                PullToProcessKey = "Y",
            });

            // Kanban3 In�҂�
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

            // Kanban3�́AX�Ƀ��[�N�������̂ŁA�L���[�ɓ��邾��
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // w3���AX��In����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.In, "9:06"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // w3��X��In����Ƃ��ɁAKanban3���L��̂ŁA���H��Y���Z�b�g�����BEvent�L���[�ɂ�����
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:08", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:11", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));

            // w2�́AY.Ci.Max=1�œ���Ȃ��̂Ō��
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:11", "X"));
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));

            // w3�́AY.Ci.Max=1�œ���Ȃ��̂Ō��
            st.DoNext();
            dat = st.Events.Peeks(99).ToList(); k = 0;
            Assert.IsTrue(CMP(dat[k++], "w1", EventTypes.Out, "9:15", "Y"));
            Assert.IsTrue(CMP(dat[k++], "w2", EventTypes.Out, "9:15", "X"));
            Assert.IsTrue(CMP(dat[k++], "w3", EventTypes.Out, "9:15", "X"));

            // w1���ASINK��In����
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

            // w1��SINK
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

            st.AddChildProcess(JP = new JitProcessPriorityJoint
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
            st.AddChildProcess(X);
            JP.AddChildProcess(X.ID);

            st.AddChildProcess(Y);
            JP.AddChildProcess(Y.ID);

            st.AddChildProcess(JP);
            st.AddChildProcess(Z = new JitProcess
            {
                Name = "Z",
            });
            st.AddChildProcess(SINK = new JitProcess
            {
                Name = "SINK",
            });

            st.AddProcessLink(JP, Z);
            st.AddProcessLink(Z, SINK);
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

            // �e�X�g���[�N����
            var today = TimeUtil.ClearTime(DateTime.Now);
            for (var i = 0; i < 3; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"x{(i + 1):0}",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, X),
                });
            }
            for (var i = 0; i < 1; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 5), EventTypes.Out, new JitWork
                {
                    Name = $"y{(i + 1):0}",
                    Current = JitLocation.CreateRoot(st, null),
                    Next = JitLocation.CreateRoot(st, Y),
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

            // x1�� �H��X��In�iX.Co.Delay=1�j
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

            // x1���AZ��In
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

            // x2�́AX��In(X.Co.Delay=1)
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

            // y1�́AY�ւ�In����
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

        /// <summary>
        /// JointProcess Check priority exit
        /// </summary>
        /// <remarks>
        /// +----------+
        /// |JP:Subset |
        /// | +-+  +-+ |  +-+  +----+
        /// | |X||||Y| |--|Z|--|SINK|
        /// | +-+  +-+ |  +-+  +----+
        /// +----------+
        /// </remarks>
        [TestMethod]
        public void Test004_JointProcess_No2()
        {
            JitProcessPriorityJoint JP;
            var st = new JitStage();

            st.AddChildProcess(JP = new JitProcessPriorityJoint
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

            JP.AddChildProcess(X);
            JP.AddChildProcess(Y);    // Y is priority

            st.AddChildProcess(JP);
            st.AddChildProcess(Z = new JitProcess
            {
                Name = "Z",
            });
            st.AddChildProcess(SINK = new JitProcess
            {
                Name = "SINK",
            });

            st.AddProcessLink(JP, Z);
            st.AddProcessLink(Z, SINK);

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
                    Current = new JitLocation   //.From(JP, null),
                    {
                        Stage = st,
                        SubsetCache = JP,
                        Path = "\\JP",
                        Process = null,
                    },
                    Next = new JitLocation   //.From(JP, null),
                    {
                        Stage = st,
                        SubsetCache = JP,
                        Path = "\\JP",
                        Process = X,
                    },
                });
            }
            for (var i = 0; i < 2; i++)
            {
                st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, new JitWork
                {
                    Name = $"y{(i + 1):0}",
                    Current = new JitLocation   //.From(JP, null),
                    {
                        Stage = st,
                        SubsetCache = JP,
                        Path = "\\JP",
                        Process = null,
                    },
                    Next = new JitLocation   //.From(JP, null),
                    {
                        Stage = st,
                        SubsetCache = JP,
                        Path = "\\JP",
                        Process = X,
                    },
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

        /// <summary>
        /// JointProcess
        /// </summary>
        /// <remarks>
        /// +----------+
        /// |JP:Subset |
        /// | +-+  +-+ |  +-+
        /// | |X||||Y| |--|Z|
        /// | +++  +++ |  +-+
        /// |  |    |  |
        /// +--+----+--+
        ///    |    |
        ///    �@  y1�`y3
        /// </remarks>
        [TestMethod]
        public void Test003_JointProcess_No1()
        {
            JitProcessPriorityJoint JP;
            var st = new JitStage();

            st.AddChildProcess(JP = new JitProcessPriorityJoint
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
            JP.AddChildProcess("X");     // Add child process (NAME only ... dummy process)
            JP.AddChildProcess(Y.ID);    // Add child process (ID only   ... dummy process)

            st.AddChildProcess(JP);
            st.AddChildProcess(Z = new JitProcess
            {
                Name = "Z",
            });

            st.AddProcessLink(JP, Z);    // LINK ON STAGE.Subset

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
                    Current = JitLocation.CreateRoot(st, null),
                    Next = new JitLocation // Next Subset, Process(class)
                    {
                        Stage = st,
                        SubsetCache = JP,
                        Path = "\\JP",
                        Process = Y,
                    },
                    Name = $"y{(i + 1):0}", 
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

            //--- inject Process instance at this timing ---------------------
            JP.AddChildProcess(X);   // Expected to replace dummy process to actual process
            JP.AddChildProcess(Y);   // Expected to replace dummy process to actual process
            //------------------------

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
            st.AddChildProcess(new[]
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

            st.AddProcessLink(X, Y);
            st.AddProcessLink(Y, Z);

            var today = TimeUtil.ClearTime(DateTime.Now);
            JitWork a, b, c;
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, a = new JitWork
            {
                Name = "a",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, X),
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, b = new JitWork
            {
                Name = "b",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, X),
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, c = new JitWork
            {
                Name = "c",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, X),
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
            st.AddChildProcess(new[]
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

            Assert.IsTrue(X.Is(":Process"));

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

            st.AddProcessLink("X", Y.ID);
            st.AddProcessLink(Y, Z);

            var today = TimeUtil.ClearTime(DateTime.Now);  // H:M:S:MS = 00:00:00:000
            JitWork a, b, c;
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 0), EventTypes.Out, a = new JitWork
            {
                Name = "a",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, X),
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 1), EventTypes.Out, b = new JitWork
            {
                Name = "b",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, X),
            });
            st.Events.Enqueue(TimeUtil.Set(today, hour: 9, minute: 2), EventTypes.Out, c = new JitWork
            {
                Name = "c",
                Current = JitLocation.CreateRoot(st, null),
                Next = JitLocation.CreateRoot(st, X),
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
            Assert.IsTrue(CMP(dat[1], "a", EventTypes.Out, "9:01")); Assert.IsTrue(dat[1].Work.Current.Process.Name == "X");
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:02"));
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("X")).Count() == 1);

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
            Assert.IsTrue(CMP(dat[2], "a", EventTypes.Out, "9:05")); Assert.IsTrue(dat[2].Work.Current.Process.Name == "Y");
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("X")).Count() == 0);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Y")).Count() == 1);

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
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.Out, "9:04")); Assert.IsTrue(dat[0].Work.Current.Process.Name == "X");
            Assert.IsTrue(CMP(dat[1], "a", EventTypes.Out, "9:05"));
            Assert.IsTrue(CMP(dat[2], "c", EventTypes.Out, "9:06"));
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("X")).Count() == 1);


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
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Y")).Count() == 0);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Z")).Count() == 1);

            st.DoNext();    // 14
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "b", EventTypes.In, "9:05"));
            Assert.IsTrue(CMP(dat[1], "c", EventTypes.Out, "9:06"));

            st.DoNext();    // 15
            dat = st.Events.Peeks(3).ToList();
            Assert.IsTrue(CMP(dat[0], "c", EventTypes.Out, "9:06"));
            Assert.IsTrue(CMP(dat[1], "b", EventTypes.Out, "9:09"));
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("X")).Count() == 0);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Y")).Count() == 1);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Z")).Count() == 1);

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
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("X")).Count() == 1);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Y")).Count() == 1);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Z")).Count() == 1);

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
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("X")).Count() == 1);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Y")).Count() == 0);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Z")).Count() == 2);

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
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("X")).Count() == 0);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Y")).Count() == 0);
            Assert.IsTrue(st.GetWorks("\\", st.FindChildProcess("Z")).Count() == 3);
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
                    if (work.Current != default)
                    {
                        ret &= work.Current.Process.Name == procName;
                    }
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
