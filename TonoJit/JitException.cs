// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Tono.Jit
{
    /// <summary>
    /// Exception object
    /// </summary>
    public class JitException : Exception
    {
        public JitException() :  base()
        {
        }

        public JitException(string message) : base(message)
        {
        }
    }
}
