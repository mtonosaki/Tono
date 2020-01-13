// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// feature interface to support pointer listener
    /// </summary>
    public interface IPointerListener
    {
        /// <summary>
        /// auto fire when pointer pressed
        /// </summary>
        /// <param name="po"></param>
        void OnPointerPressed(PointerState po);

        /// <summary>
        /// auto fire when pointer hold
        /// </summary>
        /// <param name="po"></param>
        void OnPointerHold(PointerState po);

        /// <summary>
        /// auto fire when pointer moved
        /// </summary>
        /// <param name="po"></param>
        void OnPointerMoved(PointerState po);

        /// <summary>
        /// auto fire when pointer released
        /// </summary>
        /// <param name="po"></param>
        void OnPointerReleased(PointerState po);
    }

    /// <summary>
    /// mouse wheel event catch support for features
    /// </summary>
    public interface IPointerWheelListener
    {
        void OnMouseWheelChanged(PointerState po);
    }
}
