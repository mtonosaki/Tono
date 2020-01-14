// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// pointer event handling
    /// </summary>
    public partial class TGuiView
    {
        private Dictionary<FeatureBase, Dictionary<string, EventCatchAttribute>> attrBuf = null;

        private void InitPointer()
        {
            var win = Window.Current.CoreWindow;
            win.PointerMoved += OnPointerMoved;
            win.PointerPressed += OnPointerPressed;
            win.PointerReleased += OnPointerReleased;
            win.PointerWheelChanged += OnPointerWheelChanged;

            ManipulationStarted += OnManipulationStarted;
            ManipulationMode = ManipulationModes.All;
            ManipulationStarting += OnManipulationStarting;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            ManipulationInertiaStarting += OnManipulationInertiaStarting;
            Holding += OnHolding;
            IsHoldingEnabled = true;
            //Canvas.Holding += OnHolding;
            //Canvas.IsHoldingEnabled = true;
        }

        private string ___(PointerState po)
        {
            return $"{po.Time.ToString(TimeUtil.FormatYMDHMSms)} {po.DeviceType} {po.Remarks}  Pos={po.Position}  Finger={po.FingerCount}  Contact={po.IsInContact}, Key='{(po.IsKeyControl ? "C" : "")}{(po.IsKeyControl ? "S" : "")}{(po.IsKeyShift ? "S" : "")}{(po.IsKeyWindows ? "W" : "")}{(po.IsKeyMenu ? "M" : "")}' Scale={po.Scale} Wheel={po.WheelDelta}";
        }


        bool IsOnPointerPressed = false;
        bool IsOnManipulationStarted = false;
        bool IsOnManipulationInertiaStarting = false;
        bool IsWaitingManipulationDelta = false;
        PointerState StateAtPressed = null;
        ScreenPos StartPosition;
        PointerState Move;
        int FingerCount = 0;

        private void Reset()
        {
            IsOnPointerPressed = false;
            IsOnManipulationStarted = false;
            IsOnManipulationInertiaStarting = false;
            IsWaitingManipulationDelta = false;
            _2ndFingerLostFollow?.Stop();
            _2ndFingerLostFollow = null;
            StateAtPressed = null;
            FingerCount = 0;
        }

        /// <summary>
        /// [DONW-1] On Manipulation Starting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// by all pointer's first activity
        /// </remarks>
        private void OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            Reset();    // Poka-yoke
            var po = _(e, this, "OnManipulationStarting");
            //Debug.WriteLine(___(po));
        }


        /// <summary>
        /// [DONW-2] On Pointer Pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// by Finger / Click
        /// </remarks>
        private void OnPointerPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            var po = _(e, this, "OnPointerPressed");
            FingerCount++;
            po.FingerCount = FingerCount;
            Move = po.Clone();
            //Debug.WriteLine(___(po));

            if (po.DeviceType == PointerState.DeviceTypes.Mouse)
            {
                StartPosition = po.Position;
                po.PositionOrigin = StartPosition;
                KickPointerEvent("OnPointerPressed", fc => fc.OnPointerPressed(po));
            }
            else
            {
                if (IsOnManipulationStarted)
                {
                    IsWaitingManipulationDelta = true;
                    _2ndFingerLostFollow?.Stop();
                    _2ndFingerLostFollow = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(100),
                    };
                    _2ndFingerLostFollow.Tick += _2ndFingerLostFollow_Tick;
                    StateAtPressed = po.Clone();
                    _2ndFingerLostFollow.Start();
                }
            }
            IsOnPointerPressed = true;
        }

        /// <summary>
        /// [DONW-3] On Manipulation Started
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// MOUSE : Hold + Drag
        /// TOUCH : not 1-Tap (1-Swipe, 2-Tap)
        /// </remarks>
        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var po = _(e, this, "OnManipulationStarted");
            po.FingerCount = FingerCount;
            //Debug.WriteLine(___(po));

            if (po.DeviceType == PointerState.DeviceTypes.Mouse)
            {
            }
            else
            {
                StartPosition = po.Position;
                po.PositionOrigin = StartPosition;
                KickPointerEvent("OnPointerPressed", fc => fc.OnPointerPressed(po));
            }

            IsOnManipulationStarted = true;
        }

        /// <summary>
        /// [DOWN-4] On Manipulation Inertia Starting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Auto acceleration Starging
        /// </remarks>
        private void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            var po = _(e, this, "OnManipulationInertiaStarting");
            //Debug.WriteLine(___(po));

            IsOnManipulationInertiaStarting = true;
        }

        /// <summary>
        /// [MOVE-1] On Pointer Moved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// by Finger
        /// by OnPointerPressed
        /// Coodinations are in "On Pointer Pressed"
        /// </remarks>
        private void OnPointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            var po = _(e, this, "OnPointerMoved");
            po.FingerCount = FingerCount;
            Move = po.Clone();
            //Debug.WriteLine(___(po));

            if (po.DeviceType == PointerState.DeviceTypes.Mouse)
            {
                if (IsOnManipulationStarted == false)
                {
                    po.PositionOrigin = StartPosition;
                    KickPointerEvent("OnPointerMoved", fc => fc.OnPointerMoved(po));
                }
            }
        }

        /// <summary>
        /// [MOVE-2] On Manipulation Delta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// by Action(not by finger)
        /// Coodination is in "On Manipulation Started"
        /// </remarks>
        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var po = _(e, this, "OnManipulationDelta", Move);
            po.FingerCount = FingerCount;
            //Debug.WriteLine($"{___(po)} Angular={e.Velocities.Angular}  Linear={e.Velocities.Linear}  Expansion={e.Velocities.Expansion}");

            if (po.DeviceType == PointerState.DeviceTypes.Mouse)
            {
                if (IsOnManipulationStarted)
                {
                    po.PositionOrigin = StartPosition;
                    KickPointerEvent("OnPointerMoved", fc => fc.OnPointerMoved(po));
                }
            }
            else
            {
                if (IsWaitingManipulationDelta)
                {
                    _2ndFingerLostFollow?.Stop();
                    IsWaitingManipulationDelta = false;
                    po.PositionOrigin = po.Position;
                    KickPointerEvent("OnPointerPressed", fc => fc.OnPointerPressed(po));    // Fire 2nd finger (Waited Delta to get adjusted position)
                }

                po.PositionOrigin = StartPosition;
                KickPointerEvent("OnPointerMoved", fc => fc.OnPointerMoved(po));    // TODO: When release finger 2 to 1, Position jump by UWP
            }
        }

        DispatcherTimer _2ndFingerLostFollow = null;
        private void _2ndFingerLostFollow_Tick(object sender, object e)
        {
            IsWaitingManipulationDelta = false;
            _2ndFingerLostFollow?.Stop();
            var po = StateAtPressed;
            po.PositionOrigin = StartPosition;
            KickPointerEvent("OnPointerPressed", fc => fc.OnPointerPressed(po));    // Fire 2nd finger (no Delta event follow-up)
        }


        /// <summary>
        /// [UP-1] On Pointer Released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// by Finger
        /// by OnPointerPressed
        /// The last trigger when there is no "OnManipulationStarted"
        /// </remarks>
        private void OnPointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            var po = _(e, this, "OnPointerReleased");
            FingerCount--;
            po.FingerCount = FingerCount;
            //Debug.WriteLine(___(po));

            if (IsOnManipulationInertiaStarting == false)   // otherwise, waiting OnManipulationCompleted
            {
                if (po.DeviceType == PointerState.DeviceTypes.Mouse)
                {
                    KickPointerEvent("OnPointerReleased", fc => fc.OnPointerReleased(po));
                }
                else
                {
                    if (IsOnPointerPressed && IsOnManipulationStarted == false) // One Finger Tap
                    {
                        po.PositionOrigin = po.Position;
                        po.FingerCount++;
                        po.IsInContact = true;
                        KickPointerEvent("OnPointerPressed", fc => fc.OnPointerPressed(po));    // Fire virtual press event
                        po.FingerCount--;
                        po.IsInContact = false;
                        KickPointerEvent("OnPointerReleased", fc => fc.OnPointerReleased(po));
                    }
                }
            }
        }

        /// <summary>
        /// [UP-2] On Manipulation Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// The last trigger when "OnManipulationStarted"
        /// </remarks>
        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var po = _(e, this, "OnManipulationCompleted");
            po.FingerCount = FingerCount;
            po.PositionOrigin = StartPosition;
            //Debug.WriteLine(___(po));

            if (po.DeviceType == PointerState.DeviceTypes.Mouse)
            {
                if (IsOnManipulationInertiaStarting)
                {
                    KickPointerEvent("OnPointerReleased", fc => fc.OnPointerReleased(po));
                    Reset();
                }
            }
            else
            {
                KickPointerEvent("OnPointerReleased", fc => fc.OnPointerReleased(po));
                Reset();
            }
        }

        /// <summary>
        /// [OTHER]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPointerWheelChanged(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            var po = _(e, this, "OnPointerWheelChanged");
            po.FingerCount = FingerCount;
            //Debug.WriteLine(___(po));
            KickWheelEvent("OnPointerWheelChanged", fc => fc.OnMouseWheelChanged(po));
        }

        private void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            var holding = e.HoldingState == Windows.UI.Input.HoldingState.Started;
            var po = _(e, this, $"OnHolding {e.HoldingState}");
            po.FingerCount = FingerCount;
            //Debug.WriteLine(___(po));

            KickPointerEvent(null, fc =>
            {
                if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
                {
                    if (CheckEventCatchStatus((FeatureBase)fc, "OnPointerHold"))
                    {
                        fc.OnPointerHold(po);
                    }
                }
                if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
                {
                    if (CheckEventCatchStatus((FeatureBase)fc, "OnPointerReleased"))
                    {
                        fc.OnPointerReleased(po);
                    }
                }
            });
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

        private void KickPointerEvent(string methodName, Action<IPointerListener> pointerEvent)
        {
            foreach (IPointerListener fc in getPointerListenerFeatures().Where(a => a is IPointerListener))
            {
                try
                {
                    if (string.IsNullOrEmpty(methodName) || CheckEventCatchStatus((FeatureBase)fc, methodName))
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
