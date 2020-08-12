// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using ProcessKey = System.String;


namespace Tono.Jit
{
    /// <summary>
    /// JIT-model Process standard version
    /// </summary>
    /// <remarks>
    /// Process is the general object to make operation flow
    /// 工程は、物や情報の流れを作る基本的なオブジェクト
    /// </remarks>
    [JacTarget(Name = "Process")]
    public partial class JitProcess : JitVariable, IJitObjectID
    {
        public virtual string ID { get; set; } = JacInterpreter.MakeID("Process");

        /// <summary>
        /// The Constructor of this class
        /// </summary>
        public JitProcess() : base()
        {
            Classes.Set(":Process");
        }

        /// <summary>
        /// Having in-command objects
        /// </summary>
        public List<CiBase> InCommands { get; set; } = new List<CiBase>();

        /// <summary>
        /// Having out-constraint objects to let work wait at previous process
        /// OUT制約（次工程へのINを抑制できる）
        /// </summary>
        /// <remarks>
        /// NOTE: register sequence is important. first sequence is priority.
        /// 制約登録順番に注意：先頭から制約実行し、制約有りのオブジェクトが見つかったら、以降の制約は実行しない
        /// </remarks>
        public List<CoBase> Constraints { get; } = new List<CoBase>();

        [JacListAdd(PropertyName = "Cio")]
        public void AddCio(object obj)
        {
            if (obj is CiBase ci)
            {
                ci.ParentProcess = this;
                InCommands.Add(ci);
            }
            else if (obj is CoBase co)
            {
                co.ParentProcess = this;
                Constraints.Add(co);
            }
            else
            {
                throw new JacException(JacException.Codes.TypeMismatch, $"Cio.Add type mismatch arg type={(obj?.GetType().Name ?? "null")}");
            }
        }

        [JacListRemove(PropertyName = "Cio")]
        public void RemoveCio(object obj)
        {
            string id = "";
            if (obj is CioBase cio)
            {
                id = cio.ID;
            }
            if (obj is string str)
            {
                id = str;
            }
            foreach (var item in Cios.Where(a => a.ID == id).ToArray())
            {
                if (item is CiBase ci)
                {
                    InCommands.Remove(ci);
                }
                else
                if (item is CoBase co)
                {
                    Constraints.Remove(co);
                }
                else
                {
                    throw new JacException(JacException.Codes.TypeMismatch, $"Cio.Remove type mismatch Name={id}");
                }
            }
        }

