// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uCpuCounter の概要の説明です。
    /// </summary>
    public static class CpuClockCounter
    {
        [DllImport("DposeWinNative.dll")]
        private static extern ulong GetCpuCount();

        /// <summary>
        /// 現在のCPUカウントを取得する
        /// </summary>
        /// <returns></returns>
        public static ulong Now()
        {
            return GetCpuCount();
        }
    }
}
