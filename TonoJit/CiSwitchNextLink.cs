using System;

namespace Tono.Jit
{
    /// <summary>
    /// in-command to switch next process with parameter
    /// </summary>
    public class CiSwitchNextLink : CiBase
    {
        /// <summary>
        /// ワークに書かれているスイッチ値の変数名
        /// </summary>
        public JitVariable Name { get; set; } = JitVariable.From("NextLinkNo");

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
                int nextLinkNo = DbUtil.ToInt(work.ChildVriables.GetValueOrNull(Name.Value.ToString())?.Value, def: -1);
                if (nextLinkNo >= 0)
                {
                    work.NextProcess = GetParentProcess(work).NextLinks[nextLinkNo];
                }
            }
        }
    }
}
