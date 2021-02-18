// (c) 2021 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class SoloControllerTest
    {
        [TestMethod]
        public void Test1()
        {
            var ret = "0";
            Debug.WriteLine($"0");
            var sa = new SoloController();
            var tcs1 = new TaskCompletionSource<bool>();
            var tcs2 = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                await sa.StartSoloAsync(() => Task.Run(async () =>
                {
                    await Task.Delay(75);
                    ret += "A";
                    Debug.WriteLine($"A");
                    await Task.Delay(75);
                    ret += "B";
                    Debug.WriteLine($"B");
                }),
                () => Task.Run(async () =>
                {
                    await Task.Delay(75);
                    ret += "a";
                    Debug.WriteLine($"x");
                    await Task.Delay(75);
                    ret += "b";
                    Debug.WriteLine($"y");
                }));
                ret += "C";

                Task.Delay(50).ConfigureAwait(false).GetAwaiter().GetResult();

                await sa.StartSoloAsync(
                () => Task.Run(async () =>
                {
                    await Task.Delay(75);
                    ret += "X";
                    Debug.WriteLine($"X");
                    await Task.Delay(75);
                    ret += "Y";
                    Debug.WriteLine($"Y");
                    tcs1.SetResult(false);
                }),
                () => Task.Run(async () =>
                {
                    await Task.Delay(75);
                    ret += "x";
                    Debug.WriteLine($"x");
                    await Task.Delay(75);
                    ret += "y";
                    Debug.WriteLine($"y");
                    tcs1.SetResult(true);
                }));
                ret += "Z";
                Debug.WriteLine($"Z");
                tcs2.SetResult(true);
            });

            tcs1.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            tcs2.Task.ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual("0ABCxyZ", ret);    // XY have been skipped
        }

    }
}
