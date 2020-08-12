// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Tono.Jit.Utils;
using ProcessKey = System.String;
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

        [JacListAdd(PropertyName = "Works")]
        public void AddJacWorks(object description)
        {
            if (description is JitWork work)
            {
                Events.Enqueue(Now, EventTypes.Out, work);
            }
            else if (description is ValueTuple<object, object> tpl && tpl.Item1 is DateTime dt && tpl.Item2 is JitWork work2)
            {
                Events.Enqueue(dt, EventTypes.Out, work2);
            }
            else
            {
                throw new JitException(JitException.TypeMissmatch, $"Type {description.GetType().Name} is not support to AddJacWorks");
            }
        }

        [JacListRemove(PropertyName = "Works")]
        public void RemoveJacWorks(object description)
        {
            if (description is JitWork work)
            {
                var node = Events.Find(work);
                if (node != null)
                {
                    Events.Remove(node);
                }
                else
                {
                    throw new JitException(JitException.ItemNotFound, $"Work.ID = {work.ID}");
                }
            }
            else
            {
                throw new JitException(JitException.TypeMissmatch, $"Type {description.GetType().Name} is not support to AddJacWorks");
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
            if (procKeyPath == null)
            {
                goto Error;
            }
            string path = currentLocation.Path;
            if (currentLocation.Process != null)
            {
                path = JitLocation.CombinePath(path, GetProcessKey(currentLocation.Process));
            }
            path = JitLocation.CombinePath(path, procKeyPath);
            path = JitLocation.Normalize(path);
            if (path.StartsWith("\\") == false)
            {
                goto Error;
            }
            var keys = path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            JitSubset subset = this;
            for (var i = 0; i < keys.Length - 1; i++)
            {
                var prockey = keys[i];
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
            if (proc != null)
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
            var tarProc = ei.Kanban.PullFrom;
            var usedKanban = tarProc.Process.AddKanban(ei.Kanban, Now); // Set Kanban for Jit process. 工程にかんばんを投入して、処理を促す
            usedKanban?.Work.Current.Process.AddAndAdjustExitTiming(Events, usedKanban.Work); // Enqueue out event.  (Eventキューに Outイベントを登録)
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
            if (ei.Work.Next?.Process == null)
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

        private readonly Dictionary<string/*location path*/, Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>>> _cioWorkCache = new Dictionary<string, Dictionary<CioBase, Dictionary<JitWork, bool>>>();

        public void AddWorkInReserve(JitLocation location, CioBase cio, JitWork work)
        {
            var dic = _cioWorkCache.GetValueOrDefault(location.FullPath, true, a => new Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>>());
            var works = dic.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            works[work] = true;
        }

        /// <summary>
        /// Remove Work instance
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="work"></param>
        public void RemoveWorkInReserve(JitLocation location, CioBase cio, JitWork work)
        {
            var dic = _cioWorkCache.GetValueOrDefault(location.FullPath, true, a => new Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>>());
            var works = dic.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            works.Remove(work);
        }

        /// <summary>
        /// Query works in reserve
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        public IEnumerable<JitWork> GetWorksInReserve(JitLocation location, CioBase cio)
        {
            var dic = _cioWorkCache.GetValueOrDefault(location.FullPath, true, a => new Dictionary<CioBase, Dictionary<JitWork, bool/*dummy*/>>());
            var works = dic.GetValueOrDefault(cio, true, a => new Dictionary<JitWork, bool>());
            return works.Keys;
        }

        private readonly Dictionary<string/*location full Path*/, Dictionary<CioBase, DateTime>> _lastInTimesCio = new Dictionary<string, Dictionary<CioBase, DateTime>>();


        /// <summary>
        /// Save Last Work enter time.
        /// </summary>
        /// <param name="locationPath"></param>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void SetLastInTime(JitLocation location, CioBase cio, DateTime now)
        {
            var dic = _lastInTimesCio.GetValueOrDefault(location.FullPath, true, a => new Dictionary<CioBase, DateTime>());
            dic[cio] = now;
        }

        /// <summary>
        /// last work enter time 最後にINした時刻 
        /// </summary>
        /// <param name="locationFullPath">JitLocation.FullPath</param>
        /// <param name="cio"></param>
        /// <returns></returns>
        /// <remarks>
        /// This value will be set when out timing at previous process
        /// この値でSpanを評価。実際にProcessにINしたタイミングではなく、前ProcessでOutされた時にセットされる
        /// </remarks>
        public DateTime GetLastInTime(JitLocation location, CioBase cio)
        {
            var dic = _lastInTimesCio.GetValueOrDefault(location.FullPath, true, a => new Dictionary<CioBase, DateTime>());
            return dic.GetValueOrDefault(cio);
        }

        private readonly Dictionary<string/*location Full Path*/, Dictionary<JitWork, DateTime/*Enter-Time*/>> _worksInProcess = new Dictionary<ProcessKeyPath, Dictionary<JitWork, DateTime>>();

        /// <summary>
        /// Save Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void SaveWorkToSubsetProcess(JitLocation location, JitWork work, DateTime now)
        {
            Debug.Assert(location.Process != null);
            var procworks = _worksInProcess.GetValueOrDefault(location.FullPath, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            procworks[work] = now;
        }

        /// <summary>
        /// Leave Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        public void RemoveWorkFromSubsetProcess(JitLocation location, JitWork work)
        {
            Debug.Assert(location.Process != null);
            var procworks = _worksInProcess.GetValueOrDefault(location.FullPath, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            procworks.Remove(work);
        }

        private readonly (JitWork Work, DateTime EnterTime)[] ZeroWorkCollection = new (JitWork Work, DateTime EnterTime)[] { };

        /// <summary>
        /// Query Works in process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public IEnumerable<(JitWork Work, DateTime EnterTime)> GetWorks(JitLocation location)
        {
            Debug.Assert(location.Process != null);
            var procworks = _worksInProcess.GetValueOrDefault(location.FullPath, true, a => new Dictionary<JitWork, DateTime/*Enter-Time*/>());
            if (procworks != null)
            {
                return procworks.Select(kv => (kv.Key, kv.Value));
            }
            return ZeroWorkCollection;
        }

        /// <summary>
        /// Find Process Link of the target process location considering global path.
        /// </summary>
        /// <param name="tarProcessLocation"></param>
        /// <returns></returns>
        public IReadOnlyList<(JitLocation NextLocation, ProcessKey ProcKey)> GetProcessLinkPathes(JitLocation tarProcessLocation)
        {
            if (tarProcessLocation.Process == null)  // need to set Process
            {
                throw new JitException(JitException.NoProcess);
            }
            IReadOnlyList<ProcessKey> links = null;
            JitLocation loc = null;
            var stage = tarProcessLocation.Stage;
            var root = JitLocation.CreateRoot(stage);
            var pathes = tarProcessLocation.FullPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = pathes.Length; i > 0; i--)
            {
                var checkSubsetPath = string.Join("\\", pathes, 0, i);
                loc = stage.FindSubsetProcess(root, checkSubsetPath, true);
                if (loc == null)
                {
                    throw new JitException(JitException.IllegalPath, tarProcessLocation.FullPath);
                }
                for (var ii = pathes.Length - 1; ii >= 0; ii--)
                {
                    var checkPath = string.Join("\\", pathes, ii, pathes.Length - ii);
                    links = loc.SubsetCache.GetProcessLinkPathes(checkPath);
                    if ((links?.Count ?? 0) == 0 && ii == 0)
                    {
                        links = loc.SubsetCache.GetProcessLinkPathes("\\" + checkPath);
                    }
                    if (links?.Count == 0)
                    {
                        links = null;
                    }

                    if (links != null)
                    {
                        i = 0;
                        break;
                    }
                }
            }
            if (links != null)
            {
                var ret = from pk in links
                          let proc = loc.FindSubsetProcess(pk, true)
                          select (proc, pk);
                return ret.ToList();
            }
            else
            {
                return null;
            }
        }
    }
}
