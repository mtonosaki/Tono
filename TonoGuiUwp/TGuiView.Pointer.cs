using System;
using System.Collections.Generic;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// pointer event handling
    /// </summary>
    public partial class TGuiView
    {
        private void initPointer()
        {
            var win = Window.Current.CoreWindow;
            win.PointerMoved += onPointerMoved;
            win.PointerPressed += onPointerPressed;
            win.PointerReleased += onPointerReleased;
            win.PointerWheelChanged += onPointerWheelChanged;
            ManipulationStarted += onManipulationStarted;
            ManipulationMode = Windows.UI.Xaml.Input.ManipulationModes.All;
            ManipulationStarting += onManipulationStarting;
            ManipulationDelta += onManipulationDelta;
            ManipulationCompleted += onManipulationCompleted;
            Holding += onHolding;
        }

        private Dictionary<FeatureBase, Dictionary<string, EventCatchAttribute>> attrBuf = null;

        /// <summary>
        /// cache waiting method that have EventCatchAttribute
        /// </summary>
        private void prepareEventCatchFilter()
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

        private bool isEventCatch(FeatureBase fc, string methodName)
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

        private void onHolding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            var po = _(e, this, "onHolding");
            po.Position = pressed.Position;
            po.PositionOrigin = pressed.Position;
            po.IsKeyControl = pressed.IsKeyControl;
            po.IsKeyMenu = pressed.IsKeyMenu;
            po.IsKeyShift = pressed.IsKeyShift;
            po.IsKeyWindows = pressed.IsKeyWindows;
            var isHolding = e.HoldingState == Windows.UI.Input.HoldingState.Started;

            foreach (var fc in getPointerListenerFeatures())
            {
                try
                {
                    if (isHolding)
                    {
                        if (isEventCatch(fc, "OnPointerHold"))
                        {
                            ((IPointerListener)fc).OnPointerHold(po);
                        }
                    }
                    else
                    {
                        if (isEventCatch(fc, "OnPointerReleased"))
                        {
                            ((IPointerListener)fc).OnPointerReleased(po);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG.AddException(ex);

                    if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                    {
                        Kill(fc, ex);
                    }
                }
            }
        }

        private PointerState pressed;

        private void onPointerPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            //if (e.CurrentPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var po = _(e, this, "onPointerPressed");
                po.PositionOrigin = ScreenPos.From(po.Position.X, po.Position.Y);
                pressed = po;

                foreach (var fc in getPointerListenerFeatures())
                {
                    try
                    {
                        if (isEventCatch(fc, "OnPointerPressed"))
                        {
                            ((IPointerListener)fc).OnPointerPressed(po);
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.AddException(ex);

                        if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                        {
                            Kill(fc, ex);
                        }
                    }
                }
            }
        }

        private void onPointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            if (e.CurrentPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var po = _(e, this, "onPointerMoved");
                po.PositionOrigin = pressed?.PositionOrigin ?? po.PositionOrigin;

                foreach (var fc in getPointerListenerFeatures())
                {
                    try
                    {
                        if (isEventCatch(fc, "OnPointerMoved"))
                        {
                            ((IPointerListener)fc).OnPointerMoved(po);
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.AddException(ex);

                        if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                        {
                            Kill(fc, ex);
                        }
                    }
                }
            }
        }
        private void onPointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            //if (e.CurrentPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var po = _(e, this, "onPointerReleased");
                if (pressed != null)    // Poka-yoke. released but not pressed when some windows condition
                {
                    po.PositionOrigin = pressed.PositionOrigin;
                }

                foreach (var fc in getPointerListenerFeatures())
                {
                    try
                    {
                        if (isEventCatch(fc, "OnPointerReleased"))
                        {
                            ((IPointerListener)fc).OnPointerReleased(po);
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.AddException(ex);

                        if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                        {
                            Kill(fc, ex);
                        }
                    }
                }
            }
        }

        private void onPointerWheelChanged(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs e)
        {
            if (e.CurrentPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var po = _(e, this, "onPointerWheelChanged");

                foreach (var fc in getPointerListenerFeatures())
                {
                    try
                    {
                        if (isEventCatch(fc, "OnPointerPressed"))
                        {
                            ((IPointerListener)fc).OnPointerPressed(po);
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.AddException(ex);

                        if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                        {
                            Kill(fc, ex);
                        }
                    }
                }
            }
        }

        private void onManipulationStarting(object sender, Windows.UI.Xaml.Input.ManipulationStartingRoutedEventArgs e)
        {
        }

        private void onManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch || e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                var po = _(e, this, "onManipulationStarted");
                if (pressed == null)
                {
                    pressed = po;
                }
                po.PositionOrigin = pressed?.PositionOrigin ?? po.PositionOrigin;

                foreach (var fc in getPointerListenerFeatures())
                {
                    try
                    {
                        if (isEventCatch(fc, "OnPointerMoved"))
                        {
                            ((IPointerListener)fc).OnPointerMoved(po);  // Started of tap event have already sent
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.AddException(ex);

                        if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                        {
                            Kill(fc, ex);
                        }
                    }
                }
            }
        }

        private void onManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch || e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                var po = _(e, this, "onManipulationDelta");
                po.PositionOrigin = pressed?.PositionOrigin ?? po.PositionOrigin;

                foreach (var fc in getPointerListenerFeatures())
                {
                    try
                    {
                        if (isEventCatch(fc, "OnPointerMoved"))
                        {
                            ((IPointerListener)fc).OnPointerMoved(po);
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.AddException(ex);

                        if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                        {
                            Kill(fc, ex);
                        }
                    }
                }
            }
        }
        private void onManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch || e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                var po = _(e, this, "onManipulationCompleted");
                po.PositionOrigin = pressed?.PositionOrigin ?? po.PositionOrigin;

                foreach (var fc in getPointerListenerFeatures())
                {
                    try
                    {
                        if (isEventCatch(fc, "OnPointerReleased"))
                        {
                            ((IPointerListener)fc).OnPointerReleased(po);
                        }
                    }
                    catch (Exception ex)
                    {
                        LOG.AddException(ex);

                        if (fc is IAutoRemovable)   // NOTE: cannot catch when NOT thread safe
                        {
                            Kill(fc, ex);
                        }
                    }
                }
            }
        }
    }
}
