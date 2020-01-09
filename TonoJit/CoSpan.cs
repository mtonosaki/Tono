// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono.Jit
{
    /// <summary>
    /// out-constraint to keep enter span
    /// 自工程のIN間隔を一定以上になる様、前工程からのOUTを制御 
    /// </summary>
    [JacTarget(Name = "CoSpan")]
    public class CoSpan : CoBase, CioBase.ILastInTime
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
            return $"{JacInterpreter.MakeTimeSpanString(Span)}";
        }

        /// <summary>
        /// last work enter time 最後にINした時刻
        /// </summary>
        /// <remarks>
        /// This value will be set when out timing at previous process
        /// この値でSpanを評価。実際にProcessにINしたタイミングではなく、前ProcessでOutされた時にセットされる
        /// </remarks>
        public DateTime LastInTime { get; set; }

        /// <summary>
        /// check span constraint
        /// </summary>
        /// <param name="work"></param>
        /// <param name="now"></param>
        /// <returns>true=waiting / false=Can Enter</returns>
        public override bool Check(JitWork work, DateTime now)
        {
            return (now - LastInTime) < Span;
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
        public override TimeSpan GetWaitTime(JitStage.WorkEventQueue Events, JitStage.WorkEventQueue.Item ei, DateTime Now)
        {
            TimeSpan ret = MathUtil.Min(TimeSpan.FromDays(999.9), LastInTime + Span - Now);
            if (ret < TimeSpan.FromSeconds(1))
            {
                ret = PorlingSpan;
            }
            return ret;
        }
    }
}
