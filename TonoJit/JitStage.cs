// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using ProcessKeyPath = System.String;

namespace Tono.Jit
{
    /// <summary>
    /// JIT-Model Root Object : Stage
    /// 工程やワーク全体を動かす 根幹のオブジェクト
    /// </summary>
    [JacTarget(Name = "Stage")]
    public partial class JitStage : JitSubset, IJitObjectID
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
        /// the constructor of this class
        /// </summary>
        public JitStage() : base()
        {
            ID = JacInterpreter.MakeID("Stage");
            Classes.Add(":Stage");

            Events = new WorkEventQueue();
            ProcessAdded += Subset_ProcessAdded;
        }

        /// <summary>
        /// Reset Tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Subset_ProcessAdded(object sender, JitSubset.ProcessAddedEventArgs e)
        {
        }

        /// <summary>
        /// Hashcode by ID
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// Comparison with ID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is JitStage st)
            {
                return st.ID == ID;
            }
            else
            {
                return false;
            }
        }
        public override string ToString()
        {
            return $"{GetType().Name} ID={ID}";
        }

        [JacListAdd(PropertyName = "Procs")]
        public void AddJacProcs(object obj)
        {
            if (obj is JitProcess proc)
            {
                AddChildProcess(proc);
            }
            else if (obj is string prockey)
            {
                AddChildProcess(prockey);
            }
            else
            {
                throw new JacException(JacException.Codes.TypeMismatch, $"Procs.Add type mismatch arg type={(obj?.GetType().Name ?? "null")}");
            }
        }

        [JacListRemove(PropertyName = "Procs")]
        public void RemoveJacProcs(object obj)
        {
            if (obj is JitProcess proc)
            {
                RemoveChildProcess(proc);
            }
            else if (obj is string prockey)
            {
                RemoveChildProcess(prockey);
            }
            else
            {
                throw new JacException(JacException.Codes.TypeMismatch, $"Procs.Add type mismatch arg type={(obj?.GetType().Name ?? "null")}");
            }
        }

        [JacListAdd(PropertyName = "ProcLinks")]
        public void AddJacProcLinks(object description)
        {
            if (description == null)
            {
                throw new JitException(JitException.NullValue, "ProcLinks");
            }
            if (description is JacPushLinkDescription push)
            {
                var key1 = push.From is JitProcess p1 ? p1.Name : push.From?.ToString();
                var key2 = push.To is JitProcess p2 ? p2.Name : push.To?.ToString();
                AddProcessLink(key1, key2);
            }
            else
            {
                throw new JitException(JitException.TypeMissmatch, $"Expecting type JacPushLinkDescription but {description.GetType().Name}");
            }
        }

        [JacListRemove(PropertyName = "ProcLinks")]
        public void RemoveJacProcLinks(object description)
        {
            if (description == null)
            {
                throw new JitException(JitException.NullValue, "ProcLinks");
            }
            if (description is JacPushLinkDescription push)
            {
                var key1 = push.From is JitProcess p1 ? p1.ID : push.From?.ToString();
                var key2 = push.To is JitProcess p2 ? p2.ID : push.To?.ToString();
                RemoveProcessLink(key1, key2);
            }
            else
            {
                throw new JitException(JitException.TypeMissmatch, $"Expecting type JacPushLinkDescription but {description.GetType().Name}");
            }
        }

        /// <summary>
        /// Find Process by Key(path includes)
        /// </summary>
        /// <param name="procKeyPath"></param>
        /// <param name="isReturnNull"></param>
        /// <returns></returns>
        public JitLocation FindSubsetProcess(JitLocation currentLocation, ProcessKeyPath procKeyPath, bool isReturnNull = false)
        {
            if(procKeyPath == null)
            {
                goto Error;
            }
            var path = JitLocation.CombinePath(currentLocation.Path, procKeyPath);
            path = JitLocation.Normalize(path);
            if (path.StartsWith("\\") == false)
            {
                goto Error;
            }
            var keys = path.Split('\\');
            JitSubset subset = this;
            for (var i = 0; i < keys.Length - 1; i++)
            {
                var prockey = keys[i];
                if( string.IsNullOrEmpty(prockey) && i == 0)
                {
                    continue;
                }
                var ssproc = subset.FindChildProcess(prockey, true);
                if (ssproc is JitSubset ss)
                {
                    subset = ss;
                }
                else
                {
                    goto Error;
                }
            }
            var proc = subset.FindChildProcess(keys[keys.Length - 1], true);
            if( proc != null)
            {
                return new JitLocation
                {
                    Stage = currentLocation.Stage,
                    Path = JitLocation.GetPath(path),
                    SubsetCache = subset,
                    Process = proc,
                };
            }
Error:
            if (isReturnNull)
            {
                return currentLocation.ToEmptyProcess();
            }
            else
            {
                throw new JitException(JitException.IllegalPath, $"\"{currentLocation.Path}\" + \"{procKeyPath}\"");
            }
        }

        /// <summary>
        /// do next action (from event queue)
        /// </summary>
        public void DoNext()
        {
            var ei = Events.Dequeue();
            if (ei == null) return;

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
            var tarProc = ei.Kanban.Location.SubsetCache.FindChildProcess(ei.Kanban.PullFromProcessKey); // TODO: Consider Global Path
            var usedKanban = tarProc.AddKanban(ei.Kanban.Location, ei.Kanban, Now); // 工程にかんばんを投入して、処理を促す
            if (usedKanban != null)
            {
                usedKanban.Work.Current.Process.AddAndAdjustExitTiming(Events, usedKanban.Work); // Eventキューに Outイベントを登録
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
            if (ei.Work.Next?.Process == null )
            {
                return;
            }
            // STEP1
            if (ei.Work.Next.Process.CheckConstraints(ei.Work, Now, out var co) == false)   // no constraint 制約なしの状態
            {
                // STEP2
                Events.Enqueue(Now, EventTypes.In, ei.Work);
                ei.Work.Next.Process.RememberWorkWillBeIn(Now, ei);
            }
            else // next process : have constraint 次工程 制約ありの状態
            {
                var alpha = co.GetWaitTime(ei, Now);
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
            ei.Work.Current?.Process?.Exit(ei.Work);
            ei.Work.Next?.Process?.Enter(ei.Work, Now);
            ei.Work.ExitTime = Now; // 後で、Co.Delayで上書きされる

            ei.Work.Current?.Process?.ExecInCommands(ei.Work, Now);             // execute in-command コマンドを実行
            ei.Work.Current?.Process?.AddAndAdjustExitTiming(Events, ei.Work);  // put next event item status "Out" into event queu   Eventキューに Outイベントを登録
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

        private Dictionary<string/*location path*/, Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>>> _cioWorkCache = new Dictionary<string, Dictionary<CioBase, Dictionary<JitWork, bool>>>();

        public void AddWorkInReserve(string locationPath, CioBase cio, JitWork work)
        {
            var dic = _cioWorkCache.GetValueOrDefault(locationPath, true, a => new Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>>());
            var works = dic.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            works[work] = true;
        }

        /// <summary>
        /// Remove Work instance
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="work"></param>
        public void RemoveWorkInReserve(string locationPath, CioBase cio, JitWork work)
        {
            var dic = _cioWorkCache.GetValueOrDefault(locationPath, true, a => new Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>>());
            var works = dic.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            works.Remove(work);
        }

        /// <summary>
        /// Query works in reserve
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        public IEnumerable<JitWork> GetWorksInReserve(string locationPath, CioBase cio)
        {
            var dic = _cioWorkCache.GetValueOrDefault(locationPath, true, a => new Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>>());
            var works = dic.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            return works.Keys;
        }

        private Dictionary<string/*location Path*/, Dictionary<CioBase, DateTime>> _lastInTimesCio = new Dictionary<string, Dictionary<CioBase, DateTime>>();


        /// <summary>
        /// Save Last Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void SetLastInTime(string locationPath, CioBase cio, DateTime now)
        {
            var dic = _lastInTimesCio.GetValueOrDefault(locationPath, true, a => new Dictionary<CioBase, DateTime>());
            dic[cio] = now;
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
        public DateTime GetLastInTime(string locationPath, CioBase cio)
        {
            var dic = _lastInTimesCio.GetValueOrDefault(locationPath, true, a => new Dictionary<CioBase, DateTime>());
            return dic.GetValueOrDefault(cio);
        }

        private Dictionary<string/*location Path*/, Dictionary<JitProcess, Dictionary<JitWork, DateTime/*Enter-Time*/>>> _worksInProcess = new Dictionary<string, Dictionary<JitProcess, Dictionary<JitWork, DateTime>>>();

        /// <summary>
        /// Save Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void SaveWorkToSubsetProcess(string locationPath, JitProcess process, JitWork work, DateTime now)
        {
            var procworks = _worksInProcess.GetValueOrDefault(locationPath, true, a => new Dictionary<JitProcess, Dictionary<JitWork, DateTime/*Enter-Time*/>>());
            var works = procworks.GetValueOrDefault(process, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            works[work] = now;
        }

        /// <summary>
        /// Leave Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void RemoveWorkFromSubsetProcess(string locationPath, JitProcess process, JitWork work)
        {
            var procworks = _worksInProcess.GetValueOrDefault(locationPath, true, a => new Dictionary<JitProcess, Dictionary<JitWork, DateTime/*Enter-Time*/>>());
            var works = procworks.GetValueOrDefault(process, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            works.Remove(work);
        }

        /// <summary>
        /// Query Works in process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public IEnumerable<(JitWork Work, DateTime EnterTime)> GetWorks(string locationPath, JitProcess process)
        {
            var procworks = _worksInProcess.GetValueOrDefault(locationPath, true, a => new Dictionary<JitProcess, Dictionary<JitWork, DateTime/*Enter-Time*/>>());
            var works = procworks.GetValueOrDefault(process, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            return works.Select(kv => (kv.Key, kv.Value));
        }
    }
}
