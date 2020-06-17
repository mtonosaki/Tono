﻿// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Class1 の概要の説明です。
    /// </summary>
    public interface ISpace
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>S
        bool IsIn(object value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Transfer(object value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Inflate(object value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Deflate(object value);
    }
}
