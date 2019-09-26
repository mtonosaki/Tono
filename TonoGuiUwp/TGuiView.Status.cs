using System.Collections.Generic;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Status sharing manager interface
    /// </summary>
    public interface IStatusManager
    {
        bool TryGetStatus(string name, out EventStatus sts);
        EventStatus GetStatus(string name);
    }

    /// <summary>
    /// sharing status function handling
    /// </summary>
    public partial class TGuiView : IStatusManager
    {
        private readonly Dictionary<string, EventStatus> _statusBuf = new Dictionary<string, EventStatus>();

        /// <summary>
        /// try to get status by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>ture = have</returns>
        public bool TryGetStatus(string name, out EventStatus sts)
        {
            return _statusBuf.TryGetValue(name, out sts);
        }

        /// <summary>
        /// get status
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EventStatus GetStatus(string name)
        {
            if (_statusBuf.TryGetValue(name, out var st) == false)
            {
                _statusBuf[name] = st = new EventStatus
                {
                    Name = name,
                };
            }
            return st;
        }
    }
}
