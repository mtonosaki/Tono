// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using Tono;

namespace UnitTestProject2
{
    [TestClass]
    public class TonoId
    {
#if HEAVYTEST
        [TestMethod]
        public void Test_TreeSpeed()
        {
            var dic = new Dictionary<Id, int>();
            var st = Stopwatch.StartNew();
            var count = 2000000;
            for (var i = count; i > 0; i--)
            {
                dic[Id.From(i)] = i;
            }
            for (var i = count; i > 0; i--)
            {
                var x = dic[Id.From(i)];
            }
            Debug.WriteLine($"TonoId.Test_TreeSpeed : {st.ElapsedMilliseconds:#,##0}ms for 2M count");
        }
#endif
    }
}
