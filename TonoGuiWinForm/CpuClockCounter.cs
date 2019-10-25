// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uCpuCounter �̊T�v�̐����ł��B
    /// </summary>
    public static class CpuClockCounter
    {
        [DllImport("DposeWinNative.dll")]
        private static extern ulong GetCpuCount();

        /// <summary>
        /// ���݂�CPU�J�E���g���擾����
        /// </summary>
        /// <returns></returns>
        public static ulong Now()
        {
            return GetCpuCount();
        }
    }
}
