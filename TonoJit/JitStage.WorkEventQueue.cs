// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Tono.Jit
{
    /// <summary>
    /// event type
    /// 次に行うイベントの種類
    /// </summary>
    public enum EventTypes
    {
        In,
        InCheck,
        Out,
        KanbanIn,
    }

    public partial class JitStage
    {
        /// <summary>
        /// work/kanban event queue object
        /// </summary>
        public class WorkEventQueue
        {
            /// <summary>
            /// work/kanban event item
            /// </summary>
            public class Item
            {
                /// <summary>
                /// target work  対象ワーク（対象かんばんと同時にセットされない前提）
                /// </summary>
                public JitWork Work { get; set; }

                /// <summary>
                /// target kanban  対象かんばん（対象ワークと同時にセットされない前提）
                /// </summary>
                public JitKanban Kanban { get; set; }

                /// <summary>
                /// event fire date time
                /// </summary>
                public DateTime DT { get; set; }

                /// <summary>
                /// event type
                /// </summary>
                public EventTypes Type { get; set; }

                public override string ToString()
                {
                    if (Work != null)
                    {
                        return $"{GetType().Name}  {DT.ToString(TimeUtil.FormatYMDHMS)} : {Work} --> {Type}";
                    }
                    if (Kanban != null)
                    {
                        return $"{GetType().Name}  {DT.ToString(TimeUtil.FormatYMDHMS)} : {Kanban} --> {Type}";
                    }
                    return $"{GetType().Name}  {DT.ToString(TimeUtil.FormatYMDHMS)} : ? --> {Type}";
                }
            }

            /// <summary>
            /// dummy event item for speed-up
            /// </summary>
            public class DummyItem : Item
            {
            }

            /// <summary>
            /// queue data
            /// </summary>
            private readonly LinkedList<Item> _dat = new LinkedList<Item>();
            private readonly Dictionary<DateTime, LinkedListNode<Item>> _sections = new Dictionary<DateTime, LinkedListNode<Item>>();    // 分毎のダミーアイテム. null=その時刻のデータは_datには無い

            /// <summary>
            /// Parent Stage
            /// </summary>
            public JitStage Stage { get; internal set; }

            /// <summary>
            /// remove the specified nodes 指定アイテムを全部消す
            /// </summary>
            /// <param name="nodes"></param>
            public void Remove(IEnumerable<LinkedListNode<Item>> nodes)
            {
                foreach (var node in nodes)
                {
                    Remove(node);
                }
            }

            /// <summary>
            /// remove a specified node  指定アイテムを消す
            /// </summary>
            /// <param name="node"></param>
            public void Remove(LinkedListNode<Item> node)
            {
                _dat.Remove(node);
            }

            /// <summary>
            /// add a work into event queue
            /// ワークをキューに追加（時間順。同時刻なら後の方に）
            /// </summary>
            /// <param name="workOrKanban">object either work or kanban</param>
            /// <returns></returns>
            public JitVariable Enqueue(DateTime dt, EventTypes type, JitVariable workOrKanban)
            {
                PrepareDummyItems(dt);

                LinkedListNode<Item> node;
                if (_sections.TryGetValue(TimeUtil.ClearSeconds(dt), out var topnode) == false)    // 分毎にシーク位置をスキップできる
                {
                    topnode = _dat.First;
                }
                for (node = topnode; node != null; node = node.Next)    // 挿入位置を捜す
                {
                    if (node.Value.DT > dt)
                    {
                        break;
                    }
                }
                var item = new Item
                {
                    DT = dt,
                    Work = workOrKanban as JitWork,
                    Kanban = workOrKanban as JitKanban,
                    Type = type,
                };
                if (node == null)   // ポカヨケ
                {
                    _dat.AddLast(item);
                }
                else
                {
                    _dat.AddBefore(node, item);
                }
                return workOrKanban;
            }

            /// <summary>
            /// check first item (not dequeue)
            /// 先頭のItemを覗く（Dequeueしない）
            /// </summary>
            /// <returns></returns>
            public Item PeekNext()
            {
                var node = _dat.First;
                for (; node?.Value is DummyItem; node = node.Next)
                {
                    ;
                }

                return node?.Value;
            }

            /// <summary>
            /// check event items (not dequeue)
            /// キューアイテムを指定件数覗き見る
            /// </summary>
            /// <param name="maxCount"></param>
            /// <returns></returns>
            public IEnumerable<Item> Peeks(int maxCount)
            {
                for (var node = _dat.First; node != null && maxCount > 0; node = node.Next)
                {
                    if ((node.Value is DummyItem) == false)
                    {
                        maxCount--;
                        yield return node.Value;
                    }
                }
            }

            /// <summary>
            /// find filtered work order by exit time sequence
            /// 退出する順番に、指定する属性のワークを検索する
            /// </summary>
            /// <param name="proc"></param>
            /// <param name="etype"></param>
            /// <param name="workclass"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public virtual LinkedListNode<Item> Find(JitLocation proc, EventTypes etype, string workclass = JitVariable.Class.Object)
            {
                for (var node = _dat.First; node != null; node = node.Next)
                {
                    var ei = node.Value;
                    if (ei is DummyItem || ei.Work is JitWork == false)
                    {
                        continue;
                    }
                    var w = ei.Work;
                    if (w.Current == proc && ei.Type == etype && w.Is(workclass))
                    {
                        return node;
                    }
                }
                return null;
            }

            /// <summary>
            /// find the all filtered work order by exit time sequence
            /// 退出する順番に、指定する属性のワークを すべて 検索する
            /// </summary>
            /// <param name="proc"></param>
            /// <param name="etype"></param>
            /// <param name="workclass"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public virtual IEnumerable<LinkedListNode<Item>> FindAll(JitLocation location, EventTypes etype, string workclass = JitVariable.Class.Object)
            {
                for (var node = _dat.First; node != null; node = node.Next)
                {
                    var ei = node.Value;
                    if (ei is DummyItem)
                    {
                        continue;
                    }
                    var workloc = ei.Work.Current;
                    if (workloc.Equals(location))
                    {
                        if (ei.Type == etype && ei.Work.Is(workclass))
                        {
                            yield return node;
                        }
                    }
                }
            }


            /// <summary>
            /// get first event item
            /// 最初の１件を取得する
            /// </summary>
            /// <remarks>not returns DummyItem</remarks>
            /// <returns></returns>
            public Item Dequeue()
            {
                for (var node = _dat.First; node != null; node = _dat.First)
                {
                    _dat.RemoveFirst();
                    if (node.Value is DummyItem == false)
                    {
                        return node.Value;
                    }
                    else
                    {
                        _sections.Remove(node.Value.DT);
                    }
                }
                return null;
            }

            /// <summary>
            /// event item count in queue buffer
            /// </summary>
            public int Count => _dat.Count - _sections.Count;

            /// <summary>
            /// make dummy event item that is sake for speed finding
            /// 分毎のダミーListNodeを作成する（検索高速化）
            /// </summary>
            /// <param name="dt"></param>
            private void PrepareDummyItems(DateTime dt)
            {
                if (_dat.Count > 0)
                {
                    return;
                }
                for (var t = TimeUtil.ClearSeconds(dt); t < dt + TimeSpan.FromDays(2); t += TimeSpan.FromMinutes(1))
                {
                    var node = _dat.AddLast(new DummyItem   // 検索の速度を上げるための分毎のダミーItem
                    {
                        DT = t,
                        Work = null,
                    });
                    _sections[t] = node;
                }
            }
        }
    }
}
