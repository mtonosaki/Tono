// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Tono;

namespace UnitTests1
{
    [TestClass]
    public class TonoBinary
    {
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
    }
}
