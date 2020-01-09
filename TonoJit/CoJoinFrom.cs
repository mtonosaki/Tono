// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono.Jit
{
    /// <summary> 
    /// Let Work at previous process wait when there is no signal-like-Work in side process. Then join a signal-Work as a child to the Work that come from previous process.
    /// 横工程からワークがINできる場合かどうかを評価する。INできる場合は横工程からワークをOUTさせて、対象ワークに投入する
    /// </summary>
    [JacTarget(Name = "CoJoinFrom")]
    public class CoJoinFrom : CoBase
    {
        public static readonly Type Type = typeof(CoJoinFrom);

        /// <summary>
        /// PULLする工程
        /// </summary>
        public Func<JitProcess> PullFrom { get; set; }

        /// <summary>
        /// child work name
        /// ワークに付く子ワークの名前
        /// </summary>
        public string ChildPartName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// interval time of next confirmation confluence condition
        /// PullFrom工程からワークが取得できない場合、次に確認するための時間
        /// </summary>
        public TimeSpan WaitSpan { get; set; } = TimeSpan.FromMinutes(1.0);

        /// <summary>
        /// confirm condition and pick work from PullFrom process
        /// </summary>
        /// <param name="parentWork">parent work that is requested to exit from PullFrom process 前工程から退出したい 「親」ワーク</param>
        /// <param name="now">simulation time</param>
        /// <returns>true=cannot move work / false=moved</returns>
        public override bool Check(JitWork parentWork, DateTime now)
        {
            if (parentWork.ChildWorks.ContainsKey(ChildPartName))
            {
                return false;   // already moved. すでにワークが付いているので制約なし（完了）
            }

            if (PullFrom().ExitCollectedWork(now) is JitWork sideWork)    // work at PullFrom process 横工程のワーク
            {
                parentWork.ChildWorks[ChildPartName] = sideWork;
                return false;
            }
            return true;
        }

        /// <summary>
        /// interval time of next confirmation timing when the work was in waiting condition
        /// 横工程からInできない場合、次に確認するまで待つ時間
        /// </summary>
        /// <param name="Events"></param>
        /// <param name="ei"></param>
        /// <param name="Now"></param>
        /// <returns></returns>
        public override TimeSpan GetWaitTime(JitStage.WorkEventQueue Events, JitStage.WorkEventQueue.Item ei, DateTime Now)
        {
            return WaitSpan;
        }
    }
}
