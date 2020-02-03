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
        public const string NullValue = "EJIT1001: Null value is not acceptable for {0}";
        public const string NullProcKey = "EJIT1002: Cannot find the NULL Process";
        public const string FormatNoProcKey = "EJIT1003: Cannot find the Process '{0}'";  // set ProcessKey(ID/Name)
        public const string TypeMissmatch = "EJIT1004: Type miss match {0}";

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
