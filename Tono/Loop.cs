// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// Loop Utility
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// foreach (var item in LoopUtil<int>.From(data, out var cu))
    /// {
    ///     var index = cu.CurrentIndex;
    ///     cu.DoFirstOneTime(() => { ... });   // Exec one time on the first circle
    ///     cu.DoLastOneTime(() => { ... });    // Exec one time on the last circle
    /// }
    /// </remarks>
    public class LoopUtil<T> : IEnumerable<T>, IEnumerator<T>
    {
        private IEnumerator<T> Enumerator;
        private bool IsFirstExec = false;
        private bool RequestSetFirstExecFlag = false;
        private Action LastAction = null;

        public int CurrentIndex { get; private set; } = -1;
        public T Current => Enumerator.Current;
        object IEnumerator.Current => Enumerator.Current;

        public static IEnumerable<T> From(IEnumerable<T> col, out LoopUtil<T> instance)
        {
            instance = new LoopUtil<T>
            {
                Enumerator = col.GetEnumerator(),
            };
            return instance;
        }

        public void Dispose()
        {
            Enumerator.Dispose();
            CurrentIndex = -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            if (RequestSetFirstExecFlag)
            {
                IsFirstExec = true;
                RequestSetFirstExecFlag = false;
            }
            var ret = Enumerator.MoveNext();
            if (ret)
            {
                CurrentIndex++;
            }
            else
            {
                LastAction?.Invoke();
            }
            return ret;
        }

        public void Reset()
        {
            CurrentIndex = -1;
            IsFirstExec = false;
            RequestSetFirstExecFlag = false;
            LastAction = null;
            Enumerator.Reset();
        }

        /// <summary>
        /// Exec the first 
        /// </summary>
        /// <param name="action"></param>
        /// <returns>true = Execed / false = not the first time</returns>
        public bool DoFirstTime(Action action)
        {
            if (IsFirstExec == false)
            {
                RequestSetFirstExecFlag = true;
                action?.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Exec the second times and the subsequent 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool DoSecondTimesAndSubsequent(Action action)
        {
            if (IsFirstExec)
            {
                action?.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DoLastOneTime(Action action)
        {
            LastAction = action;
        }
    }


    /// <summary>
    /// Loop Utility
    /// </summary>
    public class Loop<TSource>
    {
        private enum ActionTypes
        {
            Break, Skip, Initialize, When, ForEach, Switch, SwitchDefault,
        };
        private IEnumerable<TSource> Data = null;
        private List<(Func<TSource, bool> Condition, ActionTypes ActionType, Action<TSource, int> Action, int No)> Controls = new List<(Func<TSource, bool> Condition, ActionTypes ActionType, Action<TSource, int> Action, int No)>();
        private List<Action<TSource, int>> Finalizes = new List<Action<TSource, int>>();

        public int CurrentIndex { get; set; } = -1;
        private bool IsInitialized { get; set; }

        public static Loop<TSource> Build(IEnumerable<TSource> collection)
        {
            return new Loop<TSource>
            {
                Data = collection,
            };
        }

        public Loop<TSource> Break(Func<TSource, bool> condition)
        {
            Controls.Add((condition, ActionTypes.Break, null, Controls.Count));
            return this;
        }
        public Loop<TSource> Skip(Func<TSource, bool> condition)
        {
            Controls.Add((condition, ActionTypes.Skip, null, Controls.Count));
            return this;
        }
        public Loop<TSource> Initialize(Action<TSource> action)
        {
            Controls.Add(((a => IsInitialized == false), ActionTypes.Initialize, (a, index) =>
            {
                action?.Invoke(a);
                IsInitialized = true;
            }, Controls.Count));
            return this;
        }
        public Loop<TSource> Initialize(Action<TSource, int> action)
        {
            Controls.Add(((a => IsInitialized == false), ActionTypes.Initialize, (a, index) =>
            {
                action?.Invoke(a, index);
                IsInitialized = true;
            }, Controls.Count));
            return this;
        }
        public Loop<TSource> When(Func<TSource, bool> condition, Action<TSource> action)
        {
            Controls.Add((condition, ActionTypes.When, ((a, index) => action?.Invoke(a)), Controls.Count));
            return this;
        }
        public Loop<TSource> When(Func<TSource, bool> condition, Action<TSource, int> action)
        {
            Controls.Add((condition, ActionTypes.When, action, Controls.Count));
            return this;
        }
        public Loop<TSource> ForEach(Action<TSource> action)
        {
            Controls.Add(((a => true), ActionTypes.ForEach, ((a, index) => action?.Invoke(a)), Controls.Count));
            return this;
        }
        public Loop<TSource> ForEach(Action<TSource, int> action)
        {
            Controls.Add(((a => true), ActionTypes.ForEach, action, Controls.Count));
            return this;
        }

        public Loop<TSource> Switch(Func<TSource, bool> condition, Action<TSource> action)
        {
            Controls.Add((condition, ActionTypes.Switch, ((a, index) => action?.Invoke(a)), Controls.Count));
            return this;
        }
        public Loop<TSource> Switch(Func<TSource, bool> condition, Action<TSource, int> action)
        {
            Controls.Add((condition, ActionTypes.Switch, action, Controls.Count));
            return this;
        }

        public Loop<TSource> SwitchDefault(Action<TSource> action)
        {
            Controls.Add(((a => true), ActionTypes.SwitchDefault, ((a, index) => action?.Invoke(a)), Controls.Count));
            return this;
        }
        public Loop<TSource> SwitchDefault(Action<TSource, int> action)
        {
            Controls.Add(((a => true), ActionTypes.SwitchDefault, action, CurrentIndex));
            return this;
        }

        public Loop<TSource> Finalize(Action<TSource> action)
        {
            Finalizes.Add((a, index) => action?.Invoke(a));
            return this;
        }
        public Loop<TSource> Finalize(Action<TSource, int> action)
        {
            Finalizes.Add(action);
            return this;
        }

        public Loop<TSource> Start()
        {
            if (Data == null) return this;

            CurrentIndex = -1;
            var isBreakeRequested = false;
            IsInitialized = false;

            Controls.Sort((a, b) =>
            {
                var r1 = (int)a.ActionType - (int)b.ActionType;
                if (r1 == 0)
                {
                    return a.No - b.No;
                }
                else
                {
                    return r1;
                }
            });
            TSource lastitem = default;
            int lastIndex = -1;
            foreach (var item in Data)
            {
                CurrentIndex++;
                var isSwitched = false;
                foreach (var cnt in Controls)
                {
                    if (cnt.Condition.Invoke(item))
                    {
                        if (cnt.ActionType == ActionTypes.Break)
                        {
                            isBreakeRequested = true;
                            break;
                        }
                        else
                        if (cnt.ActionType == ActionTypes.Skip)
                        {
                            break;
                        }
                        else
                        if (cnt.ActionType == ActionTypes.Switch)
                        {
                            isSwitched = true;
                        }
                        else
                        if (cnt.ActionType == ActionTypes.SwitchDefault && isSwitched)
                        {
                            break;  // Not execute cnt.Action
                        }

                        cnt.Action?.Invoke(item, CurrentIndex);
                        lastitem = item;
                        lastIndex = CurrentIndex;
                    }
                }
                if (isBreakeRequested)
                {
                    break;
                }
            }
            foreach (var cnt in Finalizes)
            {
                cnt.Invoke(lastitem, lastIndex);
            }
            return this;
        }
    }
}
