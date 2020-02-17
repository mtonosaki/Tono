// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using static Tono.Jit.Utils;

namespace Tono.Jit
{
    /// <summary>
    /// out-constraint of maximum cost caluclated from works in this process to wait previous process exiting.
    /// 自工程のコスト制約を評価して、前工程のOUTを制御 
    /// </summary>
    [JacTarget(Name = "CoMaxCost")]
    public class CoMaxCost : CoBase
    {
        public static readonly Type Type = typeof(CoMaxCost);

        /// <summary>
        /// cost variable name of work
        /// MAX制約値の変数名
        /// </summary>
        public JitVariable ReferenceVarName { get; set; } = JitVariable.From("Count");

        /// <summary>
        /// MAX制約値
        /// </summary>
        public double Value { get; set; } = 1.0;    // maximum cost value. 最大コストの指定値

        public override string MakeShortValue()
        {
            return $"{ReferenceVarName.Value}≤{Value}";
        }

        /// <summary>
        /// check maximum cost constraint to let the work wait at previous process
        /// MAXコストの制約を調べる
        /// </summary>
        /// <param name="work"></param>
        /// <param name="now"></param>
        /// <returns>true=waiting</returns>
        public override bool Check(JitWork work, DateTime now)
        {
            var stage = work.FindStage();
            var wirs = stage.GetWorksInReserve(work.Next, this); // TODO: Before work.Current. Need to check the reason.
            var tarLocation = GetCheckTargetProcess(work);
            var works = stage.GetWorks(tarLocation).Select(wt => wt.Work);
            var costs =
                from w in works.Concat(wirs)
                let cost = w.ChildVriables.GetValueOrNull("Cost")
                where cost != null
                let varval = cost[ReferenceVarName]
                select Convert.ToDouble((varval?.Value as JitVariable)?.Value);

            double totalcost = costs.Sum();
            return totalcost >= Value;
        }

        /// <summary>
        /// interval time to check next confirmation timing
        /// 制約中のワークに対し、待ち時間を計算する
        /// </summary>
        /// <returns></returns>
        public override TimeSpan GetWaitTime(JitStage.WorkEventQueue.Item ei, DateTime Now)
        {
            var nextexit = ei.Work.FindStage().Events.Find(ei.Work.Next, EventTypes.Out, ":Work");
            if (nextexit != null)
            {
                var ret = MathUtil.Min(TimeSpan.FromDays(999.9), nextexit.Value.Work.ExitTime - Now);
                if (ret < TimeSpan.FromSeconds(1))
                {
                    ret = TimeSpan.FromMinutes(1);
                }
                return ret;
            }
            else
            {
                return TimeSpan.FromSeconds(0);    // OUT→INの処理待ちのケースなので、同時刻(IN完了)を指定する
                                                   // Note: 後工程が最終工程で、ワーク詰まって、IN制約で入れないとすると、無限ループになる
            }
        }
    }
}
