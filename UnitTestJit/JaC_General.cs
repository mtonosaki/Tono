// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Tono.Jit;

namespace UnitTestJit
{
    [TestClass]
    public class JaC_General
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
            Assert.AreEqual(st.GetChildProcesses().Count(), 3);
            Assert.AreEqual(st.GetChildProcess(0), st.GetChildProcess(0));  // check Equals
            Assert.AreEqual(st.GetChildProcess(0).GetHashCode(), st.GetChildProcess(0).GetHashCode());  // check GetHashCode
            Assert.AreNotEqual(st.GetChildProcess(0), st.GetChildProcess(1));
            Assert.AreNotEqual(st.GetChildProcess(0), st.GetChildProcess(2));
            Assert.AreNotEqual(st.GetChildProcess(1), st.GetChildProcess(2));
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
                        remove '{st.GetChildProcess(0).Name}'
            ";
            jac.Exec(code);
            Assert.AreEqual(st.GetChildProcesses().Count(), 1);
            Assert.AreEqual(st.GetChildProcess(0).Name, "IgnoreProcess");
        }

        [TestMethod]
        public void Test06_1()
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
            Assert.AreEqual(jac.GetStage("st")?.GetChildProcesses().Count(), 1);
            Assert.AreNotEqual(jac.GetStage("st")?.GetChildProcess(0).Name, "IgnoreProcess");
            Assert.IsNull(jac.GetProcess("IgnoreProcess")); // removed from VarBuffer
            Assert.IsNull(jac.GetProcess(name));            // removed from InstanceBuffer
        }

        [TestMethod]
        public void Test06_2()
        {
            var code = @"
                st = new Stage
                    Procs
                        add new Process
                            ID = 'IgnoreProcess'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.IsNotNull(jac.GetProcess("IgnoreProcess"));

            code = @"
                st
                    Procs
                        remove IgnoreProcess
            ";
            jac.Exec(code);
            Assert.IsNull(jac.GetProcess("IgnoreProcess"));

            code = @"
                st
                    Procs
                        add IgnoreProcess   // Expected cannot add removed process.
            ";
            jac.Exec(code);
            Assert.IsNull(jac.GetProcess("IgnoreProcess"));   // CANNOT ADD After Removed
        }

        [TestMethod]
        public void Test06_3()
        {
            var code = @"
                st = new Stage
                p1 = new Process
                    ID = 'IgnoreProcess'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.IsNotNull(jac.GetProcess("IgnoreProcess"));

            code = @"
                st
                    Procs
                        add IgnoreProcess
            ";
            jac.Exec(code);
            Assert.IsNotNull(jac.GetProcess("IgnoreProcess"));

            code = @"
                st
                    Procs
                        remove IgnoreProcess
            ";
            jac.Exec(code);
            Assert.IsNull(jac.GetProcess("IgnoreProcess"));
        }

        [TestMethod]
        public void Test06_4()
        {
            var code = @"
                st = new Stage
                    Procs
                        add new Process
                            ID = 'IgnoreProcess'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.IsNotNull(jac.GetProcess("IgnoreProcess"));

            code = @"
                st
                    Procs
                        remove IgnoreProcess
                        add IgnoreProcess           // Expected success add because there is in the same jac code.
            ";
            jac.Exec(code);
            Assert.IsNotNull(jac.GetProcess("IgnoreProcess"));
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
            Assert.AreEqual(jac.GetStage("st")?.GetChildProcesses().Count(), 0);
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
            Assert.AreEqual(MyStage.GetChildProcesses().Count(), 1);
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
            Assert.AreEqual(jac.GetStage("MyStage")?.GetChildProcesses().Count(), 0);
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
        public void Test13_1()
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
        public void Test13_2()
        {
            var code = @"
                bm = new Variable
                    Value = -123
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac.GetVariable("bm").Value, -123);
        }

        [TestMethod]
        public void Test13_3()
        {
            var code = @"
                cmd2 = new Variable
                    Value = -1.234e-2S
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac.GetVariable("cmd2").Value, TimeSpan.FromSeconds(-1.234e-2));
        }

        [TestMethod]
        public void Test13_4()
        {
            var code = @"
                h = new Variable
                    Value = 1.23e-10
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            Assert.AreEqual(jac.GetVariable("h").Value, 1.23e-10);
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
            Assert.AreEqual(st.FindChildProcess(i1.DestProcessKey, true), jac.GetProcess("sink")); // check lazy method

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
            var i1dest = st.FindChildProcess("SUPERLAZY", isReturnNull: true);
            Assert.IsNull(i1dest);  // Expected Null because of no registered yet.

            var code2 = @"
                st
                    Procs
                        add p2 = new Process
                            Name = 'SUPERLAZY'
            ";
            jac.Exec(code2);
            i1dest = st.FindChildProcess("SUPERLAZY", isReturnNull: true);
            var p2 = jac.GetProcess("p2");
            Assert.AreEqual(i1dest, p2);  // Then FindProcess can find p2 named SUPERLAZY

            i1dest = st.FindChildProcess(p2.ID, isReturnNull: true);  // You can also find by ID 
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
                                    ChildWorkKey = 'TEPA'
                                    PorlingSpan = 0.5M                                    
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var o1 = jac["o1"] as CoJoinFrom;
            Assert.IsNotNull(o1);
            Assert.AreEqual(o1.PullFromProcessKey, jac.GetProcess("SinkProc").Name);
            Assert.AreEqual(o1.ChildWorkKey, "TEPA");
            Assert.AreEqual(o1.PorlingSpan, TimeSpan.FromSeconds(30));
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
            Assert.AreEqual(jac.GetProcess("MyProc")?.Name, "MyProc");
            Assert.AreEqual(jac.GetProcess("MyProc")?.Cios.Count(), 1);

            var code2 = @"
                w1 = new Work
                    Next = l1 = new Location
                        Stage = st
                        SubsetCache = st
                        Path = '\'
                        Process = p1 = new Process
                w2 = new Work
                    Current = w1.Next
                w3 = new Work
                    Previous = w2.Current
                w4 = new Work
                    Current = l1
            ";
            jac.Exec(code2);
            var w1 = jac.GetWork("w1");
            var w2 = jac.GetWork("w2");
            var w3 = jac.GetWork("w3");
            var w4 = jac.GetWork("w4");
            var p1 = jac.GetProcess("p1");
            var l1 = jac.GetLocation("l1");
            Assert.IsNotNull(p1);
            Assert.IsNull(w1.Previous);
            Assert.IsNull(w1.Current);
            Assert.AreEqual(w1.Next.SubsetCache, st);
            Assert.AreEqual(w1.Next.Process, p1);
            Assert.AreEqual(w2.Current.SubsetCache, w1.Next.SubsetCache);
            Assert.AreEqual(w2.Current.SubsetCache, st);
            Assert.AreEqual(w2.Current.Process, p1);
            Assert.AreEqual(w3.Previous.SubsetCache, st);
            Assert.AreEqual(w3.Previous.Process, p1);
            Assert.AreEqual(w4.Current, l1);

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
                st = new Stage
                    Name = 'st'
                p1 = new Process
                    Name = 'p1'
                p2 = new Process
                    Name = 'p2'
                w1 = new Work
                k1 = new Kanban
                    PullFrom = new Location
                        Stage = st
                        Path = '\'
                        Process = p1
                    PullTo = new Location
                        Stage = st
                        Path = '\'
                        Process = p2
                    Work = w1
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");
            var k1 = jac.GetKanban("k1");
            var p1 = jac.GetProcess("p1");
            var p2 = jac.GetProcess("p2");
            var w1 = jac.GetWork("w1");
            Assert.IsNotNull(k1);
            Assert.IsNotNull(p1);
            Assert.IsNotNull(p2);
            Assert.IsNotNull(w1);
            Assert.AreEqual(k1.PullFrom.Stage, st);
            Assert.AreEqual(k1.PullFrom.Process.ID, p1.ID);
            Assert.AreEqual(k1.PullTo.Process.ID, p2.ID);
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

        [TestMethod]
        public void Test28()
        {
            var code = @"
                st = new Stage
                    Procs
                        add sink = new Process
                        add p1 = new Process
                            Name = 'PROCP1'
                            Cio
                                add pt = new CiPickTo
                                    Delay = 0MS
                                    TargetWorkClass = ':Car'
                                    DestProcessKey = sink.ID // To confirm Objct.Property style string set
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var pt = jac["pt"] as CiPickTo;
            Assert.IsNotNull(pt);
            var p1 = jac.GetProcess("p1");
            Assert.IsNotNull(p1);
            var sink = jac.GetProcess("sink");
            Assert.IsNotNull(p1);
            var redo = $"{pt.ID}\r\n" +
                          $"    DestProcessKey = 'REDO_PROC'r\n";
            var undo = $"{pt.ID}\r\n" +
                          $"    DestProcessKey = '{sink.ID}'\r\n";

            Assert.AreEqual(pt.DestProcessKey, sink.ID);
            jac.Exec(redo);
            jac.Exec(undo);
            Assert.AreEqual(pt.DestProcessKey, sink.ID);
        }

        [TestMethod]
        public void Test29()
        {
            var code = @"
                st = new Stage
                    Procs
                        add p1 = new Process
                            Name = 'PROC1'
                        add p2 = new Process
                            Name = 'PROC2'
                        add new Process
                            Name = 'PROC3'
                        add new Process
                            ID = 'PROCID4'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");

            code = @"
                st
                    ProcLinks
                        add p1 -> p2
            ";
            jac.Exec(code);
            var p1 = jac.GetProcess("p1");
            var tos = st.GetProcessLinkPathes(p1).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 1);
            Assert.AreEqual(tos[0], jac.GetProcess("p2"));

            code = @"
                st
                    ProcLinks
                        add p1->'PROC3'      // try to confirm super lazy link by Name
            ";
            jac.Exec(code);
            tos = st.GetProcessLinkPathes(p1).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 2);
            Assert.AreEqual(tos[0], jac.GetProcess("p2"));
            Assert.AreEqual(tos[1], jac.GetProcess("PROC3"));


            code = @"
                st
                    ProcLinks
                        add p1 ->'PROCID4'    // try to confirm lazy link by ID
            ";
            jac.Exec(code);
            tos = st.GetProcessLinkPathes(p1).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 3);
            Assert.AreEqual(tos[0], jac.GetProcess("p2"));
            Assert.AreEqual(tos[1], jac.GetProcess("PROC3"));
            Assert.AreEqual(tos[2], jac.GetProcess("PROCID4"));

            code = @"
                st
                    ProcLinks
                        remove p1 -> p2
            ";
            jac.Exec(code);
            tos = st.GetProcessLinkPathes(p1).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 2);
            Assert.AreEqual(tos[0], jac.GetProcess("PROC3"));
            Assert.AreEqual(tos[1], jac.GetProcess("PROCID4"));

            code = @"
                st
                    ProcLinks
                        remove p1->'PROC3'      // try to confirm super lazy link by Name
            ";
            jac.Exec(code);
            tos = st.GetProcessLinkPathes(p1).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 1);
            Assert.AreEqual(tos[0], jac.GetProcess("PROCID4"));

            code = @"
                st
                    ProcLinks
                        remove p1 ->'PROCID4'    // try to confirm lazy link by ID
            ";
            jac.Exec(code);
            tos = st.GetProcessLinkPathes(p1).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 0);

            //--------------------------------------------------------------------------------------------

            code = @"
                st
                    ProcLinks
                        add 'PROC3'-> p2
            ";
            jac.Exec(code);

            var PROC3 = jac.GetProcess("PROC3");
            tos = st.GetProcessLinkPathes(PROC3).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 1);
            Assert.AreEqual(tos[0], jac.GetProcess("p2"));

            code = @"
                st
                    ProcLinks
                        remove 'PROC3'-> p2
            ";
            jac.Exec(code);

            tos = st.GetProcessLinkPathes(PROC3).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 0);

            //--------------------------------------------------------------------------------------------

            code = @"
                st
                    ProcLinks
                        add 'PROCID4' ->p2
            ";
            jac.Exec(code);
            var PROCID4 = jac.GetProcess("PROCID4");
            tos = st.GetProcessLinkPathes(PROCID4).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 1);
            Assert.AreEqual(tos[0], jac.GetProcess("p2"));

            code = @"
                st
                    ProcLinks
                        remove 'PROCID4' ->p2
            ";
            jac.Exec(code);
            tos = st.GetProcessLinkPathes(PROCID4).Select(key => st.FindChildProcess(key)).ToArray();
            Assert.AreEqual(tos.Length, 0);
        }

        [TestMethod]
        public void Test30()
        {
            var l1 = JitLocation.CreateRoot(new JitStage(), new JitProcess());
            var l2 = new JitLocation
            {
                Stage = l1.Stage,
                SubsetCache = l1.Stage,
                Path = "\\",
                Process = l1.Process,
            };
            var b1 = l1.Equals(l2);
            var b2 = l1 == l2;

            Assert.IsTrue(b1);
            Assert.IsTrue(b2);
        }

        [TestMethod]
        public void Test31_TupleObject()
        {
            var code = @"
                t1 = 'tono':'saki'
                t2 = t1:'mana'
                t3 = t1:t2
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var xt1 = jac["t1"];
            var xt2 = jac["t2"];
            var xt3 = jac["t3"];
            Assert.AreEqual(xt1.GetType(), typeof(ValueTuple<object, object>));
            Assert.AreEqual(xt2.GetType(), typeof(ValueTuple<object, object>));
            Assert.AreEqual(xt3.GetType(), typeof(ValueTuple<object, object>));
            var t1 = (ValueTuple<object, object>)xt1;
            var t2 = (ValueTuple<object, object>)xt2;
            var t3 = (ValueTuple<object, object>)xt3;
            Assert.AreEqual(t1.Item1, "tono");
            Assert.AreEqual(t1.Item2, "saki");
            var t21 = (ValueTuple<object, object>)t2.Item1;
            Assert.AreEqual(t21.Item1, "tono");
            Assert.AreEqual(t21.Item2, "saki");
            Assert.AreEqual(t2.Item2, "mana");
            var t32 = (ValueTuple<object, object>)t3.Item2;
            Assert.AreEqual(t32.Item2, "mana");
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
                        ret &= work.Current.Process?.Name == procName;
                    }
                }
                else
                {
                    ret = false;
                }
            }
            return ret;
        }


        [TestMethod]
        public void Test32_DateTime()
        {
            Assert.AreEqual(DateTime.Parse("2020/7/31 13:45:56"), JacInterpreter.ParseDateTime(@"datetime('2020/7/31 13:45:56')"));
            Assert.AreEqual(DateTime.Parse("2020/7/31 13:45:56"), JacInterpreter.ParseDateTime(@"datetime ( '2020/7/31 13:45:56' )"));
            Assert.AreEqual(DateTime.Parse("2020/7/31 13:45:56"), JacInterpreter.ParseDateTime(@"'2020/7/31 13:45:56'"));
            Assert.AreEqual(DateTime.Parse("2020/7/31 13:45:56"), JacInterpreter.ParseDateTime(@"2020/7/31 13:45:56"));
            Assert.AreEqual(DateTime.Parse("2020/7/31 13:45:56.321"), JacInterpreter.ParseDateTime(@"2020/7/31 13:45:56.321"));
            Assert.AreNotEqual(DateTime.Parse("2020/7/31 13:45:56.322"), JacInterpreter.ParseDateTime(@"2020/7/31 13:45:56.321"));
        }

        /// <summary>
        /// Test Work Add to stage
        /// </summary>
        [TestMethod]
        public void Test33_1()
        {
            var code = @"
                st = new Stage
                w1 = new Work
                    Name = 'Work1'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");

            code = @"
                dt = datetime('2020/7/31 9:00:00')
                st
                    Works
                        add dt:w1
            ";
            jac.Exec(code);
            var dat = st.Events.Peeks(99).ToList();
            Assert.IsTrue(CMP(dat[0], "Work1", EventTypes.Out, $"9:00"));
        }
        [TestMethod]
        public void Test33_2()
        {
            var code = @"
                st = new Stage
                w1 = new Work
                    Name = 'Work1'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");

            code = @"
                dt = datetime('2020/7/31 9:00:00')
                st
                    Works
                        add dt:Work1
            ";
            jac.Exec(code);
            var dat = st.Events.Peeks(99).ToList();
            Assert.IsTrue(CMP(dat[0], "Work1", EventTypes.Out, $"9:00"));
        }
        [TestMethod]
        public void Test33_3()
        {
            var code = @"
                st = new Stage
                w1 = new Work
                    Name = 'Work1'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");

            code = @"
                st
                    Works
                        add datetime('2020/7/31 9:00:00'):Work1
            ";
            jac.Exec(code);
            var dat = st.Events.Peeks(99).ToList();
            Assert.IsTrue(CMP(dat[0], "Work1", EventTypes.Out, $"9:00"));
        }
        [TestMethod]
        public void Test33_4()
        {
            var code = @"
                st = new Stage
                    Works
                        add datetime('2020/7/31 9:00:00'):new Work
                            Name = 'Work1'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");
            var dat = st.Events.Peeks(99).ToList();
            Assert.IsTrue(CMP(dat[0], "Work1", EventTypes.Out, $"9:00"));
        }

        [TestMethod]
        public void Test34()
        {
            var code = @"
                st = new Stage
                    Works
                        add datetime('2020/7/31 9:00:00'):new Work
                            Name = 'Work1'
            ";
            var jac = new JacInterpreter();
            jac.Exec(code);
            var st = jac.GetStage("st");
            var dat = st.Events.Peeks(99).ToList();
            Assert.IsTrue(CMP(dat[0], "Work1", EventTypes.Out, $"9:00"));

            code = @"
                st
                    Works
                        remove Work1    // remove command is for GUI(undo) only.
            ";
            jac.Exec(code);
            dat = st.Events.Peeks(99).ToList();
            Assert.AreEqual(0, dat.Count);
        }
    }


    [JacTarget(Name = "TestJitClass")]
    public class TestJitClass
    {
        private readonly Dictionary<string, object> dic = new Dictionary<string, object>();

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

        public bool Contains(string name)
        {
            return dic.ContainsKey(name);
        }
    }
}
