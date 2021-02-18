// (c) 2021 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class TimeUtilTest
    {
        [TestMethod]
        public void TimeHelper_001()
        {
            {
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "15");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 34, 15), nt);
            }
            {
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "8");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 34, 8), nt);
            }
        }
        [TestMethod]
        public void TimeHelper_002()
        {
            {
                // Replace Seconds
                // 2000/10/28 13:34:45 --> 59  --> 2000/10/28 13:34:59 (13:34:00 + 59sec)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "59");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 34, 59), nt);
            }
            {
                // Replace Seconds
                // 2000/10/28 13:34:45 --> 60  --> 2000/10/28 13:35:00 (13:34:00 + 60sec)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "60");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 35, 00), nt);
            }
            {
                // Replace Seconds
                // 2000/10/28 13:34:45 --> 100  --> 2000/10/28 13:35:40 (13:34:00 + 100sec)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "100");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 35, 40), nt);
            }
            {
                // Replace Seconds
                // 2000/10/28 13:34:45 --> 100  --> 2000/10/28 13:35:40 (13:34:00 + 999sec)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "999");
                Assert.AreEqual((new DateTime(2000, 10, 28, 13, 34, 00)) + TimeSpan.FromSeconds(999), nt);
            }
            {
                // Replace M:S
                // 2000/10/28 13:34:45 --> 1000  --> 2000/10/28 13:10:00 (13:00:00 + 10:00)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "1000");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 10, 00), nt);
            }
            {
                // Replace M:S
                // 2000/10/28 13:34:45 --> 1059  --> 2000/10/28 13:10:00 (13:00:00 + 10:59)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "1059");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 10, 59), nt);
            }
            {
                // Replace M:S
                // 2000/10/28 13:34:45 --> 1060  --> 2000/10/28 13:10:00 (13:00:00 + 10:60 = 13:00:00 + 11:00)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "1060");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 11, 00), nt);
            }
            {
                // Replace M:S
                // 2000/10/28 13:34:45 --> 5959  --> 2000/10/28 13:59:59 (13:00:00 + 59:59)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "5959");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 59, 59), nt);
            }
            {
                // Replace M:S
                // 2000/10/28 13:34:45 --> 5960  --> 2000/10/28 13:59:59 (13:00:00 + 59:60 = 13:00:00 + 60:00 = 13:00:00 + 1:00:00)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "5960");
                Assert.AreEqual(new DateTime(2000, 10, 28, 14, 00, 00), nt);
            }
            {
                // Replace M:S
                // 2000/10/28 13:34:45 --> 9999  --> 13:00:00 + 99:99(100:39=1:40:39) = 14:40:39
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "9999");
                Assert.AreEqual(new DateTime(2000, 10, 28, 14, 40, 39), nt);
            }
            {
                // Replace H:M:S
                // 2000/10/28 13:34:45 --> 10000  --> 00:00:00 + 1:00:00 = 1:00:00
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "10000");
                Assert.AreEqual(new DateTime(2000, 10, 28, 01, 00, 00), nt);
            }
            {
                // Replace H:M:S
                // 2000/10/28 13:34:45 --> 100000  --> 00:00:00 + 10:00:00 = 10:00:00
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "100000");
                Assert.AreEqual(new DateTime(2000, 10, 28, 10, 00, 00), nt);
            }
            {
                // Replace H:M:S
                // 2000/10/28 13:34:45 --> 235959  --> 00:00:00 + 23:59:59 = 23:59:59
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "235959");
                Assert.AreEqual(new DateTime(2000, 10, 28, 23, 59, 59), nt);
            }
            {
                // Replace H:M:S
                // 2000/10/28 13:34:45 --> 235960  --> 00:00:00 + 23:59:60(24:00:00) = 2000/10/29 0:00:00
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "235960");
                Assert.AreEqual(new DateTime(2000, 10, 29, 0, 00, 00), nt);
            }
            {
                // Replace H:M:S (Overflow into days)
                // 2000/10/28 13:34:45 --> 999999  --> 00:00:00 + 99:99:99(99:100:39=100:40:39=4d+4:40:39) = 2000/11/1 4:40:39
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "999999");
                Assert.AreEqual(new DateTime(2000, 11, 1, 4, 40, 39), nt);
            }
        }
        [TestMethod]
        public void TimeHelper_003()
        {
            {
                // Plus Seconds
                // 2000/10/28 13:34:45 --> +1  --> 13:34:46
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+1");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 34, 46), nt);
            }
            {
                // Plus Seconds
                // 2000/10/28 13:34:45 --> +999  (+16:39) = 13:50:84 (13:51:24)
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+999");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 51, 24), nt);
            }
            {
                // Plus Seconds
                // 2000/10/28 13:34:45 --> +999  (+16:39) = 13:50:84 (13:51:24)
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+999");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 51, 24), nt1);
            }
            {
                // Plus M:S
                // 2000/10/28 13:34:45 --> +0100  (+01:00) = 13:35:45
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+0100");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+01:00");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 35, 45), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 35, 45), nt2);
            }
            {
                // Plus M:S
                // 2000/10/28 13:34:45 --> +1000  (+10:00) = 13:44:45
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+1000");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+10:00");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 44, 45), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 44, 45), nt2);
            }
            {
                // Plus M:S
                // 2000/10/28 13:34:45 --> +5959  (+59:59) = 13:34+59:45+59 = 13:93:104 = 14:34:44
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+5959");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+59:59");
                Assert.AreEqual(new DateTime(2000, 10, 28, 14, 34, 44), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 14, 34, 44), nt2);
            }
            {
                // Plus H:M:S
                // 2000/10/28 13:34:45 --> +10000  (+1:00:00) = 14:34:45
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+10000");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+1:00:00");
                Assert.AreEqual(new DateTime(2000, 10, 28, 14, 34, 45), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 14, 34, 45), nt2);
            }
            {
                // Plus H:M:S
                // 2000/10/28 13:34:45 --> +100000  (+10:00:00) = 23:34:45
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+100000");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+10:00:00");
                Assert.AreEqual(new DateTime(2000, 10, 28, 23, 34, 45), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 23, 34, 45), nt2);
            }
            {
                // Plus H:M:S
                // 2000/10/28 13:34:45 --> +999999  (+99:99:99 = 100:40:39 = 4d 4:40:39) = 2000/11/1 17:74:84 = 2000/11/1 18:15:24
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+999999");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "+99:99:99");
                Assert.AreEqual(new DateTime(2000, 11, 1, 18, 15, 24), nt1);
                Assert.AreEqual(new DateTime(2000, 11, 1, 18, 15, 24), nt2);
            }
        }
        [TestMethod]
        public void TimeHelper_004()
        {
            {
                // Plus Seconds
                // 2000/10/28 13:34:45 --> -1  --> 13:34:44
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-1");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 34, 44), nt);
            }
            {
                // Plus Seconds
                // 2000/10/28 13:34:45 --> -999  (-16:39) = 13:18:06
                var nt = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-999");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 18, 06), nt);
            }
            {
                // Plus M:S
                // 2000/10/28 13:34:45 --> -0100  (-01:00) = 13:33:45
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-0100");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-01:00");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 33, 45), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 33, 45), nt2);
            }
            {
                // Plus M:S
                // 2000/10/28 13:34:45 --> -1000  (-10:00) = 13:24:45
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-1000");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-10:00");
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 24, 45), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 13, 24, 45), nt2);
            }
            {
                // Plus M:S
                // 2000/10/28 13:34:45 --> -5959  (-59:59) = 13:34-59:45-59 = 12:35:45-59 = 12:34:46
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-5959");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-59:59");
                Assert.AreEqual(new DateTime(2000, 10, 28, 12, 34, 46), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 12, 34, 46), nt2);
            }
            {
                // Plus H:M:S
                // 2000/10/28 13:34:45 --> -10000  (-1:00:00) = 12:34:45
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-10000");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-1:00:00");
                Assert.AreEqual(new DateTime(2000, 10, 28, 12, 34, 45), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 12, 34, 45), nt2);
            }
            {
                // Plus H:M:S
                // 2000/10/28 13:34:45 --> -100000  (-10:00:00) = 03:34:45
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-100000");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-10:00:00");
                Assert.AreEqual(new DateTime(2000, 10, 28, 3, 34, 45), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 28, 3, 34, 45), nt2);
            }
            {
                // Plus H:M:S
                // 2000/10/28 13:34:45 --> -999999  -99:-99:-99 = 2000/10/24 08:54:06
                var nt1 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-999999");
                var nt2 = TimeUtil.TimeHelper(new DateTime(2000, 10, 28, 13, 34, 45), "-99:99:99");
                Assert.AreEqual(new DateTime(2000, 10, 24, 8, 54, 06), nt1);
                Assert.AreEqual(new DateTime(2000, 10, 24, 8, 54, 06), nt2);
            }
        }
    }
}
