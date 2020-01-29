// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.Jit
{
    public interface IJitObjectID
    {
        /// <summary>
        /// JIT Object ID
        /// </summary>
        string ID { get; set; }

        bool Equals(object obj);

        int GetHashCode();
    }
}
