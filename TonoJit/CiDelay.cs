// Copyright (c) Manabu Tonosaki All rights reserved.
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

        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);

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
