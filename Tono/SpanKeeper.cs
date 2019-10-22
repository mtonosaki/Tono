// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono
{
    /// <summary>
    /// Checking span utility
    /// </summary>
    /// <example>
    /// var sk = new SpanKeeper
    /// {
    ///		Span = TimeSpan.FromMinutes(20),
    /// };
    /// 
    /// ．．．
    /// sk.
    /// 
    /// </example>
    public class SpanKeeper
    {
        public TimeSpan Span { get; set; }

        private DateTime _dt = new DateTime(2000, 1, 1);

        /// <summary>
        /// Set current time
        /// </summary>
        public void SetNow()
        {
            _dt = DateTime.Now;
        }

        /// <summary>
        /// Reset time(means next Run will be invoked)
        /// </summary>
        public void Reset()
        {
            _dt = new DateTime(2000, 1, 1);
        }

        /// <summary>
        /// Porling method. Use this method frequently
        /// </summary>
        /// <param name="action"></param>
        public void Run(Action action)
        {
            if ((DateTime.Now - _dt) >= Span)
            {
                _dt = DateTime.Now;
                action.Invoke();
            }
        }
    }
}
