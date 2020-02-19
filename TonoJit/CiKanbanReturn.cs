// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Linq;

namespace Tono.Jit
{
    /// <summary>
    /// in-command of auto return kanban
    /// </summary>
    [JacTarget(Name = "CiKanbanReturn")]
    public class CiKanbanReturn : CiBase
    {
        public static readonly Type Type = typeof(CiKanbanReturn);

        /// <summary>
        /// work filter with class setting
        /// </summary>
        /// <example>
        /// TargetKanbanClass = ":Car" --- pick kanbans that have :Car class only
        /// TargetKanbanClass = ":Car:Man" --- pick kanbans that have both :Car and :Man
        /// </example>
        public string TargetKanbanClass { get; set; } = ":Kanban";

        /// <summary>
        /// Delay time for kanban return
        /// </summary>
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(0);

        public override string MakeShortValue()
        {
            if (TargetKanbanClass != ":Kanban")
            {
                return TargetKanbanClass;
            }
            else
            {
                return $"";
            }
        }

        /// <summary>
        /// in-command execute
        /// </summary>
        /// <param name="work">target work</param>
        /// <param name="now">simulation time</param>
        public override void Exec(JitWork work, DateTime now)
        {
            var kanbans =
                from kanban in work.Kanbans
                where kanban.Is(TargetKanbanClass)
                where kanban.PullTo.Equals(work.Current)
                select kanban;

            foreach (var kanban in kanbans.ToArray())
            {
                work.Kanbans.Remove(kanban);
                kanban.Work = null;
                work.FindStage().SendKanban(now + Delay, kanban);
            }
        }
    }
}
