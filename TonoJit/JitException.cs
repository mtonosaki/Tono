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
        public const string FormatNoProcKey = "Cannot find the Process '{0}'";  // set ProcessKey(ID/Name)
        public const string NullProcKey = "Cannot find the NULL Process";

        public JitException() :  base()
        {
        }

        public JitException(string message) : base(message)
        {
        }

        public JitException(string format, params object[] prms) : base(string.Format(format, prms))
        {
        }
    }
}
