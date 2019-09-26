using System;
using System.Collections.Generic;
using Windows.UI;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Log message panel control feature
    /// Tono.Gui Concept : No dialog window is better
    /// </summary>
    /// <example>
    ///    ＜tw:FeatureLogPanel Name = "MainLogPanel" TargetPane="LogPanel" BackgroundColor1="#00000000" BackgroundColor2="#cc000000" MessageColorDev="#FF444488" MessageColorErr="#FFff0000" MessageColorInf="#FF777777" MessageColorWar="#FFaaaa00" IsEnabled="True"＞
    ///        ＜tw:FeatureLogPanel.LogParts＞
    ///            ＜local:PartsLogCustom /＞     custom parts design example
    ///        ＜/tw:FeatureLogPanel.LogParts＞
    ///    ＜/tw:FeatureLogPanel>
    /// </example>
    [FeatureDescription(En = "Log panel", Jp = "ログパネル")]
    public class FeatureLogPanel : FeatureBase, IAutoRemovable, IPointerListener
    {
        /// <summary>
        /// target pane
        /// </summary>
        public string TargetPane { get; set; }

        /// <summary>
        /// panel color at left-top position
        /// </summary>
        public Color BackgroundColor1 { get; set; } = Color.FromArgb(224, 96, 64, 48);

        /// <summary>
        /// panel color at right-bottom position
        /// </summary>
        public Color BackgroundColor2 { get; set; } = Color.FromArgb(128, 48, 48, 0);

        /// <summary>
        /// message color of Dev level
        /// </summary>
        public Color MessageColorDev { get; set; } = Color.FromArgb(255, 128, 128, 160);

        /// <summary>
        /// message color of Inf level
        /// </summary>
        public Color MessageColorInf { get; set; } = Color.FromArgb(255, 126, 255, 58);

        /// <summary>
        /// message color of War level
        /// </summary>
        public Color MessageColorWar { get; set; } = Color.FromArgb(255, 227, 227, 227);

        /// <summary>
        /// message color of Err level
        /// </summary>
        public Color MessageColorErr { get; set; } = Color.FromArgb(255, 255, 255, 0);

        /// <summary>
        /// log panel design parts (you can set another object to customize as you like)
        /// </summary>
        public PartsLog LogParts { get; set; } = new PartsLog();

        /// <summary>
        /// visible flag
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// max log record count for memory save
        /// </summary>
        public int MaxLogCount { get => LOG.MaxLogCount; set => LOG.MaxLogCount = value; }


        /// <summary>
        /// initial feature
        /// </summary>
        public override void OnInitialInstance()
        {
            Pane.Target = Pane[TargetPane];
            LogParts.SetParent(this);
            LogParts.SetVisible(LLV.ERR, ConfigUtil.Get($"LLV.ERR", "1") == "1" ? true : false);
            LogParts.SetVisible(LLV.WAR, ConfigUtil.Get($"LLV.WAR", "1") == "1" ? true : false);
            LogParts.SetVisible(LLV.INF, ConfigUtil.Get($"LLV.INF", "1") == "1" ? true : false);
            LogParts.SetVisible(LLV.DEV, ConfigUtil.Get($"LLV.DEV", "1") == "1" ? true : false);
            Parts.Add(Pane.Target, LogParts, Layers.LogPanel);

            TS(() =>
            {
                LOG.Queue.LogAdded += onLogAdded;

                IntervalUtil.Start(TimeSpan.FromMilliseconds(789), () =>
                {
                    if (_isLogAdded)
                    {
                        Redraw();
                        _isLogAdded = false;
                    }
                });
            });
        }

        [EventCatch(TokenID = TokensGeneral.LogPanelSwitchON)]
        public void LogPanelSwitchON(EventTokenTrigger t)
        {
            setSwitch(true);
        }

        [EventCatch(TokenID = TokensGeneral.LogPanelSwitchOFF)]
        public void LogPanelSwitchOFF(EventTokenTrigger t)
        {
            setSwitch(false);
        }

        /// <summary>
        /// set visible flag
        /// </summary>
        /// <param name="sw"></param>
        private void setSwitch(bool sw)
        {
            IsVisible = sw;
            Redraw();
        }

        private bool _isLogAdded = false;

        private void onLogAdded(object sender, EventArgs e)
        {
            _isLogAdded = true;
        }

        public void OnPointerMoved(PointerState po)
        {
        }

        private readonly Dictionary<LLV, bool> _pres = new Dictionary<LLV, bool>();

        public void OnPointerPressed(PointerState po)
        {
            ChangeSw(po);
        }

        public void OnPointerHold(PointerState po)
        {
        }

        public void OnPointerReleased(PointerState po)
        {
        }

        private bool ChangeSw(PointerState po)
        {
            var dmin = double.PositiveInfinity;
            var lvmin = LLV.DEV;
            foreach ((var lv, var br) in LogParts.GetButtonAreas())
            {
                var d = GeoEu.Length(br.C, po.Position);
                if (d < dmin)
                {
                    dmin = d;
                    lvmin = lv;
                }
            }
            if (dmin < 10)
            {
                var sw = !LogParts.GetVisible(lvmin);
                LogParts.SetVisible(lvmin, sw);
                ConfigUtil.Set($"LLV.{lvmin}", sw ? "1" : "0");
                Redraw();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
