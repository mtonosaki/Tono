// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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

            var bbb = jac["IDFINDB"] as JitProcess;   // get instance by name
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
                        remove {st.Procs[0].Name}
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
            Assert.IsNotNull(jac.GetProcess("IgnoreProcess"));
            var name = jac.GetProcess("IgnoreProcess").Name;
            Assert.IsNotNull(jac.GetProcess(name));
            //--------------------------------------------------------
            code = $@"
                st
                    Procs
                        remove    IgnoreProcess  // Can specify ID (Cannot specify 'IgnoreProcess' as string)
            ";
            jac.Exec(code);
            Assert.AreEqual(jac.GetStage("st")?.Procs.Count, 1);
            Assert.AreNotEqual(jac.GetStage("st")?.Procs[0].Name, "IgnoreProcess");
            Assert.IsNull(jac.GetProcess("IgnoreProcess")); // removed from VarBuffer
            Assert.IsNull(jac.GetProcess(name));            // removed from InstanceBuffer
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
                        remove PROCP1   // Can specify ID (Cannot specify 'PROCP1' as string)
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
                        remove PROCP1   // find JitProcess instance by ID (not by ID as string)
                        remove p2       // find JitProcess instance by variable
            ";
            jac.Exec(code);
            var MyStage = jac.GetStage("MyStage");
            var MySweetStage = jac.GetStage("MySweetStage");
            Assert.IsNotNull(MyStage);
            Assert.IsNotNull(MySweetStage);
            Assert.AreEqual(MyStage, MySweetStage);
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
                MyStage                 // You can also to find with variable name 'MyStage'
                    Procs
                        remove PROCP1   // find JitProcess instance by ID
                        remove p2       // find JitProcess instance by variable
            ";
            jac.Exec(code);
            Assert.AreEqual(jac.GetStage("MyStage")?.Procs.Count, 0);
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
                bm = new Variable
                    Value = -123
                c = new Variable
                    Value = 123S
                cm = new Variable
                    Value = -123S
                cmd = new Variable
                    Value = -123.4S
                cmd2 = new Variable
                    Value = -1.234e-2S
                cmd3 = new Variable
                    Value = -1.234e+2S
                cmd4 = new Variable
                    Value = -1.234E2S
                d = new Variable
                    Value = 1.232
                e = new Variable
                    Value = a
                f = new Variable
                    Value = 1.23e10
                g = new Variable
                    Value = 1.23E10
                h = new Variable
                    Value = 1.23e-10
                i = new Variable
                    Value = 95.5%
                j = new Variable
                    Value = 9.5e-2%
                k = new Variable
                    Value = -9.5e-2%
                l = new Variable
                    Value = -9.5e+2%
                m = new Variable
                    Value = -9.5e2%
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac.GetVariable("a").Value, "STR");
            Assert.AreEqual(jac.GetVariable("b").Value, 123);
            Assert.AreEqual(jac.GetVariable("bm").Value, -123);
            Assert.AreEqual(jac.GetVariable("c").Value, TimeSpan.FromSeconds(123));
            Assert.AreEqual(jac.GetVariable("cm").Value, TimeSpan.FromSeconds(-123));
            Assert.AreEqual(jac.GetVariable("cmd").Value, TimeSpan.FromSeconds(-123.4));
            Assert.AreEqual(jac.GetVariable("cmd2").Value, TimeSpan.FromSeconds(-1.234e-2));
            Assert.AreEqual(jac.GetVariable("cmd3").Value, TimeSpan.FromSeconds(-1.234e+2));
            Assert.AreEqual(jac.GetVariable("cmd4").Value, TimeSpan.FromSeconds(-1.234E2));
            Assert.AreEqual(jac.GetVariable("d").Value, 1.232);
            Assert.AreEqual(jac.GetVariable("e").Value, jac.GetVariable("a"));
            Assert.IsTrue(ReferenceEquals(jac.GetVariable("e").Value, jac.GetVariable("a")));
            Assert.IsFalse(ReferenceEquals(jac.GetVariable("e").Value, jac.GetVariable("b")));
            Assert.AreEqual(jac.GetVariable("f").Value, 1.23e10);
            Assert.AreEqual(jac.GetVariable("g").Value, 1.23e10);
            Assert.AreEqual(jac.GetVariable("h").Value, 1.23e-10);
            Assert.AreEqual(jac.GetVariable("i").Value, 95.5 / 100.0);
            Assert.AreEqual(jac.GetVariable("j").Value, 9.5e-2 / 100.0);
            Assert.AreEqual(jac.GetVariable("k").Value, -9.5e-2 / 100.0);
            Assert.AreEqual(jac.GetVariable("l").Value, -9.5e2 / 100.0);
        }

        [TestMethod]
        public void Test14()
        {
            var code = @"
                st = new Stage
                    Procs
                        add sink = new Process
                        add p1 = new Process
                            Name = 'PROCP1'
                            Cio
                                add i1 = new CiPickTo
                                    Delay = 1.5M
                                    TargetWorkClass = ':Car'
                                    DestProcessKey = sink.ID
                                add i2 = new CiDelay
                                    Delay = 2.5H
                                add i3 = new CiSwitchNextLink
                                    NextLinkVarName = new Variable
                                        Value = 'AA'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");
            var i1 = jac["i1"] as CiPickTo;
            Assert.IsNotNull(i1);
            Assert.AreEqual(i1.Delay, TimeSpan.FromMinutes(1.5));
            Assert.AreEqual(i1.TargetWorkClass, ":Car");
            Assert.AreEqual(st.FindProcess(i1.DestProcessKey), jac.GetProcess("sink")); // check lazy method

            var i2 = jac["i2"] as CiDelay;
            Assert.IsNotNull(i2);
            Assert.AreEqual(i2.Delay, TimeSpan.FromHours(2.5));

            var i3 = jac["i3"] as CiSwitchNextLink;
            Assert.IsNotNull(i3);
            Assert.AreEqual(i3.NextLinkVarName, JitVariable.From("AA"));
        }

        [TestMethod]
        public void Test14_2()
        {
            var code = @"
                st = new Stage
                    Procs
                        add p1 = new Process
                            Name = 'PROCP1'
                            Cio
                                add i1 = new CiPickTo
                                    Delay = 1.5M
                                    TargetWorkClass = ':Car'
                                    DestProcessKey = 'SUPERLAZY'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");

            var i1 = jac["i1"] as CiPickTo;
            Assert.IsNotNull(i1);
            var i1dest = st.FindProcess("SUPERLAZY", isReturnNull: true);
            Assert.IsNull(i1dest);  // Expected Null because of no registered yet.

            var code2 = @"
                st
                    Procs
                        add p2 = new Process
                            Name = 'SUPERLAZY'
            ";
            jac.Exec(code2);
            i1dest = st.FindProcess("SUPERLAZY", isReturnNull: true);
            var p2 = jac.GetProcess("p2");
            Assert.AreEqual(i1dest, p2);  // Then FindProcess can find p2 named SUPERLAZY

            i1dest = st.FindProcess(p2.ID, isReturnNull: true);  // You can also find by ID 
            Assert.AreEqual(i1dest, p2);
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
                            Name = 'SinkProc'
                        add p1 = new Process
                            Name = 'MyProc'
                            Cio
                                add o1 = new CoJoinFrom
                                    PullFromProcessKey = 'SinkProc'
                                    ChildPartName = 'TEPA'
                                    WaitSpan = 0.5M                                    
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var o1 = jac["o1"] as CoJoinFrom;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.PullFromProcessKey, jac.GetProcess("SinkProc").Name);
            Assert.AreEqual(o1.ChildPartName, "TEPA");
            Assert.AreEqual(o1.WaitSpan, TimeSpan.FromSeconds(30));
        }
        [TestMethod]
        public void Test18()
        {
            var code = @"
                st = new Stage
                    Procs
                        add sink = new Process
                        add p1 = new Process
                            Name = 'MyProc'
                            Cio
                                add o1 = new CoMaxCost
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");

            var code2 = @"
                w1 = new Work
                    Stage = st
                w2 = new Work
                    Stage = st
                w3 = new Work
                    Stage = st
            ";
            jac.Exec(code2);
            var w2 = jac.GetWork("w2");
            Assert.AreEqual(st, w2.Stage);

            var code3 = @"
                o1
                    ReferenceVarName = 'Weight'
                    Value = 500
            ";
            jac.Exec(code3);

            var o1 = jac["o1"] as CoMaxCost;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.ReferenceVarName, JitVariable.From("Weight"));
            Assert.AreEqual(o1.Value, 500.0);
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
                    PullFromProcessKey = p1.ID
                    PullToProcessKey = p2.ID
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
            Assert.AreEqual(k1.PullFromProcessKey, p1.ID);
            Assert.AreEqual(k1.PullToProcessKey, p2.ID);
            Assert.AreEqual(k1.Work, w1);
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
            Assert.AreEqual(jac.GetProcess("p").ChildVriables["Y"]?.Value, "5678");  // To check '5678' is as string
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
                    a123.BBB = p.Name   // To check that a123.BBB reference equals to p.Name
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
        public void Test23_2()
        {
            var code = @"
                    p = new Process
                        Name = 'TestProc'
                    p.Name = 'ChangedName'
                ";
            var jac = JacInterpreter.From(code);
            var p = jac.GetProcess("p");
            Assert.IsNotNull(p);
            Assert.AreEqual(p.Name, "ChangedName");
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

        [TestMethod]
        public void Test25()
        {
            var code = @"
                    Piyo = new TestJitClass
                        ChildValueA = 'abc'
                ";
            JacInterpreter.RegisterJacTarget(typeof(TestJitClass).Assembly);
            var jac = new JacInterpreter();
            jac.Exec(code);
            var Piyo = jac["Piyo"] as TestJitClass;
            Assert.IsNotNull(Piyo);
            Assert.IsTrue(Piyo.Contains("ChildValueA"));
            Assert.AreEqual(Piyo["ChildValueA"], "abc");

            code = $@"
                Piyo.ChildValueB = 'def'
            ";
            jac.Exec(code);
            Assert.IsTrue(Piyo.Contains("ChildValueB"));
            Assert.AreEqual(Piyo["ChildValueB"], "def");
        }
        [TestMethod]
        public void Test26()
        {
            var code = @"
                p1 = new Process
                    ID = 'AAA123'
                AAA123
                    Cio
                        add o1 = new CoSpan
                            ID = 'SPANCONSTRAINTID'
                            Span = 0.1H
                            PorlingSpan = 1S
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var p1 = jac["p1"] as JitProcess;
            var o1 = p1.Cios.FirstOrDefault() as CoSpan;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.Span, TimeSpan.FromHours(0.1));
            Assert.AreEqual(o1.PorlingSpan, TimeSpan.FromSeconds(1.0));

            code = @"
                SPANCONSTRAINTID
                    Span = 0.2H
                    PorlingSpan = 1.2S
            ";
            jac.Exec(code);
            o1 = p1.Cios.FirstOrDefault() as CoSpan;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.Span, TimeSpan.FromHours(0.2));
            Assert.AreEqual(o1.PorlingSpan, TimeSpan.FromSeconds(1.2));
        }

        [TestMethod]
        public void Test27()
        {
            var code = @"
                p1 = new Process
                    ID = 'AAA123'
                AAA123
                    Cio
                        add o1 = new CoSpan
                            ID = 'SPANID'
                            Span = 0.1H
                            PorlingSpan = 1S
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var p1 = jac["p1"] as JitProcess;
            var o1 = p1.Cios.FirstOrDefault() as CoSpan;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.Span, TimeSpan.FromHours(0.1));
            Assert.AreEqual(o1.PorlingSpan, TimeSpan.FromSeconds(1.0));

            var code2 = @$"
                AAA123
                    Cio
                        remove SPANID
            ";
            jac.Exec(code2);
            var ciosCount = p1.Cios.Count();
            Assert.AreEqual(ciosCount, 0);
        }
    }

    [JacTarget(Name = "TestJitClass")]
    public class TestJitClass
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();

        [JacSetDotValue]
        public void SetDotValue(string name, object value)
        {
            dic[name] = value;
        }

        public object this[string name]
        {
            get => dic[name];
            set => dic[name] = value;
        }

        public bool Contains(string name) => dic.ContainsKey(name);
    }


}
