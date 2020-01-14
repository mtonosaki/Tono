// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Keyboard event handling
    /// </summary>
    public partial class TGuiView
    {
        private void InitKeyboard()
        {
            var win = Window.Current.CoreWindow;
            win.KeyDown += OnKeyDown;
            win.KeyUp += OnKeyUp;

            IntervalUtil.Start(TimeSpan.FromMilliseconds(2347), () =>
            {
                // make virtual key up event when out of window focus (and key was released)
                foreach (var key in _keys.Where(kv => kv.Value).Select(kv => kv.Key).ToArray())
                {
                    var ks = Window.Current.CoreWindow.GetKeyState(key);
                    if ((ks & CoreVirtualKeyStates.Down) == 0)    // no longer press
                    {
                        KeyUpProc(key);
                    }
                }
            });
        }

        private readonly Dictionary<VirtualKey, List<IKeyListener>> _keyliss = new Dictionary<VirtualKey, List<IKeyListener>>();

        private void InitKeyboard(FeatureBase _, IKeyListener keylistener)
        {
            foreach (var kls in keylistener.KeyListenSettings)
            {
                foreach (var (key, state) in kls.KeyStates)
                {
                    if (_keyliss.TryGetValue(key, out var tarkl) == false)
                    {
                        _keyliss[key] = tarkl = new List<IKeyListener>();
                    }
                    if (tarkl.Contains(keylistener) == false)
                    {
                        tarkl.Add(keylistener);
                    }
                }
            }
        }

        /// <summary>
        /// key state
        /// </summary>
        private static readonly Dictionary<VirtualKey, bool> _keys = new Dictionary<VirtualKey, bool>();

        /// <summary>
        /// check is key down condition
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool IsKeyDown(VirtualKey key)
        {
            if (_keys.TryGetValue(key, out var sw))
            {
                return sw;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// return current key state
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private KeyListenSetting.States GetKeyState(VirtualKey key)
        {
            return IsKeyDown(key) ? KeyListenSetting.States.Down : KeyListenSetting.States.Up;
        }

        /// <summary>
        /// check on keystate
        /// </summary>
        /// <param name="keystates"></param>
        /// <returns></returns>
        private bool CheckKeys(IEnumerable<(VirtualKey key, KeyListenSetting.States state)> keystates)
        {
            foreach (var (key, state) in keystates)
            {
                if (GetKeyState(key) != state)
                {
                    return false;
                }
            }
            return true;
        }

        private readonly Dictionary<IKeyListener, EventCatchAttribute> onkeyupAttributes = new Dictionary<IKeyListener, EventCatchAttribute>(); // Attributeのキャッシュ

        /// <summary>
        /// condition change process
        /// </summary>
        /// <param name="key"></param>
        private void OnKeyProc(VirtualKey key, KeyListenSetting.States _)
        {
            if (_keyliss.TryGetValue(key, out var listeners))
            {
                var klss =
                    from kl in listeners
                    from kls in kl.KeyListenSettings
                    where kls.IsOn == true
                    where CheckKeys(kls.KeyStates) == false
                    select kls;
                foreach (var kls in klss)
                {
                    kls.IsOn = false;
                }

                var kllist =
                    from kl in listeners                // listening features that are waiting key state changed
                    from kls in kl.KeyListenSettings    // 1 feature can wait multi key event
                    where kls.IsOn == false
                    where CheckKeys(kls.KeyStates) == true
                    select (kl, kls);
                foreach (var (kl, kls) in kllist)
                {
                    if (onkeyupAttributes.TryGetValue(kl, out var attr) == false)
                    {
                        var mi = kl.GetType().GetMethod("OnKey");
                        var cas = mi.GetCustomAttributes(typeof(EventCatchAttribute), true);
                        onkeyupAttributes[kl] = attr = (EventCatchAttribute)cas.FirstOrDefault();
                    }
                    if (attr?.IsStatusFilter() ?? false)
                    {
                        if (attr.CheckStatus(this) == false)
                        {
                            continue;
                        }
                    }

                    kls.IsOn = true;
                    kl.OnKey(new KeyEventToken
                    {
                        Setting = kls,
                        Sender = this,
                        Previous = null,
                        Remarks = "TGui.onKeyProc",
                    });
                }
            }
        }


        /// <summary>
        /// key down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            //Debug.WriteLine($"  --- onKeyDown: {args.VirtualKey}");
            var pre = IsKeyDown(args.VirtualKey);
            _keys[args.VirtualKey] = true;
            if (pre == false)   // 変化時のみイベント発行
            {
                OnKeyProc(args.VirtualKey, KeyListenSetting.States.Down);
            }
        }
        /// <summary>
        /// key up event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            KeyUpProc(args.VirtualKey);
        }

        private void KeyUpProc(VirtualKey key)
        {
            //Debug.WriteLine($"  --- onKeyUp  : {key}");
            var pre = IsKeyDown(key);
            _keys[key] = false;
            if (pre == true)    // do when condition changed
            {
                OnKeyProc(key, KeyListenSetting.States.Up);
            }
        }
    }
}
