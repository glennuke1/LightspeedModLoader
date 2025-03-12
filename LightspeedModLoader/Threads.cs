using System;
using System.Threading;

namespace LightspeedModLoader.Threading
{
    public static class Threads
    {
        /// <summary>
        /// Creates a new thread and runs it
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Thread RunMultithread(Action action)
        {
            Thread thread = new Thread(() => { action.Invoke(); });
            thread.Start();
            return thread;
        }
    }
}
