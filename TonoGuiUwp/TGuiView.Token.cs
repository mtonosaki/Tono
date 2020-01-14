// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// event token handling
    /// </summary>
    public partial class TGuiView
    {
        /// <summary>
        /// fired when token tray become empty
        /// </summary>
        public event EventHandler TokenLoop;

        /// <summary>
        /// Create Porling Process
        /// </summary>
        private void initToken()
        {
            // start event porling
            IntervalUtil.Start(TimeSpan.FromMilliseconds(5), () =>
            {
                killproc(); // remove kill requested features

                if (_tokenQueue.Count > 0)
                {
                    var sw = Stopwatch.StartNew();
                    while (TokenEventProc() > 0)
                    {
                        killproc(); // remove kill requested features

                        if (sw.ElapsedMilliseconds > 20000)
                        {
                            LOG.AddException(new TimeoutException("Token Loop timeout (It is possible infinity token loop)"));
                            break;
                        }
                    }
                    // Exec Finalizer
                    var ics = finalizeActions.ToArray();
                    finalizeActions.Clear();
                    foreach (var act in ics)
                    {
                        act.Invoke();
                    }
                    // all tokens are execed here. Fire the TokenLoop Event
                    TokenLoop?.Invoke(this, EventArgs.Empty);
                }
                CheckInvalidateReqested();
            });
        }

        private readonly Queue<EventToken> _tokenQueue = new Queue<EventToken>();

        /// <summary>
        /// add token
        /// </summary>
        /// <param name="token"></param>
        public void AddToken(EventToken token)
        {
            lock (_tokenQueue)
            {
                _tokenQueue.Enqueue(token);
            }
        }

        private readonly List<Action> finalizeActions = new List<Action>();

        /// <summary>
        /// register finalize action that will be called last token processed
        /// </summary>
        /// <param name="act"></param>
        public void AddFinalizeAction(Action act)
        {
            finalizeActions.Add(act);
        }

        /// <summary>
        /// token listener cache
        /// </summary>
        private readonly Dictionary<FeatureBase, List<(EventCatchAttribute Attr, MethodInfo Mi, ParameterInfo Pi)>> tlcache = new Dictionary<FeatureBase, List<(EventCatchAttribute Attr, MethodInfo Mi, ParameterInfo Pi)>>();

        /// <summary>
        /// token event implement
        /// </summary>
        private int TokenEventProc()
        {
            var execCount = 0;

            try
            {
                EventToken[] tokensNow;
                lock (_tokenQueue)
                {
                    tokensNow = _tokenQueue.ToArray();
                    _tokenQueue.Clear();
                }
                foreach (var token in tokensNow)
                {
                    foreach (var fc in getFeatures().Where(f => f.IsEnabled))
                    {
                        if (tlcache.TryGetValue(fc, out var ams))
                        {
                            foreach (var (Attr, Mi, Pi) in ams)
                            {
                                execCount = CheckAndExec(execCount, token, fc, Attr, Mi, Pi);
                            }
                        }
                        else
                        {
                            tlcache[fc] = ams = new List<(EventCatchAttribute Attr, MethodInfo Mi, ParameterInfo Pi)>();
                            foreach (var mi in fc.GetType().GetMethods())
                            {
                                foreach (var attr in mi.GetCustomAttributes(true).Where(obj => obj is EventCatchAttribute).Select(a => (EventCatchAttribute)a))
                                {
                                    var pi = mi.GetParameters().FirstOrDefault();
                                    if (pi != null)
                                    {
                                        ams.Add((attr, mi, pi));
                                        execCount = CheckAndExec(execCount, token, fc, attr, mi, pi);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.AddException(ex);
            }
            return execCount;
        }

        /// <summary>
        /// execute feature method when condition match
        /// </summary>
        /// <param name="nMatch">current number of execute</param>
        /// <param name="token">target token</param>
        /// <param name="fc">target feature</param>
        /// <param name="attr">token cache attribute</param>
        /// <param name="mi">execute method</param>
        /// <param name="pi">execute parameter</param>
        /// <returns>nMatch or nMatch+1 to know cycle finish</returns>
        private int CheckAndExec(int nMatch, EventToken token, FeatureBase fc, EventCatchAttribute attr, MethodInfo mi, ParameterInfo pi)
        {
            bool isID = false, isName = false, isButtonName = false, isType = false, isTypeInhilited = false;
            var isMatch = false;
            var tokentype = token.GetType();
            isType = tokentype.Equals(pi.ParameterType);
            isTypeInhilited = tokentype.IsSubclassOf(pi.ParameterType);
            if (token is ITokenIDKey tt)
            {
                isID = tt.TokenID.Equals(attr.TokenID);
            }
            if (token is ITokenNameKey tn)
            {
                isName = tn.Name.Equals(attr.Name);
            }
            if (token is EventTokenButton tb)
            {
                if (attr.Name == null)
                {
                    isButtonName = fc.ListeningButtonNames.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(tb.Name);
                    isMatch |= (isButtonName && (isType || isTypeInhilited));
                }
                else
                {
                    isMatch = isName && (isType | isTypeInhilited);
                }
            }
            else
            {
                if (token is ITokenIDKey && token is ITokenNameKey)
                {
                    if (isID && isName || isID && attr.Name == null || isName && attr.TokenID == null)
                    {
                        isMatch |= (isType || isTypeInhilited);
                    }
                }
                else
                {
                    isMatch |= (isID && (isType || isTypeInhilited));    // same ID
                    isMatch |= (isName && (isType || isTypeInhilited));  // same Name
                    isMatch |= (attr.TokenID == null && attr.Name == null && (isType || isTypeInhilited));
                }
            }

            if (isMatch && attr.IsStatusFilter())
            {
                isMatch = attr.CheckStatus(this);
            }

            if (isMatch)
            {
                nMatch++;
                var sw = Stopwatch.StartNew();
                mi.Invoke(fc, new[] { token });
                sw.Stop();
                if (sw.Elapsed.TotalMilliseconds > 200)
                {
                    LOG.WriteLine(LLV.DEV, $"HEAVY FEATURE {sw.Elapsed.TotalMilliseconds:#,##0}[ms] : {fc.GetType().Name} ID={fc.ID.Value}");
                }
            }

            return nMatch;
        }

        private void CheckInvalidateReqested()
        {
            if (_isDrawing == false && DataHot.IsRedrawRequested == true)
            {
                DataHot.IsRedrawRequested = false;
                Canvas.Invalidate();
            }
        }

        /// <summary>
        /// add an urgent token
        /// </summary>
        /// <param name="token"></param>
        public void AddUrgentToken(EventTokenTrigger token)
        {
            var fcs =
                from fc in getFeatures()
                where fc is IUrgentTokenListener
                select fc;
            foreach (IUrgentTokenListener fc in fcs)
            {
                fc.UrgentStart(token);
            }
        }
    }
}
