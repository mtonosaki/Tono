// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono.Jit
{
    /// <summary>
    /// TODO: WARNING : THIS CLASS IS NOT YET COMPLETED 2019.11.17
    /// in-command to switch next process with parameter
    /// </summary>
    [JacTarget(Name = "CiSwitchNextLink")]
    public class CiSwitchNextLink : CiBase
    {
        public static readonly Type Type = typeof(CiSwitchNextLink);

        /// <summary>
        /// Switching variable name of work object
        /// ワークに書かれているスイッチ値の変数名
        /// </summary>
        public JitVariable NextLinkVarName { get; set; } = JitVariable.From("NextLinkNo");

        /// <summary>
        /// work filter classes
        /// フィルターするクラス（デフォルト :Workは全種類の子ワークを対象）
        /// </summary>
        /// <example>
        /// TargetWorkClass = ":Sumaho"  ---  pick works that have :Sumaho class only
        /// TargetWorkClass = ":iOS:Sumaho"  --- pick works that have both :iOS and :Sumaho classes only.
        /// </example>
        public string TargetWorkClass { get; set; } = ":Work";


        public override void Exec(JitWork work, DateTime now)
        {
            if (work.Is(TargetWorkClass))
            {
                int nextLinkNo = DbUtil.ToInt(work.ChildVriables.GetValueOrNull(NextLinkVarName.Value.ToString())?.Value, def: -1);
                if (nextLinkNo >= 0)
                {
                    work.NextProcess = GetCheckTargetProcess(work).NextLinks[nextLinkNo];
                }
            }
        }
    }
}
