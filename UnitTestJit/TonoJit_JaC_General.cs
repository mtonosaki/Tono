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
    public class TonoJit_JaC_General
    {
        [TestMethod]
        public void Test01()
        {
            var c = $@"
                st = new Stage
                    Procs
                        add b  = new Process
                            Name = 'AAA'
                            Name = 'aaa'
                        add new Process
                            Name = 'bbb'
            ";
            var jac = new JacInterpreter();
            jac.Exec(c);
            Assert.AreEqual(jac["st"].GetType(), typeof(JitStage));
            Assert.AreEqual(jac["b"].GetType(), typeof(JitProcess));
            var b = jac["b"] as JitProcess;
            Assert.AreEqual(b.Name, "aaa");
        }
        [TestMethod]
        public void Test02()
        {
            var code = @"
                st = new Stage
                    Procs
                        add b  = new Process
                            Name = 'AAA'
                            Name = 'aaa'
                        add c=new Process
                            ID = 'IDFINDB'  // Try to find by ID
                            Name = 'BBB'
                            Name = 'bbb'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac["st"].GetType(), typeof(JitStage));

            Assert.AreEqual(jac["b"].GetType(), typeof(JitProcess));
            var b = jac["b"] as JitProcess;
            Assert.AreEqual(b.Name, "aaa");
            Assert.AreEqual(jac["c"].GetType(), typeof(JitProcess));
            var c = jac["c"] as JitProcess;         // get instance by variable
            Assert.AreEqual(c.Name, "bbb");

            var bbb = jac["'IDFINDB'"] as JitProcess;   // get instance by name
            Assert.AreEqual(c, bbb);
        }
        [TestMethod]
        public void Test03()
        {
            var code = @"
                st = new Stage
                    Procs
                        add new Process
                        add new Process
                        add new Process
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac["st"] as JitStage;
            Assert.IsNotNull(st);
            Assert.AreEqual(st.Procs.Count, 3);
            Assert.AreEqual(st.Procs[0], st.Procs[0]);  // check Equals
            Assert.AreEqual(st.Procs[0].GetHashCode(), st.Procs[0].GetHashCode());  // check GetHashCode
            Assert.AreNotEqual(st.Procs[0], st.Procs[1]);
            Assert.AreNotEqual(st.Procs[0], st.Procs[2]);
            Assert.AreNotEqual(st.Procs[1], st.Procs[2]);
        }
        [TestMethod]
        public void Test04()
        {
            var code = @"
                st = new Stage
                    Procs
                        add new Process
                            BadName = 'ShouldBeError'
            ";
            try
            {
                var jac = new JacInterpreter();
                jac.Exec(code);
                var st = jac["st"] as JitStage;
            }
            catch (JacException ex)
            {
                Assert.AreEqual(ex.Code, JacException.Codes.NotImplementedProperty);
                Assert.IsTrue(ex.Message.Contains("BadName"));
            }
        }
        [TestMethod]
        public void Test05()
        {
            var code = @"
                st = new Stage
                    Procs
                        add new Process
                        add new Process
                            Name = 'IgnoreProcess'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            //--------------------------------------------------------
            var st = jac["st"] as JitStage;
            code = $@"
                st
                    Procs
                        remove '{st.Procs[0].Name}'
            ";
            jac.Exec(code);
            Assert.AreEqual(st.Procs.Count, 1);
            Assert.AreEqual(st.Procs[0].Name, "IgnoreProcess");
        }
        [TestMethod]
        public void Test06()
        {
            var code = @"
                st = new Stage
                    Procs
                        add new Process
                        add new Process
                            ID = 'IgnoreProcess'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            //--------------------------------------------------------
            code = $@"
                st
                    Procs
                        remove    'IgnoreProcess'
            ";
            jac.Exec(code);
            Assert.AreEqual(jac.GetStage("st")?.Procs.Count, 1);
            Assert.AreNotEqual(jac.GetStage("st")?.Procs[0].Name, "IgnoreProcess");
        }
        [TestMethod]
        public void Test07()
        {
            var code = @"
                st = new Stage
                    Procs
                        add p1 = new Process
                            ID = 'PROCP1'
                        add p2 = new Process
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            //--------------------------------------------------------
            code = $@"
                st
                    Procs
                        remove 'PROCP1'
                        remove p2
            ";
            jac.Exec(code);
            Assert.AreEqual(jac.GetStage("st")?.Procs.Count, 0);
        }
        [TestMethod]
        public void Test08()
        {
            var code = @"
                new Stage
                    ID = 'MyStage'
                    Name = 'MySweetStage'
                    Procs
                        add p1 = new Process
                            ID = 'PROCP1'
                            Name = 'MyPROCP1'
                        add p2 = new Process
                        add p3 = new Process
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            //--------------------------------------------------------
            code = $@"
                MyStage               // To find Stage object named 'MyStage'
                    Procs
                        remove 'PROCP1' // find JitProcess instance by ID
                        remove p2       // find JitProcess instance by variable
            ";
            jac.Exec(code);
            Assert.IsNull(jac.GetStage("MySweetStage"));
            var MyStage = jac.GetStage("MyStage");
            Assert.IsNotNull(MyStage);
            Assert.IsNotNull(MyStage);
            Assert.AreEqual(MyStage.Procs.Count, 1);
        }
        [TestMethod]
        public void Test09()
        {
            var code = @"
                new Stage
                    Procs
                        add p1 = new Process
                            ID = 'PROCP1'
                        add p2 = new Process
                    ID = 'MyStage'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            //--------------------------------------------------------
            code = $@"
                'MyStage'               // You can also to find with string value 'MyStage' 
                    Procs
                        remove 'PROCP1' // find JitProcess instance by name
                        remove p2       // find JitProcess instance by variable
            ";
            jac.Exec(code);
            Assert.AreEqual(jac.GetStage("'MyStage'")?.Procs.Count, 0);
        }

        [TestMethod]
        public void Test10()
        {
            var code = @"
                new Stage
                    Procs
                        add p1 = new Process
                            Name = 'PROCP1'
                            Cio
                                add new CoSpan
                                    Span = 11S
                                add new CiDelay
                                    Delay = 66M
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var proc = jac.GetProcess("p1");
            Assert.IsNotNull(proc);

            var span = proc.Cios.Select(a => a as CoSpan).Where(a => a != null).FirstOrDefault();
            Assert.AreEqual(span.Span, TimeSpan.FromSeconds(11));
            var delay = proc.Cios.Select(a => a as CiDelay).Where(a => a != null).FirstOrDefault();
            Assert.AreEqual(delay.Delay, TimeSpan.FromMinutes(66));
        }

        [TestMethod]
        public void Test11()
        {
            var code = @"
                a = 1MS
                b = 2S
                c = 3M
                d = 4H
                e = 5D
                f = 6W
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac["a"], TimeSpan.FromMilliseconds(1));
            Assert.AreEqual(jac["b"], TimeSpan.FromSeconds(2));
            Assert.AreEqual(jac["c"], TimeSpan.FromMinutes(3));
            Assert.AreEqual(jac["d"], TimeSpan.FromHours(4));
            Assert.AreEqual(jac["e"], TimeSpan.FromDays(5));
            Assert.AreEqual(jac["f"], TimeSpan.FromDays(6 * 7));
        }
        [TestMethod]
        public void Test12()
        {
            var code = @"
                a = 1.1MS
                b = 2.1S
                c = 3.1M
                d = 4.1H
                e = 5.1D
                f = 6.1W
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac["a"], TimeSpan.FromMilliseconds(1.1));
            Assert.AreEqual(jac["b"], TimeSpan.FromSeconds(2.1));
            Assert.AreEqual(jac["c"], TimeSpan.FromMinutes(3.1));
            Assert.AreEqual(jac["d"], TimeSpan.FromHours(4.1));
            Assert.AreEqual(jac["e"], TimeSpan.FromDays(5.1));
            Assert.AreEqual(jac["f"], TimeSpan.FromDays(6.1 * 7));
        }

        [TestMethod]
        public void Test13()
        {
            var code = @"
                a = new Variable
                    Value = 'STR'
                b = new Variable
                    Value = 123
                c = new Variable
                    Value = 123S
                d = new Variable
                    Value = 1.232
                e = new Variable
                    Value = a
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac.GetVariable("a").Value, "STR");
            Assert.AreEqual(jac.GetVariable("b").Value, 123);
            Assert.AreEqual(jac.GetVariable("c").Value, TimeSpan.FromSeconds(123));
            Assert.AreEqual(jac.GetVariable("d").Value, 1.232);
            Assert.AreEqual(jac.GetVariable("e").Value, jac.GetVariable("a"));
            Assert.IsTrue(ReferenceEquals(jac.GetVariable("e").Value, jac.GetVariable("a")));
            Assert.IsFalse(ReferenceEquals(jac.GetVariable("e").Value, jac.GetVariable("b")));
        }

        [TestMethod]
        public void Test14()
        {
            var code = @"
                new Stage
                    Procs
                        add sink = new Process
                        add p1 = new Process
                            Name = 'PROCP1'
                            Cio
                                add i1 = new CiPickTo
                                    Delay = 1.5M
                                    TargetWorkClass = ':Car'
                                    Destination = sink
                                add i2 = new CiDelay
                                    Delay = 2.5H
                                add i3 = new CiSwitchNextLink
                                    NextLinkVarName = new Variable
                                        Value = 'AA'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);

            var i1 = jac["i1"] as CiPickTo;
            Assert.IsNotNull(i1);
            Assert.AreEqual(i1.Delay, TimeSpan.FromMinutes(1.5));
            Assert.AreEqual(i1.TargetWorkClass, ":Car");
            Assert.AreEqual(i1.Destination(), jac.GetProcess("sink")); // check lazy method

            var i2 = jac["i2"] as CiDelay;
            Assert.IsNotNull(i2);
            Assert.AreEqual(i2.Delay, TimeSpan.FromHours(2.5));

            var i3 = jac["i3"] as CiSwitchNextLink;
            Assert.IsNotNull(i3);
            Assert.AreEqual(i3.NextLinkVarName, JitVariable.From("AA"));
        }

        [TestMethod]
        public void Test15()
        {
            var code = @"
                s = new CiSwitchNextLink
                    NextLinkVarName = 'NextLinkNo2' // Try to auto cast from string to JitVariable
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var s = jac["s"] as CiSwitchNextLink;
            Assert.IsNotNull(s);
            Assert.AreEqual(s.NextLinkVarName, JitVariable.From("NextLinkNo2"));
        }


        [TestMethod]
        public void Test16()
        {
            var code = @"
                new Stage
                    Procs
                        add sink = new Process
                        add p1 = new Process
                            Name = 'MyProc'
                            Cio
                                add i1 = new CiSwitchNextLink
                                    NextLinkVarName = new Variable
                                        Value = 'NextLinkNo2222'
                                    TargetWorkClass = ':Work2'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var i1 = jac["i1"] as CiSwitchNextLink;
            Assert.IsNotNull(i1);
            Assert.AreEqual(i1.NextLinkVarName, JitVariable.From("NextLinkNo2222"));
            Assert.AreEqual(i1.TargetWorkClass, ":Work2");
        }
        [TestMethod]
        public void Test17()
        {
            var code = @"
                new Stage
                    Procs
                        add sink = new Process
                        add p1 = new Process
                            Name = 'MyProc'
                            Cio
                                add o1 = new CoJoinFrom
                                    PullFrom = sink
                                    ChildPartName = 'TEPA'
                                    WaitSpan = 0.5M                                    
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var o1 = jac["o1"] as CoJoinFrom;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.PullFrom(), jac.GetProcess("sink"));
            Assert.AreEqual(o1.ChildPartName, "TEPA");
            Assert.AreEqual(o1.WaitSpan, TimeSpan.FromSeconds(30));
        }
        [TestMethod]
        public void Test18()
        {
            var code = @"
                w1 = new Work
                w2 = new Work
                w3 = new Work
                new Stage
                    Procs
                        add sink = new Process
                        add p1 = new Process
                            Name = 'MyProc'
                            Cio
                                add o1 = new CoMaxCost
                                    ReferenceVarName = 'Weight'
                                    WorkInReserve
                                        add new Work
                                            ID = 'MyWork01'
                                        add w1
                                        add w2
                                        add w3
                                    Value = 500
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var o1 = jac["o1"] as CoMaxCost;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.ReferenceVarName, JitVariable.From("Weight"));
            Assert.AreEqual(o1.Value, 500.0);
            var w1 = jac.GetWork("MyWork01");
            Assert.IsNotNull(w1);
            Assert.AreEqual(o1.GetWorkInReserves().Count(), 4);
            Assert.AreEqual(o1.GetWorkInReserves().FirstOrDefault(), w1);

            code = @"
                o1
                    WorkInReserve
                        remove MyWork01
            ";
            jac.Exec(code);
            Assert.AreEqual(o1.GetWorkInReserves().Count(), 3);
            Assert.IsNull(o1.GetWorkInReserves().Where(a => a.Name == w1.Name).FirstOrDefault());
        }

        [TestMethod]
        public void Test19()
        {
            var code = @"
                new Stage
                    Procs
                        add sink = new Process
                        add p1 = new Process
                            Cio
                                add o1 = new CoSpan
                                    Span = 0.1H
                                    PorlingSpan = 1S
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var o1 = jac["o1"] as CoSpan;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.Span, TimeSpan.FromHours(0.1));
            Assert.AreEqual(o1.PorlingSpan, TimeSpan.FromSeconds(1.0));
        }

        [TestMethod]
        public void Test20()
        {
            var code = @"
                p1 = new Process
                p2 = new Process
                w1 = new Work
                k1 = new Kanban
                    PullFrom = p1
                    PullTo = p2
                    Work = w1
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var k1 = jac.GetKanban("k1");
            var p1 = jac.GetProcess("p1");
            var p2 = jac.GetProcess("p2");
            var w1 = jac.GetWork("w1");
            Assert.IsNotNull(k1);
            Assert.IsNotNull(p1);
            Assert.IsNotNull(p2);
            Assert.IsNotNull(w1);
            Assert.AreEqual(k1.PullFrom(), p1);
            Assert.AreEqual(k1.PullTo(), p2);
            Assert.AreEqual(k1.Work, w1);
        }

        [JacTarget(Name = "TestJitClass")]
        public class TestJitClass
        {
        }

        [TestMethod]
        public void Test21()
        {
            try
            {
                var code = @"
                    t1 = new TestJitClass
                ";
                var jac = new JacInterpreter();
                jac.Exec(code);
            }
            catch (JacException ex)
            {
                Assert.AreEqual(ex.Code, JacException.Codes.TypeNotFound);
            }
            try
            {
                var code = @"
                    t1 = new TestJitClass
                ";
                JacInterpreter.RegisterJacTarget(typeof(TestJitClass).Assembly);
                var jac = new JacInterpreter();
                jac.Exec(code);
            }
            catch (JacException)
            {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void Test22()
        {
            var code = @"
                    p = new Process
                    p.X = 1234
                    p.Y = '5678' // Jac parser deeply makes to integer 5678
                    p.Z = p.X
                ";
            var jac = JacInterpreter.From(code);
            Assert.IsNotNull(jac.GetProcess("p"));
            Assert.AreEqual(jac.GetProcess("p").ChildVriables["X"]?.Value, 1234);
            Assert.AreEqual(jac.GetProcess("p").ChildVriables["Y"]?.Value, 5678);  // To check '5678' will be parsed deeply to integer
            Assert.AreEqual(jac.GetProcess("p").ChildVriables["Z"]?.Value, 1234);
        }

        [TestMethod]
        public void Test23()
        {
            var code = @"
                    p = new Process
                        Name = 'TestProc'
                    a123 = new Variable
                    a123.AAA = p
                    a123.BBB = p.Name   // To check .Name that is NOT child value (JitProcess's property)
                ";
            var jac = JacInterpreter.From(code);
            var p = jac.GetProcess("p");
            var a123 = jac.GetVariable("a123");
            Assert.IsNotNull(p);
            Assert.IsTrue(a123.ChildVriables["AAA"].Is(":Process"));
            Assert.IsTrue(a123.ChildVriables["AAA"].Value is JitProcess);
            Assert.AreEqual(((JitProcess)a123.ChildVriables["AAA"].Value).Name, "TestProc");
            Assert.IsTrue(a123.ChildVriables["BBB"].Is(JitProcess.Class.String));
            Assert.AreEqual(a123.ChildVriables["BBB"].Value, "TestProc");
        }
        [TestMethod]
        public void Test24()
        {
            var code = @"
                    new Process
                        ID = 'TestProc'
                        AAA = 123   // When Process has NOT property named AAA then, call JacSetDotValueAttribute
                        BBB = 456   // Same
                ";
            var jac = JacInterpreter.From(code);
            var p = jac.GetProcess("TestProc");
            Assert.IsNotNull(p);
            Assert.AreEqual(p.ChildVriables["AAA"]?.Value, 123);
            Assert.AreEqual(p.ChildVriables["BBB"]?.Value, 456);
        }
    }
}
