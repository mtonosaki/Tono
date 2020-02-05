// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tono.Jit
{
    /// <summary>
    /// JIT-Model Root Object : Stage
    /// 工程やワーク全体を動かす 根幹のオブジェクト
    /// </summary>
    [JacTarget(Name = "Stage")]
    public partial class JitStage : JitStageSubset
    {
        /// <summary>
        /// the constructor of this class
        /// </summary>
        public JitStage()
        {
            Classes.Add(":Stage");
            Engine = new JitStageEngine();
            Model = new JitStageModel();
        }
    }
}
