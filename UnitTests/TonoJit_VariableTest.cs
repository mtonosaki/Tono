// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Tono.Jit;

namespace UnitTests
{
    [TestClass]
    public class TonoJit_VariableTest
    {
        [TestMethod]
        public void Test004_Variable_Class_Inheritance()
        {
            var cd = JitVariable.Null(":C:D");      // null:C:D
            var de = JitVariable.Null(":D:E");      // null:D:E
            var cde = JitVariable.Null(":C:D:E");   // null:C:D:E
            Assert.IsTrue(cd.As(cde) == false);     // null:C:D   is NOT as null:C:D:E
            Assert.IsTrue(de.As(cde) == false);     // null:D:E   is NOT as null:C:D:E
            Assert.IsTrue(cde.As(cd) == true);      // null:C:D:E is     as null:C:D
            Assert.IsTrue(cde.As(de) == true);      // null:C:D:E is     as null:D:E

            var CA330A1234 = JitVariable.Null();                        // CA330A1234 = null:Object
            CA330A1234.Classes.Set(":Mirai:Car");                       // CA330A1234 = null:Mirai:Car
            var NY500B9876 = JitVariable.Null(":Noah:Car");             // NY500B9876 = null:Noah:Car
            var TX100C5555 = JitVariable.Null(":Mirai:eCar:Car");       // TX100C5555 = null:Mirai:eCar:Car
            Assert.IsTrue(CA330A1234.As(NY500B9876) == false);          // null:Noah:Car       is NOT as null:Mirai:eCar:Car
            Assert.IsTrue(CA330A1234.As(TX100C5555) == false);          // null:Mirai:Car      is NOT as null:Mirai:eCar:Car
            Assert.IsTrue(TX100C5555.As(CA330A1234) == true);           // null:Mirai:eCar:Car is NOT as null:Mirai:Car
        }

        [TestMethod]
        public void Test003_Variable_Basic_More2()
        {
            var CA330A1234 = JitVariable.Null();    // CA330A1234:Object
            CA330A1234.Classes.Set(":Mirai:Car");   // CA330A1234:Mirai:Car

            var AAA = JitVariable.From("AAA");      // AAA
            CA330A1234.ChildVriables["Attr"][AAA] = JitVariable.From(1);                // CA330A1234.Attr[AAA] = 1
            CA330A1234.ChildVriables["X"] = JitVariable.From(2);                        // CA330A1234.X = 2
            Assert.IsTrue(CA330A1234.ChildVriables["X"] == JitVariable.From(2));        // CA330A1234.X == 2

            CA330A1234.ChildVriables["Y"] = JitVariable.From("kan", ":KanbanSheet");    // CA330A1234.Y:KanbanSheet = "kan"
            Assert.IsTrue(CA330A1234.ChildVriables["Y"] == JitVariable.From("kan"));    // CA330A1234.Y == "kan"
            Assert.IsTrue(CA330A1234.ChildVriables["Y"] == JitVariable.From("kan", ":KanbanSheet") == true);    // CA330A1234.Y == "kan":KanbanSheet
            Assert.IsTrue(CA330A1234.ChildVriables["Y"] == JitVariable.From("kan", ":Dummy") == true);          // CA330A1234.Y == "kan":Dummy because == is not consider type differences
            Assert.IsTrue(CA330A1234.ChildVriables["Y"].Is(":KanbanSheet"));            // CA330A1234.Y is :KanbanSheet
            Assert.IsTrue(CA330A1234.ChildVriables["Y"].Is(":Mirai") == false);         // CA330A1234.Y is NOT :Mirai (but CA330A1234 is :Mirai)
            Assert.IsTrue(CA330A1234.ChildVriables["Y"].Is(":Car") == false);           // CA330A1234.Y is NOT :Car   (but CA330A1234 is :Car)
            Assert.IsTrue(CA330A1234.ChildVriables["Y"].Is(":Object") == true);         // CA330A1234.Y is :Object
            Assert.IsTrue(CA330A1234.Is(":KanbanSheet") == false);                      // CA330A1234 is NOT :KanbanSheet (but CA330A1234.Y is it)

            CA330A1234.Merge(JitVariable.From(12, ":eCar"));                            // CA330A1234 Å© 12:eCar
            Assert.IsTrue(CA330A1234.Is(":eCar") == true);                              // CA330A1234 is :eCar because of merged

            var LOVE = JitVariable.From("Love");
            CA330A1234.ChildVriables["Attr"][LOVE] = JitVariable.From("d", ":Mind");        // CA330A1234.Attr[LOVE] = "d":Mind
            Assert.IsTrue(CA330A1234.ChildVriables["Attr"][LOVE].ChildVriables["Attr"][AAA] == JitVariable.Null()); // CA330A1234.Attr[LOVE].Attr[AAA] == null
            Assert.IsTrue(CA330A1234.ChildVriables["Attr"][LOVE].Is(":Mirai") == false);    // CA330A1234.Attr[LOVE] is NOT :Mirai (that's :Mind)
            Assert.IsTrue(CA330A1234.ChildVriables["Attr"][LOVE].Is(":Mind"));              // CA330A1234.Attr[LOVE] is :Mind
            Assert.IsTrue(CA330A1234.ChildVriables["Attr"][LOVE].Is(":Human") == false);    // CA330A1234.Attr[LOVE] is NOT :Human
            Assert.IsTrue(CA330A1234.ChildVriables["Attr"][LOVE].Is(":Car") == false);      // CA330A1234.Attr[LOVE] is NOT :Car (that's :Mind)
        }

