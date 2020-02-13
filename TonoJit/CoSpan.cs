// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using static Tono.Jit.Utils;

namespace Tono.Jit
{
    /// <summary>
    /// out-constraint to keep enter span
    /// 自工程のIN間隔を一定以上になる様、前工程からのOUTを制御 
    /// </summary>
    [JacTarget(Name = "CoSpan")]
    public class CoSpan : CoBase
    {
        public static readonly Type Type = typeof(CoSpan);

        /// <summary>
        /// minimum time span to enter to this owner process
        /// </summary>
        public TimeSpan Span { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// default interval time to confirm span constraint 
        /// Span制約で、再度確認する時間
        /// </summary>
        public TimeSpan PorlingSpan { get; set; } = TimeSpan.FromSeconds(60);


        public override string MakeShortValue()
        {
            return $"{MakeTimeSpanString(Span)}";
        }

        /// <summary>
        /// check span constraint
        /// </summary>
        /// <param name="work"></param>
        /// <param name="now"></param>
        /// <returns>true=waiting / false=Can Enter</returns>
        public override bool Check(JitWork work, DateTime now)
        {
            
            return (now - work.Engine.GetLastInTime(work.Current.Subset, this)) < Span;
        }

        /// <summary>
        /// caluclate next confirmation timing
        /// Span制約中のワークに対して、待ち時間を計算する
        /// </summary>
        /// <param name="Events"></param>
        /// <param name="ei"></param>
        /// <param name="work"></param>
        /// <param name="Now"></param>
        /// <returns></returns>
        public override TimeSpan GetWaitTime(IJitEngine engine, JitStage.WorkEventQueue.Item ei, DateTime Now)
        {
            var ret = MathUtil.Min(
                TimeSpan.FromDays(999.9), 
                engine.GetLastInTime(ei.Work.Current.Subset, this) + Span - Now
            );
            if (ret < TimeSpan.FromSeconds(1))
            {
                ret = PorlingSpan;
            }
            return ret;
        }
    }
}
