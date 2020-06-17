// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IRecorder の概要の説明です。
    /// </summary>
    public interface IRecorder
    {
        void RecorderStartChunk(string debugName);
        void RecorderSave(object value, Id savingObjectID);
        void RecorderEndChunk();
        void RecorderCancelChunk();
        void RecorderSetChunkLength(int length);
        bool RecorderIsChunkStarted { get; }
        void RecorderReset();
    }
}
