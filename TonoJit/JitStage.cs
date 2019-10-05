﻿using System;

namespace Tono.Jit
{
    /// <summary>
    /// JIT-Model Root Object : Stage
    /// 工程やワーク全体を動かす 根幹のオブジェクト
    /// </summary>
    public partial class JitStage : JitVariable
    {
        /// <summary>
        /// having processes
        /// </summary>
        public ProcessSet Procs { get; private set; }

        /// <summary>
        /// work event management queue
        /// </summary>
        public WorkEventQueue Events { get; private set; }

        /// <summary>
        /// simulation clock
        /// </summary>
        public DateTime Now { get; private set; }

        /// <summary>
        /// the constructor of this class
        /// </summary>
        public JitStage()
        {
            Classes.Add(":Stage");
            Procs = new ProcessSet();
            Events = new WorkEventQueue();
        }

        /// <summary>
        /// do next action (from event queue)
        /// </summary>
        public void DoNext()
        {
            WorkEventQueue.Item ei = Events.Dequeue();
            if (ei == null)
            {
                return;
            }
            Now = ei.DT;

            switch (ei.Type)
            {
                case EventTypes.Out:
                    ProcOut(ei);
                    break;
                case EventTypes.In:
                    ProcIn(ei);
                    break;
                case EventTypes.KanbanIn:
                    ProcKanban(ei);
                    break;
            }
        }

        /// <summary>
        /// process kanban control
        /// </summary>
        /// <param name="ei"></param>
        private void ProcKanban(WorkEventQueue.Item ei)
        {
            var usedKanban = ei.Kanban.PullFrom().AddKanban(Events, ei.Kanban, Now); // 工程にかんばんを投入して、処理を促す
            if (usedKanban != null)
            {
                usedKanban.Work.CurrentProcess.AddAndAdjustExitTiming(Events, usedKanban.Work); // Eventキューに Outイベントを登録
            }
        }

        /// <summary>
        /// work out process
        /// </summary>
        /// <param name="ei"></param>
        /// <remarks>
        /// STEP1：confirm out-constraint of next process 次工程のIN制約を確認
        /// STEP2：re-input event item as "IN" status. OKのItemを INステータスで イベントキューに再投入
        /// </remarks>
        private void ProcOut(WorkEventQueue.Item ei)
        {
            ei.Work.Status = JitWorkStatus.Stopping;   // change status "STOP"
            if (ei.Work.NextProcess == null)
            {
                return;
            }
            // STEP1
            if (ei.Work.NextProcess.CheckConstraints(ei.Work, Now, out CoBase co) == false)   // no constraint 制約なしの状態
            {
                // STEP2
                Events.Enqueue(Now, EventTypes.In, ei.Work);
                ei.Work.NextProcess.RememberWorkWillBeIn(Now, ei);
            }
            else // next process : have constraint 次工程 制約ありの状態
            {
                TimeSpan alpha = co.GetWaitTime(Events, ei, Now);
                Events.Enqueue(Now + alpha, EventTypes.Out, ei.Work);
            }
        }

        /// <summary>
        /// work in process
        /// </summary>
        /// <param name="ei"></param>
        private void ProcIn(WorkEventQueue.Item ei)
        {
            ei.Work.Status = JitWorkStatus.Moving;
            ei.Work.CurrentProcess?.Exit(ei.Work);
            ei.Work.NextProcess.Enter(ei.Work, Now);
            ei.Work.ExitTime = Now; // 後で、Co.Delayで上書きされる

            ei.Work.CurrentProcess.ExecInCommands(ei.Work, Now);        // execute in-command コマンドを実行
            ei.Work.CurrentProcess.AddAndAdjustExitTiming(Events, ei.Work); // put next event item status "Out" into event queu   Eventキューに Outイベントを登録
        }

        /// <summary>
        /// send request kanban object at now
        /// 指定プロセスにかんばんを送る(現在時刻に予約)
        /// </summary>
        /// <param name="kanban"></param>
        /// <returns>
        /// input kanban object
        /// </returns>
        public JitKanban SendKanban(JitKanban kanban)
        {
            Events.Enqueue(Now, EventTypes.KanbanIn, kanban);
            return kanban;
        }

        /// <summary>
        /// send request kanban object at specific time
        /// 指定プロセスにかんばんを送る（指定時刻に予約）
        /// </summary>
        /// <param name="time"></param>
        /// <param name="kanban"></param>
        public void SendKanban(DateTime time, JitKanban kanban)
        {
            Events.Enqueue(time, EventTypes.KanbanIn, kanban);
        }
    }
}