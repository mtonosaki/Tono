// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// pointer event handling
    /// </summary>
    public partial class TGuiView
    {
        private void InitPointer()
        {
            var win = Window.Current.CoreWindow;
            win.PointerMoved += OnPointerMoved;
            win.PointerPressed += OnPointerPressed;
            win.PointerReleased += OnPointerReleased;
            win.PointerWheelChanged += OnPointerWheelChanged;

            ManipulationStarted += OnManipulationStarted;
            ManipulationMode = Windows.UI.Xaml.Input.ManipulationModes.All;
            ManipulationStarting += OnManipulationStarting;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            ManipulationInertiaStarting += OnManipulationInertiaStarting;
            Holding += OnHolding;
            IsHoldingEnabled = true;
            //Canvas.Holding += OnHolding;
            //Canvas.IsHoldingEnabled = true;
        }

#pragma warning disable CS0414, IDE0052
        private bool IsOnPointerPressed;
        private bool IsOnPointerPressFirered;
        private bool IsOnPointerMoved;
        private bool IsOnPointerReleased;
        private bool IsOnPointerWheelChanged;
        private bool IsOnManipulationStarting;
        private bool IsOnManipulationStarted;
        private bool IsOnManipulationInertiaStarting;
        private bool IsOnManipulationCompleted;
        private bool IsHolding;
        private bool IsOnManipulationDelta;
#pragma warning restore CS0414, IDE0052

        private Dictionary<FeatureBase, Dictionary<string, EventCatchAttribute>> attrBuf = null;
        private PointerState PositionBak = null;    // Position
        private PointerState KeyBak = new PointerState(); // IsInContact, FingerCount and IsKey...
        private int PrePressFiredFingerCount = 0;
        private ScreenPos OriginPoint = ScreenPos.Zero;
        private DispatcherTimer PressTimer = null;

        private void Reset()
        {
            IsOnManipulationStarting = false;
            IsOnManipulationStarted = false;
            IsOnManipulationInertiaStarting = false;
            IsOnManipulationDelta = false;
            IsOnManipulationCompleted = false;
            IsOnPointerPressed = false;
            IsOnPointerPressFirered = false;
            IsOnPointerMoved = false;
            IsOnPointerReleased = false;
            IsOnPointerWheelChanged = false;
            IsHolding = false;
            PrePressFiredFingerCount = 0;
            OriginPoint = ScreenPos.Zero;
            PositionBak = null;
            KeyBak = new PointerState();
            PressTimer?.Stop();
            PressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(5),
            };
            PressTimer.Tick += PressTimer_Tick;
        }

        private void OnHolding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            // TODO: Not Implemented 'OnHolding' function yet.
            IsHolding = e.HoldingState == Windows.UI.Input.HoldingState.Started;
            var po = _(e, this, "onHolding");
            po.Position = PositionBak.Position;
            CopyKeyState(po);
            po.FingerCount = KeyBak.FingerCount;
            //Debug.WriteLine($"onHolding Finger={po.FingerCount} {po.Position}");

            KickPointerEvent(null, fc =>
            {
                if (IsHolding)
                {
                    if (CheckEventCatchStatus((FeatureBase)fc, "OnPointerHold"))
                    {
                        fc.OnPointerHold(po);
                    }
                }
                else
                {
                    if (CheckEventCatchStatus((FeatureBase)fc, "OnPointerReleased"))
                    {
                        fc.OnPointerReleased(po);
                    }
                }
            });
        }

        private void OnPointerPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            IsOnPointerPressed = true;
            var po = _(e, this, "onPointerPressed");
            CopyKeyState(from: po, to: KeyBak); // Copy latest keystate to KeyBak
            KeyBak.FingerCount++;
            if (IsOnManipulationStarted == false) // not override position because of multi finger swiping
            {
                PositionBak = po;
                OriginPoint = PositionBak.Position.Clone();
                PositionBak.PositionOrigin = OriginPoint;
            }
            //Debug.WriteLine($"onPointerPressed Finger={PointBak.FingerCount} {PointBak.Position}");
            PressTimer.Stop();
            PressTimer.Start(); // Reset Interval Timer
        }

        private void PressTimer_Tick(object sender, object e)
        {
            if (PositionBak == null)
            {
                PressTimer.Stop();  // Poka yoke
                return;
            }
            var po = PositionBak.Clone();
            CopyKeyState(from: null, to: po);
            po.FingerCount = KeyBak.FingerCount;
            po.PositionOrigin = OriginPoint;

            if (IsOnPointerPressFirered == false)
            {
                PressTimer.Stop();
                IsOnPointerPressFirered = true;
                KickPointerEvent("OnPointerPressed", fc => fc.OnPointerPressed(po));
                PrePressFiredFingerCount = PositionBak.FingerCount;
            }
            else
            {
                for (var finger = PrePressFiredFingerCount + 1; finger <= PositionBak.FingerCount; finger++)
                {
                    po.FingerCount = finger;
                    KickPointerEvent("OnPointerPressed", fc => fc.OnPointerPressed(po.Clone()));
                }
                PrePressFiredFingerCount = PositionBak.FingerCount;
            }
        }


        private void OnPointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            if (IsOnManipulationStarting && IsOnPointerPressed == false)
            {
                return; // waiting pressed timer
            }
            IsOnPointerMoved = true;
            PositionBak = _(e, this, "onPointerMoved");
            PositionBak.PositionOrigin = OriginPoint;
            CopyKeyState(from: PositionBak, to: KeyBak);    // Copy latest keystate to KeyBak

            // expecting mouse move (not for drag)
            if (IsOnPointerPressed == false)
            {
                var po = PositionBak.Clone();
                po.FingerCount = KeyBak.FingerCount;
                po.PositionOrigin = OriginPoint;
                KickPointerEvent("OnPointerMoved", fc => fc.OnPointerMoved(po));
            }
            //Debug.WriteLine($"onPointerMoved Finger={PointBak.FingerCount} {PointBak.Position}");
        }

        private void OnPointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            IsOnPointerReleased = true;
            KeyBak.FingerCount = Math.Max(KeyBak.FingerCount - 1, 0);
            var po = _(e, this, "onPointerReleased");
            CopyKeyState(from: po, to:KeyBak);  // Copy latest key state to KeyBak

            // expecting 1-finger-tap, Mouse Click, Double Click.  (not for drag, swipe. see also onManipulationCompleted)
            if (IsOnManipulationCompleted == false && KeyBak.FingerCount == 0 && IsOnManipulationInertiaStarting == false)
            {
                po.PositionOrigin = OriginPoint;
                po.FingerCount = KeyBak.FingerCount;
                KickPointerEvent("OnPointerReleased", fc => fc.OnPointerReleased(po));
            }
        }

        private void OnPointerWheelChanged(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            IsOnPointerWheelChanged = true;
            var po = _(e, this, "onPointerWheelChanged");
            CopyKeyState(from: po, to: KeyBak); // Save Latest key state
            po.FingerCount = po.FingerCount;
            po.PositionOrigin = OriginPoint;
            KickWheelEvent("OnMouseWheelChanged", fc => fc.OnMouseWheelChanged(po));

            //Debug.WriteLine($"onPointerWheelChanged {po.WheelDelta}");
        }

        private void OnManipulationStarting(object sender, Windows.UI.Xaml.Input.ManipulationStartingRoutedEventArgs e)
        {
            Reset();
            IsOnManipulationStarting = true;

            //Debug.WriteLine($"onManipulationStarting Mode={e.Mode} / {e.Pivot}");
        }

        private void OnManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            IsOnManipulationStarted = true;
            PositionBak = _(e, this, "onManipulationStarted");
            PositionBak.PositionOrigin = OriginPoint;
            PressTimer.Stop();
            PressTimer.Start(); // Reset Interval Timer
            //Debug.WriteLine($"onManipulationStarted {PointBak.Position} Finger={PointBak.FingerCount}");
        }

        private void OnManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            IsOnManipulationDelta = true;
            var po = _(e, this, "onManipulationDelta");
            CopyKeyState(po);
            po.PositionOrigin = OriginPoint;
            po.FingerCount = KeyBak.FingerCount;
            KickPointerEvent("OnPointerMoved", fc => fc.OnPointerMoved(po));

            //Debug.WriteLine($"onManipulationDelta {po.Position} Finger={po.FingerCount} Scale={po.Scale}");
        }

        private void OnManipulationInertiaStarting(object sender, Windows.UI.Xaml.Input.ManipulationInertiaStartingRoutedEventArgs e)
        {
            IsOnManipulationInertiaStarting = true;
            //Debug.WriteLine($"onManipulationInertiaStarting {e.ExpansionBehavior}");
        }

        private void OnManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            //Debug.WriteLine($"onManipulationCompleted");
            var po = _(e, this, "onManipulationCompleted");
            CopyKeyState(po);
            po.FingerCount = 0;
            po.PositionOrigin = OriginPoint;
            KickPointerEvent("OnPointerReleased", fc => fc.OnPointerReleased(po));
            Reset();
            IsOnManipulationCompleted = true;
        }

        private void CopyKeyState(PointerState to, PointerState from = null )
        {
            if( from == null)
            {
                from = KeyBak;
            }
            to.IsInContact = from.IsInContact;
            to.IsKeyControl = from.IsKeyControl;
            to.IsKeyMenu = from.IsKeyMenu;
            to.IsKeyShift = from.IsKeyShift;
            to.IsKeyWindows = from.IsKeyWindows;
        }

        /// <summary>
        /// cache waiting method that have EventCatchAttribute
        /// </summary>
        private void PrepareEventCatchFilter()
        {
            if (attrBuf != null)
            {
                return;
            }

            attrBuf = new Dictionary<FeatureBase, Dictionary<string, EventCatchAttribute>>();
            foreach (var fc in getFeatures())
            {
                if (attrBuf.TryGetValue(fc, out var mas) == false)
                {
                    attrBuf[fc] = mas = new Dictionary<string, EventCatchAttribute>();
                }
                foreach (var methodName in new[] { "OnPointerPressed", "OnPointerHold", "OnPointerMoved", "OnPointerReleased", "OnMouseWheelChanged" })
                {
                    var mi = fc.GetType().GetMethod(methodName);
                    if (mi != null)
                    {
                        var attrs = mi.GetCustomAttributes(typeof(EventCatchAttribute), true);
                        if (attrs?.Length > 0)
                        {
                            var attr = attrs[0];
                            mas[methodName] = (EventCatchAttribute)attr;
                        }
                    }
                }
            }
        }

        private bool CheckEventCatchStatus(FeatureBase fc, string methodName)
        {
            var mas = attrBuf[fc];
            if (mas.TryGetValue(methodName, out var attr))
            {
                if (attr.IsStatusFilter())
                {
                    return attr.CheckStatus(this);
                }
            }
            return true;    // when no status, return true to be a check target
        }

        private void KickPointerEvent(string description, Action<IPointerListener> pointerEvent)
        {
            foreach (IPointerListener fc in getPointerListenerFeatures().Where(a => a is IPointerListener))
            {
                try
                {
                    if (string.IsNullOrEmpty(description) || CheckEventCatchStatus((FeatureBase)fc, description))
                    {
                        pointerEvent.Invoke(fc);
                    }
                }
                catch (Exception ex)
                {
                    LOG.AddException(ex);
                    if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                    {
                        Kill((FeatureBase)fc, ex);
                    }
                }
            }
        }
        private void KickWheelEvent(string description, Action<IPointerWheelListener> pointerEvent)
        {
            foreach (IPointerWheelListener fc in getPointerListenerFeatures().Where(a => a is IPointerWheelListener))
            {
                try
                {
                    if (string.IsNullOrEmpty(description) || CheckEventCatchStatus((FeatureBase)fc, description))
                    {
                        pointerEvent.Invoke(fc);
                    }
                }
                catch (Exception ex)
                {
                    LOG.AddException(ex);
                    if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                    {
                        Kill((FeatureBase)fc, ex);
                    }
                }
            }
        }
    }
}
