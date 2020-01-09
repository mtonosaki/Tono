// Copyright (c) Manabu Tonosaki All rights reserved.
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
        /// target Stage
        /// </summary>
        private JitStage Stage { get; set; }    // TODO: Stage should come from parent process instead of set by programmer.

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
        /// Default constructor : NOTE Do not forget to set Stage property
        /// </summary>
        public CiKanbanReturn()
        {
        }

        /// <summary>
        /// The construction of this class
        /// </summary>
        /// <param name="parent"></param>
        public CiKanbanReturn(JitStage stage)
        {
            Stage = stage;
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
                where kanban.PullTo().Equals(work.CurrentProcess)
                select kanban;

            foreach (var kanban in kanbans.ToArray())
            {
                work.Kanbans.Remove(kanban);
                kanban.Work = null;
                Stage.SendKanban(now + Delay, kanban);
            }
        }
    }
}
