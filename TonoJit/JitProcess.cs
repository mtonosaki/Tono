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
    public partial class JitProcess
    {
        /// <summary>
        /// Process name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Data source
        /// </summary>
        public string Source { get; set; }

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

            public CiBase this[int index] => _data[index];
        }

        private InCommandCollection _inCommands = new InCommandCollection();

        /// <summary>
        /// Having in-command objects
        /// </summary>
        public InCommandCollection InCommands
        {
            get => _inCommands;
            set => _inCommands = value;
        }

        /// <summary>
        /// Having out-constraint objects to let work wait at previous process
        /// OUT制約（次工程へのINを抑制できる）
        /// </summary>
        /// <remarks>
        /// NOTE: register sequence is important. first sequence is priority.
        /// 制約登録順番に注意：先頭から制約実行し、制約有りのオブジェクトが見つかったら、以降の制約は実行しない
        /// </remarks>
        public List<CoBase> Constraints = new List<CoBase>();

        /// <summary>
        /// Link set of the owner stage
        /// jfStageにあるリンクセット
        /// </summary>
        public Destinations NextLinks { get; set; } = new Destinations();

        /// <summary>
        /// having work-in time mapping
        /// 属するワークのIN時刻マップ
        /// </summary>
        public Dictionary<JitWork, DateTime/*In Time*/> WorkInTimes { get; private set; } = new Dictionary<JitWork, DateTime>();

        /// <summary>
        /// Having works
        /// ワーク一覧
        /// </summary>
        public IEnumerable<JitWork> Works => WorkInTimes.Keys;

        /// <summary>
        /// Collection utility COs UNION CIs
        /// </summary>
        public IEnumerable<CioBase> Cios
        {
            get
            {
                foreach (CoBase c in Constraints)
                {
                    yield return c;
                }
                foreach (CiBase c in InCommands)
                {
                    yield return c;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is JitProcess proc)
            {
                return proc.Name?.Equals(Name) ?? false;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"{GetType().Name} {Name}";
        }

        /// <summary>
        /// Put a work into this process
        /// </summary>
        /// <param name="work"></param>
        public virtual void Enter(JitWork work, DateTime now)
        {
            foreach (CioBase.IWorkInReserved c in Cios.Where(a => a is CioBase.IWorkInReserved))
            {
                c.RemoveWorkInReserve(work);
            }

            WorkInTimes[work] = now;
            work.PrevProcess = work.CurrentProcess;
            work.CurrentProcess = work.NextProcess;
            work.NextProcess = NextLinks.FirstOrNull();
            work.EnterTime = now;
            checkAndAttachKanban(now); // かんばんが有れば、NextProcessをかんばんで更新する
        }

        /// <summary>
        /// exit a work from this process
        /// </summary>
        /// <param name="work"></param>
        /// <param name="now"></param>
        public virtual void Exit(JitWork work)
        {
            WorkInTimes.Remove(work);
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
        public virtual JitWork ExitCollectedWork(DateTime now)
        {
            IEnumerable<WorkEntery> buf =
                from w in WorkInTimes
                where w.Key.NextProcess == null // work that have not next process
                where w.Key.ExitTime <= now     // select work that exit time expired.
                select new WorkEntery { Work = w.Key, Enter = w.Value };
            JitWork work = ExitWorkSelector.Invoke(buf);
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
            foreach (CoBase co in Constraints)
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
            foreach (CiBase ci in InCommands)
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
            // save in-time (for Span constraint)
            foreach (CioBase.ILastInTime c in Cios.Where(a => a is CioBase.ILastInTime))
            {
                c.LastInTime = now;
            }

            // reserve work-in (for Max constraint)
            foreach (CioBase.IWorkInReserved c in Cios.Where(a => a is CioBase.IWorkInReserved))
            {
                c.AddWorkInReserve(ei.Work);
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
        public virtual JitKanban AddKanban(JitStage.WorkEventQueue events, JitKanban kanban, DateTime now)
        {
            kanbanQueue.Enqueue(new EventQueueKanban
            {
                EventQueue = events,
                Kanban = kanban,
            });
            return checkAndAttachKanban(now);
        }



        /// <summary>
        /// かんばんの目的地をワークに付ける（付け替える）
        /// </summary>
        /// <returns>処理されたかんばん</returns>
        private JitKanban checkAndAttachKanban(DateTime now)
        {
            if (kanbanQueue.Count == 0)
            {
                return null;
            }

            IEnumerable<WorkEntery> buf =
                from w in WorkInTimes
                where w.Key.NextProcess == null // 行先が無い
                select new WorkEntery { Work = w.Key, Enter = w.Value };
            JitWork work = ExitWorkSelector.Invoke(buf);

            if (work == null)
            {
                return null;
            }

            EventQueueKanban sk = kanbanQueue.Dequeue();
            work.NextProcess = sk.Kanban.PullTo();
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
