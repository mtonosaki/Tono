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
    public class TonoJit_JaC
    {
        [TestMethod]
        public void Test01()
        {
            var c = @"
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
            var bbb = jac["'bbb'"] as JitProcess;   // get instance by name
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
                            Name = 'IgnoreProcess'
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
            Assert.AreEqual(jac.Stage("st")?.Procs.Count, 1);
            Assert.AreNotEqual(jac.Stage("st")?.Procs[0].Name, "IgnoreProcess");
        }
        [TestMethod]
        public void Test07()
        {
            var code = @"
                st = new Stage
                    Procs
                        add p1 = new Process
                            Name = 'PROCP1'
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
            Assert.AreEqual(jac.Stage("st")?.Procs.Count, 0);
        }
        [TestMethod]
        public void Test08()
        {
            var code = @"
                new Stage
                    Name = 'MyStage'
                    Procs
                        add p1 = new Process
                            Name = 'PROCP1'
                        add p2 = new Process
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            //--------------------------------------------------------
            code = $@"
                'MyStage'               // To find Stage object named 'MyStage'
                    Procs
                        remove 'PROCP1' // find JitProcess instance by name
                        remove p2       // find JitProcess instance by variable
            ";
            jac.Exec(code);
            Assert.AreEqual(jac.Stage("'MyStage'")?.Procs.Count, 0);
        }
        [TestMethod]
        public void Test09()
        {
            var code = @"
                new Stage
                    Procs
                        add p1 = new Process
                            Name = 'PROCP1'
                        add p2 = new Process
                    Name = 'MyStage'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            //--------------------------------------------------------
            code = $@"
                'MyStage'
                    Procs
                        remove 'PROCP1' // find JitProcess instance by name
                        remove p2       // find JitProcess instance by variable
            ";
            jac.Exec(code);
            Assert.AreEqual(jac.Stage("'MyStage'")?.Procs.Count, 0);
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
                                    Span = 11s
                                add new CiDelay
                                    Delay = 66m
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var proc = jac.Process("p1");
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
                a = 1ms
                b = 2s
                c = 3m
                d = 4h
                e = 5d
                f = 6w
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
                a = 1.1ms
                b = 2.1s
                c = 3.1m
                d = 4.1h
                e = 5.1d
                f = 6.1w
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
                    Value = 123s
                d = new Variable
                    Value = 1.232
                e = new Variable
                    Value = a
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac.Variable("a").Value, "STR");
            Assert.AreEqual(jac.Variable("b").Value, 123);
            Assert.AreEqual(jac.Variable("c").Value, TimeSpan.FromSeconds(123));
            Assert.AreEqual(jac.Variable("d").Value, 1.232);
            Assert.AreEqual(jac.Variable("e").Value, jac.Variable("a"));
            Assert.IsTrue(ReferenceEquals(jac.Variable("e").Value, jac.Variable("a")));
            Assert.IsFalse(ReferenceEquals(jac.Variable("e").Value, jac.Variable("b")));
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
                                    Delay = 1.5m
                                    TargetWorkClass = ':Car'
                                    Destination = sink
                                add i2 = new CiDelay
                                    Delay = 2.5h
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
            Assert.AreEqual(i1.Destination(), jac.Process("sink")); // check lazy method

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
                                    WaitSpan = 0.5m
                                    
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var o1 = jac["o1"] as CoJoinFrom;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.PullFrom(), jac.Process("sink"));
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
                                            Name = 'MyWork01'
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
            var w1 = jac.Work("MyWork01");
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
                                    Span = 0.1h
                                    PorlingSpan = 1s
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
            var k1 = jac.Kanban("k1");
            var p1 = jac.Process("p1");
            var p2 = jac.Process("p2");
            var w1 = jac.Work("w1");
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
            catch (JacException ex)
            {
                Assert.Fail();
            }
        }
    }
}