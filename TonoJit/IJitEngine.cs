// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using static Tono.Jit.JitStage;

namespace Tono.Jit
{
    public interface IJieEngineReference
    {
        IJitEngine Engine { get; set; }
    }

    /// <summary>
    /// Interface of Jit Stage Engine
    /// </summary>
    public interface IJitEngine
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

        void AddWorkInReserve(JitSubset subset, CioBase cio, JitWork work);

        /// <summary>
        /// Remove Work instance
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="work"></param>
        void RemoveWorkInReserve(JitSubset subset, CioBase cio, JitWork work);

        /// <summary>
        /// Query works in reserve
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        IEnumerable<JitWork> GetWorksInReserve(JitSubset subset, CioBase cio);

        /// <summary>
        /// Save Last Work enter time.
        /// </summary>
        /// <param name="cio"></param>
        /// <param name="now"></param>
        void SetLastInTime(JitSubset subset, CioBase cio, DateTime now);

        /// <summary>
        /// last work enter time 最後にINした時刻
        /// </summary>
        /// <param name="cio"></param>
        /// <returns></returns>
        /// <remarks>
        /// This value will be set when out timing at previous process
        /// この値でSpanを評価。実際にProcessにINしたタイミングではなく、前ProcessでOutされた時にセットされる
        /// </remarks>
        DateTime GetLastInTime(JitSubset subset, CioBase cio);

        /// <summary>
        /// Enter work to process
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="process"></param>
        /// <param name="work"></param>
        /// <param name="now"></param>
        void EnterWorkToProcess(JitSubset subset, JitProcess process, JitWork work, DateTime now);

        /// <summary>
        /// Leave work from process
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="process"></param>
        /// <param name="work"></param>
        void ExitWorkFromProcess(JitSubset subset, JitProcess process, JitWork work);

        /// <summary>
        /// Query Works in process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        IEnumerable<(JitWork Work, DateTime EnterTime)> GetWorks(JitSubset subset,　JitProcess process);
    }
}
