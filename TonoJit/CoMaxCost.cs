// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using static Tono.Jit.CioBase;
using static Tono.Jit.JitStage;

namespace Tono.Jit
{
    /// <summary>
    /// out-constraint of maximum cost caluclated from works in this process to wait previous process exiting.
    /// 自工程のコスト制約を評価して、前工程のOUTを制御 
    /// </summary>
    [JacTarget(Name = "CoMaxCost")]
    public class CoMaxCost : CoBase, IWorkInReserved
    {
        /// <summary>
        /// cost variable name of work
        /// MAX制約値の変数名
        /// </summary>
        public JitVariable ReferenceVarName { get; set; } = JitVariable.From("Count");

        /// <summary>
        /// MAX制約値
        /// </summary>
        public double Value { get; set; } = 1.0;    // maximum cost value. 最大コストの指定値

        private Dictionary<JitWork, JitWork> WorkInReserves { get; set; } = new Dictionary<JitWork, JitWork>();

        [JacListAdd(PropertyName = "WorkInReserve")]
        public void AddWorkInReserve(JitWork work)
        {
            WorkInReserves[work] = work;
        }

        [JacListRemove(PropertyName = "WorkInReserve")]
        public void RemoveWorkInReserve(JitWork work)
        {
            WorkInReserves.Remove(work);
        }

        /// <summary>
        /// Query work in reserve
        /// </summary>
        /// <returns></returns>
        public IEnumerable<JitWork> GetWorkInReserves() => WorkInReserves.Keys;

        /// <summary>
        /// check maximum cost constraint to let the work wait at previous process
        /// MAXコストの制約を調べる
        /// </summary>
        /// <param name="work"></param>
        /// <param name="now"></param>
        /// <returns>true=waiting</returns>
        public override bool Check(JitWork work, DateTime now)
        {
            var costs =
                from w in GetParentProcess(work).Works.Concat(WorkInReserves.Keys)
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
        public override TimeSpan GetWaitTime(WorkEventQueue Events, WorkEventQueue.Item ei, DateTime Now)
        {
            var nextexit = Events.Find(ei.Work.NextProcess, EventTypes.Out, ":Work");
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
