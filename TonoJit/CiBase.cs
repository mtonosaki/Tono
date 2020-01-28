// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.
using System;

namespace Tono.Jit
{
    /// <summary>
    /// IN Command base class (executed after work-in)
    /// </summary>
    public abstract class CiBase : CioBase
    {
        /// <summary>
        /// get parent owner process
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        protected override JitProcess GetCheckTargetProcess(JitWork work)
        {
            return work.CurrentProcess;
        }

        /// <summary>
        /// Execute in-command
        /// </summary>
        /// <param name="work">target work</param>
        /// <param name="now">simulation time</param>
        public abstract void Exec(JitWork work, DateTime now);
    }
}
