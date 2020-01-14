// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono
{
    /// <summary>
    /// Mutex between processes
    /// </summary>
    /// <example>
    /// using(new uMutex("MyMutexName")
    /// {
    ///     // write mutexed task here
    /// }
    /// </example>
    public class Mutex : IDisposable
    {
        private readonly System.Threading.Mutex _mutex = null;

        public Mutex(string globalName)
        {
            _mutex = new System.Threading.Mutex(false, globalName);
        }

        public void Dispose()
        {
            try
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
                // _mutex.Close(); .NET Standard 2.0
            }
            catch
            {
            }
        }
    }
}
