// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    [NoTestClass]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NoTestClassAttribute : Attribute
    {
        public NoTestClassAttribute()
        {
        }
    }

    /// <summary>
    /// NoTestClassのアトリビュート
    /// テストを行わなくて良いクラスにつける属性
    /// </summary>
    [NoTestClass]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class NoTestAttribute : Attribute
    {
        public NoTestAttribute()
        {
        }
    }
}
