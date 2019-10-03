#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IRecorder �̊T�v�̐����ł��B
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
