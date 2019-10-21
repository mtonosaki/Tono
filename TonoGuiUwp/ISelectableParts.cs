namespace Tono.Gui.Uwp
{
    /// <summary>
    /// parts interface of selectable support
    /// </summary>
    public interface ISelectableParts
    {
        /// <summary>
        /// calculate selecting position score
        /// </summary>
        /// <param name="pane">target pane</param>
        /// <param name="pos">pointer position</param>
        /// <returns>0=exactly same position, 0.99999=Selectable limit far position,  1～Out of selection position</returns>
        float SelectingScore(IDrawArea pane, ScreenPos pos);

        /// <summary>
        /// true=Selected
        /// </summary>
        bool IsSelected { get; set; }
    }

    public interface IMovableParts
    {
        /// <summary>
        /// Remember origin position(before move)
        /// </summary>
        void SaveLocationAsOrigin();

        /// <summary>
        /// Move parts
        /// </summary>
        /// <param name="pane">target pane</param>
        /// <param name="offset">offset volume from location saved SaveLocationAsOrigin</param>
        void Move(IDrawArea pane, ScreenSize offset);

        /// <summary>
        /// Check parts moved (check current position not equals to SaveLocationAsOrigin)
        /// </summary>
        /// <returns></returns>
        bool IsMoved();
    }
}