        [TestMethod]
        public void Test002_Variable_Basic_More()
        {
            var CA330A1234 = JitVariable.Null();
            CA330A1234.Classes.Set(":Mirai:Car");   // CA330A1234:Mirai:Car
            CA330A1234.ChildVriables["Attr"][JitVariable.From("AAA")] = JitVariable.From(1);                    // CA330A1234.Attr[AAA] = 1
            Assert.IsTrue(CA330A1234.ChildVriables["Attr"][JitVariable.From("AAA")] == JitVariable.From(1));    // so, true

            Assert.IsTrue(CA330A1234.Is(":Mirai") == true);     // means CA330A1234 have class :Mirai
            Assert.IsTrue(CA330A1234.Is(":Noah") == false);     // means CA330A1234 have NOT :Noah
            Assert.IsTrue(CA330A1234.Is(":Car") == true);       // means CA330A1234 have :Car too.
            Assert.IsTrue(CA330A1234.Is(":Object") == true);    // means CA330A1234 have :Object that is set automatically.
            Assert.IsTrue(CA330A1234.Is(":Work") == false);     // means CA330A1234 have not :Work because nobody set the class name yet.

            var a = JitVariable.From(CA330A1234, ":Car");       // a:Car = CA330A1234
            Assert.IsTrue(a.ChildVriables["Attr"][JitVariable.From("AAA")] == JitVariable.From(1));             // a.Attr[AAA] == 1 == CA330A1234.Attr[AAA]
            Assert.IsTrue(a.Is(":Car") == true);
            Assert.IsTrue(a.Is(":Mirai") == true);              // a is :Mirai 
            Assert.IsTrue(CA330A1234.Is(":Mirai") == true);     // because CA330A1234 is :Mirai too.

            var b = JitVariable.From(CA330A1234);               // b = CA330A1234
            Assert.IsTrue(b.ChildVriables["Attr"][JitVariable.From("AAA")] == JitVariable.From(1));     // b.Attr[AAA] == 1 because CA330A1234.Attr[AAA] == 1
            Assert.IsTrue(b.Is(":Car") == true);                // b is :Car because CA330A1234 is :Car
            Assert.IsTrue(b.Is(":Var") == false);               // So, b is NOT :Var
            Assert.IsTrue(b.Is(":Object") == true);             // b is still :Object

            var c = JitVariable.From(CA330A1234, ":Customer");  // c:Customer = CA330A1234
            Assert.IsTrue(c.ChildVriables["Attr"][JitVariable.From("AAA")] == JitVariable.From(1));     // c.Attr[AAA] == 1 because CA330A1234.Attr[AAA] == 1
            Assert.IsTrue(c.Is(":Car") == true);                // c is :Car because CA330A1234 is :Car too.
            Assert.IsTrue(c.Is(":Customer") == true);           // c is :Customer because you set before.
            Assert.IsTrue(c.Is(":XXX") == false);               // c is not :XXX because nobody set it yet.

            var d = JitVariable.From(CA330A1234, ":Mirai:Noah:Car:XXX");    // d:Mirai:Noah:Car:XXX = CA330A1234
            Assert.IsTrue(d.Is(":Mirai") == true);              // d is :Mirai
            Assert.IsTrue(d.Is(":Noah") == true);               // d is :Noah
            Assert.IsTrue(d.Is(":XXX") == true);                // d is :XXX
            Assert.IsTrue(d.Is(JitVariable.Class.Object));      // d is :Object
        }

