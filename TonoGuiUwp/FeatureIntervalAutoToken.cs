// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Timers;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// interval timer auto throw IntervalEventTokenTrigger
    /// </summary>
    [FeatureDescription(En = "Interval timer to throw token", Jp = "定間隔トークン発行")]
    public class FeatureIntervalAutoToken : FeatureBase
    {
        /// <summary>
        /// Value of interval timer
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// initialize task
        /// </summary>
        /// <returns></returns>
        public override void OnInitialInstance()
        {
            var interval = new Timer
            {
                Interval = Interval.TotalMilliseconds,
            };
            interval.Elapsed += Interval_Elapsed;
            interval.Start();
        }

        private void Interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            Token.AddNew(new IntervalEventTokenTrigger
            {
                Name = Name,
                DT = DateTime.Now,
                TokenID = TokensGeneral.IntervalElapsed,
                Sender = this,
                Remarks = "auto fire fcIntervalAutoToken）",
            });
        }
    }
}
