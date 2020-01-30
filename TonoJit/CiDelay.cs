// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono.Jit
{
    /// <summary>
    /// in-command to make exit delay time
    /// </summary>
    [JacTarget(Name = "CiDelay")]
    public class CiDelay : CiBase
    {
        public static readonly Type Type = typeof(CiDelay);

        /// <summary>
        /// PROPERTY: Delay
        /// </summary>
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// To make short value of instance
        /// </summary>
        /// <returns></returns>
        public override string MakeShortValue()
        {
            return $"{JacInterpreter.MakeTimeSpanString(Delay)}";
        }

        /// <summary>
        /// set exit time
        /// </summary>
        /// <param name="work">target work</param>
        /// <param name="now">simulation time</param>
        public override void Exec(JitWork work, DateTime now)
        {
            work.ExitTime = work.EnterTime + Delay;
        }
    }
}
