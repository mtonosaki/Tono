// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using System.Threading;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class CollectionTest
    {
        [TestMethod]
        public void Test001()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var ret = new StringBuilder();
            foreach (var item in col.CancelBy(cs.Token))
            {
                ret.Append(".");
                ret.Append(item);
            }
            Assert.AreEqual(".10.20.30.40.50.60.70.80.90.100", ret.ToString());
        }

        [TestMethod]
        public void Test002()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var ret = new StringBuilder();
            foreach (var item in col.Skip(1).CancelBy(cs.Token).Take(3))
            {
                ret.Append(".");
                ret.Append(item);
            }
            Assert.AreEqual(".20.30.40", ret.ToString());
        }

        [TestMethod]
        public void Test003()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var ret = new StringBuilder();
            foreach (var item in col.CancelBy(cs.Token))
            {
                ret.Append(".");
                ret.Append(item);
                if (item == 40)
                {
                    cs.Cancel();
                }
            }
            Assert.AreEqual(".10.20.30.40", ret.ToString());
        }
        [TestMethod]
        public void Test004()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var ret = new StringBuilder();
            foreach (var item in col.Reverse().CancelBy(cs.Token))
            {
                ret.Append(".");
                ret.Append(item);
                if (item == 40)
                {
                    cs.Cancel();
                }
            }
            Assert.AreEqual(".100.90.80.70.60.50.40", ret.ToString());
        }

        [TestMethod]
        public void Test005()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 1, 5, 0, 2, 4, 3, 6, 7, 9, 8, };
            var ret = new StringBuilder();
            foreach (var item in col.OrderBy(a => a).CancelBy(cs.Token))
            {
                ret.Append(".");
                ret.Append(item);
                if (item == 4)
                {
                    cs.Cancel();
                }
            }
            Assert.AreEqual(".0.1.2.3.4", ret.ToString());
        }

        [TestMethod]
        public void Test006()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 1, 5, 0, 2, 4, 3, 6, 7, 9, 8, };
            var ret = new StringBuilder();
            foreach (var item in col.CancelBy(cs.Token).OrderBy(a => a))
            {
                ret.Append(".");
                ret.Append(item);
                if (item == 4)
                {
                    cs.Cancel();      // NOT COME HERE; .OrderBy requests all collection at the time.
                }
            }
            Assert.AreEqual(".0.1.2.3.4.5.6.7.8.9", ret.ToString());
        }

        [TestMethod]
        public void Test007()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 1, 5, 0, 2, 4, 3, 6, 7, 9, 8, };
            var ret = new StringBuilder();
            foreach (var item in col.Skip(1).Take(4).CancelBy(cs.Token).OrderBy(a => a))
            {
                ret.Append(".");
                ret.Append(item);
                if (item == 4)
                {
                    cs.Cancel();      // NOT COME HERE; .OrderBy requests all collection at the time.
                }
            }
            Assert.AreEqual(".0.2.4.5", ret.ToString());    // Order By 5,0,2,4 (Skip1, Take4)
        }

        private int loopCount = 0;
        private bool LoopCondition()
        {
            return ++loopCount > 3 ? false : true;
        }

        [TestMethod]
        public void Test100()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var ret = new StringBuilder();
            loopCount = 0;
            foreach (var item in col.When(LoopCondition))
            {
                ret.Append(".");
                ret.Append(item);
            }
            Assert.AreEqual(".10.20.30", ret.ToString());
        }

        [TestMethod]
        public void Test101()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var ret = new StringBuilder();
            loopCount = 0;
            foreach (var item in col.Skip(1).When(LoopCondition))
            {
                ret.Append(".");
                ret.Append(item);
            }
            Assert.AreEqual(".20.30.40", ret.ToString());
        }

        [TestMethod]
        public void Test102()
        {
            var cs = new CancellationTokenSource();
            var col = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            var ret = new StringBuilder();
            loopCount = 0;
            foreach (var item in col.When(LoopCondition).Skip(1))
            {
                ret.Append(".");
                ret.Append(item);
            }
            Assert.AreEqual(".20.30", ret.ToString());
        }
    }
}
