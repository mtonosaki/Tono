// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tono.Jit
{
    /// <summary>
    /// Jit model template powerd with JaC
    /// </summary>
    [JacTarget(Name = "Template")]
    public class JitTemplate : JitVariable
    {
        public string ID { get; set; } = "Template." + string.Join("", Guid.NewGuid().ToByteArray().Select(a => $"{a:X2}"));

        /// <summary>
        /// The constructor of this class
        /// </summary>
        public JitTemplate() : base()
        {
            Classes.Set(":Template");
        }

        /// <summary>
        /// Instance Name = Template name
        /// </summary>
        ///public string Name { get; set; } // Using Variable's member

        private readonly List<(string RemoveKey, string Value)> JacBlock = new List<(string RemoveKey, string Value)>();

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
            return JacBlock.Select(a => a.Value);
        }

        private static int recordCounter = 0;

        /// <summary>
        /// JaC support Block.add
        /// </summary>
        /// <param name="jac"></param>
        /// <returns>remove key</returns>
        [JacListAdd(PropertyName = "Block")]
        public string AddBlock(string jac)
        {
            var key = $"t.{DateTime.Now.ToString("yyyyMMddHHmmss")}.{++recordCounter}";
            JacBlock.Add((key, jac));
            return key;
        }

        /// <summary>
        /// For RemoveBlock jac
        /// </summary>
        public const string LastBlockJac = "::LAST::";

        /// <summary>
        /// Jac Support Block.remove
        /// </summary>
        /// <param name="jac"></param>
        [JacListRemove(PropertyName = "Block")]
        public void RemoveBlock(string removeKey)
        {
            if (JacBlock.Count < 1) return;

            if (removeKey == LastBlockJac)
            {
                JacBlock.RemoveAt(JacBlock.Count - 1);
            }
            else
            {
                for (var i = JacBlock.Count - 1; i >= 0; i--)
                {
                    if (JacBlock[i].RemoveKey == removeKey)
                    {
                        JacBlock.RemoveAt(i);
                    }
                }
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
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is JitTemplate te)
            {
                return te.ID == ID;
            }
            else
            {
                return false;
            }
        }
        public override string ToString()
        {
            return $"{GetType().Name} ID={ID}";
        }
    }
}
