// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono.Gui
{
    /// <summary>
    /// Pointer state object
    /// </summary>
    public class PointerState : EventToken
    {
        /// <summary>
        /// kind of device
        /// </summary>
        public enum DeviceTypes
        {
            None,   // none device
            Mouse,  // mouse device
            Touch,  // touch device
            Pen,    // pen device
        }

        /// <summary>
        /// device type
        /// </summary>
        public DeviceTypes DeviceType { get; set; }

        /// <summary>
        /// origin position (canvas screen coodinate)
        /// </summary>
        public ScreenPos PositionOrigin { get; set; }

        /// <summary>
        /// current position (canvas screen coodinate)
        /// </summary>
        public ScreenPos Position { get; set; }

        /// <summary>
        /// amount of pointer move from origin to current
        /// </summary>
        public ScreenSize PositionDelta => Position - PositionOrigin;

        /// <summary>
        /// number of finger.
        /// </summary>
        public int FingerCount { get; set; }

        /// <summary>
        /// mouse wheel notch amount
        /// </summary>
        public static readonly int WheelNotch = 120;

        /// <summary>
        /// mouse wheel position
        /// </summary>
        public int WheelDelta { get; set; }

        /// <summary>
        /// pinch size of tablet mode (default = 1.0)
        /// </summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>
        /// rotation of tablet mode (default = 0)
        /// </summary>
        /// <remarks>
        /// Clock-wise : plus value
        /// </remarks>
        public Angle Rotation { get; set; } = Angle.Zero;

        /// <summary>
        /// point local time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// check any point contacted like mouse clicking(mouse, touch and etc...)
        /// </summary>
        public bool IsInContact { get; set; }

        /// <summary>
        /// check no key push
        /// </summary>
        public bool IsKeyNone => !IsKeyControl && !IsKeyMenu && !IsKeyMenu && !IsKeyShift;

        /// <summary>
        /// check pushing control key
        /// </summary>
        public bool IsKeyControl { get; set; }

        /// <summary>
        /// check pushing menu key
        /// </summary>
        public bool IsKeyMenu { get; set; }

        /// <summary>
        /// check pushing shift key
        /// </summary>
        public bool IsKeyShift { get; set; }

        /// <summary>
        /// check pushing windows key
        /// </summary>
        public bool IsKeyWindows { get; set; }

        public PointerState Clone()
        {
            return new PointerState
            {
                DeviceType = DeviceType,
                PositionOrigin = PositionOrigin.Clone(),
                Position = Position.Clone(),
                FingerCount = FingerCount,
                WheelDelta = WheelDelta,
                Scale = Scale,
                Rotation = Angle.FromDeg(Rotation.Deg),
                Time = Time,
                IsInContact = IsInContact,
                IsKeyControl = IsKeyControl,
                IsKeyMenu = IsKeyMenu,
                IsKeyShift = IsKeyShift,
                IsKeyWindows = IsKeyWindows,
            };
        }
    }
}
