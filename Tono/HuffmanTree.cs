// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tono
{
    /// <summary>
    /// Huffman Tree Utility
    /// </summary>
    public class HuffmanTree : IList<HuffmanTree.INode>
    {
        public interface INode
        {
            double Cost { get; }
        }

        public class InternalNode : INode
        {
            public INode Left { get; set; }
            public INode Right { get; set; }
            public InternalNode Parent { get; set; }
            public double Cost => Left.Cost + Right.Cost;

            public override string ToString()
            {
                return $"IN(Cost:{Cost} Left:{Left} / Right:{Right})";
            }
        }

        private List<INode> leafs = new List<INode>();
        private Dictionary<INode, InternalNode> leafParents;

        public HuffmanTree()
        {
        }

        public HuffmanTree(IEnumerable<INode> items)
        {
            AddRange(items);
        }

        public void Add(INode node)
        {
            leafs.Add(node);
            Root = null;
        }

        public HuffmanTree AddRange(IEnumerable<INode> nodes)
        {
            foreach (var node in nodes)
            {
                Add(node);
            }
            return this;
        }

        /// <summary>
        /// Build Tree
        /// </summary>
        public HuffmanTree Build() => Rebuild();

        /// <summary>
        /// Rebuild Tree Constuction
        /// </summary>
        public HuffmanTree Rebuild()
        {
            // SORT LIST
            var sortedLeafs = new List<INode>(leafs);
            sortedLeafs.Sort((a, b) => Compare.Normal(b.Cost, a.Cost));
            var nodes = new LinkedList<INode>(sortedLeafs);
            leafParents = new Dictionary<INode, InternalNode>();

            // MAKE TREE
            while (nodes.Count > 1)
            {
                var last1 = nodes.Last;
                nodes.RemoveLast();
                var last2 = nodes.Last;
                nodes.RemoveLast();

                var inode = new InternalNode
                {
                    Left = last2.Value,
                    Right = last1.Value,
                };
                if (inode.Left is InternalNode in1)
                {
                    in1.Parent = inode;
                }
                else
                {
                    leafParents[inode.Left] = inode;
                }
                if (inode.Right is InternalNode in2)
                {
                    in2.Parent = inode;
                }
                else
                {
                    leafParents[inode.Right] = inode;
                }
                SortedInsert(nodes, inode);
            }
            Root = nodes.First.Value;

            return this;
        }

        public BitList GetBitResult(INode leaf)
        {
            var ret = new List<bool>();
            InternalNode parent = null;
            for (var node = leaf; node != null; node = parent)
            {
                parent = GetParent(node);
                if (parent != null)
                {
                    ret.Add(ReferenceEquals(node, parent.Right));
                }
            }
            return BitList.From(((IEnumerable<bool>)ret).Reverse());
        }

        public IEnumerable<BitList> GetBitResults()
        {
            foreach (var leaf in leafs)
            {
                yield return GetBitResult(leaf);
            }
        }

        private InternalNode GetParent(INode node)
        {
            if (node is InternalNode inode)
            {
                return inode.Parent;
            }
            else
            {
                return leafParents[node];
            }
        }

        public INode Root { get; set; }

        public int Count => leafs.Count;

        public bool IsReadOnly { get; } = false;

        public INode this[int index] { get => leafs[index]; set => leafs[index] = value; }

        private void SortedInsert(LinkedList<INode> nodes, INode joinnode)
        {
            if (nodes.Count > 0)
            {
                LinkedListNode<INode> n;
                for (n = nodes.Last; n != null && n.Value.Cost <= joinnode.Cost; n = n.Previous)
                {
                    ;
                }
                if (n == null)
                {
                    nodes.AddFirst(joinnode);
                }
                else
                {
                    nodes.AddAfter(n, joinnode);
                }
            }
            else
            {
                nodes.AddLast(joinnode);
            }
        }

        public int IndexOf(INode item)
        {
            return leafs.IndexOf(item);
        }

        public void Insert(int index, INode item)
        {
            leafs.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            leafs.RemoveAt(index);
        }

        public void Clear()
        {
            leafs.Clear();
            Root = null;
        }

        public bool Contains(INode item)
        {
            return leafs.Contains(item);
        }

        public void CopyTo(INode[] array, int arrayIndex)
        {
            leafs.CopyTo(array, arrayIndex);
        }

        public bool Remove(INode item)
        {
            return leafs.Remove(item);
        }

        public IEnumerator<INode> GetEnumerator()
        {
            return leafs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return leafs.GetEnumerator();
        }
    }
}
