// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Tono;
using Tono.Jit;

namespace Tono.Jit
{
    [TestClass]
    public class TonoJit_JaC_Template
    {
        [TestMethod]
        public void Test01()
        {
            var c = @"
                te = new Template
                    Block
                        add 'st = new Stage'
                    Name = 'MyTemp'
            ";
            var jac = new JacInterpreter();
            jac.Exec(c);
            Assert.AreEqual(jac["te"].GetType(), typeof(JitTemplate));
            Assert.IsNotNull(jac.GetTemplate("te"));
            Assert.AreEqual(jac.GetTemplate("te").Name, "MyTemp");
        }


        [TestMethod]
        public void Test02()
        {
            var c = @"
                te = new Template
                    Block
                        add 'st = new Stage'
                        add 'p1 = new Process'
                        add 'w1 = new Work'
                        add 'k1 = new Kanban'
            ";
            var jac = new JacInterpreter();
            jac.Exec(c);
            var te = jac.GetTemplate("te");
            Assert.IsNotNull(te);
            Assert.AreEqual(te.Count, 4);

            c = @"
                te
                    Block
                        add 'w2 = new Work'
                        add 'w3 = new Work'
                        add 'w4 = new Work'
            ";
            jac.Exec(c);
            Assert.AreEqual(te.Count, 7);

            c = @"
                te
                    Block
                        remove '::LAST::'
            ";
            jac.Exec(c);
            Assert.AreEqual(te.Count, 6);
        }

        [TestMethod]
        public void Test03()
        {
            var c = @"
                te = new Template
                    Block
                        add 'st = new Stage'
                        add 'p1 = new Process'
                        add 'w1 = new Work'
                        add 'k1 = new Kanban'
            ";
            var jac = new JacInterpreter();
            jac.Exec(c);

            var jac2 = JacInterpreter.From(jac.GetTemplate("te"));
            Assert.IsNotNull(jac2.GetStage("st"));
            Assert.IsNotNull(jac2.GetProcess("p1"));
            Assert.IsNotNull(jac2.GetWork("w1"));
            Assert.IsNotNull(jac2.GetKanban("k1"));
            Assert.IsNull(jac2.GetTemplate("te")); // child JacInterpreter should has NOT the template instance
        }
    }
}
