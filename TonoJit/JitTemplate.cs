// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Tono.Jit
{
    /// <summary>
    /// Jit model template powerd with JaC
    /// </summary>
    [JacTarget(Name = "Template")]
    public class JitTemplate
    {
        /// <summary>
        /// Instance Name = Template name
        /// </summary>
        public string Name { get; set; }

        private readonly List<string> JacBlock = new List<string>();

        /// <summary>
        /// Template block count
        /// </summary>
        public int Count => JacBlock.Count;

        /// <summary>
        /// Get Block Enumerable
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetBlocks()
        {
            return JacBlock;
        }

        /// <summary>
        /// JaC support Block.add
        /// </summary>
        /// <param name="jac"></param>
        [JacListAdd(PropertyName = "Block")]
        public void AddBlock(string jac)
        {
            JacBlock.Add(jac);
        }

        /// <summary>
        /// Jac Support Block.remove
        /// </summary>
        /// <param name="jac"></param>
        [JacListRemove(PropertyName = "Block")]
        public void RemoveBlock(string jac)
        {
            if (jac.Equals("::LAST::", System.StringComparison.OrdinalIgnoreCase))
            {
                RemoveLastBlock();
            }
            else
            {
                JacBlock.Add(jac);
            }
        }

        /// <summary>
        /// Remove last block
        /// </summary>
        public void RemoveLastBlock()
        {
            if (JacBlock.Count > 0)
            {
                JacBlock.RemoveAt(JacBlock.Count - 1);
            }
        }
    }
}
