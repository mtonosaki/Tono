// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using ProcessKey = System.String;

namespace Tono.Jit
{
    /// <summary>
    /// process group as priority joint
    /// 優先ジョイント工程 
    /// </summary>
    /// <remarks>
    /// find work to make priority in the child processes
    /// 工程毎に指定した優先度に従い、優先工程内のワークを優先してOUTするようにワークを選択する工程

    /// Note : Need to add Process Object to Stage before use thid method.
    /// </remarks>
    public class JitProcessPriorityJoint : JitSubset
    {
        private readonly Dictionary<ProcessKey, int> procPriority = new Dictionary<ProcessKey, int>();

        public JitProcessPriorityJoint()
        {
            //ChildProcesses.Added += ChildProcesses_Added;
            ProcessAdded += JitProcessPriorityJoint_ProcessAdded;
        }

        /// <summary>
        /// Add child process as top priority 工程を追加。後に追加したものが高優先でOUTされる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JitProcessPriorityJoint_ProcessAdded(object sender, ProcessAddedEventArgs e)
        {
            AddProcessLink(GetProcessKey(e.Process), GetProcessKey(this));    // Make the work leave route from child to parent

            int no = 0;
            foreach (var pkey in GetProcessKeys())
            {
                procPriority[pkey] = ++no; // larger number is priority 数字が大きい方が、先にOUTされる
            }
        }


        /// <summary>
        /// change exit schedule depends on priorities of each child processes
        /// 工程毎優先順に従って、OUTするスケジュールに変更する
        /// </summary>
        /// <param name="events"></param>
        /// <param name="work"></param>
        public override void AddAndAdjustExitTiming(JitStage.WorkEventQueue events, JitWork work)
        {
            if (work.Next != default && work.Next.Process != null)
            {
                var sortList = events.FindAll((work.Current.Subset, this), EventTypes.Out).ToList();
                var tarDT = work.ExitTime;
                if (sortList.Count > 0)
                {
                    tarDT = MathUtil.Max(sortList.Min(a => a.Value.DT), work.ExitTime);    // 退場時刻は、既存のものに合わせる
                    events.Remove(sortList);
                }
                var nn = new LinkedListNode<JitStage.WorkEventQueue.Item>(new JitStage.WorkEventQueue.Item
                {
                    DT = tarDT,
                    Type = EventTypes.Out,
                    Work = work,
                });
                sortList.Add(nn);
                sortList.Sort(new QueueItemComparer
                {
                    ProcPriorities = procPriority,  // Larger number is priority
                }.Comparer);

                foreach (var node in sortList)
                {
                    events.Enqueue(tarDT, EventTypes.Out, node.Value.Work);  // 退場予約 並び替えて、再登録
                }
            }
        }

        private class QueueItemComparer
        {
            public Dictionary<ProcessKey, int> ProcPriorities { get; set; }

            /// <summary>
            /// sort rule of out sequece
            /// OUT順をソートする条件
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public int Comparer(LinkedListNode<JitStage.WorkEventQueue.Item> a, LinkedListNode<JitStage.WorkEventQueue.Item> b)
            {
                // 1st condition: priority of process 第１条件＝工程の優先順
                int ret = GetProcPriority(JitWork.GetProcess(a.Value.Work.Previous)) - GetProcPriority(JitWork.GetProcess(b.Value.Work.Previous));
                if (ret == 0)
                {
                    // 2nd condition: enter time 第2条件＝進入時刻準（FIFO）
                    return (int)(a.Value.Work.EnterTime - b.Value.Work.EnterTime).TotalSeconds;
                }
                else
                {
                    return ret;
                }
            }

            private int GetProcPriority(JitProcess proc)
            {
                if (ProcPriorities.TryGetValue(proc.ID, out var p1))
                {
                    return p1;
                }
                if (ProcPriorities.TryGetValue(proc.Name, out var p2))
                {
                    return p2;
                }
                throw new JitException(JitException.NoProcKey, $"{proc.Name} or {proc.ID}");
            }
        }
    }
}
