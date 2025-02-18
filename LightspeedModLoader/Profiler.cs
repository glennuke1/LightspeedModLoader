using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LightspeedModLoader
{
    public class Profiler
    {
        private Dictionary<string, Stopwatch> timers = new Dictionary<string, Stopwatch>();

        public void Start(string name)
        {
            if (!timers.ContainsKey(name))
                timers[name] = new Stopwatch();

            timers[name].Reset();
            timers[name].Start();
        }

        public void Stop(string name)
        {
            if (timers.ContainsKey(name))
            {
                timers[name].Stop();
                LML_Debug.Log($"[Profiler] {name} took {timers[name].ElapsedMilliseconds} ms");
                timers.Remove(name);
            }
            else
            {
                LML_Debug.LogWarning($"[Profiler] Timer '{name}' was never started.");
            }
        }
    }
}
