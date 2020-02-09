// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using Tono;

namespace Tono.Core
{
    [TestClass]
    public class TonoBinary
    {
#if HEAVYTEST
        [TestMethod]
        public void Test_ByteReverse()
        {
            var st = Stopwatch.StartNew();
            for (var i = 0; i < 10000000; i++)
            {
                var v = Binary.ByteReverse(i);
            }
            Debug.WriteLine($"TonoBinary.Test_ByteReverse : {st.ElapsedMilliseconds:#,##0}ms for 10M count");
        }
#endif
        [TestMethod]
        public void Test001()
        {
            var bits = Binary.GetBits(0x55555555).ToArray();
            for( var i = 0; i < 32; i += 2) // 0=Upper / 31=Lower bits
            {
                Assert.IsFalse(bits[i]);
            }
        }
    }
}
