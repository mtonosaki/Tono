// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.Jit
{
    /// <summary>
    /// Just-in-time Object of Work Location
    /// </summary>
    [JacTarget(Name = "Location")]
    public class JitLocation
    {
        public JitSubset Subset { get; set; }
        public JitProcess Process { get; set; }

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public static JitLocation From(JitSubset subset, JitProcess process)
        {
            return new JitLocation
            {
                Subset = subset,
                Process = process,
            };
        }

        public static bool operator ==(JitLocation a, JitLocation b) { return a?.Equals(b) ?? b?.Equals(a) ?? true; }
        public static bool operator !=(JitLocation a, JitLocation b) { return !a?.Equals(b) ?? !b?.Equals(a) ?? false; }


        public override bool Equals(object obj)
        {
            if (obj is JitLocation tar)
            {
                if( tar.Subset?.Equals(Subset) ?? false)
                {
                    return tar.Process?.Equals(Process) ?? false;
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            var sid = Subset?.ID ?? "";
            var pid = Process?.ID ?? "";
            return (sid + pid).GetHashCode();
        }
        public override string ToString()
        {
            return $"{GetType().Name} Subset={(Subset?.ToString() ?? "n/a")}, Process={(Process?.ToString() ?? "n/a")}";
        }
    }
}
