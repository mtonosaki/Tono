// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using static Tono.Jit.JitStage;

namespace Tono.Jit
{
    /// <summary> 
    /// Co = out-constraint base class (to check if work can exit and enter next process)
    /// OUT制約ベース（工程のOUT直前で 次工程のモノを評価。次工程へのINを抑制できる）
    /// </summary>
    public abstract class CoBase : CioBase
    {
        /// <summary>
        /// owner process
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        /// <remarks>
        /// Check(...)実行時に、work.NextProcessは nullにならない
        /// </remarks>

        protected override (JitSubset Subset, JitProcess Process) GetCheckTargetProcess(JitWork work)
        {
            return work.Next;
        }

        /// <summary>
        /// check the target work can NOT enter to next process
        /// 制約中かどうかを調べる
        /// </summary>
        /// <param name="work"></param>
        /// <param name="now"></param>
        /// <returns>true=cannot enter to next process / false=can</returns>
        public abstract bool Check(JitWork work, DateTime now);

        /// <summary>
        /// caluclate waiting time in this constraint
        /// </summary>
        /// <param name="Events"></param>
        /// <param name="ei"></param>
        /// <param name="work"></param>
        /// <param name="Now"></param>
        /// <returns></returns>
        public virtual TimeSpan GetWaitTime(IJitEngine engine, WorkEventQueue.Item ei, DateTime Now)
        {
            return TimeSpan.FromDays(999.9);
        }
    }
}
