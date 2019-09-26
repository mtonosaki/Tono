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
        private void initKeyboard()
        {
            var win = Window.Current.CoreWindow;
            win.KeyDown += onKeyDown;
            win.KeyUp += onKeyUp;

            IntervalUtil.Start(TimeSpan.FromMilliseconds(2347), () =>
            {
                // make virtual key up event when out of window focus (and key was released)
                foreach (var key in _keys.Where(kv => kv.Value).Select(kv => kv.Key).ToArray())
                {
                    var ks = Window.Current.CoreWindow.GetKeyState(key);
                    if ((ks & CoreVirtualKeyStates.Down) == 0)    // no longer press
                    {
                        keyupProc(key);
                    }
                }
            });
        }

        private readonly Dictionary<VirtualKey, List<IKeyListener>> _keyliss = new Dictionary<VirtualKey, List<IKeyListener>>();

        private void initKeyboard(FeatureBase feature, IKeyListener keylistener)
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
        private bool isKeyDown(VirtualKey key)
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
        private KeyListenSetting.States getKeyState(VirtualKey key)
        {
            return isKeyDown(key) ? KeyListenSetting.States.Down : KeyListenSetting.States.Up;
        }

        /// <summary>
        /// check on keystate
        /// </summary>
        /// <param name="keystates"></param>
        /// <returns></returns>
        private bool checkKeys((VirtualKey key, KeyListenSetting.States state)[] keystates)
        {
            foreach (var (key, state) in keystates)
            {
                if (getKeyState(key) != state)
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
        /// <param name="state"></param>
        private void onKeyProc(VirtualKey key, KeyListenSetting.States state)
        {
            if (_keyliss.TryGetValue(key, out var listeners))
            {
                var klss =
                    from kl in listeners
                    from kls in kl.KeyListenSettings
                    where kls.IsOn == true
                    where checkKeys(kls.KeyStates) == false
                    select kls;
                foreach (var kls in klss)
                {
                    kls.IsOn = false;
                }

                var kllist =
                    from kl in listeners                // listening features that are waiting key state changed
                    from kls in kl.KeyListenSettings    // 1 feature can wait multi key event
                    where kls.IsOn == false
                    where checkKeys(kls.KeyStates) == true
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
        private void onKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            //Debug.WriteLine($"  --- onKeyDown: {args.VirtualKey}");
            var pre = isKeyDown(args.VirtualKey);
            _keys[args.VirtualKey] = true;
            if (pre == false)   // 変化時のみイベント発行
            {
                onKeyProc(args.VirtualKey, KeyListenSetting.States.Down);
            }
        }
        /// <summary>
        /// key up event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void onKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            keyupProc(args.VirtualKey);
        }

        private void keyupProc(VirtualKey key)
        {
            //Debug.WriteLine($"  --- onKeyUp  : {key}");
            var pre = isKeyDown(key);
            _keys[key] = false;
            if (pre == true)    // do when condition changed
            {
                onKeyProc(key, KeyListenSetting.States.Up);
            }
        }
    }
}