        /// <summary>
        /// Collection utility COs UNION CIs
        /// </summary>
        public IEnumerable<CioBase> Cios
        {
            get
            {
                foreach (var c in Constraints)
                {
                    yield return c;
                }
                foreach (var c in InCommands)
                {
                    yield return c;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is JitProcess proc)
            {
                return proc.ID.Equals(ID);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name} {(Name ?? "")} (ID={ID})";
        }

        /// <summary>
        /// Put a work into this process
        /// </summary>
        /// <param name="work"></param>
        public virtual void Enter(JitWork work, DateTime now)
        {
            var stage = work.FindStage();

            stage.SaveWorkToSubsetProcess(work.Next, work, now);     // Add work to JitStage._worksInProcess
            work.Previous = work.Current;
            work.Current = work.Next;

            var nextProcs = stage.GetProcessLinkPathes(work.Current);
            if (nextProcs != null && nextProcs.Count > 0)
            {
                work.Next = nextProcs[0].NextLocation;
            }
            else
            {
                work.Next = null;
            }
            work.EnterTime = now;
            CheckAndAttachKanban(work.Current, now); // かんばんが有れば、NextProcessをかんばんで更新する
        }

        /// <summary>
        /// exit a work from this process
        /// </summary>
        /// <param name="work"></param>
        /// <param name="now"></param>
        public virtual void Exit(JitWork work)
        {
            var stage = work.FindStage();
            stage.RemoveWorkFromSubsetProcess(work.Current, work);

            var currentcios = work.Current?.Process?.Cios;
            if (currentcios != null)
            {
                foreach (var cio in currentcios)
                {
                    stage.RemoveWorkInReserve(work.Current, cio, work);    // Remove work from JitStage._cioWorkCache
                }
            }
        }

        /// <summary>
        /// Exit a priority work from this process that have not next process
        /// Engineに溜まっているこの工程のワーク(Work.NextProcess==null)から Exit優先の高いものを一つ退出させる
        /// </summary>
        /// <returns>
        /// selected work. null=no work
        /// </returns>
        /// <remarks>
        /// ret.PrevProcess = THIS PROCESS (do not confuse prev is current)
        /// ret.NextProcess = null
        /// ret.CurrentProcess = null
        /// </remarks>
        public virtual JitWork ExitCollectedWork(JitLocation location, DateTime now)
        {
            var buf =
                from wt in location.GetWorks(this)
                where wt.Work?.Next?.Process == null       // work that have not next process
                where wt.Work.ExitTime <= now       // select work that exit time expired.
                select new WorkEntery { Work = wt.Work, Enter = wt.EnterTime };
            var work = ExitWorkSelector.Invoke(buf);
            if (work != null)
            {
                Exit(work);

                work.Previous = work.Current.ToChangeProcess(this);
                work.Current = work.Current.ToEmptyProcess();
                work.Next = work.Current.ToEmptyProcess();
            }
            return work;
        }

        /// <summary>
        /// check out-constraint
        /// </summary>
        /// <param name="time"></param>
        /// <returns>true=cannot in yet</returns>
        /// <remarks>
        /// stop follow constraint checks if return false this constraint
        /// ※どれかの制約にあたったら、以降の制約は実行しない。
        /// </remarks>
        public bool CheckConstraints(JitWork work, DateTime time, out CoBase hitConstraint)
        {
            hitConstraint = null;
            foreach (var co in Constraints)
            {
                bool ret = co.Check(work, time);
                if (ret)
                {
                    hitConstraint = co;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Execute in-commands
        /// </summary>
        /// <param name="work">target work</param>
        /// <param name="time">simulation time</param>
        public void ExecInCommands(JitWork work, DateTime time)
        {
            foreach (var ci in InCommands)
            {
                ci.Exec(work, time);
            }
        }

        /// <summary>
        /// remember event item work will be into process
        /// INが Eventで予約されている事を 各種制約で考慮できるように覚えておく
        /// </summary>
        /// <param name="now"></param>
        public void RememberWorkWillBeIn(DateTime now, JitStage.WorkEventQueue.Item ei)
        {
            foreach (var cio in Cios)
            {
                var stage = ei.Work.FindStage();
                stage.SetLastInTime(ei.Work.Next, cio, now);  // save in-time (for Span constraint)
                stage.AddWorkInReserve(ei.Work.Next, cio, ei.Work);   // reserve work-in (for Max constraint) 
            }
        }

        /// <summary>
        /// 通常のProcessは、delayを考慮するだけの、FIFO
        /// </summary>
        /// <param name="events"></param>
        /// <param name="work"></param>
        public virtual void AddAndAdjustExitTiming(JitStage.WorkEventQueue events, JitWork work)
        {
            if (work.Next?.Process != null)
            {
                events.Enqueue(work.ExitTime, EventTypes.Out, work);  // 退場予約
            }
        }

        private class EventQueueKanban
        {
            public JitStage.WorkEventQueue EventQueue { get; set; }
            public JitKanban Kanban { get; set; }
        }

        /// <summary>
        /// Event queue kanban
        /// </summary>
        private readonly Dictionary<JitStage, Queue<EventQueueKanban>> kanbanQueue = new Dictionary<JitStage, Queue<EventQueueKanban>>();

        /// <summary>
        /// Add kanban
        /// </summary>
        /// <param name="kanban"></param>
        public virtual JitKanban AddKanban(JitKanban kanban, DateTime now)
        {
            var stage = kanban.FindStage();
            kanbanQueue.GetValueOrDefault(stage, true, a => new Queue<EventQueueKanban>()).Enqueue(new EventQueueKanban
            {
                EventQueue = stage.Events,
                Kanban = kanban,
            });
            return CheckAndAttachKanban(kanban.PullFrom, now);
        }

        /// <summary>
        /// Set work next location from Kanban information
        /// かんばんの目的地をワークに付ける（付け替える）
        /// </summary>
        /// <param name="location">target location of this Process instance</param>
        /// <returns>Kanban that is attached to a work</returns>
        private JitKanban CheckAndAttachKanban(JitLocation location, DateTime now)
        {
            var queue = kanbanQueue.GetValueOrDefault(location.Stage, true, a => new Queue<EventQueueKanban>());
            if (queue.Count == 0)
            {
                return null;
            }

            var buf =
                from we in location.Stage.GetWorks(location.ToChangeProcess(this))
                where we.Work?.Next?.Process == null  // No Next work
                select new WorkEntery { Work = we.Work, Enter = we.EnterTime };

            var work = ExitWorkSelector.Invoke(buf);
            if (work == null)
            {
                return null;
            }
            var sk = queue.Dequeue();
            work.Next = sk.Kanban.PullTo;
            work.Kanbans.Add(sk.Kanban);
            sk.Kanban.Work = work;
            if (work.ExitTime < now)
            {
                work.ExitTime = now;  // Because the work should be waiting kanban to exit from this process. かんばんを待っていたので、現在時刻が進んだときは、現在時刻でExitしたいこととする。
            }
            return sk.Kanban;
        }
    }

    #region Dummy Process Object
    public class JitProcessDummy : JitProcess
    {
        public override string ID { get; set; } = JacInterpreter.MakeID("ProcessDummy");

        public ProcessKey ProcessKey { get; set; } // Possible Name or ID

        public override string Name { get => throw new NotAllowErrorException(); set => throw new NotAllowErrorException(); }
        public override void AddAndAdjustExitTiming(JitStage.WorkEventQueue events, JitWork work)
        {
            throw new NotAllowErrorException();
        }

        public override JitKanban AddKanban(JitKanban kanban, DateTime now)
        {
            throw new NotAllowErrorException();
        }

        public override void Enter(JitWork work, DateTime now)
        {
            throw new NotAllowErrorException();
        }

        public override void Exit(JitWork work)
        {
            throw new NotAllowErrorException();
        }

        public override JitWork ExitCollectedWork(JitLocation location, DateTime now)
        {
            throw new NotAllowErrorException();
        }
    }
    #endregion

}
