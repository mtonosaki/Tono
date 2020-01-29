// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        public string ID { get; set; } = JacInterpreter.MakeID("Process");

        /// <summary>
        /// The Constructor of this class
        /// </summary>
        public JitProcess() : base()
        {
            Classes.Set(":Process");
        }

        /// <summary>
        /// in-command collection utility
        /// </summary>
        public class InCommandCollection : IEnumerable<CiBase>
        {
            private readonly List<CiBase> _data = new List<CiBase>();

            public IEnumerator<CiBase> GetEnumerator()
            {
                return _data.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _data.GetEnumerator();
            }

            public void Add(CiBase ci)
            {
                _data.Add(ci);
            }

            public void Remove(CiBase item)
            {
                _data.Remove(item);
            }

            public CiBase this[int index] => _data[index];
        }

        /// <summary>
        /// Having in-command objects
        /// </summary>
        public InCommandCollection InCommands { get; set; } = new InCommandCollection();

        [JacListAdd(PropertyName = "Cio")]
        public void CioAdd(object obj)
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
        public void CioRemove(object obj)
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
        /// Having out-constraint objects to let work wait at previous process
        /// OUT制約（次工程へのINを抑制できる）
        /// </summary>
        /// <remarks>
        /// NOTE: register sequence is important. first sequence is priority.
        /// 制約登録順番に注意：先頭から制約実行し、制約有りのオブジェクトが見つかったら、以降の制約は実行しない
        /// </remarks>
        public List<CoBase> Constraints { get; } = new List<CoBase>();

        /// <summary>
        /// Link set of the owner stage
        /// jfStageにあるリンクセット
        /// </summary>
        public Destinations NextLinks { get; set; } = new Destinations(); // TODO: Change super lazy by process key

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
            return $"{GetType().Name} {Name} (ID={ID})";
        }

        /// <summary>
        /// Put a work into this process
        /// </summary>
        /// <param name="work"></param>
        public virtual void Enter(JitWork work, DateTime now)
        {
            foreach (var cio in Cios)
            {
                work.Stage.RemoveWorkInReserve(cio, work);
            }

            work.Stage.EnterWorkToProcess(this, work, now);
            work.PrevProcess = work.CurrentProcess;
            work.CurrentProcess = work.NextProcess;
            work.NextProcess = NextLinks.FirstOrNull();
            work.EnterTime = now;
            CheckAndAttachKanban(work.Stage, now); // かんばんが有れば、NextProcessをかんばんで更新する
        }

        /// <summary>
        /// exit a work from this process
        /// </summary>
        /// <param name="work"></param>
        /// <param name="now"></param>
        public virtual void Exit(JitWork work)
        {
            work.Stage.ExitWorkFromProcess(this, work);
        }

        /// <summary>
        /// Exit a priority work from this process that have not next process
        /// この工程に 溜まっているワーク(Work.NextProcess==null)から Exit優先の高いものを一つ退出させる
        /// </summary>
        /// <returns>
        /// selected work. null=no work
        /// </returns>
        /// <remarks>
        /// ret.PrevProcess = THIS PROCESS (do not confuse prev is current)
        /// ret.NextProcess = null
        /// ret.CurrentProcess = null
        /// </remarks>
        public virtual JitWork ExitCollectedWork(JitStage stage, DateTime now)
        {
            var buf =
                from wt in stage.GetWorks(this)
                where wt.Work.NextProcess == null // work that have not next process
                where wt.Work.ExitTime <= now     // select work that exit time expired.
                select new WorkEntery { Work = wt.Work, Enter = wt.EnterTime };
            var work = ExitWorkSelector.Invoke(buf);
            if (work != null)
            {
                Exit(work);

                work.PrevProcess = this;
                work.CurrentProcess = null;
                work.NextProcess = null;
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
                ei.Work.Stage.SetLastInTime(cio, now);  // save in-time (for Span constraint)
                ei.Work.Stage.AddWorkInReserve(cio, ei.Work);   // reserve work-in (for Max constraint)
            }
        }

        /// <summary>
        /// 通常のProcessは、delayを考慮するだけの、FIFO
        /// </summary>
        /// <param name="events"></param>
        /// <param name="work"></param>
        public virtual void AddAndAdjustExitTiming(JitStage.WorkEventQueue events, JitWork work)
        {
            if (work.NextProcess != null)
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
        private readonly Queue<EventQueueKanban> kanbanQueue = new Queue<EventQueueKanban>();

        /// <summary>
        /// Add kanban
        /// </summary>
        /// <param name="kanban"></param>
        public virtual JitKanban AddKanban(JitStage stage, JitKanban kanban, DateTime now)
        {
            kanbanQueue.Enqueue(new EventQueueKanban
            {
                EventQueue = stage.Events,
                Kanban = kanban,
            });
            return CheckAndAttachKanban(stage, now);
        }

        /// <summary>
        /// かんばんの目的地をワークに付ける（付け替える）
        /// </summary>
        /// <returns>処理されたかんばん</returns>
        private JitKanban CheckAndAttachKanban(JitStage stage, DateTime now)
        {
            if (kanbanQueue.Count == 0)
            {
                return null;
            }

            var buf =
                from w in stage.GetWorks(this)
                where w.Work.NextProcess == null // 行先が無い
                select new WorkEntery { Work = w.Work, Enter = w.EnterTime };
            var work = ExitWorkSelector.Invoke(buf);

            if (work == null)
            {
                return null;
            }

            var sk = kanbanQueue.Dequeue();
            work.NextProcess = sk.Kanban.Stage.FindProcess(sk.Kanban.PullToProcessKey);
            work.Kanbans.Add(sk.Kanban);
            sk.Kanban.Work = work;
            if (work.ExitTime < now)
            {
                work.ExitTime = now;    // かんばんを待っていたので、現在時刻が進んだときは、現在時刻でExitしたいこととする。
            }

            return sk.Kanban;
        }
    }
}
