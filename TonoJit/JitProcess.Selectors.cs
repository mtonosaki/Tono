// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tono.Jit
{
    public partial class JitProcess
    {
        public class WorkEntery
        {
            public JitWork Work { get; set; }
            public DateTime Enter { get; set; }
        }

        /// <summary>
        /// work select logic
        /// </summary>
        public Func<IEnumerable<WorkEntery>, JitWork> ExitWorkSelector { get; set; } = FIFOSelector;

        /// <summary>
        /// work FIFO exit logic
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static JitWork FIFOSelector(IEnumerable<WorkEntery> buf)
        {
            DateTime min = DateTime.MaxValue;
            JitWork ret = null;
            foreach (WorkEntery wt in buf)
            {
                if (wt.Enter < min)
                {
                    min = wt.Enter;
                    ret = wt.Work;
                }
            }
            return ret;
        }

        /// <summary>
        /// work LIFO logic such as a stack
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static JitWork LIFOSelector(IEnumerable<WorkEntery> buf)
        {
            DateTime max = DateTime.MinValue;
            JitWork ret = null;
            foreach (WorkEntery wt in buf)
            {
                if (wt.Enter > max)
                {
                    max = wt.Enter;
                    ret = wt.Work;
                }
            }
            return ret;
        }

        /// <summary>
        /// work select logic (no rule for speed)
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static JitWork SomeSelector(IEnumerable<WorkEntery> buf)
        {
            WorkEntery item = buf.FirstOrDefault();
            if (item != default)
            {
                return item.Work;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// work random selector
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static JitWork RandomSelector(IEnumerable<WorkEntery> buf)
        {
            WorkEntery[] works = buf.ToArray();
            if (works.Length == 0)
            {
                return null;
            }

            return works[(int)(MathUtil.Rand0(false) * works.Length)].Work;
        }
    }
}
