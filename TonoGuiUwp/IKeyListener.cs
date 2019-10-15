using System;
using System.Collections.Generic;
using System.Text;
using Windows.System;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// feature interface of Key event listener
    /// </summary>
    public interface IKeyListener
    {
        /// <summary>
        /// Key event settings (event trigger catch condition)
        /// </summary>
        IEnumerable<KeyListenSetting> KeyListenSettings { get; }

        /// <summary>
        /// Key event
        /// </summary>
        /// <param name="ks"></param>
        void OnKey(KeyEventToken kt);
    }

    /// <summary>
    /// Key event information
    /// </summary>
    public class KeyEventToken : EventToken
    {
        /// <summary>
        /// Key listener setting
        /// </summary>
        public KeyListenSetting Setting { get; set; }

        /// <summary>
        /// execute action when kls setting is for this object(compare to Setting property)
        /// </summary>
        /// <param name="kls"></param>
        /// <param name="action"></param>
        public void Select(KeyListenSetting kls, Action<KeyListenSetting> action)
        {
            if (kls.Equals(Setting))
            {
                action?.Invoke(Setting);
            }
        }

        /// <summary>
        /// excecute action with Setting property and "additionalCondition" flag
        /// </summary>
        /// <param name="kls"></param>
        /// <param name="additionalCondition"></param>
        /// <param name="action"></param>
        public void Select(KeyListenSetting kls, bool additionalCondition, Action<KeyListenSetting> action)
        {
            if (additionalCondition && kls.Equals(Setting))
            {
                action?.Invoke(Setting);
            }
        }
    }

    /// <summary>
    /// key event catch condition
    /// </summary>
    public class KeyListenSetting
    {
        public enum States
        {
            Down,   // catch event when key down
            Up,     // catch event when key up
        }

        /// <summary>
        /// key event name that you need to filter events
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// event catch condition
        /// </summary>
        public IEnumerable<(VirtualKey key, States state)> KeyStates { get; set; }

        /// <summary>
        /// is on "OnKeyEvent"
        /// </summary>
        public bool IsOn { get; set; } = false;

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var (key, state) in KeyStates)
            {
                sb.Append($"{key}={state} ");
            }
            return $"KeyListenSetting Name:{Name}, IsOn:{IsOn}, KeyStates:{sb}";
        }

        /// <summary>
        /// execute action filtering to compare to name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void SelectName(string name, Action<KeyListenSetting> action)
        {
            if (name.Equals(Name))
            {
                action?.Invoke(this);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is KeyListenSetting ks)
            {
                return ks.Name.Equals(Name);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
