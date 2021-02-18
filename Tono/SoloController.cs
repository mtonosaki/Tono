// (c) 2021 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tono
{
    /// <summary>
    /// Controlls action executed only one time.
    /// </summary>
    /// <see>UnitTest</see>
    /// <remarks>
    /// var solo = new SoloController();
    /// solo.StartSoloAsync(() => Task.Run(async () => { ... }));  // The action will be execute only one time. The other flow will be wait by the action finished.
    /// </remarks>
    public class SoloController
    {
        public bool IsSoloEnabled { get; set; } = true;

        private TaskCompletionSource<bool> sync = null;

        public async Task StartSoloAsync(Func<Task> action, Func<Task> CancelledCallback = null)
        {
            if (IsSoloEnabled && sync != null)
            {
                await sync.Task;
                var ct = CancelledCallback?.Invoke();
                if (ct != null)
                {
                    await ct;
                }
                return;
            }
            sync = new TaskCompletionSource<bool>();
            var at = action.Invoke();
            await at;
            sync.SetResult(true);
        }

        public void Clear()
        {
            sync = null;
        }
        public Task ClearAsync()
        {
            Clear();
            return Task.CompletedTask;
        }
    }
}
