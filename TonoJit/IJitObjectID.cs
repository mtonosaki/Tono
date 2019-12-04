using System;
using System.Collections.Generic;
using System.Text;

namespace Tono.Jit
{
    interface IJitObjectID
    {
        /// <summary>
        /// JIT Object ID
        /// </summary>
        string ID { get; set; }

        bool Equals(object obj);

        int GetHashCode();
    }
}
