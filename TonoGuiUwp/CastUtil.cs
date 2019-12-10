// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using Windows.Devices.Input;

namespace Tono.Gui.Uwp
{
    public static class CastUtil
    {
        /// <summary>
        /// save pointer state from ManipulationStartedRoutedEventArgs
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static PointerState _(Windows.UI.Xaml.Input.ManipulationStartingRoutedEventArgs e, object sender, string remarks)
        {
            var ret = new PointerState
            {
                Sender = sender,
                Remarks = remarks,
                IsInContact = true,
                Scale = 1.0f,
                Time = DateTime.Now,
            };
            if (e.Pivot != null)
            {
                ret.PositionOrigin = ScreenPos.From(e.Pivot.Center.X, e.Pivot.Center.Y);
                ret.Position = ScreenPos.From(e.Pivot.Center.X, e.Pivot.Center.Y);
                ret.Rotation = Angle.FromRad(e.Pivot.Radius);
            }

            ret.DeviceType = PointerState.DeviceTypes.Touch;    // set Touch even if using Pen
            return ret;
        }

        /// <summary>
        /// save pointer state from ManipulationStartedRoutedEventArgs
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static PointerState _(Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e, object sender, string remarks)
        {
            var ret = new PointerState
            {
                Sender = sender,
                Remarks = remarks,
                PositionOrigin = ScreenPos.From(e.Position.X, e.Position.Y),
                Position = ScreenPos.From(e.Position.X, e.Position.Y),
                IsInContact = true,
                Scale = e.Cumulative.Scale,
                Rotation = Angle.FromDeg(e.Cumulative.Rotation),
                Time = DateTime.Now,
            };
            switch (e.PointerDeviceType)
            {
                case PointerDeviceType.Pen:
                    ret.DeviceType = PointerState.DeviceTypes.Pen;
                    break;
                case PointerDeviceType.Touch:
                    ret.DeviceType = PointerState.DeviceTypes.Touch;
                    break;
                case PointerDeviceType.Mouse:
                    ret.DeviceType = PointerState.DeviceTypes.Mouse;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// save pointer state from ManipulationCompletedRoutedEventArgs
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static PointerState _(Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e, object sender, string remarks)
        {
            var ret = new PointerState
            {
                Sender = sender,
                Remarks = remarks,
                Position = ScreenPos.From(e.Position.X, e.Position.Y),
                IsInContact = false,
                Scale = e.Cumulative.Scale,
                Rotation = Angle.FromDeg(e.Cumulative.Rotation),
                Time = DateTime.Now,
            };
            switch (e.PointerDeviceType)
            {
                case PointerDeviceType.Pen:
                    ret.DeviceType = PointerState.DeviceTypes.Pen;
                    break;
                case PointerDeviceType.Touch:
                    ret.DeviceType = PointerState.DeviceTypes.Touch;
                    break;
                case PointerDeviceType.Mouse:
                    ret.DeviceType = PointerState.DeviceTypes.Mouse;
                    break;
            }
            return ret;
        }
        /// <summary>
        /// save pointer state from ManipulationDeltaRoutedEventArgs
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static PointerState _(Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e, object sender, string remarks)
        {
            var ret = new PointerState
            {
                Sender = sender,
                Remarks = remarks,
                Position = ScreenPos.From(e.Position.X, e.Position.Y),
                IsInContact = true,
                Scale = e.Cumulative.Scale,
                Rotation = Angle.FromDeg(e.Cumulative.Rotation),
                Time = DateTime.Now,
            };
            switch (e.PointerDeviceType)
            {
                case PointerDeviceType.Pen:
                    ret.DeviceType = PointerState.DeviceTypes.Pen;
                    break;
                case PointerDeviceType.Touch:
                    ret.DeviceType = PointerState.DeviceTypes.Touch;
                    break;
                case PointerDeviceType.Mouse:
                    ret.DeviceType = PointerState.DeviceTypes.Mouse;
                    break;
            }
            return ret;
        }

        public static PointerState _(Windows.UI.Xaml.Input.ManipulationInertiaStartingRoutedEventArgs e, object sender, string remarks)
        {
            var ret = new PointerState
            {
                Sender = sender,
                Remarks = remarks,
                IsInContact = true,
                Scale = e.Cumulative.Scale,
                Rotation = Angle.FromDeg(e.Cumulative.Rotation),
                Time = DateTime.Now,
            };
            switch (e.PointerDeviceType)
            {
                case PointerDeviceType.Pen:
                    ret.DeviceType = PointerState.DeviceTypes.Pen;
                    break;
                case PointerDeviceType.Touch:
                    ret.DeviceType = PointerState.DeviceTypes.Touch;
                    break;
                case PointerDeviceType.Mouse:
                    ret.DeviceType = PointerState.DeviceTypes.Mouse;
                    break;
            }
            return ret;
        }
        


        /// <summary>
        /// save pointer state from HoldingRoutedEventArgs
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public static PointerState _(Windows.UI.Xaml.Input.HoldingRoutedEventArgs e, object sender, string remarks)
        {
            var ret = new PointerState
            {
                Sender = sender,
                Remarks = remarks,
                IsInContact = e.HoldingState == Windows.UI.Input.HoldingState.Started ? true : false,
                Time = DateTime.Now,
            };
            return ret;
        }

        /// <summary>
        /// save pointer state from PointerEventArgs
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static PointerState _(Windows.UI.Core.PointerEventArgs e, object sender, string remarks)
        {
            var ret = new PointerState
            {
                Sender = sender,
                Remarks = remarks,

                Position = ScreenPos.From(e.CurrentPoint.Position.X, e.CurrentPoint.Position.Y),
                WheelDelta = e.CurrentPoint.Properties.MouseWheelDelta,

                Time = DateTime.Now,
                IsInContact = e.CurrentPoint.IsInContact,
                IsKeyControl = (e.KeyModifiers & Windows.System.VirtualKeyModifiers.Control) != 0,
                IsKeyMenu = (e.KeyModifiers & Windows.System.VirtualKeyModifiers.Menu) != 0,
                IsKeyShift = (e.KeyModifiers & Windows.System.VirtualKeyModifiers.Shift) != 0,
                IsKeyWindows = (e.KeyModifiers & Windows.System.VirtualKeyModifiers.Windows) != 0,
            };
            switch (e.CurrentPoint.PointerDevice.PointerDeviceType)
            {
                case PointerDeviceType.Mouse:
                    ret.DeviceType = PointerState.DeviceTypes.Mouse;
                    break;
                case PointerDeviceType.Pen:
                    ret.DeviceType = PointerState.DeviceTypes.Pen;
                    break;
                default:
                    ret.DeviceType = PointerState.DeviceTypes.Touch;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// cast support from ScreenRect to Windows.Foundation.Rect
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Windows.Foundation.Rect _(ScreenRect value)
        {
            return new Windows.Foundation.Rect
            {
                X = value.LT.X.Sx,
                Y = value.LT.Y.Sy,
                Width = value.Width.Sx,
                Height = value.Height.Sy,
            };
        }

        /// <summary>
        /// cast support
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static System.Numerics.Vector2 _((double X, double Y) value)
        {
            return new System.Numerics.Vector2
            {
                X = (float)value.X,
                Y = (float)value.Y,
            };
        }

        /// <summary>
        /// cast support
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static System.Numerics.Vector2 _(ScreenPos value)
        {
            return new System.Numerics.Vector2
            {
                X = value.X,
                Y = value.Y,
            };
        }

        /// <summary>
        /// cast support
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static System.Numerics.Vector2 _(ScreenSize value)
        {
            return new System.Numerics.Vector2
            {
                X = value.Width,
                Y = value.Height,
            };
        }

        /// <summary>
        /// cast support
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenRect _(Windows.Foundation.Rect value)
        {
            return new ScreenRect
            {
                LT = new ScreenPos
                {
                    X = new ScreenX { Sx = (float)value.Left },
                    Y = new ScreenY { Sy = (float)value.Top },
                },
                RB = new ScreenPos
                {
                    X = new ScreenX { Sx = (float)value.Right },
                    Y = new ScreenY { Sy = (float)value.Bottom },
                },
            };
        }

        /// <summary>
        /// cast support
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenSize _(Windows.Foundation.Size value)
        {
            return new ScreenSize
            {
                Width = ScreenX.From(value.Width),
                Height = ScreenY.From(value.Height),
            };
        }

        /// <summary>
        /// cast support
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScreenPos _(System.Numerics.Vector2 value)
        {
            return new ScreenPos
            {
                X = ScreenX.From(value.X),
                Y = ScreenY.From(value.Y),
            };
        }

    }
}
