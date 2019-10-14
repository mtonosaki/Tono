﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Tono;

namespace UnitTests
{
    [TestClass]
    public class TonoId
    {
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
    }
}