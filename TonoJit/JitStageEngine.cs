﻿// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Tono.Jit.JitStage;

namespace Tono.Jit
{
    /// <summary>
    /// Interface of Jit Stage Engine
    /// </summary>
    public interface IJitStageEngine
    {
        /// <summary>
        /// work event management queue
        /// </summary>
        WorkEventQueue Events { get; }

        /// <summary>
        /// simulation clock
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// do next action (from event queue)
        /// </summary>
        void DoNext();

        /// <summary>
        /// send request kanban object at now
        /// 指定プロセスにかんばんを送る(現在時刻に予約)
        /// </summary>
        /// <param name="kanban"></param>
        /// <returns>
        /// input kanban object
        /// </returns>
        JitKanban SendKanban(JitKanban kanban);

        /// <summary>
        /// send request kanban object at specific time
        /// 指定プロセスにかんばんを送る（指定時刻に予約）
        /// </summary>
        /// <param name="time"></param>
        /// <param name="kanban"></param>
        void SendKanban(DateTime time, JitKanban kanban);

        void AddWorkInReserve(CioBase cio, JitWork work);

        /// <summary>
        /// Remove Work instance
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="work"></param>
        void RemoveWorkInReserve(CioBase cio, JitWork work);

        /// <summary>
        /// Query works in reserve
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        IEnumerable<JitWork> GetWorksInReserve(CioBase cio);

        /// <summary>
        /// Save Last Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        void SetLastInTime(CioBase cio, DateTime now);

        /// <summary>
        /// last work enter time 最後にINした時刻
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        /// <remarks>
        /// This value will be set when out timing at previous process
        /// この値でSpanを評価。実際にProcessにINしたタイミングではなく、前ProcessでOutされた時にセットされる
        /// </remarks>
        DateTime GetLastInTime(CioBase cio);

        /// <summary>
        /// Save Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        void EnterWorkToProcess(JitProcess process, JitWork work, DateTime now);

        /// <summary>
        /// Leave Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        void ExitWorkFromProcess(JitProcess process, JitWork work);

        /// <summary>
        /// Query Works in process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        IEnumerable<(JitWork Work, DateTime EnterTime)> GetWorks(JitProcess process);
    }

    /// <summary>
    /// Runtime Simulator Engine Data
    /// </summary>
    public class JitStageEngine : IJitStageEngine
    {
        /// <summary>
        /// work event management queue
        /// </summary>
        public WorkEventQueue Events { get; private set; }

        /// <summary>
        /// simulation clock
        /// </summary>
        public DateTime Now { get; private set; }

        /// <summary>
        /// The constructor
        /// </summary>
        public JitStageEngine()
        {
            Events = new WorkEventQueue();
        }

        /// <summary>
        /// do next action (from event queue)
        /// </summary>
        public void DoNext()
        {
            var ei = Events.Dequeue();
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
            var usedKanban = ei.Kanban.Stage.FindProcess(ei.Kanban.PullFromProcessKey).AddKanban(this, ei.Kanban, Now); // 工程にかんばんを投入して、処理を促す
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
            if (ei.Work.NextProcess.CheckConstraints(ei.Work, Now, out var co) == false)   // no constraint 制約なしの状態
            {
                // STEP2
                Events.Enqueue(Now, EventTypes.In, ei.Work);
                ei.Work.NextProcess.RememberWorkWillBeIn(Now, ei);
            }
            else // next process : have constraint 次工程 制約ありの状態
            {
                var alpha = co.GetWaitTime(this, ei, Now);
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

        private Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>> _cioWorkCache = new Dictionary<CioBase, Dictionary<JitWork, bool>>();

        public void AddWorkInReserve(CioBase cio, JitWork work)
        {
            var works = _cioWorkCache.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            works[work] = true;
        }

        /// <summary>
        /// Remove Work instance
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="work"></param>
        public void RemoveWorkInReserve(CioBase cio, JitWork work)
        {
            var works = _cioWorkCache.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            works.Remove(work);
        }

        /// <summary>
        /// Query works in reserve
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        public IEnumerable<JitWork> GetWorksInReserve(CioBase cio)
        {
            var works = _cioWorkCache.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            return works.Keys;
        }

        private Dictionary<CioBase, DateTime> _lastInTimesCio = new Dictionary<CioBase, DateTime>();

        /// <summary>
        /// Save Last Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void SetLastInTime(CioBase cio, DateTime now)
        {
            _lastInTimesCio[cio] = now;
        }

        /// <summary>
        /// last work enter time 最後にINした時刻
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        /// <remarks>
        /// This value will be set when out timing at previous process
        /// この値でSpanを評価。実際にProcessにINしたタイミングではなく、前ProcessでOutされた時にセットされる
        /// </remarks>
        public DateTime GetLastInTime(CioBase cio)
        {
            return _lastInTimesCio.GetValueOrDefault(cio);
        }

        private Dictionary<JitProcess, Dictionary<JitWork, DateTime/*Enter-Time*/>> _worksInProcess = new Dictionary<JitProcess, Dictionary<JitWork, DateTime>>();

        /// <summary>
        /// Save Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void EnterWorkToProcess(JitProcess process, JitWork work, DateTime now)
        {
            var works = _worksInProcess.GetValueOrDefault(process, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            works[work] = now;
        }

        /// <summary>
        /// Leave Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void ExitWorkFromProcess(JitProcess process, JitWork work)
        {
            var works = _worksInProcess.GetValueOrDefault(process, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            works.Remove(work);
        }

        /// <summary>
        /// Query Works in process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public IEnumerable<(JitWork Work, DateTime EnterTime)> GetWorks(JitProcess process)
        {
            var works = _worksInProcess.GetValueOrDefault(process, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            return works.Select(kv => (kv.Key, kv.Value));
        }
    }
}