        [TestMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<OK>")]
        public void Test001_Variable_Basic()
        {
            var v1 = JitVariable.Null();
            Assert.IsTrue(v1.Value == null);

            var i123 = JitVariable.From(123);
            Assert.IsTrue(i123.Value.Equals(123));
            Assert.IsTrue(i123.Is(JitVariable.Class.Int));
            Assert.IsTrue(i123.Is(":Object:Int"), "contains both :Object and :Int classes");
            Assert.IsTrue(i123.Is(":Int:Object"), "sequence free");
            Assert.IsTrue(i123.Is(":Object"), "contains :Object");
            Assert.IsTrue(i123.Is(":Int"), "contains :Int");
            Assert.IsTrue(i123.Is(":Object:Int:Dummy") == false, "check false because it is NOT contains :Dummy");
            Assert.IsTrue(i123.Is(":Dummy") == false, "check false because it is NOT contains :Dummy");
            try
            {
                i123.Is("Object:Int");
                Debug.Fail("expected an exception here because Object is not start with :");
            }
            catch (JitVariable.SyntaxErrorException)
            {
                // OK
            }

            var d34556 = JitVariable.From(345.56);
            Assert.IsTrue(d34556.Value.Equals(345.56));
            Assert.IsTrue(d34556.Is(":Object:Double"));
            Assert.IsTrue(d34556.Is(":Object:Int") == false);

            var ahaha = JitVariable.From("ahaha");
            Assert.IsTrue(ahaha.Value.Equals("ahaha"));
            Assert.IsTrue(ahaha.Is(":Object:String"));
            Assert.IsTrue(ahaha.Is(":Object:Int") == false);

            var ihihi = JitVariable.From("ihihi");
            Assert.IsTrue(ahaha.Classes.Equals(ihihi.Classes), "compare classes");

            ahaha.Classes.Add(":Test");
            Assert.IsTrue(ahaha.Is(":Object:Test:String"), "should contains :Test");
            Assert.IsTrue(ahaha.Classes.Equals(ihihi.Classes) == false, "should not equal because of :Test");

            var x = JitVariable.Null();
            Assert.IsTrue(x == JitVariable.Null(), "should be able to compare Null and Null");

            x[JitVariable.From("EE")] = JitVariable.From("EE-VAL");
            var tmp = x[JitVariable.From("EE")];
            Assert.IsTrue(tmp.Equals(JitVariable.From("EE-VAL")), "that value should be transmoved from x[EE]");
            Assert.IsTrue(tmp.Equals(JitVariable.From("EE-VAL-DUMMY")) == false);

            var f = JitVariable.From(4);    // f = 4
            x[f] = JitVariable.From(6);     // x[4] = 6
            var fval = x[f];                // fval = x[4] = 6
            Assert.IsTrue(fval == JitVariable.From(6));

            var g = JitVariable.From("PartNumber");     // g = PartNumber
            x[g] = JitVariable.From("1234567");         // x[PartNumber] = "1234567"
            var gval = x[g];                            // gval = x[PartNumber] = "1234567"
            Assert.IsTrue(gval == JitVariable.From("1234567"));
            Assert.IsTrue(fval != JitVariable.From("1234567"), "fval == 6");
            Assert.IsTrue(fval != JitVariable.From("6"), "fval == 6. and more, fval(int) != '6'(string)");

            var CA330A1234 = JitVariable.Null();
            CA330A1234.Classes.Set(":Mirai:Car");                                                           // CA330A1234:Mirai:Car
            CA330A1234.ChildVriables["Attr"][JitVariable.From("A")] = JitVariable.From(1);                  // CA330A1234.Attr[A] = 1
            Assert.IsTrue(CA330A1234.ChildVriables["Attr"][JitVariable.From("A")] == JitVariable.From(1));  // CA330A1234.Attr[A] == 1 (true)
        }
    }
}
