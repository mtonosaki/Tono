﻿using System;
using System.Linq;

namespace Tono.Jit
{
    /// <summary>
    /// in-command : request to push works into other process
    /// 子ワークを指定工程にPUSH(要求) 
    /// </summary>
    public class CiPickTo : CiBase
    {
        /// <summary>
        /// owner stage object
        /// 子ワークを返却するステージインスタンス（EventキューにワークをPUSH要求する為に使う）
        /// </summary>
        private JitStage Stage { get; set; }

        /// <summary>
        /// work filter classes
        /// フィルターするクラス（デフォルト :Workは全種類の子ワークを対象）
        /// </summary>
        /// <example>
        /// TargetWorkClass = ":Sumaho"  ---  pick works that have :Sumaho class only
        /// TargetWorkClass = ":iOS:Sumaho"  --- pick works that have both :iOS and :Sumaho classes only.
        /// </example>
        public string TargetWorkClass { get; set; } = ":Work";

        /// <summary>
        /// delay time to complete push work
        /// </summary>
        public TimeSpan DelayTime { get; set; } = TimeSpan.FromSeconds(0);

        /// <summary>
        /// destination prosess of push operation
        /// </summary>
        public Func<JitProcess> Destination { get; set; }

        /// <summary>
        /// the constructor of this class
        /// </summary>
        /// <param name="parent"></param>
        public CiPickTo(JitStage stage)
        {
            Stage = stage;
        }

        /// <summary>
        /// in-command execute
        /// </summary>
        /// <param name="work">target work</param>
        /// <param name="now">simulation time</param>
        public override void Exec(JitWork work, DateTime now)
        {
            System.Collections.Generic.IEnumerable<string> childworkNames =
                from cw in work.ChildWorks
                where cw.Value.Is(TargetWorkClass)
                select cw.Key;

            foreach (string childWorkName in childworkNames.ToArray())
            {
                JitWork childWork = work.ChildWorks[childWorkName];
                childWork.NextProcess = Destination();
                childWork.CurrentProcess = null; // 子Workであった事を null とする。
                                                 // childWork.PrevProcess = null; // workがAssyされた元工程を覚えておく

                work.ChildWorks.Remove(childWorkName);  // 子ワークから外す
                Stage.Events.Enqueue(now + DelayTime, EventTypes.Out, childWork);   // 次工程にPUSH予約
            }
        }
    }
}