// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// TokenID interface
    /// </summary>
    internal interface ITokenIDKey
    {
        string TokenID { get; set; }
    }

    /// <summary>
    /// Token Name interface
    /// </summary>
    internal interface ITokenNameKey
    {
        string Name { get; set; }
    }

    /// <summary>
    /// general event token trigger to file feature
    /// </summary>
    public class EventTokenTrigger : EventToken, ITokenIDKey
    {
        /// <summary>
        /// TokenID for selecting token
        /// </summary>
        public virtual string TokenID { get; set; }

        /// <summary>
        /// execute action when token id is same.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="action"></param>
        public static void Select(EventToken token, string tokenID, Action<EventTokenTrigger> action)
        {
            if (token is EventTokenTrigger tt && tt.TokenID.Equals(tokenID))
            {
                action(tt);
            }
        }

        /// <summary>
        /// execute action when token id is same (to see multi token id)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenIDs"></param>
        /// <param name="func"></param>
        public static void Select(EventToken token, string[] tokenIDs, Action<EventTokenTrigger> func)
        {
            foreach (var tokenID in tokenIDs)
            {
                Select(token, tokenID, func);
            }
        }
    }


    /// <summary>
    /// UWP control event token trigger
    /// </summary>
    public class ControlEventTokenTrigger : EventTokenTrigger, ITokenNameKey
    {
        /// <summary>
        /// Control name (for thread safe. TGuiView does not need to access main thread control)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// checked flag
        /// </summary>
        public bool? IsChecked { get; set; }

        /// <summary>
        /// execute action when control name is equal to the specified one.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="name">UWP control name</param>
        /// <param name="action"></param>
        public static void Select(EventToken token, string name, Action<ControlEventTokenTrigger> action)
        {
            if (token is ControlEventTokenTrigger tt)
            {
                if (tt.Name.Equals(name))
                {
                    action.Invoke(tt);
                }
            }
        }
    }

    /// <summary>
    /// part move event token
    /// </summary>
    public class EventTokenPartsMovedTrigger : EventTokenTrigger
    {
        public IEnumerable<IMovableParts> Parts { get; set; }

        public static void Select(EventToken token, Action<EventTokenPartsMovedTrigger> func)
        {
            if (token is EventTokenPartsMovedTrigger tt)
            {
                func(tt);
            }
        }
        public static void Select(EventToken token, string tokenID, Action<EventTokenPartsMovedTrigger> func)
        {
            if (token is EventTokenPartsMovedTrigger tt)
            {
                if (tt.TokenID.Equals(tokenID))
                {
                    func(tt);
                }
            }
        }
    }

    /// <summary>
    /// parts select event token
    /// </summary>
    public class EventTokenPartsSelectChangedTrigger : EventTokenTrigger
    {
        public (ISelectableParts parts, bool sw)[] PartStates { get; set; }

        public static void Select(EventToken token, Action<EventTokenPartsSelectChangedTrigger> func)
        {
            if (token is EventTokenPartsSelectChangedTrigger tt)
            {
                func(tt);
            }
        }
        public static void Select(EventToken token, string tokenID, Action<EventTokenPartsSelectChangedTrigger> func)
        {
            if (token is EventTokenPartsSelectChangedTrigger tt)
            {
                if (tt.TokenID.Equals(tokenID))
                {
                    func(tt);
                }
            }
        }
    }


    /// <summary>
    /// Pane property changed event token
    /// </summary>
    public class EventTokenPaneChanged : EventTokenTrigger
    {
        public IDrawArea TargetPane { get; set; }
    }

    /// <summary>
    /// window size changed event token
    /// </summary>
    public class SizeChangedEventTokenTrigger : EventTokenTrigger
    {
        /// <summary>
        /// new window size
        /// </summary>
        public ScreenSize WindowSize { get; set; }

        /// <summary>
        /// execute action when token type is this class
        /// </summary>
        /// <param name="token"></param>
        /// <param name="action"></param>
        public static void Select(EventToken token, Action<SizeChangedEventTokenTrigger> action)
        {
            if (token is SizeChangedEventTokenTrigger tt)
            {
                action(tt);
            }
        }
    }

    /// <summary>
    /// interval time event token trigger
    /// </summary>
    public class IntervalEventTokenTrigger : EventTokenTrigger, ITokenNameKey
    {
        /// <summary>
        /// timer name
        /// </summary>
        public string Name { get; set; } = "default";

        /// <summary>
        /// time fire date time
        /// </summary>
        public DateTime DT { get; set; }

        /// <summary>
        /// time span from set to now
        /// </summary>
        public TimeSpan Span => DateTime.Now - DT;

    }

    /// <summary>
    /// event token that is wrapped DispatchTimer object
    /// </summary>
    public class EventTokenDispatchTimerWrapper : EventToken
    {
        public DateTime Time { get; set; }
        public TimeSpan Interval { get; set; }

        public static EventTokenDispatchTimerWrapper From(DispatcherTimer timer, object sender, string remarks)
        {
            return new EventTokenDispatchTimerWrapper
            {
                Time = DateTime.Now,
                Interval = timer.Interval,
                Sender = sender,
                Remarks = remarks,
            };
        }
    }


    /// <summary>
    /// tap event token
    /// </summary>
    public class EventTokenButton : EventToken, ITokenNameKey
    {
        public string Name { get; set; }
        public object Content { get; set; }
        public TappedRoutedEventArgs TappedRoutedEventArgs { get; set; }

        public KeyboardAcceleratorInvokedEventArgs KeyboardAcceleratorInvokedEventArgs { get; set; }

        public static void Select(EventToken token, string name, Action<EventTokenButton> func)
        {
            if (token is EventTokenButton tt && tt.Name.Equals(name))
            {
                func(tt);
            }
        }
    }
}
