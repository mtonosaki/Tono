// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tono;

namespace UnitTestProject2
{
    [TestClass]
    public class TonoNamedId
    {
        [TestMethod]
        public void Test_AutoID()
        {
            var v1 = NamedId.FromName("Test1");
            var v2 = NamedId.FromName("Test1");
            Assert.AreEqual<NamedId>(v1, v2, "Should equal because of same name");
            Assert.AreEqual<Id>(v1.Id, v2.Id);
            var v3 = NamedId.FromName("test1");
            Assert.AreNotEqual<NamedId>(v1, v3, "Should not equal because of different character cap");
            Assert.AreNotEqual<Id>(v1.Id, v3.Id);
        }
        [TestMethod]
        public void Test_ManualID()
        {
            var v1 = NamedId.From("Test1", Id.From(999));
            var v2 = NamedId.FromName("Test1");
            Assert.AreEqual<NamedId>(v1, v2, "Should equal because of same name");
            Assert.AreEqual<Id>(v1.Id, v2.Id);
        }
    }
}
